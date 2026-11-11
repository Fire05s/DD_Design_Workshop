using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Giants Strength")]

    public class GiantsStrength : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            if (opponentStats != null)
            {
                return damage * (1 + (0.025f * Mathf.Floor(stats.MaximumHealth.value / 250f)));
            }
            return damage;
        }
    }
}
