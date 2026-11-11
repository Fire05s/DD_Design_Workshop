
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Third Time's the Charm")]
    public class ThirdTimesTheCharm : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return stats.CurrentHealth.value < stats.MaximumHealth.value * 0.25f;
        }

        public void ApplyConditionalEffect(SyncedStats stats)
        {
            stats.GetComponentInChildren<HealthHandler>().ApplyStatusEffectServerRpc(Priestess.StatusEffectType.Enraged, null);
        }

        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}
