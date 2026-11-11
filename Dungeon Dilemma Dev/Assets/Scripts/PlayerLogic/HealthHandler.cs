using ControllerLogic;
using Priestess;
using PurrNet;
using Sync_Modules;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the health values, take damage function, and heal function for all objects with health
/// </summary>
public class HealthHandler : NetworkBehaviour, IDamageable
{
    [Header("Entity Stats")]
    public SyncedStats Stats;
    [SerializeField] private DungeonBoss _dungeonBoss;

    [Header("Status Effect VFX")]
    [SerializeField] protected StatusEffectPrefabMap _sePrefabMap;
    [SerializeField] private bool _showDeathParticles;

    [Header("Damage Text VFX")]
    [SerializeField] protected GameObject _damageTextPrefab;

    public event Action OnDeath;
    public event Action<Vector2, bool> OnAttacked; // attack source (to handle knockback)

    private CombatManager _combat;
    private bool _isInvincible = false;

    private float _regenTime = 1f;
    private float _curRegenTime = 0f;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        InitializeStatsReference();
    }

    void FixedUpdate()
    {
        HandleRegen();
    }

    /// <summary>
    /// If a sync stat exists, use it for current and max hp values.
    /// </summary>
    private void InitializeStatsReference()
    {
        StateMachineCore core = GetComponent<StateMachineCore>();
        _combat = GetComponentInParent<CombatManager>();
        if (core != null && core.stats != null)
        {
            Stats = core.stats;
        }
        else if (Stats == null)
        {
            Stats = GetComponentInParent<SyncedStats>();
        }
        if (Stats == null) Debug.LogWarning("Health Handler missing stats reference, it NEEDS it.");
    }

    /// <summary>
    /// Updates the object's max health.
    /// </summary>
    /// <param name="newMax"> The new max health value. </param>
    [ServerRpc(requireOwnership: false)]
    public void UpdateMaxHealth(float newMax)
    {
        if (Stats == null) InitializeStatsReference();
        Stats.MaximumHealth.value = newMax;
        Stats.CurrentHealth.value = newMax;
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

        bool prevBossState = false;
        _combat.RegisterDamageIncoming(damage, effects, source, isStatusEffect, type);
        if (_dungeonBoss != null) {
            prevBossState = GlobalController.Singleton.BossSecondPhaseActive.value;
            GlobalController.Singleton.CheckBossPhase(Stats);
        }

        // Score related info
        /// Blocked Damage Score
        if (CompareTag("Boss") || CompareTag("Challenger"))
        {
            GlobalController.Singleton?.UpdatePlayerScoreData(Stats.owner.Value, ScoreType.BlockedDamage, damage);
        }
        /// Boss Last Hits Score
        if (_dungeonBoss != null && (prevBossState != GlobalController.Singleton.BossSecondPhaseActive.value || Stats.CurrentHealth <= 0))
        {
            GlobalController.Singleton?.UpdatePlayerScoreData(source.GetComponentInParent<SyncedStats>().owner.Value, ScoreType.BossLastHits);
        }

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
    }

    [ServerRpc(requireOwnership: false)]
    public void ApplyStatusEffectServerRpc(StatusEffectType type, Transform source)
    {
        _combat.ApplyStatusEffect(type, source);
        SpawnStatusEffectRpc(type);
    }

    /// <summary>
    /// Public method for calling the spawning of a status effect particle on a unit.
    /// </summary>
    /// <param name="type"> Enum value for status effect type. </param>
    [ServerRpc(requireOwnership: false)]
    public void SpawnStatusEffectRpc(StatusEffectType type, bool allowParenting = true)
    {
        SpawnStatusEffect(type, allowParenting);
    }

    /// <summary>
    /// Private method for spawning a status effect particle on a unit.
    /// </summary>
    /// <param name="type"> Enum value for status effect type. </param>
    [ObserversRpc(runLocally: true)]
    private void SpawnStatusEffect(StatusEffectType type, bool allowParenting = true)
    {
        // find the prefab matching the type
        GameObject sePrefab = _sePrefabMap.GetPrefab(type);

        if (sePrefab != null)
        {
            // instantiate the prefab
            GameObject effectInstance = Instantiate(sePrefab, transform.position, Quaternion.identity);

            // parent it to this object so it follows this unit
            if (allowParenting)
            {
                effectInstance.transform.SetParent(transform);
            }
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

    [ServerRpc(requireOwnership: false)]
    public void SpawnGoldTextRpc(int goldValue)
    {
        SpawnGoldText(goldValue);
    }

    [ObserversRpc(runLocally: true)]
    private void SpawnGoldText(int goldValue)
    {
        GameObject newDamageText = Instantiate(_damageTextPrefab, transform.position, Quaternion.identity);
        Vector3 newPos = newDamageText.transform.position;
        newPos.x += UnityEngine.Random.Range(-1f, 1f);
        newPos.y += UnityEngine.Random.Range(-0.5f, 0.5f);
        newDamageText.transform.position = newPos;
        newDamageText.GetComponent<DamageTextDriver>().InitializeText(goldValue);
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
            if (_showDeathParticles)
            {
                SpawnStatusEffect(StatusEffectType.DeathVFX, false);
            }
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

        if ((CompareTag("Boss") || CompareTag("Challenger")) && scored)
        {
            GlobalController.Singleton?.UpdatePlayerScoreData(Stats.owner.Value, ScoreType.RegeneratedHealth, (int)heal);
        }
    }

    /// <summary>
    /// Handles the regeneration logic
    /// </summary>
    private void HandleRegen()
    {
        // regen is disabled in overtime
        if (_curRegenTime >= _regenTime && !CompareTag("Dead") && Stats.CurrentHealth < Stats.MaximumHealth && GlobalController.Singleton.WorldData.RoundState != RoundState.RaidOvertime)
        {
            if (Stats.CurrentHealth >= Stats.MaximumHealth) return;
            _curRegenTime = 0f;
            HealServerRpc(Stats.PassiveRegen.value);
        }
        else if (!CompareTag("Dead") && isServer)
        {
            _curRegenTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Prevents the player from taking damage.
    /// </summary>
    /// <param name="status"> Setting to true enables invincibility, false disables it. </param>
    [ServerRpc(requireOwnership: false)]
    public void SetInvincibility(bool status)
    {
        _isInvincible = status;
    }
}