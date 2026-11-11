using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Heroic Helmet")]
    public class HeroicHelmet : ItemAbility, IRepeatedEffect, ITimedAbility
    {
        public AbilityStatDeltaState ApplyRepeatedEffect(SyncedStats stats, AbilityStatDeltaState state)
        {
            if (!stats.GetComponentInChildren<Challenger>().CompareTag("Dead"))
            {
                stats.AddArmor(50, 300);
            }
            return state;
        }
        public AbilityTimedState InstantiateAbilityTime()
        {
            AbilityTimedState newInstance = new AbilityTimedState(10);
            return newInstance;
        }
        public AbilityStatDeltaState InstantiateStatState()
        {
            return new AbilityStatDeltaState();
        }
    }
}