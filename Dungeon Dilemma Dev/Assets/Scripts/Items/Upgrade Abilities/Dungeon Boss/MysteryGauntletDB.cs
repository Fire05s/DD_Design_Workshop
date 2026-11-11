using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Mystery Gauntlet DB")]

    public class MysteryGauntletDB : ItemAbility, IConditionalEffect, ILimitedAbility
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            int randomNum = Random.Range(0, 6);
            switch (randomNum)
            {
                case 0:
                    stats.AddMaxHP(1000f);
                    break;
                case 1:
                    stats.AddRegen(5f);
                    break;
                case 2:
                    stats.AddSpeed(1f);
                    break;
                case 3:
                    stats.AddPower(15f);
                    break;
                case 4:
                    stats.AddResist(15f);
                    break;
                case 5:
                    stats.AddScale(1f);
                    break;
            }
        }
        public AbilityUsageState InstantiateAbilityUsage()
        {
            return new AbilityUsageState(1);
        }
    }
}
