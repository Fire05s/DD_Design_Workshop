using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Giants Vest")]

    public class GiantsVest : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            return damage * (1 + (0.025f * Mathf.Floor(stats.MaximumHealth.value / 500)));
        }
    }
}
