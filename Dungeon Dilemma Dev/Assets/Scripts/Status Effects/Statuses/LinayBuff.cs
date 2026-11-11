using UnityEngine;

namespace Status
{
    public class LinayBuff : StatusEffect, IBuffStatus
    {
        private float currentPrimaryCDBuff;
        private float currentDashCDBuff;
        private float currentMovementSpeedBuff;
        public LinayBuff()
        {
            cooldown = 10;
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            float _cooldownBuffAmountPercentage = stats.AttackPower * 0.33f / 100;
            float _movementBuffAmountPercentage = stats.AttackPower * 0.15f / 100;
            currentPrimaryCDBuff = stats.PrimaryAbilityCooldown.value * _cooldownBuffAmountPercentage;
            currentDashCDBuff = stats.DashCooldown.value * _cooldownBuffAmountPercentage;
            currentMovementSpeedBuff = stats.MovementSpeed.value * _movementBuffAmountPercentage;
            // stats.MovementSpeed.value += currentMovementSpeedBuff;
            // stats.PrimaryAbilityCooldown.value -= currentPrimaryCDBuff;
            // stats.DashCooldown.value -= currentDashCDBuff;

            stats.MovementSpeed.value = stats.MovementSpeed.value + currentMovementSpeedBuff;
            float targetPrimaryAbilityCooldownReduction = stats.PrimaryAbilityCooldown.value - currentPrimaryCDBuff;
            stats.PrimaryAbilityCooldown.value = Mathf.Clamp(stats.PrimaryAbilityCooldown.value - currentPrimaryCDBuff, 0.1f, stats.PrimaryAbilityCooldown.value);
            currentPrimaryCDBuff -= stats.PrimaryAbilityCooldown.value - targetPrimaryAbilityCooldownReduction;
            float targetDashAbilityCooldownReduction = stats.DashCooldown.value - currentDashCDBuff;
            stats.DashCooldown.value = Mathf.Clamp(stats.DashCooldown.value - currentDashCDBuff, 0.1f, stats.DashCooldown.value);
            currentDashCDBuff -= stats.DashCooldown.value - targetDashAbilityCooldownReduction;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.MovementSpeed.value -= currentMovementSpeedBuff;
            stats.PrimaryAbilityCooldown.value += currentPrimaryCDBuff;
            stats.DashCooldown.value += currentDashCDBuff;
        }
    }
}