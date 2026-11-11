namespace Status
{
    public class RuruBuff : StatusEffect, IBuffStatus
    {
        private float currentAttackBuff;
        public RuruBuff()
        {
            cooldown = 2;
        }
        public void ApplyStatusEffect(SyncedStats stats)
        {
            currentAttackBuff = 0.5f * stats.AttackPower.value / 100;
            stats.AttackBuff.value += currentAttackBuff;
        }
        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.AttackBuff.value -= currentAttackBuff;
        }
    }
}