using PurrNet;
using UnityEngine;

namespace Sync_Modules {

    public enum RoundState {
        Setup = 0,
        Raid = 1,
        RaidOvertime = 2,
        BossFight = 3,
        PostRound = 4
    }

    public class SyncedWorld : NetworkBehaviour {
        public SyncList<SyncedRoom> RoomLayout = new();
        public SyncVar<int> Round = new(0);
        public SyncVar<RoundState> RoundState = new();
        public SyncVar<int> BossLives = new(3);
        public SyncVar<int> ChallengerLives = new(3);
        public SyncVar<string> PrevWinner = new();
    }
}