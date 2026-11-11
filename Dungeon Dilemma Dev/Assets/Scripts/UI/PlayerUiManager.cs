using Avocado;
using ControllerLogic;
using Priestess;
using PurrNet;
using Shop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerUiManager : NetworkBehaviour
{
    // private string ShopViewKey = "ShopView";
    public enum PlayerUIState {Hud, Pause, Shop, Scoreboard, None};
    public PlayerUIState state;
    private PlayerUIState prev;
    public SyncedInput input;
    [System.NonSerialized] public bool canOpenShop;
    

    [Header("HUD Screen")]
    [SerializeField] private string _playerSetupHUD;
    [SerializeField] private string _playerRaidHUD;
    [SerializeField] private string _playerBossPhaseHUD;
    [SerializeField] private string _playerEndPhaseHUD;
    [SerializeField] private bool _openHudOnStart = true;
    private string _currentPhaseHUD;

    [Header("Icons")]
    [SerializeField] private List<Sprite> _initialIcons;
    [Header("Secondary Icon (used by some abilities)")]
    [SerializeField] private List<Sprite> _secondaryIcons;

    public event Action<List<Sprite>> OnIconChanged;
    public event Action<PlayerAttack> OnAttackChanged;

    private List<Sprite> _currentIcons;
    private PlayerAttack _currentAttack;
    private bool _isSwitchedIcons;

    private void Start()
    {
        if (!isOwner) {this.enabled = false;}
        input = GetComponent<SyncedInput>();
        _currentPhaseHUD = _playerSetupHUD;
        // ShopViewKey += owner.Value;
        StartCoroutine(WaitForStats());
    }

    private IEnumerator WaitForStats()
    {
        while (!GlobalController.Singleton.IsStatsReady()) yield return null;
        Initialize();
    }

    private void Initialize()
    {
        prev = PlayerUIState.None;
        _currentIcons = _initialIcons;
        if (isOwner && _openHudOnStart) {state = PlayerUIState.Hud; UpdateScreens();}
        GetEventHooks();

        if (!isOwner) return;
        ShopEntryPoint.localShopInstance.InitiateShop(this.gameObject.GetComponentInParent<SyncedStats>(), this.gameObject.GetComponentInParent<SyncedInventory>(), this.gameObject.GetComponentInParent<CombatManager>(), this.gameObject.CompareTag("Boss"));
    }

    private void OnEnable()
    {
        
    }

    public List<Sprite> GetIcons()
    {
        return _currentIcons;
    }

    public void UpdateIcons(List<Sprite> newIcons)
    {
        for (int i = 0; i < 3; i++)
        {
            _currentIcons[i] = newIcons[i];
        }
        OnIconChanged?.Invoke(_currentIcons);
    }

    public void SwitchIcons()
    {
        if (!_isSwitchedIcons)
        {
            _currentIcons = _secondaryIcons;
        }
        else
        {
            _currentIcons = _initialIcons;
        }
        _isSwitchedIcons = !_isSwitchedIcons;
        OnIconChanged?.Invoke(_currentIcons);
    }
    public void SetPlayerAttack(PlayerAttack newAttack)
    {
        if (_currentAttack == newAttack) return;

        _currentAttack = newAttack;
        OnAttackChanged?.Invoke(_currentAttack);
    }
    public PlayerAttack GetCurrentAttack()
    {
        return _currentAttack;
    }

    private void GetEventHooks()
    {
        input.ShopValChanged += HandleShopInput;
        input.PauseValChanged += HandlePauseInput;
        input.ScoreboardValChanged += HandleScoreboardInput;
    }
    private void UpdateScreens()
    {
        switch(prev)
        {
            case PlayerUIState.Hud: SetHud(false); break;
            case PlayerUIState.Pause: SetPause(false); break;
            case PlayerUIState.Shop: SetShop(false); break;
            case PlayerUIState.Scoreboard: SetScoreboard(false); break;
        }
        switch(state)
        {
            case PlayerUIState.Hud: SetHud(true); break;
            case PlayerUIState.Pause: SetPause(true); break;
            case PlayerUIState.Shop: SetShop(true); break;
            case PlayerUIState.Scoreboard: SetScoreboard(true); break;
        }
    }
    public void ChangeGamePhaseScreens(Sync_Modules.RoundState gameState)
    {
        string oldPhaseHUD = _currentPhaseHUD;
        if (gameState == Sync_Modules.RoundState.Setup)
        {
            _currentPhaseHUD = _playerSetupHUD;
        }
        else if (gameState == Sync_Modules.RoundState.Raid)
        {
            _currentPhaseHUD = _playerRaidHUD;
        }
        else if (gameState == Sync_Modules.RoundState.BossFight)
        {
            _currentPhaseHUD = _playerBossPhaseHUD;
        }

        if (gameState == Sync_Modules.RoundState.PostRound)
        {
            _currentPhaseHUD = _playerEndPhaseHUD;
            input.ToggleAllCombatInputs(false);
        }
        else
        {
            input.ToggleAllCombatInputs(true);
        }

        prev = state;
        state = PlayerUIState.Hud;
        if (oldPhaseHUD != null && (oldPhaseHUD != _currentPhaseHUD || prev != state))
        {
            UiManager.CloseScreen(oldPhaseHUD);
            UpdateScreens();
        }
    }
    private void SetShop(bool val)
    {
        if (!isOwner) { return; }
        if (val) {
            // int shopView = PlayerPrefs.GetInt(ShopViewKey + owner.Value, 2); // defaulting to Equipment Shop view
            UiManager.OpenScreen("EquipmentShopUI", owner.Value);
            input.ToggleAllCombatInputs(false);
            // if (shopView == 2)
            // {
            //     UiManager.OpenScreen("EquipmentShopUI", owner.Value);
            // }
            // else if (shopView == 1)
            // {
            //     UiManager.OpenScreen("InventoryShopUI", owner.Value);
            // }
            // else if (shopView == 3)
            // {
            //     UiManager.OpenScreen("UpgradeShopUI", owner.Value);
            // }
        }
        else {
            UiManager.CloseScreen("EquipmentShopUI"); 
            UiManager.CloseScreen("UpgradeShopUI");
            UiManager.CloseScreen("InventoryShopUI");
            input.ToggleAllCombatInputs(true);
        }
    }
    public void SetHud(bool val)
    {
        if (!isOwner) { return; }
        if (val) { UiManager.OpenScreen(_currentPhaseHUD, owner.Value); }
        else { UiManager.CloseScreen(_currentPhaseHUD); }
    }
    private void SetPause(bool val)
    {
        if (!isOwner) { return; }
        if (val) { UiManager.OpenScreen("PauseUI", owner.Value); input.ToggleAllCombatInputs(false); }
        else { UiManager.CloseScreen("PauseUI"); UiManager.CloseScreen("InGameSettingsUI"); input.ToggleAllCombatInputs(true); }
    }
    private void SetScoreboard(bool val)
    {
        if (!isOwner) { return; }
        if (val) { UiManager.OpenScreen("ScoreboardUI", owner.Value); input.ToggleAllCombatInputs(false); }
        else { UiManager.CloseScreen("ScoreboardUI"); input.ToggleAllCombatInputs(true); }
    }
    private void HandleShopInput(bool val)
    {
        if (!val) { return; }
        if (val && state==PlayerUIState.Shop) { state=PlayerUIState.Hud; prev=PlayerUIState.Shop; }
        else if (val && state!=PlayerUIState.Shop) { prev = state; state=PlayerUIState.Shop; }
        UpdateScreens();
    }
    public void HandlePauseInput(bool val)
    {
        if (!val) { return; }
        if (val && state==PlayerUIState.Pause) { state=PlayerUIState.Hud; prev=PlayerUIState.Pause; }
        else if (val && state!=PlayerUIState.Pause) { prev = state; state=PlayerUIState.Pause; }
        UpdateScreens();
    }
    private void HandleScoreboardInput(bool val)
    {
        if (val && state!=PlayerUIState.Scoreboard) { prev = state; state = PlayerUIState.Scoreboard; }
        else if (!val && state==PlayerUIState.Scoreboard) { state = PlayerUIState.Hud; prev = PlayerUIState.Scoreboard; }
        UpdateScreens();
    }
}
