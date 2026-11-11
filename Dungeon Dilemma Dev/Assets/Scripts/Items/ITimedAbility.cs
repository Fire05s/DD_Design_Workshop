using System;
using System.Collections;
using UnityEngine;

namespace Items
{
    /// <summary>
    /// Interface for <c>ItemAbility</c> objects that require a timer. Must also use <c>IInstanceAbility</c>
    /// </summary>
    public interface ITimedAbility
    {
        /// <summary>
        /// Return a new AbilityTimedState to track cooldowns
        /// </summary>
        /// <returns></returns>
        public AbilityTimedState InstantiateAbilityTime();
    }

    public class AbilityTimedState {
        public bool InCooldown {get; private set;}
        private short _cooldownTick;
        private short _cooldownDuration;
        public AbilityTimedState(short duration)
        {
            _cooldownDuration = duration;
        }
        public void ResetTimer()
        {
            _cooldownTick = _cooldownDuration;
            InCooldown = true;
        }
        public void TickCooldown()
        {
            if (_cooldownTick==0) {return;}

            _cooldownTick -= 1;
            
            if (_cooldownTick==0)
            {
                InCooldown = false;
            }
        }
    }
}