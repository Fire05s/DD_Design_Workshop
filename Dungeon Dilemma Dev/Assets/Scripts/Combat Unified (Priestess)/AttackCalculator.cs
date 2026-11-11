using UnityEngine;

namespace Priestess
{
    public static class AttackCalculator
    {
        public static float CalculateAttackDamage(SyncedStats self, float atkMult)
        {
            // Base damage from attack power, including buffs and debuffs (pre-resistance).
            float dmg = self.AttackPower.value * atkMult;
            dmg += dmg * self.AttackBuff.value;
            dmg -= dmg * self.AttackDebuff.value;

            return dmg;
        }
    }
}