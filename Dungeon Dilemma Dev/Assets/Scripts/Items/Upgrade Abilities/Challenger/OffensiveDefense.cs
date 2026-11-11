using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Offensive Defense")]

    public class OffensiveDefense : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            return damage * (1 + (0.02f * Mathf.Floor(stats.Resistance.value / 10f)));
        }
    }
}
