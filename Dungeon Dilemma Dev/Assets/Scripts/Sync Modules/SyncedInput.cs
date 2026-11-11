using Avocado;
using ControllerLogic;
using PlayerLogic.BossLogic;
using PurrNet;
using PurrNet.Transports;
using Sync_Modules;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class SyncedInput : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Camera _camera;

    [Header("Inputs")]
    public Vector2 MovementVector;
    public bool PrimaryAbilityPressed;
    public bool SecondaryAbilityPressed;
    public bool ScoreboardPressed;
    public bool PausePressed;
    public bool DashAbilityPressed;
    public Vector2 MousePosition;
    public bool ShopPressed;

    [SerializeField] private InputAction _movementAxis;
    [SerializeField] private InputAction _primaryAbility;
    [SerializeField] private InputAction _secondaryAbility;
    [SerializeField] private InputAction _dash;
    [SerializeField] private InputAction _scoreboard;
    [SerializeField] private InputAction _pause;
    [SerializeField] private InputAction _mousePos;
    [SerializeField] private InputAction _shop;

    public delegate void BoolDelegate(bool val);
    public event BoolDelegate ShopValChanged;
    public event BoolDelegate PauseValChanged;
    public event BoolDelegate ScoreboardValChanged;
    private bool _combatControlsActive;

    void Start()
    {
        if (!isOwner) return;
        _movementAxis.Enable();
        _primaryAbility.Enable();
        _secondaryAbility.Enable();
        _scoreboard.Enable();
        _pause.Enable();
        _dash.Enable();
        _mousePos.Enable();
        _shop.Enable();

        ToggleAllCombatInputs(true);
        _pause.performed += OnPauseInput;
        _pause.canceled += OnPauseInput;
        _scoreboard.performed += OnScoreboardInputStarted;
        _scoreboard.canceled += OnScoreboardInputCanceled;
        _mousePos.performed += OnMouseInput;
        _shop.performed += OnShopInput;
        _shop.canceled += OnShopInput;
    }
    public void ResumeUIButton()
    {
        ToggleAllCombatInputs(true);
        GetComponent<PlayerUiManager>().HandlePauseInput(true);
    }
    public void ToggleAllCombatInputs(bool controlsActive)
    {
        _combatControlsActive = controlsActive;
        if (controlsActive)
        {
            _movementAxis.performed += OnMoveInput;
            _movementAxis.canceled += OnMoveInput;
            _primaryAbility.performed += OnPrimaryInput;
            _primaryAbility.canceled += OnPrimaryInput;
            _secondaryAbility.performed += OnSecondaryInput;
            _secondaryAbility.canceled += OnSecondaryInput;
            _dash.performed += OnDashInput;
            _dash.canceled += OnDashInput;
            DungeonBossBuildingController dbController = GetComponentInChildren<DungeonBossBuildingController>();
            BuildingCamera buildingCamera = GetComponentInChildren<BuildingCamera>();
            if (dbController != null) { dbController.controlsActive = true; }
            if (buildingCamera != null) { buildingCamera.controlsActive = true; }
        }
        else
        {
            MovementVector = Vector2.zero;
            _movementAxis.performed -= OnMoveInput;
            _movementAxis.canceled -= OnMoveInput;
            _primaryAbility.performed -= OnPrimaryInput;
            _primaryAbility.canceled -= OnPrimaryInput;
            _secondaryAbility.performed -= OnSecondaryInput;
            _secondaryAbility.canceled -= OnSecondaryInput;
            _dash.performed -= OnDashInput;
            _dash.canceled -= OnDashInput;
            PrimaryAbilityPressed = false;
            SecondaryAbilityPressed = false;
            DashAbilityPressed = false;
            DungeonBossBuildingController dbController = GetComponentInChildren<DungeonBossBuildingController>();
            BuildingCamera buildingCamera = GetComponentInChildren<BuildingCamera>();
            if (dbController != null) { dbController.controlsActive = false; }
            if (buildingCamera != null) { buildingCamera.controlsActive = false; }
        }
    }
    void OnMoveInput(InputAction.CallbackContext context)
    {
        MovementVector = context.ReadValue<Vector2>();
    }
    void OnShopInput(InputAction.CallbackContext context)
    {
        if (GlobalController.Singleton.WorldData.RoundState.value != Sync_Modules.RoundState.Setup) { return; }
        ShopPressed = context.performed;
        ShopValChanged?.Invoke(ShopPressed);
    }
    void OnPrimaryInput(InputAction.CallbackContext context)
    {
        PrimaryAbilityPressed = context.performed;
    }
    void OnSecondaryInput(InputAction.CallbackContext context)
    {
        SecondaryAbilityPressed = context.performed;
    }
    void OnDashInput(InputAction.CallbackContext context)
    {
        DashAbilityPressed = context.performed;
    }
    void OnMouseInput(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }
    void OnPauseInput(InputAction.CallbackContext context)
    {
        PausePressed = context.performed;
        PauseValChanged?.Invoke(PausePressed);
    }
    void OnScoreboardInputStarted(InputAction.CallbackContext context)
    {
        // prevent leaderboard from being opened post round
        if (GlobalController.Singleton.WorldData.RoundState.value == Sync_Modules.RoundState.PostRound) { return; }
        ScoreboardPressed = true;
        ScoreboardValChanged?.Invoke(ScoreboardPressed);
    }
    void OnScoreboardInputCanceled(InputAction.CallbackContext context)
    {
        if (GlobalController.Singleton.WorldData.RoundState.value == Sync_Modules.RoundState.PostRound) { return; }
        ScoreboardPressed = false;
        ScoreboardValChanged?.Invoke(ScoreboardPressed);
    }

    /// <summary>
    /// Retrieves the mouse's position relative to the player.
    /// </summary>
    /// <param name="playerPos"> Coordinates of current player position. </param>
    /// <returns> World coordinates of the mouse's position. </returns>
    public Vector2 GetMouseDirection(Vector3 playerPos)
    {
        if (_camera != null)
        {
            return _camera.ScreenToWorldPoint(MousePosition) - playerPos;
        }
        return Vector2.zero;
    }

    public Vector3 GetMouseWorldPosition() {
        return _camera.ScreenToWorldPoint(MousePosition);
    }

    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        
    }
}