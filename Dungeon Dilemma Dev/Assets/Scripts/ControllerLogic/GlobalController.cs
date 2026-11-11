using Items;
using PurrNet;
using Sync_Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ControllerLogic {
    [RequireComponent(typeof(SyncedWorld))]
    public class GlobalController: NetworkBehaviour {
        public static GlobalController Singleton;
        public SyncedWorld WorldData => _world;
        public SyncVar<bool> BossSecondPhaseActive = new();
        public SyncVar<bool> RoundInProgress = new();

        [Header("Training Dummy")]
        public SyncVar<int> TDLevel = new(1);
        public int TDBasePayout = 50;
        public int TDPayoutIncrement = 50;
        public TDHealthHandler TDHealthHandler;

        [Header("Debug")]
        public bool IsDebugLevel = false;

        private SyncedWorld _world;
        private readonly SyncDictionary<PlayerID, PlayerData> _playersData = new();
        private readonly SyncDictionary<PlayerID, SyncedStats> _statsMap = new();
        private SyncVar<float> _globalTime = new();
        private bool _isTimeStarted;
        private SyncVar<bool> _isStatsReady = new();
        private SyncVar<bool> _levelingTrainingDummy = new();

        [SerializeField] private GameOngoing gameOngoing;

        private void Start() {
            _world = GetComponent<SyncedWorld>();
            Singleton = this;
        }

        protected override void OnSpawned() {
            base.OnSpawned();
            TDHealthHandler = FindFirstObjectByType<TDHealthHandler>();
            TDHealthHandler.OnDeath += LevelTrainingDummy;
            if (isServer) {
                _world.Round.value = 0;
                _world.RoundState.value = RoundState.Setup;
            }
        }

        protected override void OnDespawned()
        {
            base.OnDespawned();
            TDHealthHandler.OnDeath -= LevelTrainingDummy;
        }

        private void Update()
        {
            if (!isServer) return;
            
            if (_world.RoundState == RoundState.RaidOvertime)
            {
                _globalTime.value += Time.deltaTime;
            }
            else if (_isTimeStarted && _globalTime.value > 0)
            {
                _globalTime.value -= Time.deltaTime;
            }
            else
            {
                _isTimeStarted = false;
                _globalTime.value = 0;
            }
        }

        [ServerRpc]
        public void SetRaidDuration(float amount)
        {
            gameOngoing.SetRaidDuration(amount);
        }

        [ServerRpc]
        public void RegisterPlayer(SyncedStats stats) {
            PlayerData data = new PlayerData(stats);
            _statsMap.Add(stats.owner.Value, stats);
            _playersData[stats.owner.Value] = data;
        }

        [ServerRpc]
        public void UpdatePlayerScoreData(PlayerID player, ScoreType type, float value=0)
        {
            if (player == null) return;
            _playersData[player].UpdateScore(type, value);
        }

        /// <summary>
        /// Notifies global controller that all player stats are initialized
        /// </summary>
        public void SetStatsReady()
        {
            _isStatsReady.value = true;
        }

        /// <summary>
        /// Checks if all players' sync stats are ready.
        /// Call this function to check when there are desync issues.
        /// </summary>
        /// <returns> True if all players are initialized, false otherwise. </returns>
        public bool IsStatsReady()
        {
            return _isStatsReady.value;
        }

        /// <summary>
        /// Retrieves the specified player's sync stats.
        /// </summary>
        /// <param name="plrId"> The specific player's id. </param>
        /// <returns> The player's sync stats component. </returns>
        public SyncedStats GetPlayer(PlayerID plrId) {
            return _statsMap[plrId];
        }

        /// <summary>
        /// Retrieves all players' sync stats to sync data between clients
        /// </summary>
        /// <returns> List of all sync stats. </returns>
        public List<SyncedStats> GetAllStats()
        {
            return _statsMap.Values.ToList();
        }

        /// <summary>
        /// Retrieves all players' data to display scoreboard info
        /// </summary>
        /// <returns> List of all players' data and scores. </returns>
        public List<PlayerData> GetAllData()
        {
            return _playersData.Values.ToList();
        }

        /// <summary>
        /// Allows players to retrieve global time
        /// </summary>
        /// <returns> The current raid time. </returns>
        public float GetGlobalTime()
        {
            return _globalTime.value;
        }

        /// <summary>
        /// Allows other components to start and stop the global time on the server.
        /// </summary>
        [ServerRpc]
        public void SetGlobalTime(float time)
        {
            _globalTime.value = time;
            _isTimeStarted = true;
        }

        public SyncedWorld GetWorldData()
        {
            return _world;
        }

        [ServerRpc]
        public void ActivateBossFight()
        {
            _world.RoundState.value = RoundState.BossFight;
        }

        [ServerRpc]
        public void CheckBossPhase(SyncedStats bossStats)
        {
            if (!BossSecondPhaseActive)
            {
                BossSecondPhaseActive.value = bossStats.CurrentHealth.value <= bossStats.MaximumHealth.value / 2;
            }
        }

        [ServerRpc]
        public void CheckChallengerWipe()
        {
            if (!RoundInProgress.value) { return; }
            // check if all challengers are dead
            foreach (SyncedStats s in _statsMap.Values.ToList())
            {
                Challenger c = s.GetComponentInChildren<Challenger>();
                if (c != null && !c.CompareTag("Dead"))
                {
                    return;
                }
            }

            // passed the check that all challengers (skipping dungeon boss) are dead
            EndRound("Boss");
        }

        [ServerRpc]
        public void EndRound(string teamWon)
        {
            RoundInProgress.value = false;
            _world.PrevWinner.value = teamWon;
            if (teamWon == "Challengers")
            {
                _world.BossLives.value--;
            }
            else if (teamWon == "Boss")
            {
                _world.ChallengerLives.value--;
            }
            else
            {
                Debug.LogWarning($"'{teamWon}' is an invalid winner, set it to 'Boss' or 'Challengers'");
            }
            _world.RoundState.value = RoundState.PostRound;
        }

        [ServerRpc]
        public void ResetRound()
        {
            _world.RoundState.value = RoundState.Setup;
        }

        [ServerRpc]
        public void RequestDisconnectServerRpc(PlayerID leavingPlayer)
        {
            if (NetworkManager.main != null)
            {
                NetworkManager.main.StopServer();
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainLobby");
        }

        [ServerRpc]
        private void LevelTrainingDummy()
        {
            if (!_levelingTrainingDummy.value)
            {
                _levelingTrainingDummy.value = true;
                int goldPayout = TDBasePayout + TDPayoutIncrement * (TDLevel.value - 1);
                foreach (SyncedStats stat in GetAllStats())
                {
                    if (stat.Name != "Yew")
                    {
                        // split payout gold (= Base * 2^(Level - 1)) among all Challengers
                        stat.ModifyGold(goldPayout);
                        stat.GetComponentInChildren<HealthHandler>().SpawnGoldTextRpc(goldPayout);
                    }
                }
                StartCoroutine(RespawnTimer());
            }
        }

        private IEnumerator RespawnTimer()
        {
            TDHealthHandler.SetInvincibility(true);
            TDHealthHandler.UpdateMaxHealth(TDHealthHandler.Stats.MaximumHealth.value * 2);
            TDHealthHandler.ApplyStatusEffectServerRpc(Priestess.StatusEffectType.TDRegeneration, null);
            yield return new WaitForSeconds(5f);
            TDHealthHandler.HealServerRpc(TDHealthHandler.Stats.MaximumHealth.value);
            TDHealthHandler.SetInvincibility(false);
            TDLevel.value++;
            _levelingTrainingDummy.value = false;
        }
    }
}