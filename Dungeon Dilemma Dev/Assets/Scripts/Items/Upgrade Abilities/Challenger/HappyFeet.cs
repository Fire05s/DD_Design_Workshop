using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Happy Feet")]

    public class HappyFeet : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            return damage * (1 + (0.05f * stats.MovementSpeed.value));
        }
    }
}
