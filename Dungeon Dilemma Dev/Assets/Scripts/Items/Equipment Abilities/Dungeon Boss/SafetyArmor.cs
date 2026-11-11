using ControllerLogic;
using Sync_Modules;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Safety Armor")]
    public class SafetyArmor : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return GlobalController.Singleton.WorldData.RoundState == RoundState.Raid;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            stats.AddArmor(0.20f * stats.MaximumHealth.value);
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}