using ControllerLogic;
using UnityEngine;

namespace Status
{
    public class Withering : StatusEffect, IDamageStatus
    {
        public Withering()
        {
            cooldown = 1;
        }
        public void DealDamage(HealthHandler healthHandler, Transform source)
        {
            // this deals damage based on overtime game time
            // at timer = 0 seconds, this deals 1% Max HP damage
            // each additional second deals (4.5/30)% more Max HP damage
            healthHandler.TakeDamageServerRpc(healthHandler.Stats.MaximumHealth.value * ((1f + Mathf.FloorToInt(GlobalController.Singleton.GetGlobalTime()) * (4.5f/30f)) / 100), null, source, true, Priestess.StatusEffectType.Withering);
        }
        public void DealDamage(TDHealthHandler healthHandler, Transform source)
        {
            // this deals damage based on overtime game time
            // at timer = 0 seconds, this deals 1% Max HP damage
            // each additional second deals (4.5/30)% more Max HP damage
            healthHandler.TakeDamageServerRpc(healthHandler.Stats.MaximumHealth.value * ((1f + Mathf.FloorToInt(GlobalController.Singleton.GetGlobalTime()) * (4.5f/30f)) / 100), null, source, true, Priestess.StatusEffectType.Withering);
        }
    }
}