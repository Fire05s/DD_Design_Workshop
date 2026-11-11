using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priestess;

/// <summary>
/// Haunted Armor is a wall-adjacent trap that attacks horizontally.
/// It checks an area in front of it; when a Challenger is detected, it waits a brief delay
/// then instantly strikes anything still in the strike area.
/// </summary>
public class HauntedArmor : Trap
{
    private bool _isOnCooldown = false; // Whether trap is currently on cooldown
    private HashSet<GameObject> _damagedChallengers = new HashSet<GameObject>(); // Track challengers damaged during current activation

    [Header("Haunted Armor Trap Settings")]
    [SerializeField] private Collider2D _triggerCollider; // Collider that triggers and deals damage when haunted armor is active
    [SerializeField] private Animator _armorAnimator; // Animator for haunted armor animations
    [SerializeField] private float _activationDelay = 0.5f; // Delay between player collision and haunted armor activation
    [SerializeField] private float _activationTime = 0.5f; // Time the haunted armor will be active for

    [Header("Audio")]
    [SerializeField] private AudioClip _armorAudio;

    /// <summary>
    /// Called when a challenger collides with the trap. Deals damage if haunted armor is active,
    /// otherwise starts the activation sequence.
    /// </summary>
    /// <param name="challenger">The challenger that collided with this trap.</param>
    protected override void ActivateTrap(Collider2D challenger)
    {
        if (!_isOnCooldown)
        {
            StartCoroutine(Strike());
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
    /// Coroutine that handles the haunted armor activation sequence: delay, activation, and cooldown.
    /// </summary>
    private IEnumerator Strike()
    {
        _isOnCooldown = true;
        
        // Disable damage collider during delay
        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }

        // Update animator if available
        if (_armorAnimator != null)
        {
            _armorAnimator.SetBool("isInactive", false);
            _armorAnimator.SetBool("isActivating", true);
        }

        // Wait for activation delay
        yield return new WaitForSeconds(_activationDelay);

        // Activate haunted armor
        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = true;
        }

        // Play haunted armor activation sound
        SoundFXManager.Instance?.PlaySoundFXClip(_armorAudio, transform.position);

        foreach (GameObject c in UniqueChallengersInZone())
        {
            DealDamage(c);
        }

        // Keep haunted armor active for activation time
        yield return new WaitForSeconds(_activationTime);

        // Deactivate haunted armor
        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }

        // Clear the list of damaged challengers so they can be damaged again on next activation
        _damagedChallengers.Clear();

        // Update animator
        if (_armorAnimator != null)
        {
            _armorAnimator.SetBool("isActivating", false);
            _armorAnimator.SetBool("isDeactivating", true);
        }

        // Wait for cooldown (handled by base class, but we also track it here)
        yield return new WaitForSeconds(_cooldown);

        // Reset animator state
        if (_armorAnimator != null)
        {
            _armorAnimator.SetBool("isDeactivating", false);
            _armorAnimator.SetBool("isInactive", true);
        }

        // Re-enable trigger collider for next activation
        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = true;
        }

        _isOnCooldown = false;
    }

    /// <summary>
    /// Override the base cooldown timer to work with our custom cooldown logic.
    /// The actual cooldown is handled in Strike coroutine, so this can be a no-op.
    /// </summary>
    protected override IEnumerator CooldownTimer(float time)
    {
        // Cooldown is handled in Strike coroutine, so we don't need to do anything here
        yield return null;
    }

    private List<GameObject> UniqueChallengersInZone()
    {
        List<GameObject> uniqueChallengers = new List<GameObject>();
        if (_triggerCollider == null) return uniqueChallengers;

        Bounds b = _triggerCollider.bounds;
        Collider2D[] overlapping = Physics2D.OverlapBoxAll(b.center, b.size, 0f);

        foreach (Collider2D c in overlapping)
        {
            if (c != _triggerCollider && c.CompareTag("Challenger"))
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
