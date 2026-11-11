using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles the logic for player attack/ability states.
/// </summary>
public class AIAttack : State
{
    [Header("Abilities")]
    [SerializeField] private List<State> _attacks;

    private Vector2 _targetPos;

    public override void Enter()
    {
        if (_attacks[0] is ITargettedAttack targetAttack) targetAttack.SetAttackDirection(_targetPos - new Vector2(transform.position.x, transform.position.y));
        machine.Set(_attacks[0], true);
    }

    public void SetTarget(Vector2 targetPos)
    {
        _targetPos = targetPos;
    }

    public override void Do()
    {
        if (machine.state.isComplete)
        {
            isComplete = true;
        }
    }
}
