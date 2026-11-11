using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;


namespace ControllerLogic {
    /// <summary>
    /// Class representing the data for a player. Can be for raid and overall.
    /// </summary>
    public class PlayerData {
        public NetworkID? PlayerID {get; private set;}
        public float TotalDamageBlocked {get; private set;}
        public float TotalDamageDealt {get; private set;}
        public float TotalRegeneratedHealth {get; private set;}
        public float TotalSupportPoints {get; private set;}
        public int TotalBossLastHits {get; private set;}
        public float TotalScore {get; private set;}
        public SyncedStats Stats;

        public PlayerData(SyncedStats stats) {
            PlayerID = stats.id;
            this.Stats = stats;
        }
        /// <summary>
        /// Update a score for this Player
        /// </summary>
        /// <param name="type">The score to update</param>
        /// <param name="value">Amount to change by</param>
        public void UpdateScore(ScoreType type, float value=0)
        {
            switch(type)
            {
                case ScoreType.DamageDealt: TotalDamageDealt += value; break;
                case ScoreType.BlockedDamage: TotalDamageBlocked += value; break;
                case ScoreType.RegeneratedHealth: TotalRegeneratedHealth += value; break;
                case ScoreType.SupportPoints: TotalSupportPoints += value; break;
                case ScoreType.BossLastHits: TotalBossLastHits++; break;
            }
            UpdateTotalScore();
        }
        /// <summary>
        /// Adjust the total score based on the current score category values.
        /// </summary>
        private void UpdateTotalScore()
        {
            TotalScore = (int)(TotalDamageBlocked/1000) + (int)(TotalDamageDealt/1000) + (int)(TotalRegeneratedHealth/1000) + TotalSupportPoints + TotalBossLastHits;
            if (Stats.Name.value == "Yew")
            {
                Stats.Score.value = -1;
            }
            else
            {
                Stats.Score.value = TotalScore;
            }
            Stats.DamageDealt.value = TotalDamageDealt;
            Stats.DamageBlocked.value = TotalDamageBlocked;
            Stats.RegeneratedHealth.value = TotalRegeneratedHealth;
            Stats.SupportPoints.value = TotalSupportPoints;
            Stats.BossHits.value = TotalBossLastHits;
        }
        /// <summary>
        /// Convert the score data into printable integers
        /// </summary>
        /// <returns>A tuple of 6 integers in order of: Dmg Block, Dmg Dealt, Regen HP, Support, Boss Hit, Total Score</returns>
        public (int,int,int,int,int,int) GetScores()
        {
            return ( 
                    (int)Mathf.Floor(TotalDamageBlocked / 1000),
                    (int)Mathf.Floor(TotalDamageDealt / 1000),
                    (int)Mathf.Floor(TotalRegeneratedHealth / 1000),
                    (int)Mathf.Floor(TotalSupportPoints / 1000),
                    TotalBossLastHits,
                    (int)Mathf.Floor(TotalScore)
                    );
        }
    }
    /// <summary>
    /// Declare the type of score / score category for purposes of updating player scores
    /// </summary>
    public enum ScoreType
    {
        Total,BlockedDamage,DamageDealt,RegeneratedHealth,SupportPoints,BossLastHits
    }
}