using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Harvester")]
    public class Harvester : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            if (opponentStats != null)
            {
                if (opponentStats.GetComponent<TDHealthHandler>() != null)
                {
                    // capping amount of damage Harvester can deal to Training Dummy
                    return Mathf.Clamp(damage + (0.05f * opponentStats.MaximumHealth), damage, damage * 2);
                }
                return damage + (0.05f * opponentStats.MaximumHealth);
            }
            return damage;
        }
    }
}