using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Giant Slayer")]

    public class GiantSlayer : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            if (opponentStats != null)
            {
                if (opponentStats.GetComponent<TDHealthHandler>() != null)
                {
                    // capping amount of damage Giant Slayer can deal to Training Dummy
                    return Mathf.Clamp(damage * (1 + (0.02f * Mathf.Floor((opponentStats.MaximumHealth.value - stats.MaximumHealth.value) / 500f))), damage, damage * 2);
                }
                return damage * (1 + (0.02f * Mathf.Floor((opponentStats.MaximumHealth.value - stats.MaximumHealth.value) / 500f)));
            }
            return damage;
        }
    }
}
