using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using Sync_Modules;
using UnityEngine;

/// <summary>
/// A spike trap that activates when a challenger steps on it. The spikes pop out after a delay,
/// deal damage for a duration, then retract and enter cooldown.
/// </summary>
public class SpikeTrap : Trap
{
    private bool _isOnCooldown = false; // Whether trap is currently on cooldown
    private HashSet<GameObject> _damagedChallengers = new HashSet<GameObject>(); // Track challengers damaged during current activation

    [Header("Spike Trap Settings")]
    [SerializeField] private Collider2D _damageCollider; // Collider that deals damage when spikes are active
    [SerializeField] private Animator _spikeAnimator; // Animator for spike animations
    [SerializeField] private float _activationDelay = 0.5f; // Delay between player collision and spike activation
    [SerializeField] private float _activationTime = 0.5f; // Time the spikes will stay out for

    [Header("Audio")]
    [SerializeField] private AudioClip _spikeAudio;

    private void FixedUpdate()
    {
        // show hidden trap in Setup Phase
        if (GlobalController.Singleton.WorldData.RoundState == RoundState.Setup)
        {
            _spikeAnimator.SetBool("isSetup", true);
        }
        else
        {
            _spikeAnimator.SetBool("isSetup", false);
        }
    }

    /// <summary>
    /// Called when a challenger collides with the trap. Deals damage if spikes are active,
    /// otherwise starts the activation sequence.
    /// </summary>
    /// <param name="challenger">The challenger that collided with this trap.</param>
    protected override void ActivateTrap(Collider2D challenger)
    {
        if (!_isOnCooldown)
        {
            StartCoroutine(PopOut());
        }
    }

    /// <summary>
    /// Calculates and applies damage to the challenger using the AttackModule once per activation cycle.
    /// </summary>
    /// <param name="challenger">The challenger to damage.</param>
    private void DealDamage(GameObject challenger)
    {
        if (!isServer) { return; }
        // Check if this challenger has already been damaged during this activation
        if (_damagedChallengers.Contains(challenger))
        {
            return; 
        }
        // Get the challenger's components - get from gameObject, not collider
        HealthHandler healthHandler = challenger.GetComponent<HealthHandler>();

        if (healthHandler != null)
        {
            // Deal damage to the challenger
            healthHandler.TakeDamageServerRpc(_damage, new List<Priestess.StatusEffectType>(), transform);
            
            // Mark this challenger as damaged for this activation cycle
            _damagedChallengers.Add(challenger);
        }
    }

    /// <summary>
    /// Coroutine that handles the spike activation sequence: delay, activation, and cooldown.
    /// </summary>
    private IEnumerator PopOut()
    {
        _isOnCooldown = true;
        
        // Disable damage collider during delay
        if (_damageCollider != null)
        {
            _damageCollider.enabled = false;
        }

        // Wait for activation delay
        yield return new WaitForSeconds(_activationDelay);

        // Activate spikes
        if (_damageCollider != null)
        {
            _damageCollider.enabled = true;
        }

        // Update animator if available
        if (_spikeAnimator != null)
        {
            _spikeAnimator.SetBool("isInactive", false);
            _spikeAnimator.SetBool("isActivating", true);
        }

        // Play spike activation sound
        SoundFXManager.Instance?.PlaySoundFXClip(_spikeAudio, transform.position);

        foreach (GameObject c in UniqueChallengersInZone())
        {
            DealDamage(c);
        }

        // Keep spikes active for activation time
        yield return new WaitForSeconds(_activationTime);

        // Deactivate spikes
        if (_damageCollider != null)
        {
            _damageCollider.enabled = false;
        }

        // Clear the list of damaged challengers so they can be damaged again on next activation
        _damagedChallengers.Clear();

        // Update animator
        if (_spikeAnimator != null)
        {
            _spikeAnimator.SetBool("isActivating", false);
            _spikeAnimator.SetBool("isDeactivating", true);
        }

        // Wait for cooldown (handled by base class, but we also track it here)
        yield return new WaitForSeconds(_cooldown);

        // Reset animator state
        if (_spikeAnimator != null)
        {
            _spikeAnimator.SetBool("isDeactivating", false);
            _spikeAnimator.SetBool("isInactive", true);
        }

        // Re-enable trigger collider for next activation
        if (_damageCollider != null)
        {
            _damageCollider.enabled = true;
        }

        _isOnCooldown = false;
    }

    /// <summary>
    /// Override the base cooldown timer to work with our custom cooldown logic.
    /// The actual cooldown is handled in PopOut coroutine, so this can be a no-op.
    /// </summary>
    protected override IEnumerator CooldownTimer(float time)
    {
        // Cooldown is handled in PopOut coroutine, so we don't need to do anything here
        yield return null;
    }

    private List<GameObject> UniqueChallengersInZone()
    {
        List<GameObject> uniqueChallengers = new List<GameObject>();
        if (_damageCollider == null) return uniqueChallengers;

        Bounds b = _damageCollider.bounds;
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(b.center, b.size, 0f);

        foreach (Collider2D c in overlapping)
        {
            if (c != _damageCollider && c.CompareTag("Challenger"))
            {
                GameObject go = c.gameObject;
                // only add the GameObject if we haven't seen it yet in this query
                if (!uniqueChallengers.Contains(go))
                {
                    uniqueChallengers.Add(go);
                }
            }
        }

        return uniqueChallengers;
    }
}