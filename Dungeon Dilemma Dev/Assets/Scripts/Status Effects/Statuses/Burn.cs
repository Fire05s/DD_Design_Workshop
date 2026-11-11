using UnityEngine;

namespace Status
{
    public class Burn : StatusEffect, IDamageStatus
    {
        public Burn()
        {
            cooldown = 5;
        }
        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(healthHandler.Stats.MaximumHealth.value * 0.025f, null, source, true, Priestess.StatusEffectType.Burn);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(healthHandler.Stats.MaximumHealth.value * 0.025f, null, source, true, Priestess.StatusEffectType.Burn);
        }
    }
}