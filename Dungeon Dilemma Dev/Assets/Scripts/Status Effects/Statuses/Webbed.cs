using UnityEngine;

namespace Status
{
    public class Webbed : StatusEffect, IBuffStatus
    {
        private float currentMovementSpeedDebuff;
        public Webbed()
        {
            cooldown = 2;
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            float originalSpeed = stats.MovementSpeed.value;
            float targetSpeed = originalSpeed * 0.75f;
            stats.MovementSpeed.value = Mathf.Max(1f, targetSpeed);
            currentMovementSpeedDebuff = originalSpeed - stats.MovementSpeed.value;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.MovementSpeed.value += currentMovementSpeedDebuff;
        }
    }
}
