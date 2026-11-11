using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Pocket Puncher")]
    public class PocketPuncher : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats=null)
        {
            return damage * (1 + (0.02f * Mathf.Floor(stats.Gold.value / 200)));
        }
    }
}