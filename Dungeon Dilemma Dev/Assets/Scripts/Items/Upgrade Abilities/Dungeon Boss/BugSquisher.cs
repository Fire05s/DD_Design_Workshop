using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Bug Squisher")]

    public class BugSquisher : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            if (opponentStats != null && opponentStats.CurrentHealth.value < 1000)
            {
                return damage * 1.15f;
            }
            return damage;
        }
    }
}
