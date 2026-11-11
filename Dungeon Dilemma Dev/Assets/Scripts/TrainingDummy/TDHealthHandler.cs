using ControllerLogic;
using Priestess;
using PurrNet;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the health values, take damage function, and heal function for all objects with health
/// </summary>
public class TDHealthHandler : NetworkBehaviour, IDamageable
{
    [Header("Training Dummy Stats")]
    public SyncedStats Stats;
    public TDCombatManager Combat;

    [Header("Status Effect VFX")]
    [SerializeField] private StatusEffectPrefabMap _sePrefabMap;

    [Header("Damage Text VFX")]
    [SerializeField] private GameObject _damageTextPrefab;

    [Header("Training Dummy Health Bar")]
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _healColor;
    [SerializeField] private Color _pulseColor;
    [SerializeField] private RectTransform _healthBarRect;
    [SerializeField] private Image _healthBarFill;

    [Header("Training Dummy Stats")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _goldPayoutText;

    // Public accessors for colors
    public Color DefaultColor => _defaultColor;
    public Color HealColor => _healColor;
    public Color PulseColor => _pulseColor;

    public event Action OnDeath;
    public event Action<Vector2, bool> OnAttacked; // attack source (to handle knockback)
    private bool _isInvincible = false;
    private float _fullHealthBarWidth;
    private float _flashDuration = 0.25f;
    private float _lowHPThreshold = 0.25f;
    private float _pulseSpeed = 5f;

    // Health bar animation states
    private bool _isFlashing = false;
    private bool _isPulsing = false;
    private Color _originalBarColor;
    private Coroutine _flashRoutine;

    private void Start()
    {
        _fullHealthBarWidth = _healthBarRect.rect.width;
        _originalBarColor = _healthBarFill.color;
        Stats.CurrentHealth.onChanged += UpdateHealthBar;
    }

    private void Update()
    {
        // Handle pulsing when health is low
        if (_isPulsing && !_isFlashing)
        {
            float lerp = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            _healthBarFill.color = Color.Lerp(_originalBarColor, _pulseColor, lerp);
        }
        else if (!_isFlashing)
        {
            // Reset to original color if not flashing
            if (_healthBarFill.color != _originalBarColor)
                _healthBarFill.color = _originalBarColor;
        }
        _levelText.text = $"Level {GlobalController.Singleton.TDLevel.value}";
        _goldPayoutText.text = $"{GlobalController.Singleton.TDBasePayout + GlobalController.Singleton.TDPayoutIncrement * (GlobalController.Singleton.TDLevel.value - 1)} Gold Payout";
    }

    /// <summary>
    /// Updates the object's max health.
    /// </summary>
    /// <param name="newMax"> The new max health value. </param>
    [ServerRpc(requireOwnership: false)]
    public void UpdateMaxHealth(float newMax)
    {
        Stats.MaximumHealth.value = newMax;
    }

    /// <summary>
    /// Subtract total damage taken from health and updates value in server.
    /// </summary>
    /// <param name="damage"> Damage taken (calculated externally). </param>
    /// <param name="effects"> List of StatusEffectType enum values applied to the target. </param>
    /// <param name="source"> The origin of the damage dealt. </param>
    /// <param name="isStatusEffect"> Whether the damage dealt was from a status effect or not. </param>
    [ServerRpc(requireOwnership: false)]
    public void TakeDamageServerRpc(float damage, List<StatusEffectType> effects, Transform source, bool isStatusEffect = false, StatusEffectType type = StatusEffectType.Bleed)
    {
        if (Stats.CurrentHealth.value <= 0 || _isInvincible) return;

        Combat.RegisterDamageIncoming(damage, effects, source, isStatusEffect, type);

        // Spawn visuals for Status Effects
        if (effects != null)
        {
            foreach (var effectType in effects)
            {
                SpawnStatusEffect(effectType);
            }
        }

        if (source == null)
        {
            InvokeDamageEvents(new Vector2(), Stats.CurrentHealth.value <= 0, isStatusEffect);
        }
        else
        {
            InvokeDamageEvents(source.position, Stats.CurrentHealth.value <= 0, isStatusEffect);
        }

        // Trigger damage flash on all clients
        TriggerDamageFlashRpc();
    }

    [ServerRpc(requireOwnership: false)]
    public void ApplyStatusEffectServerRpc(StatusEffectType type, Transform source)
    {
        Combat.ApplyStatusEffect(type, source);
        SpawnStatusEffectRpc(type);
    }

    /// <summary>
    /// Public method for calling the spawning of a status effect particle on a unit.
    /// </summary>
    /// <param name="type"> Enum value for status effect type. </param>
    [ServerRpc(requireOwnership: false)]
    public void SpawnStatusEffectRpc(StatusEffectType type)
    {
        SpawnStatusEffect(type);
    }

    /// <summary>
    /// Private method for spawning a status effect particle on a unit.
    /// </summary>
    /// <param name="type"> Enum value for status effect type. </param>
    [ObserversRpc(runLocally: true)]
    private void SpawnStatusEffect(StatusEffectType type)
    {
        // find the prefab matching the type
        GameObject sePrefab = _sePrefabMap.GetPrefab(type);

        if (sePrefab != null)
        {
            // instantiate the prefab
            GameObject effectInstance = Instantiate(sePrefab, transform.position, Quaternion.identity);

            // parent it to this object so it follows this unit
            effectInstance.transform.SetParent(transform);
        }
    }

    [ServerRpc(requireOwnership: false)]
    public void SpawnDamageTextRpc(float value, bool armorConsumed, bool isHealing, bool isStatusEffect, StatusEffectType sourceType)
    {
        SpawnDamageText(value, armorConsumed, isHealing, isStatusEffect, sourceType);
    }

    [ObserversRpc(runLocally: true)]
    private void SpawnDamageText(float value, bool armorConsumed, bool isHealing, bool isStatusEffect, StatusEffectType sourceType)
    {
        if (isStatusEffect)
        {
            GameObject newDamageText = Instantiate(_damageTextPrefab, transform.position, Quaternion.identity);
            Vector3 newPos = newDamageText.transform.position;
            newPos.x += UnityEngine.Random.Range(-1f, 1f);
            newPos.y += UnityEngine.Random.Range(-0.5f, 0.5f);
            newDamageText.transform.position = newPos;
            newDamageText.GetComponent<DamageTextDriver>().InitializeText(value, sourceType);
        }
        else
        {
            GameObject newDamageText = Instantiate(_damageTextPrefab, transform.position, Quaternion.identity);
            Vector3 newPos = newDamageText.transform.position;
            newPos.x += UnityEngine.Random.Range(-1f, 1f);
            newPos.y += UnityEngine.Random.Range(-0.5f, 0.5f);
            newDamageText.transform.position = newPos;
            newDamageText.GetComponent<DamageTextDriver>().InitializeText(value, armorConsumed, isHealing);
        }
    }

    /// <summary>
    /// Call to invoke the actions that trigger on-damage effects.
    /// </summary>
    /// <param name="source"> The origin of the damage dealt. </param>
    /// <param name="isDead"> Whether the unit that was damaged is dead or not. </param>
    /// <param name="isStatusEffect"> Whether the damage dealt was from a status effect or not. </param>
    [ObserversRpc(runLocally:true)]
    private void InvokeDamageEvents(Vector2 source, bool isDead, bool isStatusEffect)
    {
        if (isDead)
        {
            Combat.ClearStatuses();
            OnDeath?.Invoke();
        }
        OnAttacked?.Invoke(source, isStatusEffect);
    }

    /// <summary>
    /// Increase current health by heal amount and updates value in server.
    /// </summary>
    /// <param name="heal"> Heal amount (calculated externally). </param>
    [ServerRpc(requireOwnership: false)]
    public void HealServerRpc(float heal, bool scored = true)
    {
        if (Stats.CurrentHealth.value >= Stats.MaximumHealth.value) return;

        float targetHeal = Stats.CurrentHealth.value + heal;
        Stats.CurrentHealth.value = Mathf.Clamp(Stats.CurrentHealth.value + heal, 0, Stats.MaximumHealth.value);
        float actualHeal = heal - (targetHeal - Stats.CurrentHealth.value);
        if (actualHeal > 5f)
        {
            SpawnDamageText(actualHeal, false, true, false, StatusEffectType.Poison);
        }

        // Trigger heal flash on all clients
        TriggerHealFlashRpc();
    }

    /// <summary>
    /// Prevents the Training Dummy from taking damage.
    /// </summary>
    /// <param name="status"> Setting to true enables invincibility, false disables it. </param>
    [ServerRpc(requireOwnership: false)]
    public void SetInvincibility(bool status)
    {
        _isInvincible = status;
    }

    /// <summary>
    /// Triggers the damage flash effect on all clients.
    /// </summary>
    [ObserversRpc(runLocally: true)]
    private void TriggerDamageFlashRpc()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }
        _flashRoutine = StartCoroutine(FlashHealthBarRoutine(Color.red));
    }

    /// <summary>
    /// Triggers the heal flash effect on all clients.
    /// </summary>
    [ObserversRpc(runLocally: true)]
    private void TriggerHealFlashRpc()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }
        _flashRoutine = StartCoroutine(FlashHealthBarRoutine(_healColor));
    }

    /// <summary>
    /// Coroutine to flash the health bar between a color and the original color.
    /// </summary>
    /// <param name="flashColor"> The color to flash to. </param>
    /// <returns></returns>
    private IEnumerator FlashHealthBarRoutine(Color flashColor)
    {
        _isFlashing = true;
        float elapsed = 0f;

        // Flash to the damage/heal color
        while (elapsed < _flashDuration)
        {
            elapsed += Time.deltaTime;
            _healthBarFill.color = Color.Lerp(flashColor, _originalBarColor, elapsed / _flashDuration);
            yield return null;
        }
        _healthBarFill.color = _originalBarColor;
        _isFlashing = false;

        // Check if we should start pulsing (low health)
        if (Stats.CurrentHealth.value / Stats.MaximumHealth.value <= _lowHPThreshold)
        {
            _isPulsing = true;
        }
    }

    private void UpdateHealthBar(float newHealth)
    {
        _healthBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newHealth * _fullHealthBarWidth / Stats.MaximumHealth.value);

        // Update pulsing state based on current health
        if (newHealth / Stats.MaximumHealth.value <= _lowHPThreshold && !_isFlashing)
        {
            _isPulsing = true;
        }
        else if (newHealth / Stats.MaximumHealth.value > _lowHPThreshold)
        {
            _isPulsing = false;
            if (!_isFlashing)
                _healthBarFill.color = _originalBarColor;
        }
    }
}