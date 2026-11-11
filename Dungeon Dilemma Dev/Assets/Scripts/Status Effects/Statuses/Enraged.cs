using ControllerLogic;
using UnityEngine;

namespace Status
{
    public class Enraged : StatusEffect, IBuffStatus
    {
        public Enraged()
        {
            cooldown = 7;
        }

        public void ApplyStatusEffect(SyncedStats stats)
        {
            stats.AttackBuff.value += 0.25f;
            stats.Resistance.value += 100f;
        }

        public void UnapplyStatusEffect(SyncedStats stats)
        {
            stats.AttackBuff.value -= 0.25f;
            stats.Resistance.value -= 100f;
        }
    }
}