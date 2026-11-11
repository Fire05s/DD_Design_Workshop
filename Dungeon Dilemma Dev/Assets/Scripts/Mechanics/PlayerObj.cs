using System.Collections.Generic;
using UnityEngine;

public enum PlayerType { challenger, boss };

public class PlayerObj : MonoBehaviour
{
    public PlayerObj(string playerID, PlayerType playerType, string playerClass)
    {
        clientID = playerID;
        type = playerType;
        pclass = playerClass;
    }
    public int playerNumber;
    public string clientID;
    public PlayerType type;
    public string pclass;
    public List<string> items;
    public List<string> upgrades;

    public int totalDamageBlocked;
    public int totalDamageDealt;
    public int totalRegeneratedHealth;
    public int totalSupportPoints;
    public int totalBossLastHits;
    public int totalScore;
}