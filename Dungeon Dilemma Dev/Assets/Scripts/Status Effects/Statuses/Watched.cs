using UnityEngine;

namespace Status
{
    public class Watched : StatusEffect, IBuffStatus
    {
        private float currentMovementSpeedDebuff;
        public Watched()
        {
            cooldown = 2;
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            float originalSpeed = stats.MovementSpeed.value;
            float targetSpeed = originalSpeed - 1;
            stats.MovementSpeed.value = Mathf.Max(1f, targetSpeed);
            currentMovementSpeedDebuff = originalSpeed - stats.MovementSpeed.value;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.MovementSpeed.value += currentMovementSpeedDebuff;
        }
    }
}
