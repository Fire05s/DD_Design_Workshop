using ControllerLogic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Upgrade Ability/Busy Dungeon")]

    public class BusyDungeon : ItemAbility, IConditionalEffect
    {
        public bool CheckForCondition(SyncedStats stats)
        {
            return true;
        }
        public void ApplyConditionalEffect(SyncedStats stats)
        {
            GlobalController.Singleton.SetRaidDuration(60f);
        }
    }
}
