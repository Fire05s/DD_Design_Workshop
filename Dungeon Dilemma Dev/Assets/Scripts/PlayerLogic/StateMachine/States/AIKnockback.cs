using UnityEngine;
using UnityEngine.AI;

public class AIKnockback : State
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
    [Header("Knockback Values")]
    [SerializeField] private float _knockbackDuration = 0.3f; // seconds
    [SerializeField] private float _knockbackForce = 5f;

    private Vector2 _direction;

    /// <summary>
    /// Call this before setting the state
    /// </summary>
    public void SetKnockback(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    public override void Enter()
    {
        // Disable NavMesh so it doesn't fight knockback
        _agent.enabled = false;
        rb.linearVelocity = _direction * _knockbackForce;
    }

    public override void Do()
    {
        // stop early if velocity is near zero
        if (rb.linearVelocity.sqrMagnitude < 0.01f || time > _knockbackDuration)
        {
            isComplete = true;
        }
    }

    public override void Exit()
    {
        _agent.enabled = true;
        rb.linearVelocity = Vector2.zero;
    }
}
