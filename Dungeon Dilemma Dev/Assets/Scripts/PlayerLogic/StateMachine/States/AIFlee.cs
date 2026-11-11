using UnityEngine;
using UnityEngine.AI;

public class AIFlee : State
{
    [SerializeField] private float _fleeDistance = 4f; // How far the enemy moves back
    [SerializeField] private NavMeshAgent _agent;

    private Transform _target;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override void Enter()
    {
        if (_target == null) return;

        Vector3 dir = (transform.position - _target.position).normalized;
        Vector3 fleePosition = transform.position + dir * _fleeDistance;

        _agent.isStopped = false;
        _agent.SetDestination(fleePosition);
    }

    public override void Do()
    {
        if (!_agent.pathPending &&
            _agent.remainingDistance <= _agent.stoppingDistance)
        {
            isComplete = true;
        }
    }
}
