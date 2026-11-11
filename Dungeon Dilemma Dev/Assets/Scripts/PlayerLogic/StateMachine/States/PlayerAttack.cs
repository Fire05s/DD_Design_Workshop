using System;
using UnityEngine;

/// <summary>
/// Handles the logic for player attack/ability states.
/// </summary>
public class PlayerAttack : State
{
    [Header("Abilities")]
    [SerializeField] private State _primary;
    [SerializeField] private State _secondary;
    [SerializeField] private DashState _dash;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private MovementController _movement;

    public event Action<float> OnPrimaryStarted; // passes cooldown time to event
    public event Action<float> OnSecondaryStarted;
    public event Action<float> OnDashStarted;

    private float nextPrimaryReady;
    private float nextSecondaryReady;
    private float nextDashReady;

    public override void Enter()
    {
        float now = Time.time;

        if (_input.PrimaryAbilityPressed && now >= nextPrimaryReady)
        {
            float cd = stats.PrimaryAbilityCooldown.value;

            machine.Set(_primary, true);

            nextPrimaryReady = now + cd + 0.01f; // slight buffer to avoid coroutine shenanigans
            OnPrimaryStarted?.Invoke(cd);
        }
        else if (_input.SecondaryAbilityPressed && now >= nextSecondaryReady)
        {
            float cd = stats.SecondaryAbilityCooldown.value;

            machine.Set(_secondary, true);

            nextSecondaryReady = now + cd + 0.01f;
            OnSecondaryStarted?.Invoke(cd);
        }
        else if (_input.DashAbilityPressed && _dash.DashesLeft > 0)
        {
            float cd = stats.DashCooldown.value;

            // _dash.SetDashDirection(_input.GetMouseDirection(core.transform.position)); // (to change back to mouse directed dashing)
            Vector2 dashDir = _movement.CurrentVelocity;
            _dash.SetDashDirection(dashDir);

            machine.Set(_dash, true);

            nextDashReady = now + cd + 0.01f;
            OnDashStarted?.Invoke(cd);
        }
    }

    public override void Do()
    {
        // _dash.SetDashDirection(_input.GetMouseDirection(core.transform.position)); // (to change back to mouse directed dashing)
        if (machine.state.isComplete)
        {
            isComplete = true;
        }
    }

    public void SetPrimary(State new_primary)
    {
        _primary = new_primary;
    }
}
