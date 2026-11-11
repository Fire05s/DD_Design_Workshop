using System.Collections.Generic;
using UnityEngine;
using Items;
namespace ItemReference {
[CreateAssetMenu(menuName = "Reference/BuildableList")]
public class BuildableList : ScriptableObject
{
    [field: SerializeField] public List<Buildable> Items = new List<Buildable>();
}
}