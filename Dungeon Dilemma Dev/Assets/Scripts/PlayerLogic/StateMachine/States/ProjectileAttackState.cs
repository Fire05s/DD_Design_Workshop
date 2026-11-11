using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the logic and animations for the Projectile Attack State.
/// </summary>
public class ProjectileAttackState : State, ITargettedAttack
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _attackAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _attackAudio;

    [Header("Attack Targets")]
    [SerializeField] private List<string> _targetTags;

    [Header("Sub Attack")]
    [SerializeField] private State _subAttack;
    [SerializeField] private float _subAttackDelay;

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Attack Stats")]
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private float _attackSpeed;

    [Header("Projectile")]
    [SerializeField] private ProjectileData _projectileData;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private GameObject _projectileAltPrefab;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private bool _disableProjectileRotation;
    [SerializeField] private int _projectileCount = 1;
    [SerializeField] private float _projectileAngleDelta;
    [SerializeField] private AOEAttackState _parentAOEAttack;

    private bool _canAttack = true;

    private Vector2 _attackDirection = Vector2.zero; // relative to player
    private Vector3 _baseDirection;
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

        // get direction of attack
        if (_parentAOEAttack != null)
        {
            _baseDirection = _parentAOEAttack.AttackDirection;
        }
        else if (_playerCamera == null) 
        {
            _baseDirection = ((Vector3)_attackDirection).normalized;
        }
        else
        {
            Vector3 _worldMousePos = _playerCamera.ScreenToWorldPoint(Input.mousePosition);
            _worldMousePos.z = transform.position.z;
            _baseDirection = Vector3.Normalize(_worldMousePos - transform.position);
        }

        // Adjust animation speed to match attack speed
        if (_attackAnim != null)
        {
            anim.speed = _attackAnim.length / _attackSpeed;
            anim.Play(_attackAnim.name, 0, 0f);
        }

        if (_parentAOEAttack == null)
        {
            StartCoroutine(ProjectileCooldown());
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
        if (!_hasAttacked && time >= _attackSpeed)
        {
            _hasAttacked = true;
            Attack();

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
        anim.speed = 1f; // reset animation speed
    }

    private void Attack()
    {
        float baseAngle = Mathf.Atan2(_baseDirection.y, _baseDirection.x) * Mathf.Rad2Deg;

        SoundFXManager.Instance?.PlaySoundFXClip(_attackAudio, transform.position);

        var _playerCollider = GetComponent<Collider2D>();

        for (int i = 0; i < _projectileCount; i++)
        {
            int multiplier = (i + 1) / 2; 
            if (i % 2 == 0 && i > 0) multiplier *= -1;

            float currentAngleOffset = multiplier * _projectileAngleDelta;
            float finalAngle = baseAngle + currentAngleOffset;

            Vector3 finalDirection = new Vector3(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad), 0f);

            float spawnRotationZ = Vector3.Angle(Vector3.up, finalDirection) * (finalDirection.x > 0 ? -1 : 1);
            Quaternion spawnRotation = _disableProjectileRotation ? Quaternion.identity : Quaternion.Euler(0, 0, spawnRotationZ);

            GameObject projectileObject;
            if (_projectileAltPrefab == null || (_projectileAltPrefab != null && Random.Range(1, 2) == 1))
            {
                projectileObject = Instantiate(_projectilePrefab, transform.position, spawnRotation);
            }
            else
            {
                projectileObject = Instantiate(_projectileAltPrefab, transform.position, spawnRotation);
            }

            var _projectileCollider = projectileObject.GetComponent<Collider2D>();
            if (_projectileCollider != null && _playerCollider != null)
            {
                Physics2D.IgnoreCollision(_projectileCollider, _playerCollider, true);
            }

            Projectile projectileScript = projectileObject.GetComponent<Projectile>();
            projectileScript.InitProjectile(stats, _projectileData, finalDirection, combat, _targetTags, _damageMultiplier);
            projectileScript.GiveOwnership(owner);
        }
    }

    /// <summary>
    /// Set the direction of the projectile attack for non-player entities. 
    /// </summary>
    /// <param name="direction"> New Direction to attack. </param>
    public void SetAttackDirection(Vector2 direction)
    {
        _attackDirection = direction;
    }

    private IEnumerator ProjectileCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canAttack = true;
    }
}
