using Status;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Priestess
{
    public class StatusHandler
    {
        public SyncedStats stats;
        public HealthHandler healthHandler;
        public TDHealthHandler tdHealthHandler;
        public StatusParser parser;
        public List<StatusEffect> statuses = new List<StatusEffect>();
        public Dictionary<StatusEffect, Transform> damageStatusSources = new Dictionary<StatusEffect, Transform>();

        public StatusHandler(SyncedStats _stats, HealthHandler _healthHandler, StatusParser _parser)
        {
            stats = _stats;
            healthHandler = _healthHandler;
            parser = _parser;
        }
        public StatusHandler(SyncedStats _stats, TDHealthHandler _healthHandler, StatusParser _parser)
        {
            stats = _stats;
            tdHealthHandler = _healthHandler;
            parser = _parser;
        }
        /// <summary>
        /// Add a status effect to the player
        /// </summary>
        /// <param name="status"></param>
        public void AddStatusEffect(StatusEffectType sty, Transform source)
        {
            StatusEffect status = parser.getNewStatusFromEnum(sty);
            statuses.Add(status);
            if (status is IBuffStatus) {
                IBuffStatus ibf = (IBuffStatus)status;
                ibf.ApplyStatusEffect(stats);
            }
            if (status is IDamageStatus)
            {
                damageStatusSources[status] = source;
            }
        }
        /// <summary>
        /// Run cooldown on statuses and apply
        /// </summary>
        public void TickStatuses()
        {
            List<StatusEffect> keptStatuses = new List<StatusEffect>();
            foreach (StatusEffect status in statuses)
            {
                bool ended = status.CheckCooldown();
                if (status is IDamageStatus ids) {
                    if (healthHandler != null) {
                        ids.DealDamage(healthHandler, damageStatusSources[status]);
                    }
                    else
                    {
                        ids.DealDamage(tdHealthHandler, damageStatusSources[status]);
                    }
                }
                if (!ended) {keptStatuses.Add(status);}
                if (ended && status is IBuffStatus ibf) {ibf.UnapplyStatusEffect(stats);}
                if (ended && status is IDamageStatus) {damageStatusSources.Remove(status);}
            }
            statuses = keptStatuses;
        }
        /// <summary>
        /// Remove all statuses on this unit
        /// </summary>
        public void ClearStatuses()
        {
            foreach (StatusEffect status in statuses)
            {
                if (status is IBuffStatus ibf) {ibf.UnapplyStatusEffect(stats);}
            }
            statuses.Clear();
            damageStatusSources.Clear();
        }
    }
}