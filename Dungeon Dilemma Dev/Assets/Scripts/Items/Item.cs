using UnityEngine;

namespace Items {
public class Item : ScriptableObject
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    [field: TextArea]
    public string AbilityDescription { get; private set; }
    [field: SerializeField]
    [field: TextArea]
    public string FlavorText { get; private set; }
    [field: SerializeField]
    public Sprite Icon { get; private set; }
    [field: SerializeField]
    public int Cost { get; private set; }
}
}
