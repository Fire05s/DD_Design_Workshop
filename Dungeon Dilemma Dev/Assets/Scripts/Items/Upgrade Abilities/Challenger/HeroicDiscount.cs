using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Heroic Discount")]

    public class HeroicDiscount : ItemAbility, IConditionalEffect
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }

        public void ApplyConditionalEffect(SyncedStats stats)
        {
            Shop.ShopEntryPoint.localShopInstance.SetPriceDiscount(0.1f);
        }
    }
}
