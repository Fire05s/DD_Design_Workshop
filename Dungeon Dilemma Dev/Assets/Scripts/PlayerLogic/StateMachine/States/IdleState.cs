using UnityEngine;

/// <summary>
/// Handles the animations for the Idle State.
/// </summary>
public class IdleState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _idleAnim;

    public override void Enter()
    {
        anim.Play(_idleAnim.name);
    }
}
