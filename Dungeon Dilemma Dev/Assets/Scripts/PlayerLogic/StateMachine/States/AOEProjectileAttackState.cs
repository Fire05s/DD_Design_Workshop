using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEProjectileAttackState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _attackAnim;
    [Header("Audio")]
    [SerializeField] private AudioClip _abilityAudio;

    [Header("Attack Targets")]
    [SerializeField] private List<string> _targetTags;

    [Header("Sub Attack")]
    [SerializeField] private State _subAttack;
    [SerializeField] private float _subAttackDelay;
    [Header("Components")]
    [SerializeField] private MovementController movement; 

    [Header("Attack Type")]
    [SerializeField] private SyncedStats.AttackType _attackType;

    [Header("Attack Stats")]
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private float _attackSpeed;

    [Header("Projectile Stats")]
    [SerializeField] private ProjectileData _projectileData;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private GameObject _projectileAltPrefab;
    [SerializeField] private float _amountPerAttack;
    [SerializeField] private float _rotationOffset;
    [SerializeField] private bool _disableProjectileRotation;
    [SerializeField] private AOEAttackState _parentAOEAttack;

    private float _numCasts = 0;
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

        movement?.SetCanMove(false);
        _canAttack = false;
        _hasAttacked = false;
        _isWaitingForSubAttack = false;

        // Adjust animation speed to match attack speed
        if (_attackAnim != null)
        {
            anim.speed = _attackAnim.length / _attackSpeed;
            anim.Play(_attackAnim.name, 0, 0f);
        }

        SoundFXManager.Instance?.PlaySoundFXClip(_abilityAudio, transform.position);
        ShootProjectileAOE();
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
        movement?.SetCanMove(true);
    }

    private void ShootProjectileAOE()
    {
        float baseAngle = _numCasts * _rotationOffset; //Each cast comes out at a slightly different angle
        float step = 360f / _amountPerAttack;
        for (int i = 0; i < _amountPerAttack; ++i)
        {
            float angleInDegrees = baseAngle + (i * step);
            Vector2 direction = new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));

            SpawnProjectile(angleInDegrees, direction);
        }

        ++_numCasts;
    }

    private void SpawnProjectile(float angleInDegrees, Vector2 direction)
    {
        // float _changingSpriteDirectionDegrees = -90f;
        // GameObject projectileObject = Instantiate(_projectilePrefab, transform.position, Quaternion.Euler(0, 0, angleInDegrees + _changingSpriteDirectionDegrees));

        float spawnRotationZ = Vector3.Angle(Vector3.up, direction) * (direction.x > 0 ? -1 : 1);
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
        var swingCollider = projectileObject.GetComponent<Collider2D>();
        var shooterCollider = GetComponent<Collider2D>();

        if (swingCollider != null && shooterCollider != null)
        {
            Physics2D.IgnoreCollision(swingCollider, shooterCollider, ignore: true);
        }

        projectileObject.GetComponent<Projectile>().InitProjectile(stats, _projectileData, direction, combat, _targetTags, _damageMultiplier);
        projectileObject.GetComponent<Projectile>().GiveOwnership(owner);
    }

    private IEnumerator ProjectileCooldown()
    {
        yield return new WaitForSeconds(stats.GetCooldown(_attackType));
        _canAttack = true;
    }
}
