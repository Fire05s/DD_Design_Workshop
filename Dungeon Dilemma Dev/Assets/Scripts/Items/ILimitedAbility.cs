using System;
using System.Collections;
using UnityEngine;

namespace Items
{
    /// <summary>
    /// Interfaces for <c>ItemAbility</c> objects that require non static data, such as uses or a timer.
    /// </summary>
    public interface ILimitedAbility
    {
        /// <summary>
        /// Return a new AbilityUsageState necessary.
        /// </summary>
        /// <returns>A new <c>AbilityUsageState</c> object</returns>
        public AbilityUsageState InstantiateAbilityUsage();
    }

    /// <summary>
    /// Class representing an instance of an Item Ability that requires non static data
    /// </summary>
    public class AbilityUsageState
    {
        public int BaseUses;
        public int Uses;
        public AbilityUsageState(int numUses)
        {
            BaseUses = numUses;
            Uses = numUses;
        }
        public void ConsumeAbilityUse()
        {
            Uses -= 1;
        }
        public void RefreshUses()
        {
            Uses = BaseUses;
        }
    } 

}