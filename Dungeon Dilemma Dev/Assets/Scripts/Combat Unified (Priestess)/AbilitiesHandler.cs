using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using PurrNet;

using Items;
using Status;

namespace Priestess
{
    public class AbilitiesHandler {
        public List<StatusEffectType> attackStatuses = new List<StatusEffectType>();
        private List<ItemAbility> upgradeAbilities = new List<ItemAbility>();
        private List<ItemAbility> equipmentAbilities = new List<ItemAbility>();
        private Dictionary<ItemAbility, AbilityUsageState> abilityUsages = new Dictionary<ItemAbility, AbilityUsageState>();
        private Dictionary<ItemAbility, AbilityTimedState> abilityTimes = new Dictionary<ItemAbility, AbilityTimedState>();
        private Dictionary<ItemAbility, AbilityStatDeltaState> abilityStatDeltas = new Dictionary<ItemAbility, AbilityStatDeltaState>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgrade"></param>
        public void RegisterUpgradeAbility(Upgrade upgrade)
        {
            ItemAbility ability = upgrade.Ability;
            if (ability is null)
            {
                return;
            }
            upgradeAbilities.Add(ability);
            if (ability is ITimedAbility itb)
            {
                abilityTimes.Add(ability, itb.InstantiateAbilityTime());
            }
            if (ability is ILimitedAbility ilb)
            {
                abilityUsages.Add(ability, ilb.InstantiateAbilityUsage());
            }
            if (ability is IRepeatedEffect ire)
            {
                abilityStatDeltas.Add(ability, ire.InstantiateStatState());
            }
            if (ability is IStatusGiver isg)
            {
                attackStatuses.Add(isg.GetStatus());
            }
        }
        /// <summary>
        /// Register an equipment with the ability manager
        /// </summary>
        /// <param name="equipment"></param>
        public void RegisterEquipmentAbility(Equipment equipment)
        {
            ItemAbility ability = equipment.Ability;
            if (ability is null)
            {
                return;
            }
            equipmentAbilities.Add(ability);
            if (ability is ITimedAbility itb)
            {
                abilityTimes.Add(ability, itb.InstantiateAbilityTime());
            }
            if (ability is ILimitedAbility ilb)
            {
                abilityUsages.Add(ability, ilb.InstantiateAbilityUsage());
            }
            if (ability is IRepeatedEffect ire)
            {
                abilityStatDeltas.Add(ability, ire.InstantiateStatState());
            }
            if (ability is IStatusGiver isg)
            {
                attackStatuses.Add(isg.GetStatus());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dmg"></param>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public float ApplyOnHitAbilities(float dmg, SyncedStats stats, SyncedStats opponent = null)
        {
            float newDmg = dmg;
            foreach (ItemAbility ab in equipmentAbilities)
            {
                if (ab is not IOnHitEffect) { continue; }
                if (ab is ILimitedAbility && ab is ITimedAbility)
                {
                    if (abilityUsages[ab].Uses > 0 && abilityTimes[ab].InCooldown==false)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityUsages[ab].ConsumeAbilityUse();
                        abilityTimes[ab].ResetTimer();
                    }
                }
                else if (ab is ILimitedAbility && ab is not ITimedAbility)
                {
                    if (abilityUsages[ab].Uses > 0)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityUsages[ab].ConsumeAbilityUse();
                    }
                }
                else if (ab is not ILimitedAbility && ab is ITimedAbility)
                {
                    if (abilityTimes[ab].InCooldown==false)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityTimes[ab].ResetTimer();
                    }
                }
                else
                {
                    IOnHitEffect ioh = (IOnHitEffect)ab;
                    newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                }
            }

            foreach (ItemAbility ab in upgradeAbilities)
            {
                if (ab is not IOnHitEffect) { continue; }
                if (ab is ILimitedAbility && ab is ITimedAbility)
                {
                    if (abilityUsages[ab].Uses > 0 && abilityTimes[ab].InCooldown==false)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityUsages[ab].ConsumeAbilityUse();
                        abilityTimes[ab].ResetTimer();
                    }
                }
                else if (ab is ILimitedAbility && ab is not ITimedAbility)
                {
                    if (abilityUsages[ab].Uses > 0)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityUsages[ab].ConsumeAbilityUse();
                    }
                }
                else if (ab is not ILimitedAbility && ab is ITimedAbility)
                {
                    if (abilityTimes[ab].InCooldown==false)
                    {
                        IOnHitEffect ioh = (IOnHitEffect)ab;
                        newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                        abilityTimes[ab].ResetTimer();
                    }
                }
                else
                {
                    IOnHitEffect ioh = (IOnHitEffect)ab;
                    newDmg = ioh.ApplyOnHitEffect(newDmg, stats, opponent);
                }
            }
            return newDmg;
        }
        /// <summary>
        /// 
        /// </summary>
        public void TickAbilities(SyncedStats _stats)
        {
            //Run cooldown on all timed abilities
            foreach (AbilityTimedState state in abilityTimes.Values)
            {
                state.TickCooldown();
            }
            //Apply effect for upgrade abilities
            foreach (ItemAbility ability in upgradeAbilities)
            {
                if (ability is IRepeatedEffect ire)
                {
                    bool canTrigger = true;
                    if (ability is ITimedAbility && abilityTimes[ability].InCooldown==true) { canTrigger = false;}
                    if (ability is ILimitedAbility && abilityUsages[ability].Uses <= 0) { canTrigger = false; }
                    if (!canTrigger){continue;}
                    abilityStatDeltas[ability] = ire.ApplyRepeatedEffect(_stats, abilityStatDeltas[ability]);
                    if (ability is ITimedAbility) { abilityTimes[ability].ResetTimer(); }
                    if (ability is ILimitedAbility) { abilityUsages[ability].ConsumeAbilityUse(); }
                }
                else if (ability is IConditionalEffect icf)
                {
                    bool canTrigger = true;
                    if (ability is ITimedAbility && abilityTimes[ability].InCooldown==true) { canTrigger = false;}
                    if (ability is ILimitedAbility && abilityUsages[ability].Uses <= 0) { canTrigger = false; }
                    if (!canTrigger){continue;}
                    if (icf.CheckForCondition(_stats)) { icf.ApplyConditionalEffect(_stats); }
                    else { continue;}
                    if (ability is ITimedAbility) { abilityTimes[ability].ResetTimer(); }
                    if (ability is ILimitedAbility) { abilityUsages[ability].ConsumeAbilityUse(); }
                }
            }
            //check equipment abilities
            foreach (ItemAbility ability in equipmentAbilities)
            {
                if (ability is IRepeatedEffect ire)
                {
                    bool canTrigger = true;
                    if (ability is ITimedAbility && abilityTimes[ability].InCooldown==true) { canTrigger = false;}
                    if (ability is ILimitedAbility && abilityUsages[ability].Uses <= 0) { canTrigger = false; }
                    if (!canTrigger){continue;}
                    abilityStatDeltas[ability] = ire.ApplyRepeatedEffect(_stats, abilityStatDeltas[ability]);
                    if (ability is ITimedAbility) { abilityTimes[ability].ResetTimer(); }
                    if (ability is ILimitedAbility) { abilityUsages[ability].ConsumeAbilityUse(); }
                }
                else if (ability is IConditionalEffect icf)
                {
                    bool canTrigger = true;
                    if (ability is ITimedAbility && abilityTimes[ability].InCooldown==true) { canTrigger = false;}
                    if (ability is ILimitedAbility && abilityUsages[ability].Uses <= 0) { canTrigger = false; }
                    if (!canTrigger){continue;}
                    if (icf.CheckForCondition(_stats)) { icf.ApplyConditionalEffect(_stats); }
                    else {continue;}
                    if (ability is ITimedAbility) { abilityTimes[ability].ResetTimer(); }
                    if (ability is ILimitedAbility) { abilityUsages[ability].ConsumeAbilityUse(); }
                }
            }
        }
        public void RefreshLimitedAbilities()
        {
            foreach ((ItemAbility ability, AbilityUsageState abilityUse) in abilityUsages)
            {
                abilityUse.RefreshUses();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipment"></param>
        public void RemoveEquipmentAbility(Equipment equipment)
        {
            ItemAbility eqa = equipment.Ability;
            equipmentAbilities.Remove(eqa);
            if (eqa is ITimedAbility) { abilityTimes.Remove(eqa); }
            if (eqa is ILimitedAbility) { abilityUsages.Remove(eqa); }
            if (eqa is IRepeatedEffect) { abilityStatDeltas.Remove(eqa); }

            if (eqa is IStatusGiver isg)
            {
                attackStatuses.Remove(isg.GetStatus());
            }
        }
    }
}