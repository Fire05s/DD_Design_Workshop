using ControllerLogic;
using PurrNet;
using Sync_Modules;
using UnityEngine;
using UnityEngine.Rendering;

public class DungeonBoss : StateMachineCore
{
    [Header("State Machine")]
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private PlayerAttack _playerAttackP1;
    [SerializeField] private PlayerAttack _playerAttackP2;
    [SerializeField] private BossDeathState _bossDeathState;

    [Header("Components")]
    [SerializeField] private SyncedInput _input;
    [SerializeField] private MovementController _movement;
    [SerializeField] private PlayerUiManager _playerUiManager;

    [Header("Stats")]
    [SerializeField] private float _raidHealthMult;

    private PlayerAttack _currentAttackPhase;

    private bool _isDead;

    private void Awake()
    {
        SetupInstances();
        machine.Set(_playerMove);
        _currentAttackPhase = _playerAttackP1;
        _playerUiManager.SetPlayerAttack(_currentAttackPhase);
        health.OnDeath += HandleDeath;
    }

    // private void Start()
    // {
    //     ProcessHealth();
    // }

    private void ProcessHealth()
    {
        int challengerNum = GlobalController.Singleton.GetAllStats().Count - 1;
        if (challengerNum <= 0) challengerNum = 1; // Default to 1 if no challengers
        SyncedStats bossStats = GetComponentInParent<SyncedStats>();
        float health = bossStats.MaximumHealth.value + challengerNum * _raidHealthMult;
        GetComponent<HealthHandler>().UpdateMaxHealth(health);
    }

    private void OnEnable()
    {
        stats.CurrentHealth.onChanged += OnPhaseCheck;
        OnPhaseCheck(stats.CurrentHealth.value);
        ProcessHealth();
        _playerUiManager.SetHud(true);
    }

    private void OnDisable() {
        stats.CurrentHealth.onChanged -= OnPhaseCheck;
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
        if (IsUsingAbility() || _isDead) return;
        if (IsAbilityInput())
        {
            machine.Set(_currentAttackPhase);
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
        return (machine.state == _currentAttackPhase && !machine.state.isComplete);
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
        _movement.SetMovement(_input.MovementVector, stats.MovementSpeed.value);
    }

    /// <summary>
    /// Checks and switchs player to phase 2 if below half hp.
    /// </summary>
    /// <param name="currentHP"> Current health value.</param>
    private void OnPhaseCheck(float currentHP)
    {
        if ((stats.CurrentHealth.value <= stats.MaximumHealth.value / 2 || 
            GlobalController.Singleton.BossSecondPhaseActive.value) && _currentAttackPhase != _playerAttackP2)
        {
            _currentAttackPhase = _playerAttackP2;
            _playerUiManager.SetPlayerAttack(_currentAttackPhase);
            _playerUiManager.SwitchIcons();
        }
        else if (!GlobalController.Singleton.BossSecondPhaseActive.value && _currentAttackPhase != _playerAttackP1)
        {
            _currentAttackPhase = _playerAttackP1;
            _playerUiManager.SetPlayerAttack(_currentAttackPhase);
            _playerUiManager.SwitchIcons();
        }
    }
    public void Revive(float healthPercent)
    {
        if (!_isDead) return;
        _isDead = false;
        _bossDeathState.RevivePlayer(stats.MaximumHealth.value * healthPercent);
    }

    private void HandleDeath()
    {
        if (_isDead) return;
        _isDead = true;
        machine.Set(_bossDeathState);
    }
}