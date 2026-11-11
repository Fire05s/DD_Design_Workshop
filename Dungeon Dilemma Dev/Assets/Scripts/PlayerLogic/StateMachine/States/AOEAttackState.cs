using Priestess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the AOE Attack State.
/// </summary>
public class AOEAttackState : State, ITargettedAttack
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _attackAnim;
    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Components")]

    [Header("Attack Targets")]
    [SerializeField] private List<string> _targetTags;

    [Header("Components")]
    [SerializeField] private MovementController _movement;
    [SerializeField] private SyncedInput _input;

    [Header("Hitbox/Warning")]
    [SerializeField] private GameObject _warningBoxPrefab;
    [SerializeField] private float _hitboxWidth;
    [SerializeField] private float _hitboxHeight;

    [Header("Sub Attack")]
    [SerializeField] private State _subAttack;
    [SerializeField] private float _subAttackDelay;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Attack Stats")]
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackWarningTime;
    [SerializeField] private float _attackDelayTime;
    [SerializeField] private bool _disableProjectileRotation;
    [SerializeField] private AOEAttackState _parentAOEAttack;

    [Header("Visuals")]
    [SerializeField] private PlayerVisuals _playerVisuals;

    public Vector2 AttackDirection; // relative to player
    public Vector2 AttackPosition; // world position of attack
    private float _attackAngle;
    private bool _canAttack = true;
    private bool _hasAttacked = false;
    private bool _isWaitingForSubAttack = false;
    private bool _waitingForDelay = false;
    private float _delayTimer = 0f;

    public override void Enter()
    {
        if (!_canAttack)
        {
            isComplete = true;
            return;
        }

        _canAttack = false;
        _hasAttacked = false;
        _isWaitingForSubAttack = false;

        _movement?.SetCanMove(false);
        MouseInputCheck();

        // Adjust animation speed to match attack speed
        if (_attackAnim != null)
        {
            anim.speed = _attackAnim.length / _attackSpeed;
            anim.Play(_attackAnim.name, 0, 0f);
        }

        DisplayWarningIndicator();

        if (_parentAOEAttack == null)
        {
            StartCoroutine(AttackCooldown());
        }
        else
        {
            // this "attack" has no cooldown if it's a visual attack for a parent AOE attack
            _canAttack = true;
        }
    }

    public override void FixedDo()
    {
        // Wait for sub-attack completion
        if (_isWaitingForSubAttack)
        {
            if (machine.state == null || machine.state.isComplete)
            {
                isComplete = true;
                _isWaitingForSubAttack = false;
            }
            return;
        }

        // Handles sub attack delay
        if (_waitingForDelay)
        {
            _delayTimer += Time.fixedDeltaTime;

            if (_delayTimer >= _subAttackDelay)
            {
                _waitingForDelay = false;
                _isWaitingForSubAttack = true;
                Set(_subAttack, true);
            }
            return;
        }

        // Handles main attack
        if (!_hasAttacked && time >= _attackWarningTime)
        {
            _hasAttacked = true;
            StartCoroutine(AttackDelay());

            if (_subAttack != null)
            {
                _waitingForDelay = true;
                _delayTimer = 0f;
            }
            else
            {
                isComplete = true;
            }
        }
    }

    public override void Exit()
    {
        anim.speed = 1f;
        _movement?.SetCanMove(true);
    }

    /// <summary>
    /// Changes the attack direction of the aoe attack. (Mainly for non-player entities)
    /// </summary>
    /// <param name="direction"> New direction to attack. </param>
    public void SetAttackDirection(Vector2 direction)
    {
        AttackDirection = direction;
    }

    /// <summary>
    /// Sets attack direction to mouse direction if it exists.
    /// </summary>
    private void MouseInputCheck()
    {
        if (_input != null)
        {
            AttackDirection = _input.GetMouseDirection(core.transform.position);
        }
    }

    /// <summary>
    /// Sets up and spawns warning indicator for aoe attack.
    /// </summary>
    private void DisplayWarningIndicator()
    {
        Vector2 mouseDir = AttackDirection.normalized;
        if (_parentAOEAttack == null)
        {
            AttackPosition = new Vector2(transform.position.x, transform.position.y) + mouseDir * (_hitboxHeight * 0.5f);
        }
        else
        {
            AttackPosition = _parentAOEAttack.AttackPosition;
        }
        Quaternion rotation = Quaternion.identity;
        if (!_disableProjectileRotation)
        {
            float _attackAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg + 90f; //Used to calculate angle of the attack (moving the attack box)
            rotation = Quaternion.Euler(0, 0, _attackAngle);
        }

        Vector3 scale = new Vector3(_hitboxWidth, _hitboxHeight, 1f);

        // Scale the warning to match the physics hitbox
        SpriteRenderer sr = _warningBoxPrefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            scale = new Vector3(_hitboxWidth / spriteSize.x, _hitboxHeight / spriteSize.y, 1f);
        }

        // Spawn warning hitbox
        GameObject warning = Instantiate(_warningBoxPrefab, AttackPosition, rotation);
        warning.transform.localScale = scale;
        Destroy(warning, _attackWarningTime);
    }

    /// <summary>
    /// Attacks everything in a box radius toward the attack position.
    /// </summary>
    private void Attack()
    {
        Collider2D[] _attackedTargets = Physics2D.OverlapBoxAll(AttackPosition, new Vector2(_hitboxWidth + (stats.Scale.value - 1) * 0.1f, _hitboxHeight + (stats.Scale.value - 1) * 0.1f), _attackAngle);
        bool hasHitTarget = false;

        // checks all colliders in loop: if they are attached to an enemy then deal damage
        foreach (Collider2D targetCollider in _attackedTargets)
        {
            GameObject attackedTarget = targetCollider.gameObject;
            if (_targetTags.Contains(attackedTarget.tag))
            {
                (float damage, List<StatusEffectType> effects, float lifeSteal)
                    = combat.RegisterAttackOutgoing(attackedTarget.GetComponentInParent<SyncedStats>(), _damageMultiplier);
                attackedTarget.GetComponent<IDamageable>().TakeDamageServerRpc(damage, effects, transform);
                health.HealServerRpc(lifeSteal);
                hasHitTarget = true;
            }
        }

        // camera shake the attacker (if they are a player) if they hit something
        if (hasHitTarget && _playerVisuals)
        {
            _playerVisuals.CameraShake();
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canAttack = true;
    }

    private IEnumerator AttackDelay()
    {
        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position);
        yield return new WaitForSeconds(_attackDelayTime);

        Attack();
    }
}
