using UnityEngine;
namespace Items {
    /// <summary>
    /// An interface for items that have a continually applied effect
    /// </summary>
    public interface IRepeatedEffect
    {
        /// <summary>
        /// Applies a repeated effect to the provided sync stats
        /// </summary>
        AbilityStatDeltaState ApplyRepeatedEffect(SyncedStats stats, AbilityStatDeltaState lastSavedState);
        AbilityStatDeltaState InstantiateStatState();
    }

    /// <summary>
    /// Struct representing the effect an item has to a stat module.
    /// </summary>
    public struct AbilityStatDeltaState
    {
        public int Regen;
        public int AttackPower;
        public int Resistance;
        public int AttackRadius;
        public int MaximumHealth;
        public float AttackBuff;
        public float AttackDebuff;
        public float Lifesteal;
        public float Speed;
        public float DashStrength;
        public float Scale;
        public float PrimaryAbilityCooldown;
        public float SecondaryAbilityCooldown;
        public float DashCooldown;
        public int ShopRerollCost;
        public float ShopCostMultiplier;

    }
}