using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/On a Reroll")]
    public class OnAReroll : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            Shop.ShopEntryPoint.localShopInstance.ModifyRerollCost(25);
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}