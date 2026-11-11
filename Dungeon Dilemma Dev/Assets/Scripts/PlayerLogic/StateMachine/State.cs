using Priestess;
using PurrNet;
using UnityEngine;

/// <summary>
/// The base class containing the variables and methods for all states.
/// </summary>
public abstract class State : NetworkBehaviour
{
    // State variables
    public bool isComplete { get; protected set; }
    protected float startTime;
    public float time => Time.time - startTime;

    // Blackboard variables
    protected StateMachineCore core;
    protected Rigidbody2D rb => core.rb;
    protected NetworkAnimator anim => core.anim;
    protected SyncedStats stats => core.stats;
    protected CombatManager combat => core.combat;
    protected IDamageable health => core.health;

    // Branch state functions
    public StateMachine machine;
    public StateMachine stateParent;
    public State state => machine.state;

    /// <summary>
    /// Changes the current branching state to a new state.
    /// </summary>
    /// <param name="newState"> The new, different state that's being transitioned to. </param>
    /// <param name="forceReset"> If true, forces the state to switch without checking the current state. </param>
    protected void Set(State newState, bool forceReset = false)
    {
        machine.Set(newState, forceReset);
    }

    // State functions

    /// <summary>
    /// Called upon entering the state.
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// Called every frame in update.
    /// </summary>
    public virtual void Do() { }

    /// <summary>
    /// Called every frame in fixed update.
    /// </summary>
    public virtual void FixedDo() { }
    /// <summary>
    /// Calls Do() and the corresponding DoBranch() on the branching state.
    /// </summary>
    public virtual void DoBranch()
    {
        Do();
        state?.DoBranch();
    }
    /// <summary>
    /// Calls FixedDo() and the corresponding FixedDoBranch() on the branching state.
    /// </summary>
    public virtual void FixedDoBranch()
    {
        FixedDo();
        state?.FixedDoBranch();
    }

    /// <summary>
    /// Called upon exiting the state.
    /// </summary>
    public virtual void Exit() { }
    /// <summary>
    /// Calls Exit() and the corresponding ExitBranch() on the branching state.
    /// </summary>
    public virtual void ExitBranch()
    {
        Exit();
        state?.ExitBranch();
    }
    public void SetCore(StateMachineCore _core)
    {
        machine = new StateMachine();
        core = _core;
    }
    public void Initialize(StateMachine parentMachine)
    {
        stateParent = parentMachine;
        isComplete = false;
        startTime = Time.time;
    }
}
