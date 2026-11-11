using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priestess;
using PurrNet;

/// <summary>
/// A cobweb trap that damages challengers who pass through and applies a slowing debuff on them
/// </summary>
public class CobwebTrap : Trap
{
    [Header("Cobweb Settings")]
    [SerializeField] float _damageInterval = 0.5f; //intervals between damage ticks (damage/sec is preserved)

    // key = challenger, value = the time they can be damaged again
    private Dictionary<GameObject, float> _nextDamageTimeMap = new Dictionary<GameObject, float>();
    private List<GameObject> _challengersInRange = new List<GameObject>();

    protected override void ActivateTrap(Collider2D challenger)
    {
        //no activation necessary; always active
    }

    private void Update()
    {
        // process damage for everyone on this trap if active, applying per defined interval
        if (isServer)
        {
            for (int i = _challengersInRange.Count - 1; i >= 0; i--)
            {
                GameObject challenger = _challengersInRange[i];

                // safety check in case object was destroyed
                if (challenger == null) {
                    _challengersInRange.RemoveAt(i);
                    continue;
                }

                if (!_nextDamageTimeMap.ContainsKey(challenger)) _nextDamageTimeMap[challenger] = 0;

                if (Time.time >= _nextDamageTimeMap[challenger])
                {
                    ApplyDamage(challenger);
                    _nextDamageTimeMap[challenger] = Time.time + _damageInterval;
                }
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) { return; }
        // add challengers on this trap to a tracking list
        if (other.CompareTag("Challenger") && !_challengersInRange.Contains(other.gameObject))
        {
            _challengersInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isServer) { return; }
        // remove challengers leaving this trap from the tracking list
        if (other.CompareTag("Challenger") || other.CompareTag("Dead"))
        {
            _challengersInRange.Remove(other.gameObject);
            if (_nextDamageTimeMap.ContainsKey(other.gameObject)) 
                _nextDamageTimeMap.Remove(other.gameObject);
        }
    }

    /// <summary>
    /// Called when a Challenger is in the trap. Deals damage and applies debuff
    /// </summary>
    /// <param name="challenger">The Challenger that collided with this trap.</param>
    protected void ApplyDamage(GameObject challenger)
    {
        HealthHandler health = challenger.GetComponent<HealthHandler>();
        if (health != null)
        {
            health.TakeDamageServerRpc(_damage, new List<StatusEffectType>() { StatusEffectType.Webbed }, transform);
        }
    }
}
