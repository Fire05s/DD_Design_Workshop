using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using Sync_Modules;
using UnityEngine;

/// <summary>
/// The Enhanced Windmill is a wall-adjacent trap that attacks by pushing challengers 
/// in the direction it is facing, before going into a cooldown.
/// it is hidden before it is triggered. 
/// </summary>
public class EnhancedWindmill : Trap
{
    private bool _isOnCooldown;

    private Vector2 _pushDirection = Vector2.right; //only horizontal
    [Header("EW Settings")]
    [SerializeField] private float _pushForce = 5f;
    [SerializeField] private float _activationDelay = 1f;
    [SerializeField] private float _pushTime; //length of time to push challenger
    [SerializeField] private BoxCollider2D _triggerCollider;
    [SerializeField] private Animator _windmillAnimator;
    [Header("Audio")]
    [SerializeField] private AudioClip _windmillAudio;

    private void FixedUpdate()
    {
        // show hidden trap in Setup Phase
        if (GlobalController.Singleton.WorldData.RoundState == RoundState.Setup)
        {
            _windmillAnimator.SetBool("isSetup", true);
        }
        else
        {
            _windmillAnimator.SetBool("isSetup", false);
        }
    }

    /// <summary>
    /// On trigger, enters the 1 second delay, and then pushes the challenger.
    /// </summary>
    /// <param name="challenger">The Challenger that collided with this trap.</param>
    protected override void ActivateTrap(Collider2D challenger)
    {
        if (!_isOnCooldown)
        {
            StartCoroutine(DelayedPush());
        }
    }

    /// <summary>
    /// Waits for the activation delay, then applies a push force to the Challenger
    /// if they are still within hitbox..
    /// </summary>
    private IEnumerator DelayedPush()
    {
        _isOnCooldown = true;

        //update animation
        if (_windmillAnimator != null) {
            _windmillAnimator.SetBool("isInactive", false);
            _windmillAnimator.SetBool("isActivating", true);
        }

        yield return new WaitForSeconds(_activationDelay);

        //update animator
        if (_windmillAnimator != null) {
            _windmillAnimator.SetBool("isActivating", false);
            _windmillAnimator.SetBool("isActive", true);
        }

        SoundFXManager.Instance?.PlaySoundFXClip(_windmillAudio, transform.position);

        foreach (GameObject c in UniqueChallengersInZone())
        {
            MovementController mc = c.GetComponent<MovementController>();
            Rigidbody2D rb = c.GetComponent<Rigidbody2D>();
            if (mc != null && rb != null){
                mc.SetCanMove(false);
                rb.AddForce(_pushDirection * _pushForce, ForceMode2D.Impulse);
                StartCoroutine(ReenableAfterDelay(mc, _pushTime));
            }
        }

        yield return new WaitForSeconds(_pushTime);

        //update animation
        if (_windmillAnimator != null) {
            _windmillAnimator.SetBool("isActive", false);
            _windmillAnimator.SetBool("isDeactivating", true);
        }

        yield return new WaitForSeconds(_cooldown);

        _isOnCooldown = false;

        if (_windmillAnimator != null) {
            _windmillAnimator.SetBool("isDeactivating", false);
            _windmillAnimator.SetBool("isInactive", true);
        }
    }

    /// <summary>
    /// waits for wind to finish pushing the challenger, before reenabling movement
    /// </summary>
    private IEnumerator ReenableAfterDelay(MovementController mc, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (mc != null){
            mc.SetCanMove(true);
        }
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