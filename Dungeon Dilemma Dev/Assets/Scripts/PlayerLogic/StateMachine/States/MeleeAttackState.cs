using Priestess;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the Melee Attack State.
/// </summary>
public class MeleeAttackState : State, ITargettedAttack
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _attackAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _attackAudio;

    [Header("Attack Targets")]
    [SerializeField] private List<string> _targetTags;

    [Header("Sub Attack")]
    [SerializeField] private State _subAttack;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Attack Stats")]
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackDelay;

    [Header("Attack Indicator")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private GameObject _attackIndicatorPrefab;
    private Vector2 _attackDirection = Vector2.zero; // relative to player
    private float _attackAngle;

    [Header("Visuals")]
    [SerializeField] private PlayerVisuals _playerVisuals;

    private bool _canAttack = true;
    public override void Enter()
    {
        if (!_canAttack)
        {
            isComplete = true;
            return;
        }

        _canAttack = false;
        // Adjust animation speed to match attack speed
        anim.speed = _attackAnim.length / _attackSpeed;
        anim.Play(_attackAnim.name, 0, 0f);
        StartCoroutine(AttackDelay());
    }

    public override void FixedDo()
    {
        HandleSubAttack();

        if (time >= _attackSpeed)
        {
            if (_subAttack != null) Set(_subAttack, true);
            else isComplete = true;
        }
    }

    public override void Exit()
    {
        anim.speed = 1f; // reset animation speed
    }

    /// <summary>
    /// Wait for any sub attack states to complete before finishing the current state.
    /// </summary>
    private void HandleSubAttack()
    {
        if (machine.state != null)
        {
            // Currently handling sub attack
            if (machine.state.isComplete)
            {
                isComplete = true;
            }
            return;
        }
    }

    private Quaternion GetAttackRotation()
    {
        Vector3 targetPos;

        if (Camera.main != null) 
        {
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else 
        {
            targetPos = transform.position + transform.right; 
        }

        Vector3 direction = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Attacks in a radius around the entity.
    /// </summary>
    private void Attack()
    {
        Vector3 _attackOrigin = core.gameObject.transform.position;
        List<Collider2D> _attackTargets = new List<Collider2D>();
        int _attackRange = Physics2D.OverlapCircle(_attackOrigin, stats.AttackRadius.value + (stats.Scale.value - 1) * 0.1f, new ContactFilter2D().NoFilter(), _attackTargets);
        bool hasHitTarget = false;

        SoundFXManager.Instance?.PlaySoundFXClip(_attackAudio, transform.position);

        // spawning attack indicator effect prefab
        float currentRange = stats.AttackRadius.value + (stats.Scale.value - 1) * 0.1f;

        Vector3 _projectileDirection;
        if (_playerCamera == null) 
        {
            _projectileDirection = ((Vector3)_attackDirection).normalized;
        }
        else
        {
            Vector3 _worldMousePos = _playerCamera.ScreenToWorldPoint(Input.mousePosition);
            _worldMousePos.z = _attackOrigin.z;
            _projectileDirection = (_worldMousePos - _attackOrigin).normalized;
        }

        float angle = Mathf.Atan2(_projectileDirection.y, _projectileDirection.x) * Mathf.Rad2Deg;

        Vector3 spawnPos = _attackOrigin + (_projectileDirection * (currentRange * 0.5f));
        spawnPos.z -= 0.1f;

        GameObject attackIndicator = Instantiate(_attackIndicatorPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        attackIndicator.transform.localScale = new Vector3(currentRange, currentRange * 0.5f, 1);
        Destroy(attackIndicator, 0.1f);

        // checks all colliders in loop: if they are attached to an enemy then deal damage
        for (int i = 0; i < _attackRange; i++)
        {
            GameObject attackedTarget = _attackTargets[i].gameObject;
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

    private IEnumerator MeleeCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canAttack = true;
    }

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(_attackDelay);

        Attack();
        StartCoroutine(MeleeCooldown());
    }

    /// <summary>
    /// Changes the attack direction of the aoe attack. (Mainly for non-player entities)
    /// </summary>
    /// <param name="direction"> New direction to attack. </param>
    public void SetAttackDirection(Vector2 direction)
    {
        _attackDirection = direction;
    }
}