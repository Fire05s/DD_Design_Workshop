using UnityEngine;

namespace Status
{
    public class Regeneration : StatusEffect, IDamageStatus
    {

        public Regeneration()
        {
            cooldown = 10;
        }
        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            healthHandler.HealServerRpc(healthHandler.GetComponentInParent<SyncedStats>().MaximumHealth.value * 0.25f/10);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            healthHandler.HealServerRpc(healthHandler.GetComponentInParent<SyncedStats>().MaximumHealth.value * 0.25f/10);
        }
    }
}