using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Trap Master")]
    public class TrapMaster : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            Shop.ShopEntryPoint.localShopInstance.ModifyWeights(1, 0, 0);
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}