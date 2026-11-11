using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Penny Puncher")]
    public class PennyPuncher : ItemAbility, IOnHitEffect
    {
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats=null)
        {
            return damage * (1 + (0.02f * Mathf.Floor(stats.Gold.value / 100)));
        }
    }
}