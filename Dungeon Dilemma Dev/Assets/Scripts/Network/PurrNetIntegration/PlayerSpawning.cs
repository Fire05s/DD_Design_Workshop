using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawning : StateNode
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CharacterSelection _characterSelection;
    [SerializeField] private GameOngoing _ongoingGame;

    private Dictionary<PlayerID, PlayerContainer> _players = new();

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

        foreach (var player in networkManager.players)
        {
            var newPlayer = Instantiate(_playerPrefab, _spawnPoint.position, _spawnPoint.rotation);
            newPlayer.GetComponent<NetworkIdentity>().GiveOwnership(player, propagateToChildren: true);
            PlayerContainer container = newPlayer.GetComponent<PlayerContainer>();
            _players.Add(player, container);
        }

        _characterSelection.SetPlayers(_players);
        
        machine.Next();
    }
}
