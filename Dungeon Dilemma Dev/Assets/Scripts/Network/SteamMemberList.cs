using System.Collections.Generic;
using PurrLobby;

public class SteamMemberList
{
    private static SteamMemberList _instance;

    public static SteamMemberList Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SteamMemberList();

            return _instance;
        }
    }

    public List<LobbyUser> Members = new();

    public void UpdateMembers(List<LobbyUser> membersList)
    {
        Members = membersList ?? new List<LobbyUser>();
    }
}