using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using Sync_Modules;

namespace Priestess
{
    [RequireComponent(typeof(SyncedStats))]
    public class TDCombatManager : MonoBehaviour
    {
        public float TickCooldown = 1;
        private bool inCooldown;
        private SyncedStats stats;
        private TDHealthHandler healthHandler;
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
            healthHandler = GetComponent<TDHealthHandler>();
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
        /// Process an incoming attack to this player. Applies armor and damage consumption as well.
        /// </summary>
        /// <param name="incomingDmg">Full incoming damage</param>
        /// <param name="statuses">List of status effects applied by the attack</param>
        /// <returns>Nothing</returns>
        public void RegisterDamageIncoming(float incomingDmg, List<StatusEffectType> setl, Transform source, bool isStatusEffect, StatusEffectType sourceType)
        {
            float postResistanceDmg = incomingDmg / (1 + (stats.Resistance.value / 100));
            float newHealth  = stats.CurrentHealth.value - postResistanceDmg;
            stats.CurrentHealth.value = Mathf.Clamp(newHealth, 0, stats.MaximumHealth.value);

            if (setl != null)
            {
                foreach (StatusEffectType type in setl)
                {
                    statuses.AddStatusEffect(type, source);
                }
            }

            healthHandler.SpawnDamageTextRpc(-1 * incomingDmg, false, false, isStatusEffect, sourceType);
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