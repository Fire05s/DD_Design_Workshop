using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// A state that handles the logic and branching for AI movement.
/// </summary>
public class AIMove : State
{
    [Header("States")]
    [SerializeField] private IdleState _idle;
    [SerializeField] private WalkState _walk; //aka Chase State

    [Header("Components")]
    [SerializeField] private UnityEngine.AI.NavMeshAgent _agent;

    private Transform _target;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override void Enter()
    {
        if (_target == null)
        {
            machine.Set(_idle, true);
            return;
        }

        _agent.isStopped = false;
        _agent.SetDestination(_target.position);
    }

    /// <summary>
    /// Continuously move toward the destination and
    /// enter idle upon reaching it.
    /// </summary>
    public override void Do()
    {
        // Safety checks for disabled agents
        if (!_agent.enabled)
        {
            machine.Set(_idle, true);
            return;
        }

        if (_target == null)
        {
            machine.Set(_idle, true);
            return;
        }

        // Continuously update destination
        _agent.SetDestination(_target.position);

        bool isMoving =
            _agent.hasPath &&
            !_agent.isStopped &&
            _agent.remainingDistance > _agent.stoppingDistance &&
            _agent.velocity.sqrMagnitude > 0.01f;

        if (isMoving)
            machine.Set(_walk, true);
        else
            machine.Set(_idle, true);
    }

    public override void Exit()
    {
        _agent.ResetPath();
        _agent.SetDestination(core.transform.position);
    }
}
