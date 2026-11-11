using Priestess;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for all entities with a state machine.
/// </summary>
public abstract class StateMachineCore : NetworkBehaviour
{
    [Header("Core Components")]
    public Rigidbody2D rb;
    public NetworkAnimator anim;
    public IDamageable health;
    public SyncedStats stats;
    public CombatManager combat;
    [Header("Debug")]
    [SerializeField] private bool _debug;

    private float _facingDirection;

    protected StateMachine machine;

    /// <summary>
    /// Creates a state machine and assigns the core for each state.
    /// </summary>
    public void SetupInstances()
    {
        if (stats == null) stats = GetComponentInParent<SyncedStats>();
        if (combat == null) combat = GetComponentInParent<CombatManager>();
        health = GetComponent<IDamageable>();

        machine = new StateMachine();

        State[] allChildStates = GetComponentsInChildren<State>();
        foreach (State state in allChildStates)
        {
            state.SetCore(this);
        }
    }

    /// <summary>
    /// Flips the gameobject based on the movement direction.
    /// </summary>
    // Add to Update in inherited classes
    protected void SetFacingDirection()
    {
        // Returns -1, 0, or 1 for horizontal facing direction
        _facingDirection = (Mathf.Abs(rb.linearVelocityX) > 0.1f) ? Mathf.Sign(rb.linearVelocityX) : 0;

        if (_facingDirection == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _facingDirection;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
        if (Application.isPlaying && machine.state != null && _debug)
        {
            List<State> states = machine.GetActiveStatesBranch();
            UnityEditor.Handles.Label(transform.position, "Active States: " + string.Join(" > ", states));
        }
        #endif
    }
}
