using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Minion General")]
    public class MinionGeneral : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            Shop.ShopEntryPoint.localShopInstance.ModifyWeights(0, 1, 0);
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}