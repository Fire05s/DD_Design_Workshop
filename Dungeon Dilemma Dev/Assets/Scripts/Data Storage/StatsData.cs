using UnityEngine;

[CreateAssetMenu(menuName = "Stats")]
public class StatsData : ScriptableObject
{
    [field: Header("Basic Information")]
    [field: SerializeField]
    public string Name {get; private set;}
    [field: SerializeField]
    public Sprite Icon {get; private set;}
    [field: SerializeField]
    public Sprite DeadIcon {get; private set;}
    [field: Header("Combat Base Stats")]
    [field: SerializeField]
    public float BasePower {get; private set;}
    [field: SerializeField]
    public float BaseHealth {get; private set;}
    [field: SerializeField]
    public int BaseArmor {get; private set;}
    [field: SerializeField]
    public float PassiveRegen {get; private set;}
    [field: SerializeField]
    public int BaseRevives {get; private set;}
    [field: Header("Combat Multipliers")]
    [field: SerializeField]
    [Range(0f,5f)]
    public float BaseResist {get; private set;}
    [field: Header("Movement Stats")]
    [field: SerializeField]
    public float Speed {get; private set;}
    [field: SerializeField]
    public float DashForce {get; private set;}
    [field: SerializeField]
    public float Scale {get; private set;}
    [field: Header("Cooldowns")]
    [field: SerializeField]
    public float PrimaryAbilityCooldown { get; private set; }
    [field: SerializeField]
    public float SecondaryAbilityCooldown {get; private set;}
    [field: SerializeField]
    public float DashCooldown {get; private set;}
    [field: Header("Combat Radii")]
    [field: SerializeField]
    public float AttackRadius {get; private set;}
}
