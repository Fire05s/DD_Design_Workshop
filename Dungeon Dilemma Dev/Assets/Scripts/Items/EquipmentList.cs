using System.Collections.Generic;
using UnityEngine;
using Items;
namespace ItemReference {
[CreateAssetMenu(menuName = "Reference/EquipmentList")]
public class EquipmentList : ScriptableObject
{
    [field: SerializeField] public List<Equipment> Items = new List<Equipment>();
}
}