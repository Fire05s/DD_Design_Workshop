using UnityEngine;

/// <summary>
/// A state that handles the logic and branching for player movement.
/// </summary>
public class PlayerMove : State
{
    [Header("States")]
    [SerializeField] private IdleState _idle;
    [SerializeField] private WalkState _walk;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private Camera _camera;

    public override void Do()
    {
        // Select corresponding movement animation
        if (_input.MovementVector.magnitude > 0.1f)
        {
            machine.Set(_walk, true);
        }
        else
        {
            machine.Set(_idle, true);
        }
    }
}
