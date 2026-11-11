using UnityEngine;
using System;
using System.Collections.Generic;
using Shop;
using ItemReference;
namespace Items {
    public class References : MonoBehaviour
    {
        public static References Instance;
        void Awake()
        {
            if (Instance==null) {Instance=this;}
        }
        public UpgradeList ChallengerUpgrades;
        public UpgradeList BossUpgrades;
        public EquipmentList ChallengerEquipment;
        public EquipmentList BossEquipment;
        public BuildableList Minions;
        public BuildableList Traps;
        public ShopDialogList ShopDialog;
    }
}