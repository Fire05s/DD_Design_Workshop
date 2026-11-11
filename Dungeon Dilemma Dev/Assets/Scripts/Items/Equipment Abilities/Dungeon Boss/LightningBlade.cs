using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using Priestess;

namespace Items
{
    [CreateAssetMenu(menuName = "Item/Equipment Ability/Lightning Blade")]
    public class LightningBlade : ItemAbility, IOnHitEffect, ITimedAbility
    {
        private const short CD_DURATION = 20;
        private const float DMG_MULTIPLIER = 1.15f;
        public float ApplyOnHitEffect(float damage, SyncedStats stats, SyncedStats opponentStats = null)
        {
            AbilityTimedState abilityTimedState = InstantiateAbilityTime();

            if (abilityTimedState.InCooldown) return damage;
            float newDamage = damage * DMG_MULTIPLIER;

            /// applying bounce damage
            Vector3 bossPosition = stats.transform.position;

            // find all challengers currently active in the scene
            GameObject[] challengers = GameObject.FindGameObjectsWithTag("Challenger");

            GameObject nearestChallenger = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (GameObject challenger in challengers)
            {
                // skip the current target
                if (opponentStats != null && challenger == opponentStats.gameObject) continue;

                // find nearest challenger
                Vector3 directionToTarget = challenger.transform.position - bossPosition;
                float dSqr = directionToTarget.sqrMagnitude;

                if (dSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqr;
                    nearestChallenger = challenger;
                }
            }

            if (nearestChallenger != null)
            {
                if (nearestChallenger.TryGetComponent(out HealthHandler secondaryChallenger))
                {
                    secondaryChallenger.TakeDamageServerRpc(newDamage, new List<StatusEffectType>(), stats.transform);
                }
            }
            abilityTimedState.ResetTimer();

            return newDamage;
        }

        public AbilityTimedState InstantiateAbilityTime()
        {
            return new AbilityTimedState(CD_DURATION);
        }
    }
}