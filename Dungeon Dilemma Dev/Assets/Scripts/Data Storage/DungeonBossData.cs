// using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonBossData", menuName = "Entities/Boss")]
public class DungeonBossData : ScriptableObject
{
    [field: SerializeField] public Sprite Sprite { private set; get; }

    [field: Header("Boss Stats")]
    [field: SerializeField] public float BaseHealth { private set; get; }
    [field: SerializeField] public float HealthMultiplier { private set; get; }
    [field: SerializeField] public float PassiveRegen { private set; get; }
    [field: SerializeField] public float MoveSpeed { private set; get; }
    [field: SerializeField] public float Power { private set; get; }
    [field: SerializeField] public float Resistance { private set; get; }
    [field: SerializeField] public float Armor { private set; get; }
    [field: Header("Cooldowns")]
    [field: SerializeField]
    public float PrimaryOneAbilityCooldown { get; private set; }
    [field: SerializeField]
    public float SecondaryOneAbilityCooldown {get; private set;}
    [field: SerializeField]
    public float DashOneCooldown {get; private set;}
    [field: SerializeField]
    public float PrimaryTwoAbilityCooldown { get; private set; }
    [field: SerializeField]
    public float SecondaryTwoAbilityCooldown {get; private set;}
    [field: SerializeField]
    public float DashTwoCooldown {get; private set;}
}
