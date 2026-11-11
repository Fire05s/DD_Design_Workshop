using UnityEngine;
using PurrNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Priestess;

public class Projectile : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb2d;
    [SerializeField] private bool _fixedSize;

    private SyncedStats _ownerStats;
    private CombatManager _combat;
    private HealthHandler _ownerHealthHandler;
    private Vector2 _direction;
    private float _speed;
    private float _damageMultiplier;

    /// <summary>
    /// Time until projectile despawns automatically. Set to -1 to ignore.
    /// </summary>
    private float _despawnTime;
    /// <summary>
    /// Maximum amount of collisions a projectile can have before despawn. Defaults to 1.
    /// </summary>
    private int _maxCollisions;
    private bool _isExplosive;
    private GameObject _explosionPrefab;
    private List<string> _targets = new List<string>();
    private string[] _hittable = {"Border"};

    private bool _isInitialized = false;

    void FixedUpdate()
    {
        if (!_isInitialized) return;
        MoveProjectile();
        if (_maxCollisions <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Moves the projectile in the attack direction.
    /// </summary>
    void MoveProjectile()
    {
        if (_rb2d != null)
        {
            _rb2d.linearVelocity = _direction * _speed;
        }
    }

    /// <summary>
    /// Sets the stat values for the projectile.
    /// </summary>
    /// <param name="data">A scriptable object containing the projectile information</param>
    /// <param name="direction">A vector2 to represent the flight direction of the projectile</param>
    public void InitProjectile(SyncedStats ownerStats, ProjectileData data, Vector2 direction, CombatManager combat, List<string> targets, float damageMultiplier)
    {
        Debug.Log("initialized projectile");
        _ownerStats = ownerStats;
        if (!_fixedSize)
        {
            transform.localScale = transform.localScale * (data.Size + (_ownerStats.Scale.value - 1) * 0.15f);
        }
        _direction = direction;
        _maxCollisions = data.ProjectileUses;
        _speed = data.FlightSpeed;
        _isExplosive = data.IsExplosive;
        _explosionPrefab = data.ExplosionPrefab;
        _targets = targets;
        gameObject.name = data.ProjectileType;
        _combat = combat;
        _damageMultiplier = damageMultiplier;
        _ownerHealthHandler = ownerStats.GetComponentInChildren<HealthHandler>();
        if (data.Despawns == true)
        {
            _despawnTime = data.DespawnTime;
            StartCoroutine(Despawner());
        }
        _isInitialized = true;
    }
    /// <summary>
    /// Coroutine handling projectile despawn timer.
    /// </summary>
    /// <returns> Despawn timer. </returns>
    private IEnumerator Despawner()
    {
        yield return new WaitForSeconds(_despawnTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targets.Count == 0) { return; }

        if (_targets.Contains(collision.tag) && collision.gameObject.GetComponent<IDamageable>() != null)
        {
            (float damage, List<StatusEffectType> effects, float lifeSteal)
                = _combat.RegisterAttackOutgoing(collision.GetComponentInParent<SyncedStats>(), _damageMultiplier);
            collision.gameObject.GetComponent<IDamageable>().TakeDamageServerRpc(damage, effects, _ownerHealthHandler.transform);
            _ownerHealthHandler.HealServerRpc(lifeSteal);

            if (_isExplosive)
            {
                GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
                explosion.GetComponent<Explosion>().InitExplosion(_combat, _ownerHealthHandler, _targets, _damageMultiplier, new Collider2D[1]{ collision });
            }

            Destroy(gameObject);
        }
        else if (_hittable.Contains(collision.tag))
        {
            if (_isExplosive)
            {
                GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
                explosion.GetComponent<Explosion>().InitExplosion(_combat, _ownerHealthHandler, _targets, _damageMultiplier, new Collider2D[0]);
            }

            Destroy(gameObject);
        }
    }
}
