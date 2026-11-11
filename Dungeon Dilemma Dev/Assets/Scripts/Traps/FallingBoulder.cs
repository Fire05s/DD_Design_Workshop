using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priestess;
using PurrNet;

public class FallingBoulder : Trap
{
    [Header("Boulder Settings")]
    [SerializeField] private Collider2D _damageCollider; // Collider that deals damage when falling boulder is active
    [SerializeField] private float _activeDuration = 0.5f;
    [SerializeField] private float _fallingDuration = 1.0f;
    [SerializeField] private Animator _boulderAnimator;

    [Header("Audio")]
    [SerializeField] private AudioClip _fallAudio;
    [SerializeField] private AudioClip _resetAudio;

    private enum ActiveState { Activating, Active, Deactivating }
    private SyncVar<ActiveState> _isActive = new();
    private ActiveState _lastActiveState = ActiveState.Deactivating;
    private bool _isCycling = false;
    private List<GameObject> _challengersInRange = new List<GameObject>();
    private HashSet<GameObject> _damagedChallengers = new HashSet<GameObject>(); // Track challengers damaged during current activation


    protected override void ActivateTrap(Collider2D challenger)
    {
        ; // no necessary activation for this looping trap
    }

    private void Update()
    {
        if (isServer && !_isCycling) 
        {
            StartCoroutine(BoulderCycle());
        }

        // process damage for everyone on this trap if active, applying once per challenger
        if (_isActive.value == ActiveState.Active && isServer)
        {
            for (int i = _challengersInRange.Count - 1; i >= 0; i--)
            {
                GameObject challenger = _challengersInRange[i];

                // safety check in case object was destroyed
                if (challenger == null) {
                    _challengersInRange.RemoveAt(i);
                    continue;
                }

                ApplyDamage(challenger);
            }
        }

        if (_lastActiveState != _isActive)
        {
            switch (_isActive.value)
            {
                case ActiveState.Activating:
                    _boulderAnimator.SetBool("isDeactivating", false);
                    _boulderAnimator.SetBool("isActivating", true);
                    break;
                case ActiveState.Deactivating:
                    _boulderAnimator.SetBool("isActivating", false);
                    _boulderAnimator.SetBool("isDeactivating", true);
                    break;
            }
        }
        _lastActiveState = _isActive.value;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // add challengers on this trap to a tracking list
        if (other.CompareTag("Challenger") && !_challengersInRange.Contains(other.gameObject))
        {
            _challengersInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // remove challengers leaving this trap from the tracking list
        if (other.CompareTag("Challenger") || other.CompareTag("Dead"))
        {
            _challengersInRange.Remove(other.gameObject);
        }
    }


    private IEnumerator BoulderCycle()
    {
        _isCycling = true;

        _isActive.value = ActiveState.Activating;

        // Play fall audio sound
        SoundFXManager.Instance?.PlaySoundFXClip(_fallAudio, transform.position);

        yield return new WaitForSeconds(_fallingDuration);

        _isActive.value = ActiveState.Active;

        // Damage State (aka squish everyone currently under the boulder)
        // deal damage
        if (_damageCollider != null)
        {
            _damageCollider.enabled = true;
        }

        yield return new WaitForSeconds(_activeDuration);

        // Resetting State
        if (_damageCollider != null)
        {
            _damageCollider.enabled = false;
        }

        // Clear the list of damaged challengers so they can be damaged again on next activation
        _damagedChallengers.Clear();

        _isActive.value = ActiveState.Deactivating;

        // Play reset audio sound
        SoundFXManager.Instance?.PlaySoundFXClip(_resetAudio, transform.position);

        yield return new WaitForSeconds(_cooldown);

        _isCycling = false;
    }

    private void ApplyDamage(GameObject challengerObject)
    {
        // Check if this challenger has already been damaged during this activation
        if (_damagedChallengers.Contains(challengerObject))
        {
            return; 
        }
        // Get the challenger's components - get from gameObject, not collider
        HealthHandler healthHandler = challengerObject.GetComponent<HealthHandler>();

        if (healthHandler != null)
        {
            // Deal damage to the challenger
            healthHandler.TakeDamageServerRpc(_damage, new List<Priestess.StatusEffectType>(), transform);
            
            // Mark this challenger as damaged for this activation cycle
            _damagedChallengers.Add(challengerObject);
        }
    }
}