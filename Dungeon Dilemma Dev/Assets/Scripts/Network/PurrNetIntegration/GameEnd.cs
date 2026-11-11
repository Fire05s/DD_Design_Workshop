using ControllerLogic;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : StateNode<GameOngoingInitData>
{
    [Header("State Component")]
    [SerializeField] private GameOngoing _gameState;

    [Header("Spawn Points")]
    [SerializeField] private Transform _bossSpawn;
    [SerializeField] private Transform _challengerSpawn;

    private Dictionary<PlayerID, SyncedStats> _stats;
    private Dictionary<PlayerID, int> _players;
    private SyncList<PlayerID> _playersReady = new();

    public SyncVar<int> ReadyCount = new();
    public SyncVar<int> PlayerCount = new();

    private GameOngoingInitData _gameData;

    private bool _gameRestarted;
    public override void Enter(GameOngoingInitData data)
    {
        base.Enter(data);

        if (!isServer) return; 
        _gameData = data;
        _stats = data.Stats;
        _players = data.Players;

        ReadyCount.value = _playersReady.Count;
        PlayerCount.value = GlobalController.Singleton.GetAllData().Count;
    }

    /// <summary>
    /// Marks the player as ready.
    /// </summary>
    /// <param name="player"> The readied player's id.</param>
    [ServerRpc]
    public void SetReadyServerRpc(PlayerID player)
    {
        if (_playersReady.Contains(player))
        {
            _playersReady.Remove(player);
        }
        else
        {
            _playersReady.Add(player);
        }

        ReadyCount.value = _playersReady.Count;
        PlayerCount.value = GlobalController.Singleton.GetAllData().Count;

        CheckAllReady();
    }

    /// <summary>
    /// Checks if player is readied.
    /// </summary>
    /// <param name="player"> ID of player being checked. </param>
    public bool IsPlayerReady(PlayerID player)
    {
        return _playersReady.Contains(player);
    }

    private void CheckAllReady()
    {
        if (!_gameRestarted && _playersReady.Count == PlayerCount)
        {
            _gameRestarted = true;
            _playersReady.Clear();
            ResetAllPlayers();
            machine.SetState(_gameState, _gameData);
        }
    }

    private void ResetAllPlayers()
    {
        GlobalController.Singleton.BossSecondPhaseActive.value = false;

        foreach (PlayerID playerID in _players.Keys)
        {
            // Reset positions
            if (_players[playerID] == 0)
            {
                SetPosition(playerID, _bossSpawn);
            }
            else
            {
                SetPosition(playerID, _challengerSpawn);
            }

            // Revive and heal
            ReviveTarget(playerID, _stats[playerID], _players[playerID] == 0);
            _stats[playerID].CurrentHealth.value = _stats[playerID].MaximumHealth.value;
        }
        _gameRestarted = false;
    }

    private void SetPosition(PlayerID id, Transform marker)
    {
        _stats[id].GetComponent<PlayerContainer>().SetCharacterPosition(id, marker.position);
    }

    [TargetRpc]
    private void ReviveTarget(PlayerID id, SyncedStats stat, bool isBoss)
    {
        if (isBoss)
        {
            stat.GetComponentInChildren<DungeonBoss>().Revive(1);
        }
        else
        {
            stat.GetComponentInChildren<Challenger>().Revive(1);
        }
    }
}
