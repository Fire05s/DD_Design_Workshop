using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Soul Armor")]
    public class SoulArmor : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return (stats.CurrentHealth.value / stats.MaximumHealth.value) <= 0.25;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            stats.AddArmor(0.25f * stats.MaximumHealth.value);
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}