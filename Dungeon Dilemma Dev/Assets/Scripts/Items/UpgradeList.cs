using System.Collections.Generic;
using UnityEngine;
using Items;
namespace ItemReference {
[CreateAssetMenu(menuName = "Reference/UpgradeList")]
    public class UpgradeList : ScriptableObject
    {
        [field: SerializeField] public List<Upgrade> Items = new List<Upgrade>();
    }
}