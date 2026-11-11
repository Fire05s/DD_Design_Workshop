using UnityEngine;

namespace Status
{
    public class Bleed : StatusEffect, IDamageStatus
    {

        public Bleed()
        {
            cooldown = 5;
        }
        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(40, null, source, true, Priestess.StatusEffectType.Bleed);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            healthHandler.TakeDamageServerRpc(40, null, source, true, Priestess.StatusEffectType.Bleed);
        }
    }
}