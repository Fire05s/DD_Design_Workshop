using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Packed Lunch")]
    public class PackedLunch : ItemAbility, IConditionalEffect, ITimedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return (stats.CurrentHealth.value / stats.MaximumHealth.value) <= 0.25;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            stats.GetComponentInChildren<HealthHandler>().ApplyStatusEffectServerRpc(Priestess.StatusEffectType.Regeneration, stats.transform);
        }
        public AbilityTimedState InstantiateAbilityTime()
        {
            return new AbilityTimedState(60);
        }
    }
}