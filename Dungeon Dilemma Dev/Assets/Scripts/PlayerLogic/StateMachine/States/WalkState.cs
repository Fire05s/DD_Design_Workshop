using UnityEngine;

/// <summary>
/// Handles the animations for the Move State.
/// </summary>
public class WalkState : State
{
    [Header("Animation")]
    [SerializeField] private AnimationClip _walkAnim;

    public override void Enter()
    {
        anim.Play(_walkAnim.name);
    }
}
