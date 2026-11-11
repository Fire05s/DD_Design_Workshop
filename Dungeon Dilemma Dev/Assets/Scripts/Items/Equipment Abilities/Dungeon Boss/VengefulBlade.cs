using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Vengeful Blade")]

    public class VengefulBlade : ItemAbility, IOnHitEffect {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            float missingHP = stats.MaximumHealth.value - stats.CurrentHealth.value;
            if (missingHP > 0)
            {
                return damage * (1 + (0.05f * Mathf.Floor(missingHP / 500)));
            }
            return damage;
        }
    }
}
