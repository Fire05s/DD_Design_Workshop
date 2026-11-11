using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A state machine class that keeps track of the current state and manages switching between states.
/// </summary>
public class StateMachine
{
    public State state;

    /// <summary>
    /// Changes the current state to a new state.
    /// </summary>
    /// <param name="newState"> The new, different state that's being transitioned to. </param>
    /// <param name="forceReset"> If true, forces the state to switch without checking the current state. </param>
    public void Set(State newState, bool forceReset = false)
    {
        if (newState != state || forceReset)
        {
            state?.ExitBranch();
            state = newState;
            state.Initialize(this);
            state.Enter();
        }
    }

    /// <summary>
    /// Traverses through and keeps track of the current active states.
    /// </summary>
    /// <param name="list"> Current list of the active states. </param>
    /// <returns> An ordered list of the active states. </returns>
    public List<State> GetActiveStatesBranch(List<State> list = null)
    {
        if (list == null) list = new List<State>();

        if (state == null) return list;

        list.Add(state);
        return state.machine.GetActiveStatesBranch(list);
    }
}