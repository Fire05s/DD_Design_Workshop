using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using Priestess;

public class CursedGround : NetworkBehaviour
{
    [Header("Cursed Ground Settings")]
    [SerializeField] private float _damageCooldown = 1f; //intervals between damage ticks (damage/sec is preserved)
    [SerializeField] private float _duration = 3f; //how long this cursed ground lasts
    [SerializeField] private bool _fixedSize;

    // key = enemy, value = the time they can be damaged again
    private Dictionary<GameObject, float> _nextDamageTimeMap = new Dictionary<GameObject, float>();
    private List<GameObject> _enemiesInRange = new List<GameObject>();
    private float _damageMultiplier;
    private List<string> _targets;

    private CombatManager _combat;
    private HealthHandler _ownerHealthHandler;
    private SyncedStats _ownerStats;

    public void InitGround(CombatManager combat, HealthHandler health, List<string> targets, float damageMultiplier)
    {
        _ownerStats = combat.GetComponent<SyncedStats>();
        if (!_fixedSize)
        {
            transform.localScale = transform.localScale * (1 + (_ownerStats.Scale.value - 1) * 0.15f);
        }
        _combat = combat;
        _ownerHealthHandler = health;
        _targets = targets;
        _damageMultiplier = damageMultiplier;

        Destroy(gameObject, _duration);
    }

    private void Update()
    {
        // process damage for every enemy on this cursed ground, applying per defined interval
        if (isServer)
        {
            for (int i = _enemiesInRange.Count - 1; i >= 0; i--)
            {
                GameObject enemy = _enemiesInRange[i];

                // safety check in case object was destroyed
                if (enemy == null) {
                    _enemiesInRange.RemoveAt(i);
                    continue;
                }

                if (!_nextDamageTimeMap.ContainsKey(enemy)) _nextDamageTimeMap[enemy] = 0;

                if (Time.time >= _nextDamageTimeMap[enemy])
                {
                    DoDamage(enemy);
                    _nextDamageTimeMap[enemy] = Time.time + _damageCooldown;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) { return; }
        // add enemies on this to a tracking list
        if (_targets.Contains(other.tag) && !_enemiesInRange.Contains(other.gameObject))
        {
            _enemiesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isServer) { return; }
        // remove enemies leaving this from the tracking list
        if (_targets.Contains(other.tag) || other.CompareTag("Dead"))
        {
            _enemiesInRange.Remove(other.gameObject);
            if (_nextDamageTimeMap.ContainsKey(other.gameObject)) 
                _nextDamageTimeMap.Remove(other.gameObject);
        }
    }

    /// <summary>
    /// Called when an enemy is on this ground.
    /// </summary>
    /// <param name="enemy">The enemy that collided with this trap.</param>
    protected void DoDamage(GameObject enemy)
    {
        HealthHandler health = enemy.GetComponent<HealthHandler>();
        TDHealthHandler tdHealthHandler = enemy.GetComponent<TDHealthHandler>();
        if (health != null)
        {
            (float damage, List<StatusEffectType> effects, float lifeSteal)
                = _combat.RegisterAttackOutgoing(health.GetComponentInParent<SyncedStats>(), _damageMultiplier);
            health.TakeDamageServerRpc(damage, effects, _ownerHealthHandler.transform, true, StatusEffectType.Burn);
            _ownerHealthHandler.HealServerRpc(lifeSteal);
        }
        else if (tdHealthHandler != null)
        {
            (float damage, List<StatusEffectType> effects, float lifeSteal)
                = _combat.RegisterAttackOutgoing(tdHealthHandler.GetComponentInParent<SyncedStats>(), _damageMultiplier);
            tdHealthHandler.TakeDamageServerRpc(damage, effects, _ownerHealthHandler.transform, true, StatusEffectType.Burn);
            _ownerHealthHandler.HealServerRpc(lifeSteal);
        }
    }
}