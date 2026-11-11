using ControllerLogic;
using UnityEngine;

namespace Status
{
    public class Cripple : StatusEffect, IBuffStatus
    {
        private float currentAttackDebuff;
        private float currentResistanceDebuff;
        private float currentMovementSpeedDebuff;
        public Cripple()
        {
            cooldown = 2;
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            SyncedStats dungeonBossStats = null;
            foreach (SyncedStats syncedStats in GlobalController.Singleton.GetAllStats())
            {
                if (syncedStats.CompareTag("Boss"))
                {
                    dungeonBossStats = syncedStats;
                    break;
                }
            }
            currentAttackDebuff = (dungeonBossStats.AttackPower.value / 8) / 100;
            currentResistanceDebuff = stats.Resistance.value * (dungeonBossStats.AttackPower.value / 8) / 100;
            currentMovementSpeedDebuff = stats.MovementSpeed.value / 2;
            stats.AttackDebuff.value += currentAttackDebuff;
            stats.Resistance.value -= currentResistanceDebuff;
            stats.MovementSpeed.value -= currentMovementSpeedDebuff;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.AttackDebuff.value -= currentAttackDebuff;
            stats.Resistance.value += currentResistanceDebuff;
            stats.MovementSpeed.value += currentMovementSpeedDebuff;
        }
    }
}