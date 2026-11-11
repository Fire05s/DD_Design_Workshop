using System;
using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using Priestess;
using PurrNet;
using Sync_Modules;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossGaze : NetworkBehaviour
{
    [SerializeField] private Transform _raidPhaseFollowObject;
    [SerializeField] private Transform _bossPhaseFollowObject;
    [SerializeField] private Light2D _lightObject;
    [SerializeField] private Light2D _spriteLightObject;
    [SerializeField] private HealthHandler _health;

    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Hitbox/Warning")]
    [SerializeField] private GameObject _warningBoxPrefab;
    [SerializeField] private float _hitboxWidth;
    [SerializeField] private float _hitboxHeight;
    [SerializeField] private GameObject _attackVFXPrefab;

    [Header("Attack Targets")]
    [SerializeField] private List<string> _targetTags;

    [Header("Gaze Attack")]
    [SerializeField] public float AttackCooldown;
    [SerializeField] private float _damageMultiplier;
    [SerializeField] private float _attackWarningTime;
    [SerializeField] private float _attackDelayTime;

    public float RemainingCooldown;
    private SyncVar<float> lightIntensity = new(ownerAuth: true);
    private SyncVar<float> spriteLightIntensity = new(ownerAuth: true);
    private SyncVar<Color> lightColor = new(ownerAuth: true);
    private SyncedStats _stats;
    private CombatManager _combat;
    private bool _canAttack = true;
    private Vector3 _cachedPosition;

    private void Start()
    {
        lightIntensity.onChanged += OnLightIntensityChanged;
        spriteLightIntensity.onChanged += OnSpriteLightIntensityChanged;
        lightColor.onChanged += OnLightColorChanged;
        _stats = GetComponentInParent<SyncedStats>();
        _combat = GetComponentInParent<CombatManager>();
    }

    private void Update()
    {
        if (!isOwner) return;

        RoundState currentState = GlobalController.Singleton.WorldData.RoundState;

        if (currentState == RoundState.Setup)
        {
            lightIntensity.value = 0f;
            spriteLightIntensity.value = 0f;
        }
        else if (currentState == RoundState.Raid || currentState == RoundState.RaidOvertime)
        {
            lightIntensity.value = 6f;
            spriteLightIntensity.value = 6f;
            lightColor.value = Color.red;

            if (_raidPhaseFollowObject != null)
            {
                transform.position = new Vector3(_raidPhaseFollowObject.position.x, _raidPhaseFollowObject.position.y, transform.position.z);
            }
        }
        else
        {
            lightIntensity.value = 1f;
            spriteLightIntensity.value = 0f;
            lightColor.value = Color.white;

            if (_bossPhaseFollowObject != null)
            {
                transform.position = new Vector3(_bossPhaseFollowObject.position.x, _bossPhaseFollowObject.position.y, transform.position.z);
            }
        }

        if (RemainingCooldown > 0f)
        {
            RemainingCooldown -= Time.deltaTime;
        }
        else if (!_canAttack)
        {
            _canAttack = true;
        }

        if (currentState == RoundState.Raid && _canAttack && Input.GetMouseButtonDown(0))
        {
            _canAttack = false;
            _cachedPosition = transform.position;
            StartCoroutine(AttackDelay());
            RemainingCooldown = AttackCooldown;
        }
    }

    /// <summary>
    /// Sets up and spawns warning indicator for aoe attack.
    /// </summary>
    private void DisplayVFXIndicator(GameObject vfxPrefab, Vector2 attackPosition)
    {
        Vector3 scale = new Vector3(_hitboxWidth, _hitboxHeight, 1f);

        // Scale the warning to match the physics hitbox
        SpriteRenderer sr = vfxPrefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            scale = new Vector3(_hitboxWidth / spriteSize.x, _hitboxHeight / spriteSize.y, 1f);
        }

        // Spawn warning hitbox
        GameObject warning = Instantiate(vfxPrefab, attackPosition, Quaternion.identity);
        warning.transform.localScale = scale;
        Destroy(warning, _attackWarningTime);
    }

    private void Attack()
    {

        Collider2D[] _attackedTargets = Physics2D.OverlapBoxAll(_cachedPosition, new Vector2(_hitboxWidth + (_stats.Scale.value - 1) * 0.1f, _hitboxHeight + (_stats.Scale.value - 1) * 0.1f), 0f);

        // checks all colliders in loop: if they are attached to an enemy then deal damage
        foreach (Collider2D targetCollider in _attackedTargets)
        {
            GameObject attackedTarget = targetCollider.gameObject;
            if (_targetTags.Contains(attackedTarget.tag))
            {
                (float damage, List<StatusEffectType> effects, float lifeSteal)
                    = _combat.RegisterAttackOutgoing(attackedTarget.GetComponentInParent<SyncedStats>(), _damageMultiplier);
                attackedTarget.GetComponent<IDamageable>().TakeDamageServerRpc(damage, new List<StatusEffectType>(){StatusEffectType.Watched}, transform);
                _health.HealServerRpc(lifeSteal);
            }
        }
    }

    private IEnumerator AttackDelay()
    {
        DisplayVFXIndicator(_warningBoxPrefab, _cachedPosition); // warning indicator
        yield return new WaitForSeconds(_attackWarningTime);
        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position); 
        DisplayVFXIndicator(_attackVFXPrefab, _cachedPosition); // damage indicator
        yield return new WaitForSeconds(_attackDelayTime);
        Attack(); // actual damage attack
    }

    private void OnLightIntensityChanged(float newValue)
    {
        if (_lightObject != null) _lightObject.intensity = newValue;
    }

    private void OnSpriteLightIntensityChanged(float newValue)
    {
        if (_spriteLightObject != null) _spriteLightObject.intensity = newValue;
    }

    private void OnLightColorChanged(Color newValue)
    {
        if (_lightObject != null) _lightObject.color = newValue;
    }
}