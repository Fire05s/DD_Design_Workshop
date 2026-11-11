using UnityEngine;

namespace Status
{
    public class TDRegeneration : StatusEffect, IDamageStatus
    {

        public TDRegeneration()
        {
            cooldown = 5;
        }
        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            healthHandler.HealServerRpc(healthHandler.GetComponentInParent<SyncedStats>().MaximumHealth.value / 5);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            healthHandler.HealServerRpc(healthHandler.GetComponentInParent<SyncedStats>().MaximumHealth.value / 5);
        }
    }
}