using PurrLobby;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using UnityEngine;

public class WaitForPlayers : StateNode
{
    public float PlayerCount = 2;
    [SerializeField] private bool _isTestingLobby;

    private bool _isSoloTesting;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;

        GetPlayerCount();
        StartCoroutine(WaitForPlayersLoading());
    }

    private IEnumerator WaitForPlayersLoading()
    {
        if (_isSoloTesting) yield return new WaitForSeconds(3);

        while (networkManager.players.Count < PlayerCount)
        {
            yield return null;
        }

        machine.Next();
    }

    private void GetPlayerCount()
    {
        LobbyDataHolder _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        if (_lobbyDataHolder != null)
        {
#if UNITY_EDITOR
            if (_isTestingLobby)
            {
                return;
            }
#endif
            PlayerCount = _lobbyDataHolder.CurrentLobby.Members.Count;
        }
        else
        {
            _isSoloTesting = true;
        }
    }
}