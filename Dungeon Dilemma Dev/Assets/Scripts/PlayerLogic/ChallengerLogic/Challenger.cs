using Avocado;
using PurrNet;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;

/// <summary>
/// The base class for all challenger classes.
/// </summary>
public class Challenger : StateMachineCore
{
    [Header("State Machine")]
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private ChallengerDeathState _deathState;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private MovementController _movement;
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private PlayerUiManager _playerUiManager;

    private SyncVar<bool> _isDead = new();

    private void Start()
    {
        SetupInstances();
        machine.Set(_playerMove);
        _playerUiManager.SetPlayerAttack(_playerAttack);
        health.OnDeath += HandleDeath;
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        SetFacingDirection();
        HandleMovementLogic();
        SelectState();
        machine.state.DoBranch();
    }

    private void FixedUpdate()
    {
        machine.state.FixedDoBranch();
    }

    /// <summary>
    /// Determines the state to apply and sets the state machine to that state.
    /// </summary>
    /// Add new conditions for challenger states here.
    protected virtual void SelectState()
    {
        if (IsUsingAbility()) return;
        if (IsAbilityInput() && !_isDead.value)
        {
            machine.Set(_playerAttack);
            if (!machine.state.isComplete) return;
        }
        machine.Set(_playerMove);
    }

    /// <summary>
    /// Determines if challenger is currently using an ability.
    /// </summary>
    /// <returns> True if using ability, false otherwise. </returns>
    private bool IsUsingAbility()
    {
        return (machine.state == _playerAttack && !machine.state.isComplete);
    }

    /// <summary>
    /// Determines if challenger inputted an ability.
    /// </summary>
    /// <returns> True if player is inputting ability, false otherwise. </returns>
    private bool IsAbilityInput()
    {
        return (_input.PrimaryAbilityPressed || _input.SecondaryAbilityPressed || _input.DashAbilityPressed);
    }

    /// <summary>
    /// Moves the character based on the player's input.
    /// </summary>
    // Move logic handled here since player needs to move while attacking.
    private void HandleMovementLogic()
    {
        if (stats == null) Debug.LogError("Stats ScriptableObject is missing!");
        _movement.SetMovement(_input.MovementVector, stats.MovementSpeed.value, !_isDead.value);
    }

    [ServerRpc]
    public void Revive(float healthPercent)
    {
        if (!_isDead.value) return;
        _isDead.value = false;
        _deathState.RevivePlayer(stats.MaximumHealth.value * healthPercent);
    }

    private void HandleDeath()
    {
        if (!isServer) { return; }
        if (_isDead.value) return;
        _isDead.value = true;
        machine.Set(_deathState);
    }
}
