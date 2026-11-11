using UnityEngine;
using System.Collections.Generic;

namespace Items
{
    [CreateAssetMenu(menuName = "Reference/ItemList")]
    public class ItemList : ScriptableObject
    {
        [field: SerializeField] public List<Item> Items = new List<Item>();
    }
}