using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Mandatory Sale")]

    public class MandatorySale : ItemAbility, IConditionalEffect
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }

        public void ApplyConditionalEffect(SyncedStats stats)
        {
            Shop.ShopEntryPoint.localShopInstance.SetPriceDiscount(0.2f);
        }
    }
}
