using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Vampiric Hunger")]
    public class VampiricHunger : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats=null)
        {
            float lifestealAmount = damage * stats.Lifesteal.value / 100;
            float lifestealOverheal = lifestealAmount - (stats.MaximumHealth.value - stats.CurrentHealth.value);
            if (lifestealOverheal > 0)
            {
                stats.AddArmor(lifestealOverheal, stats.MaximumHealth.value * 0.25f);
            }
            return damage;
        }
    }
}