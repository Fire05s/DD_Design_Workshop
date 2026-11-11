using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(menuName = "Item/New Buildable Item", fileName = "New Buildable Item")]
    [Serializable]
    public class Buildable : Item {
        public float Health;
        public float Power;
        public float Cooldown;
        public float MinionCap;
        public float TrapCap;
        public int PurchaseAmount;
        public Vector2Int Size;
        public GameObject Prefab;
        public Vector2 TileOffset;

        public Direction direction;
    }

    [CreateAssetMenu(menuName = "Item/Minion")]
    public class MinionBuildable : Buildable {
        
    }

    public class TrapBuildable : Buildable {
        
    }
}