using UnityEngine;
using PurrNet;
using UnityEngine.SceneManagement;

public class DisconnectHandler : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(SubscribeToEvents), 0.1f);
    }

    private void SubscribeToEvents()
    {
        if (!NetworkManager.main) return;

        NetworkManager.main.onClientConnectionState += OnLocalConnectionChanged;

        if (NetworkManager.main.isServer)
        {
            NetworkManager.main.onPlayerLeft += MonitorPlayerExits;
        }
    }

    private void OnDestroy()
    {
        if (!NetworkManager.main) return;

        NetworkManager.main.onClientConnectionState -= OnLocalConnectionChanged;

        if (NetworkManager.main.isServer)
        {
            NetworkManager.main.onPlayerLeft -= MonitorPlayerExits;
        }
    }

    private void MonitorPlayerExits(PlayerID player, bool sessionEnd)
    {
        NetworkManager.main.StopServer();
        MusicManager.Instance?.PlayMusic("Menu", true);
        SceneManager.LoadScene("MainLobby");
    }

    private void OnLocalConnectionChanged(PurrNet.Transports.ConnectionState state)
    {
        if (state == PurrNet.Transports.ConnectionState.Disconnected)
        {
            MusicManager.Instance?.PlayMusic("Menu", true);
            SceneManager.LoadScene("MainLobby");
        }
    }
}