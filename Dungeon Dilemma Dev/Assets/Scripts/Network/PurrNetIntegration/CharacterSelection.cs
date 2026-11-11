using Avocado;
using ControllerLogic;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CharacterSelection : StateNode
{
    [Header("Available Characters")]
    [SerializeField] private List<GameObject> _characterPrefabs; // Dungeon boss: key = 0, Challengers: key >= 1
    [SerializeField] private List<StatsData> _charactersData; // MUST MATCH ORDER OF CHARACTER PREFABS

    [Header("Spawn Points")]
    [SerializeField] private Transform _bossSpawn;
    [SerializeField] private Transform _challengerSpawn;

    [Header("Camera")]
    [SerializeField] private Camera _initialCam;

    [Header("UI")]
    [SerializeField] private GameObject _loadingUI;

    private Dictionary<PlayerID, PlayerContainer> _playerData;
    private SyncDictionary<PlayerID, int> _selection = new();
    private Dictionary<PlayerID, SyncedStats> _stats = new();
    private SyncList<int> _takenCharacters = new();
    private SyncList<PlayerID> _playersReady = new();
    private bool _playersSpawned = false;
    private HashSet<PlayerID> _spawnConfirmed = new();

    public void SetPlayers(Dictionary<PlayerID, PlayerContainer> containers)
    {
        _playerData = containers;
    }

    /// <summary>
    /// Check if character has already been selected on the server
    /// </summary>
    /// <param name="index"> Index value of the chosen character. </param>
    /// <returns> True if character is taken, false otherwise. </returns>
    public bool IsCharacterTaken(int index)
    {
        return _takenCharacters.Contains(index);
    }

    /// <summary>
    /// Stores the character selection choice.
    /// </summary>
    /// <param name="id"> The player's id. </param>
    /// <param name="index"> The chosen character index. </param>
    [ServerRpc]
    public void SetCharacterServerRpc(PlayerID player, int index)
    {
        if (_takenCharacters.Contains(index) || _playersReady.Contains(player))
        {
            return;
        }
        if (_selection.ContainsKey(player))
        {
            _takenCharacters.Remove(_selection[player]);
        }
        _selection[player] = index;
        _takenCharacters.Add(index);
    } 

    /// <summary>
    /// Marks the player as ready.
    /// </summary>
    /// <param name="player"> The readied player's id.</param>
    [ServerRpc]
    public void SetReadyServerRpc(PlayerID player)
    {
        if (!_selection.Keys.Contains(player)) return;
        if (_playersReady.Contains(player))
        {
            _playersReady.Remove(player);
        }
        else
        {
            _playersReady.Add(player);
        }
    }

    /// <summary>
    /// Checks if player is readied.
    /// </summary>
    /// <param name="player"> ID of player being checked. </param>
    public bool IsPlayerReady(PlayerID player)
    {
        return _playersReady.Contains(player);
    }

    public SyncDictionary<PlayerID, int> GetPlayersSelection()
    {
        return _selection;
    }

    public void NotifySpawnConfirmed(PlayerID player)
    {
        if (_spawnConfirmed.Contains(player))
            return;

        _spawnConfirmed.Add(player);
        if (_spawnConfirmed.Count == _playerData.Count)
        {
            GlobalController.Singleton.SetStatsReady();
            machine.Next(new GameOngoingInitData()
            {
                Players = _selection.ToDictionary(),
                Stats = _stats
            });
        }
    }

    private IEnumerator SetStatsReady()
    {
        yield return new WaitForSeconds(4);

        GlobalController.Singleton.SetStatsReady();
        machine.Next(new GameOngoingInitData()
        {
            Players = _selection.ToDictionary(),
            Stats = _stats
        });
    }

    public override void StateUpdate(bool asServer)
    {
        if (!asServer) return;

        if (!_playersSpawned && _playersReady.Count == _playerData.Count &&
            _takenCharacters.Contains(0))
        {
            _playersSpawned = true;
            SpawnPlayers();
        }
    }

    /// <summary>
    /// Spawns in the player as their selected character at the corresponding spawn point.
    /// </summary>
    private void SpawnPlayers()
    {
        _stats.Clear();
        foreach (KeyValuePair<PlayerID, int> entry in _selection)
        {
            GameObject character = _characterPrefabs[entry.Value];
            StatsData stats = _charactersData[entry.Value];
            PlayerContainer container = _playerData[entry.Key];

            Transform _spawnPoint = _challengerSpawn;
            string tag = "Challenger";
            if (entry.Value == 0)
            {
                // Boss character
                _spawnPoint = _bossSpawn;
                tag = "Boss";
            }

            // Set up stats
            SyncedStats playerStats = container.gameObject.GetComponent<SyncedStats>();
            if (playerStats == null || !playerStats.SetupStatValues(stats))
            {
                Debug.Log($"Failed to setup stats for player {entry.Key.id}");
            }

            _stats.Add(entry.Key, playerStats);
            // Spawn player
            AssignTag(container.gameObject, tag);
            container.SpawnCharacter(character, _spawnPoint.position, tag);
            GlobalController.Singleton?.RegisterPlayer(playerStats);
        }
        CloseSelectionScreens();
    }

    [ObserversRpc(runLocally:true)]
    private void CloseSelectionScreens()
    {
        UiManager.CloseScreen("CharacterSelectionUI");
        _initialCam.gameObject.SetActive(false);
        _loadingUI.SetActive(false);
    }

    [ObserversRpc(runLocally:true)]
    private void AssignTag(GameObject target, string tag)
    {
        target.tag = tag;
    }
}
