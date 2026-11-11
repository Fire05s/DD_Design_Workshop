using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Items;
using ControllerLogic;
using Sync_Modules;

namespace Priestess
{
    [RequireComponent(typeof(SyncedStats))]
    public class CombatManager : MonoBehaviour
    {
        public float TickCooldown = 1;
        private bool inCooldown;
        private SyncedStats stats;
        private HealthHandler healthHandler;
        private AbilitiesHandler abilities;
        private StatusHandler statuses;
        private bool _refreshedAbilities = false;
        
        void Start() {
            Initialize();
        }

        /// <summary>
        /// Initialize the Combat Manager and all subsequent systems
        /// </summary>
        void Initialize()
        {
            inCooldown = false;
            TickCooldown = 1f;
            stats = gameObject.GetComponentInParent<SyncedStats>();
            healthHandler = gameObject.GetComponentInParent<HealthHandler>(); // finding HealthHandler for minions
            abilities = new AbilitiesHandler();
            statuses = new StatusHandler(stats, healthHandler, new StatusParser());
        }
        /// <summary>
        /// Runs every set number of frames (dependent on FPS)
        /// </summary>
        void FixedUpdate()
        {
            if (!inCooldown)
            {
                abilities.TickAbilities(stats);
                statuses.TickStatuses();

                StartCoroutine(TickDelay());
            }

            if (GlobalController.Singleton.WorldData.RoundState == RoundState.Setup && !_refreshedAbilities)
            {
                _refreshedAbilities = true;
                abilities.RefreshLimitedAbilities();
            }
            else if (GlobalController.Singleton.WorldData.RoundState == RoundState.PostRound && _refreshedAbilities)
            {
                _refreshedAbilities = false;
            }
        }
        /// <summary>
        /// Add an item to the combat system
        /// </summary>
        /// <typeparam name="T">Equipment and Upgrade only</typeparam>
        /// <param name="item">Item to add</param>
        public void RegisterItem(Item item){
            if (item is Equipment _equip) { abilities.RegisterEquipmentAbility(_equip); }
            else if (item is Upgrade _upgrade) { abilities.RegisterUpgradeAbility(_upgrade); }
        }
        /// <summary>
        /// Remove an item from the combat system
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(Equipment item)
        {
            abilities.RemoveEquipmentAbility(item);
        }
        /// <summary>
        /// Calculate the outgoing attack value for this player.
        /// </summary>
        /// <param name="_stats">SyncedStats of opponent (if applicable)</param>
        /// <param name="baseMult"> Base multiplier of the attack (1 by default) </param>
        /// <returns>A tuple (Outgoing Dmg, List of StatusEffects, Lifesteal)</returns>
        public (float, List<StatusEffectType>, float) RegisterAttackOutgoing(SyncedStats _stats = null, float baseMult = 1)
        {
            float dmg = AttackCalculator.CalculateAttackDamage(stats, baseMult);
            dmg = abilities.ApplyOnHitAbilities(dmg, stats, _stats);
            List<StatusEffectType> atkEffect = abilities.attackStatuses;
            float lf = stats.Lifesteal.value/100 * dmg;

            return (dmg, atkEffect, lf);
        }
        /// <summary>
        /// Process an incoming attack to this player. Applies armor and damage consumption as well.
        /// </summary>
        /// <param name="incomingDmg">Full incoming damage</param>
        /// <param name="statuses">List of status effects applied by the attack</param>
        /// <returns>Nothing</returns>
        public void RegisterDamageIncoming(float incomingDmg, List<StatusEffectType> setl, Transform source, bool isStatusEffect, StatusEffectType sourceType)
        {
            float postResistanceDmg = incomingDmg / (1 + (stats.Resistance.value / 100));
            float consumedArmor;
            float takenDamage;
            if (postResistanceDmg >= stats.Armor.value)
            {
                consumedArmor = stats.Armor.value;
                takenDamage = postResistanceDmg - stats.Armor.value;
            } else
            {
                consumedArmor = postResistanceDmg;
                takenDamage = 0;
            }

            float newHealth  = stats.CurrentHealth.value - takenDamage;
            stats.CurrentHealth.value = Mathf.Clamp(newHealth, 0, stats.MaximumHealth.value);
            stats.Armor.value -= consumedArmor;

            if (healthHandler == null)
            {
                healthHandler = gameObject.GetComponentInChildren<HealthHandler>(); // finding HealthHandler for players
                statuses.healthHandler = healthHandler;
            }

            if (setl != null)
            {
                foreach (StatusEffectType type in setl)
                {
                    statuses.AddStatusEffect(type, source);
                }
            }

            // Applying Damage Dealt Score
            if (source != null && source.GetComponentInParent<SyncedStats>() != null)
            {
                GlobalController.Singleton?.UpdatePlayerScoreData(source.GetComponentInParent<SyncedStats>().owner.Value, ScoreType.DamageDealt, postResistanceDmg);
            }

            healthHandler.SpawnDamageTextRpc(-1 * postResistanceDmg, consumedArmor > 0, false, isStatusEffect, sourceType);
        }
        public void ApplyStatusEffect(StatusEffectType type, Transform source)
        {
            statuses.AddStatusEffect(type, source);
        }
        /// <summary>
        /// Tick combat related effects based on the tick cooldowns
        /// </summary>
        /// <returns></returns>
        private IEnumerator TickDelay()
        {
            inCooldown = true;
            yield return new WaitForSeconds(TickCooldown);
            inCooldown = false;
        }

        /// <summary>
        /// Call StatusHandler to clear all statuses
        /// </summary>
        public void ClearStatuses()
        {
            statuses.ClearStatuses();
        }
    }
}