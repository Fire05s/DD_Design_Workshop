using System.Collections;
using System.Collections.Generic;
using ControllerLogic;
using PlayerLogic.BossLogic;
using PurrNet;
using PurrNet.StateMachine;
using Shop;
using Sync_Modules;
using UnityEngine;

public struct GameOngoingInitData {
    public Dictionary<PlayerID, int> Players;
    public Dictionary<PlayerID, SyncedStats> Stats;
}

public class GameOngoing : StateNode<GameOngoingInitData> {
    [Header("Phase Duration")]
    [SerializeField] private float _buildDuration = 120f;
    [SerializeField] private float _raidDuration = 120f;

    [Header("Gold Amount")]
    [SerializeField] private int _loserGoldBonus = 1000;
    [SerializeField] private int _challengerGoldBaseIncrement = 1000;
    [SerializeField] private int _dbGoldBaseIncrement = 2000;

    private Dictionary<PlayerID, SyncedStats> _stats;
    private Dictionary<PlayerID, int> _players;
    private List<PlayerID> _challengers;
    private GameObject _startMarker;
    private GameObject _bossStart;
    private GameObject _raidEntrance;
    private PlayerID _boss;
    private GameObject _bossObject;

    private GameOngoingInitData _gameData;
    private bool _isInitialized = false;
    private bool _roundEnded = false;
    private bool _bossPhaseActivated = false;
    private float _nextOvertimeDamageTick;

    private void Start()
    {
        _startMarker = GameObject.Find("Challenger SpawnPoint");
        _bossStart = GameObject.Find("Boss Spawnpoint");
        _raidEntrance = GameObject.Find("Raid Start Point");
    }

    public override void Enter(GameOngoingInitData data) {
        base.Enter(data);

        _gameData = data;
        _stats = data.Stats;
        _players = data.Players;

        _challengers = new List<PlayerID>();
        
        foreach (PlayerID playerID in _players.Keys) {
            if (_players[playerID] == 0) {
                _boss = playerID;
            } else {
                _challengers.Add(playerID);
            }
        }

        _bossObject = _stats[_boss].gameObject;
        _isInitialized = true;
        if (!isServer) return;

        StartRound();
        _roundEnded = false;
    }

    private void Update()
    {
        if (!isServer) return;

        // boss fight handling
        if (!_bossPhaseActivated && GlobalController.Singleton.WorldData.RoundState.value == RoundState.BossFight)
        {
            StartBossFight();
        }
        // overtime damage ticking
        else if (GlobalController.Singleton.WorldData.RoundState.value == RoundState.RaidOvertime && GlobalController.Singleton.GetGlobalTime() > _nextOvertimeDamageTick)
        {
            List<SyncedStats> allStats = GlobalController.Singleton.GetAllStats();
            foreach (SyncedStats stat in allStats)
            {
                if (stat.CompareTag("Challenger"))
                {
                    stat.GetComponentInChildren<HealthHandler>().ApplyStatusEffectServerRpc(Priestess.StatusEffectType.Withering, null);
                }
            }
            _nextOvertimeDamageTick += 1f;
        }
        else if (GlobalController.Singleton.WorldData.RoundState.value == RoundState.PostRound && !_roundEnded)
        {
            _roundEnded = true;
            EndRound();
        }

        if (GlobalController.Singleton.GetGlobalTime() > 0 || !_isInitialized) return;

        if (GlobalController.Singleton.WorldData.RoundState.value == RoundState.Setup)
        {
            StartRaid();
        }
        else if (GlobalController.Singleton.WorldData.RoundState.value == RoundState.Raid)
        {
            StartOvertime();
        }
    }

    private void StartRound() {
        foreach (PlayerID playerID in _players.Keys) {

            GrantGold(playerID);

            // Update all UIs
            PlayerContainer container = _stats[playerID].GetComponent<PlayerContainer>();
            ChangeLocalGamePhaseScreens(container, RoundState.Setup);

            if (playerID == _boss) continue;
            
            _stats[playerID].gameObject.transform.position = _startMarker.transform.position +
                                                             new Vector3(Random.Range(0, 5), Random.Range(0, 5), 0);
        }

        GlobalController.Singleton.WorldData.Round.value++;
        GlobalController.Singleton.BossSecondPhaseActive.value = false;
        GlobalController.Singleton.RoundInProgress.value = true;
        GlobalController.Singleton.WorldData.RoundState.value = RoundState.Setup;
        GlobalController.Singleton.SetGlobalTime(_buildDuration);
        _bossPhaseActivated = false;
        PlaySetupMusic();

        ToggleBossStateTargetRpc(_boss, true, false);
        ToggleShopRpc(true);
    }

    private void StartRaid()
    {
        foreach (PlayerID playerID in _players.Keys)
        {
            PlayerContainer container = _stats[playerID].GetComponent<PlayerContainer>();
            ChangeLocalGamePhaseScreens(container, RoundState.Raid);

            if (playerID == _boss) continue;

            // move challengers to spawn
            Vector3 raidStartPos = Vector3.zero;
            if (_raidEntrance) raidStartPos = _raidEntrance.transform.position;
            container.SetCharacterPosition(_stats[playerID].owner.Value, raidStartPos + new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), 0));
        }

        GlobalController.Singleton.WorldData.RoundState.value = RoundState.Raid;
        GlobalController.Singleton.SetGlobalTime(_raidDuration);
        PlayRaidMusic();

        ToggleBossStateTargetRpc(_boss, false, false);
        ToggleShopRpc(false);
    }

    private void StartOvertime()
    {
        GlobalController.Singleton.WorldData.RoundState.value = RoundState.RaidOvertime;
        _nextOvertimeDamageTick = GlobalController.Singleton.GetGlobalTime() + 1f;
    }

    private void StartBossFight()
    {
        GlobalController.Singleton.WorldData.RoundState.value = RoundState.BossFight;
        PlayBossMusic();
        ToggleBossStateTargetRpc(_boss, false, true);
        PlayerContainer bossContainer = _bossObject.GetComponent<PlayerContainer>();
        bossContainer.SetCharacterPosition(_boss, _bossStart.transform.position);
        ChangeLocalGamePhaseScreens(bossContainer, RoundState.BossFight);
        _bossPhaseActivated = true;
    }

    private void EndRound()
    {
        foreach (PlayerID playerID in _players.Keys)
        {
            // Update UI
            PlayerContainer container = _stats[playerID].GetComponent<PlayerContainer>();
            ChangeLocalGamePhaseScreens(container, RoundState.PostRound);
            machine.Next(_gameData);
        }

        ToggleBuildingTargetRpc(_boss, _bossObject, false);
    }

    // HELPER FUNCTIONS

    [TargetRpc]
    private void ToggleBossStateTargetRpc(PlayerID player, bool buildingEnabled, bool fightEnabled) {
        ToggleBossState(buildingEnabled, fightEnabled);
    }

    [TargetRpc]
    private void ToggleBuildingTargetRpc(PlayerID boss, GameObject bossObj, bool state)
    {
        bossObj.GetComponentInChildren<DungeonBossBuildingController>().enabled = false;
    }

    [ObserversRpc(runLocally:true)]
    private void ToggleShopRpc(bool shopEnabled)
    {
        if (shopEnabled)
        {
            StartCoroutine(ShopEntryPoint.localShopInstance.StartNewSetupSession());
        }
        else
        {
            ShopEntryPoint.localShopInstance.EndSetupSession();
        }
    }
    [ObserversRpc(runLocally:true)]
    private void ChangeLocalGamePhaseScreens(PlayerContainer playerContainer, RoundState gameState)
    {
        playerContainer.GetComponentInChildren<PlayerUiManager>().ChangeGamePhaseScreens(gameState);
    }

    private void ToggleBossState(bool buildingEnabled, bool fightEnabled)
    {
        _bossObject.GetComponentInChildren<DungeonBoss>().enabled = fightEnabled;
        _bossObject.GetComponentInChildren<Collider2D>().enabled = fightEnabled;
        _bossObject.GetComponentInChildren<SpriteRenderer>().enabled = fightEnabled;

        if (!buildingEnabled) DungeonBossBuildingController.Singleton.DisableBuilding();
        _bossObject.GetComponentInChildren<DungeonBossBuildingController>().enabled = !fightEnabled;
    }

    [ObserversRpc(runLocally:true)]
    private void PlaySetupMusic()
    {
        MusicManager.Instance?.PlayMusic("Setup", true);
    }

    [ObserversRpc(runLocally:true)]
    private void PlayRaidMusic()
    {
        MusicManager.Instance?.PlayMusic("Raid", true);
    }

    [ObserversRpc(runLocally:true)]
    private void PlayBossMusic()
    {
        MusicManager.Instance?.PlayMusic("Boss", true);
    }

    private void GrantGold(PlayerID player)
    {
        string prevWinner = GlobalController.Singleton.GetWorldData().PrevWinner;

        if (prevWinner == "Boss")
        {
            if (player == _boss)
            {
                _stats[player].ModifyGold(_dbGoldBaseIncrement);
            }
            else
            {
                _stats[player].ModifyGold(_challengerGoldBaseIncrement + _loserGoldBonus);
            }
        }
        else if (prevWinner == "Challengers")
        {
            if (player == _boss)
            {
                _stats[player].ModifyGold(_dbGoldBaseIncrement + _loserGoldBonus);
            }
            else
            {
                _stats[player].ModifyGold(_challengerGoldBaseIncrement);
            }
        }
        else
        {
            if (player == _boss)
            {
                _stats[player].ModifyGold(_dbGoldBaseIncrement);
            }
            else
            {
                _stats[player].ModifyGold(_challengerGoldBaseIncrement);
            }
        }
    }

    public void SetRaidDuration(float amount)
    {
        _raidDuration = amount;
    }
}
