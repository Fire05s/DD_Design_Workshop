using UnityEngine;

namespace Status
{
    public class Poison : StatusEffect, IDamageStatus, IBuffStatus
    {
        private bool removedMovementSpeed = true;
        public Poison()
        {
            cooldown = 2;
        }

        public void ApplyStatusEffect(SyncedStats stats)
        {
            if (stats.MovementSpeed.value == 1)
            {
                removedMovementSpeed = false;
                return;
            }
            stats.MovementSpeed.value -= 1;
        }

        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(50, null, source, true, Priestess.StatusEffectType.Poison);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(50, null, source, true, Priestess.StatusEffectType.Poison);
        }

        public void UnapplyStatusEffect(SyncedStats stats)
        {
            if (removedMovementSpeed)
            {
                stats.MovementSpeed.value += 1;
            }
        }
    }
}