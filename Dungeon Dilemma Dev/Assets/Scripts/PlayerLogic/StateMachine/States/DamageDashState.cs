using System.Collections.Generic;
using Priestess;
using PurrNet;
using UnityEngine;

public class DamageDashState : DashState
{
    [Header("Damaging Dash")]
    [SerializeField] private float _damageMultiplier;
    [SerializeField] private List<string> _enemyTags;
    [SerializeField] private Collider2D _damageCollider;
    private List<Collider2D> _damagedEnemies = new List<Collider2D>();

    public override void Enter()
    {
        base.Enter();
        _damagedEnemies.Clear();
    }

    protected override void ProcessDashEffect()
    {
        ApplyDashDamage();
    }

    /// <summary>
    /// Damages all enemies hit by the dash
    /// </summary>
    private void ApplyDashDamage()
    {
        if (stats == null) return;

        List<Collider2D> hits = new List<Collider2D>();
        _damageCollider.Overlap(new ContactFilter2D().NoFilter(), hits);

        foreach (Collider2D col in hits)
        {
            if (col.gameObject == core.gameObject || _damagedEnemies.Contains(col)) continue;
            if (_enemyTags is null || !_enemyTags.Contains(col.gameObject.tag)) continue;

            _damagedEnemies.Add(col);
            HealthHandler otherHealth = col.GetComponentInParent<HealthHandler>();
            TDHealthHandler tdHealthHandler = col.GetComponent<TDHealthHandler>();
            if (otherHealth == null)
            {
                otherHealth = col.GetComponent<HealthHandler>();
            }
            if (otherHealth != null)
            {
                (float damage, List<StatusEffectType> effects, float lifeSteal)
                    = combat.RegisterAttackOutgoing(col.GetComponentInParent<SyncedStats>(), _damageMultiplier);
                otherHealth.TakeDamageServerRpc(damage, effects, transform);
                health.HealServerRpc(lifeSteal);
            }
            else if (tdHealthHandler != null)
            {
                (float damage, List<StatusEffectType> effects, float lifeSteal)
                    = combat.RegisterAttackOutgoing(col.GetComponentInParent<SyncedStats>(), _damageMultiplier);
                tdHealthHandler.TakeDamageServerRpc(damage, effects, transform);
                health.HealServerRpc(lifeSteal);
            }
        }
    }
}
