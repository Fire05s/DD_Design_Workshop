using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Equipment Expert")]
    public class EquipmentExpert : ItemAbility, IConditionalEffect, ILimitedAbility
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