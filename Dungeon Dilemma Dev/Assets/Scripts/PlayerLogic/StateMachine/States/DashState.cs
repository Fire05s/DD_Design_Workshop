using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SyncedStats;

/// <summary>
/// Handles the logic and animations for the Dash State.
/// </summary>
public class DashState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _dashAnim;

    [Header("Audio")]
    [SerializeField] private AudioClip _dashAudio;

    [Header("Collision Layers")]
    [SerializeField] private int _currentLayerInt;
    [SerializeField] private List<int> _ignoreLayersInt;
    [SerializeField] private LayerMask _dashIgnoreLayers;

    [Header("Components")]
    [SerializeField] private MovementController _movement;
    [SerializeField] private HealthHandler _health;
    [SerializeField] private Collider2D _physicsCollider;

    [Header("Dash Specific Stats")]
    [SerializeField] private float _dashDuration;
    public int MaximumDashes = 1;

    public int DashesLeft;
    private Coroutine _cooldownCoroutine;
    private Vector2 _dashDirection = Vector2.zero;
    private LayerMask _originalExcludedLayers;
    private bool _isDashing;

    private void Start()
    {
        DashesLeft = MaximumDashes;
    }

    public override void Enter()
    {
        if (DashesLeft <= 0)
        {
            // No dashes left: return to different state
            isComplete = true;
            return;
        }
        // Setup
        Vector2 currentVelocity = _movement.CurrentVelocity;

        SoundFXManager.Instance?.PlaySoundFXClip(_dashAudio, transform.position);
        anim.Play(_dashAnim.name);
        _movement.SetCanMove(false);
        _health.SetInvincibility(true);
        DashesLeft--;

        foreach (int layer in _ignoreLayersInt)
        {
            Physics2D.IgnoreLayerCollision(_currentLayerInt, layer, true);
        }

        _originalExcludedLayers = _physicsCollider.excludeLayers;
        _physicsCollider.excludeLayers = _originalExcludedLayers | _dashIgnoreLayers;
        Physics2D.SyncTransforms();
        _physicsCollider.enabled = false;
        _physicsCollider.enabled = true;

        // Dash
        if (_dashDirection == Vector2.zero)
        {
            // if dash direction has not yet been provided/standing still
            SetDashDirection(currentVelocity);
        }
        _movement.ApplyImpulse(_dashDirection * stats.DashStrength);

        _isDashing = true;

        ProcessDashEffect();
        if (_cooldownCoroutine != null) StopCoroutine(_cooldownCoroutine);
        _cooldownCoroutine = StartCoroutine(DashCooldown());
    }

    private void Update()
    {
        if (_isDashing)
        {
            ProcessDashEffect();
        }
    }

    /// <summary>
    /// used to apply any effects during duration of the dash. 
    /// </summary>
    protected virtual void ProcessDashEffect() { }

    /// <summary>
    /// Sets the direction for the dash.
    /// </summary>
    /// <param name="direction"> Vector value for the direction. </param>
    public void SetDashDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            if (core.transform.localScale.x > 0)
            {
                direction = Vector2.right;
            }
            else 
            {
                direction = Vector2.left;
            }
        }
        _dashDirection = direction.normalized;
    }

    public override void FixedDo()
    {
        if (time >= _dashDuration)
        {
            isComplete = true;
        }
    }

    private IEnumerator DashCooldown()
    {
        while (DashesLeft < MaximumDashes)
        {
            yield return new WaitForSeconds(stats.DashCooldown);
            DashesLeft++;
        }
    }

    public override void Exit()
    {
        _movement.SetCanMove(true);
        rb.linearVelocity = Vector2.zero;
        _health.SetInvincibility(false);
        _isDashing= false;

        foreach (int layer in _ignoreLayersInt)
        {
            Physics2D.IgnoreLayerCollision(_currentLayerInt, layer, false);
        }
        _physicsCollider.excludeLayers = _originalExcludedLayers;
    }
}
