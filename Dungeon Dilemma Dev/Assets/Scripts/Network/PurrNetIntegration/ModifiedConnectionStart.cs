using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Transports;
using System.Collections;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ModifiedConnectionStart : MonoBehaviour
{
    private PurrTransport _purrTransport;
    private UDPTransport _udpTransport;
    private NetworkManager _networkManager;
    private LobbyDataHolder _lobbyDataHolder;

    private bool _isFromLobby;

    private void Awake()
    {
        if (!TryGetComponent(out _networkManager))
        {
            PurrLogger.LogError($"Failed to get {nameof(NetworkManager)} component.", this);
        }
        if (!TryGetComponent(out _purrTransport))
        {
            PurrLogger.LogError($"Failed to get {nameof(PurrTransport)} component.", this);
        }
        if (!TryGetComponent(out _udpTransport))
        {
            PurrLogger.LogError($"Failed to get {nameof(UDPTransport)} component.", this);
        }

        _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        if (_lobbyDataHolder)
            _isFromLobby = true;
    }

    private void Start()
    {
        if (!_networkManager)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)} is null!", this);
            return;
        }

        if (_isFromLobby)
        {
            StartFromLobby();
        }
        else
        {
            StartNormal();
        }
    }

    private void StartNormal()
    {
        _networkManager.transport = _udpTransport;

        foreach (string tag in Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags())
        {
            if (tag == "Host") _networkManager.StartServer();
        }
        StartCoroutine(StartClient());
    }

    private void StartFromLobby()
    {
        _networkManager.transport = _purrTransport;
        if (!_lobbyDataHolder)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(LobbyDataHolder)} is null!", this);
            return;
        }

        if (!_lobbyDataHolder.CurrentLobby.IsValid)
        {
            PurrLogger.LogError($"Failed to start connection. Lobby is invalid!", this);
            return;
        }

        if (_networkManager.transport is PurrTransport)
        {
            (_networkManager.transport as PurrTransport).roomName = _lobbyDataHolder.CurrentLobby.LobbyId;
        }

#if UTP_LOBBYRELAY
            else if(_networkManager.transport is UTPTransport) {
                if(_lobbyDataHolder.CurrentLobby.IsOwner) {
                    (_networkManager.transport as UTPTransport).InitializeRelayServer((Allocation)_lobbyDataHolder.CurrentLobby.ServerObject);
                }
                (_networkManager.transport as UTPTransport).InitializeRelayClient(_lobbyDataHolder.CurrentLobby.Properties["JoinCode"]);
            }
#else
        //P2P Connection, receive IP/Port from server
#endif

        if (_lobbyDataHolder.CurrentLobby.IsOwner)
            _networkManager.StartServer();
        StartCoroutine(StartClient());
    }

    private IEnumerator StartClient()
    {
        yield return new WaitForSeconds(1f);
        _networkManager.StartClient();
    }
}
