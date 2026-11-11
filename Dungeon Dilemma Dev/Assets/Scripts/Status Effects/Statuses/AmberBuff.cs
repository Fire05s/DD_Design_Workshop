namespace Status
{
    public class AmberBuff : StatusEffect, IBuffStatus
    {
        private float currentResistanceBuff;
        private float currentMovespeedDebuff;
        public AmberBuff()
        {
            cooldown = 10; // apply per second that this ability is active
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            currentResistanceBuff = stats.Resistance.value;
            currentMovespeedDebuff = stats.MovementSpeed.value * 0.15f;
            stats.Resistance.value += currentResistanceBuff;
            stats.MovementSpeed.value -= currentMovespeedDebuff;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.Resistance.value -= currentResistanceBuff;
            stats.MovementSpeed.value += currentMovespeedDebuff;
        }
    }
}