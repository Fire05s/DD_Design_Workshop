using System.Collections.Generic;
using UnityEngine;
using PurrNet;
using Priestess;
using Unity.VisualScripting;
using System.Linq;

public class Explosion : NetworkBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionTime;
    [SerializeField] private AudioClip _explosionSFX;
    [SerializeField] private bool _fixedSize;
    private float _damageMultiplier;
    private List<string> _targets;

    private CombatManager _combat;
    private HealthHandler _ownerHealthHandler;
    private SyncedStats _ownerStats;
    private Collider2D[] _alreadyHitTargets;

    public void InitExplosion(CombatManager combat, HealthHandler health, List<string> targets, float damageMultiplier, Collider2D[] alreadyHitTargets)
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
        _alreadyHitTargets = alreadyHitTargets;
        if (isServer) 
        {
            Explode();
        }

        Destroy(gameObject, _explosionTime);
    }

    private void Explode()
    {
        Collider2D[] _attackedTargets = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        SoundFXManager.Instance.PlaySoundFXClip(_explosionSFX, transform.position);

        foreach (Collider2D targetCollider in _attackedTargets)
        {
            GameObject attackedTarget = targetCollider.gameObject;
            if (!_alreadyHitTargets.Contains(targetCollider) && _targets.Contains(attackedTarget.tag))
            {
                (float damage, List<StatusEffectType> effects, float lifeSteal)
                    = _combat.RegisterAttackOutgoing(attackedTarget.GetComponentInParent<SyncedStats>(), _damageMultiplier);
                attackedTarget.GetComponent<IDamageable>().TakeDamageServerRpc(damage, effects, _ownerHealthHandler.transform);
                _ownerHealthHandler.HealServerRpc(lifeSteal);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}