using PurrNet;
using UnityEditor;
using UnityEngine;

namespace Items {
[CreateAssetMenu(menuName = "Item/Equipment")]
public class Equipment : Item
{
    [Header("Stats Modifier")]
    [field: SerializeField]
    public float Health { get; protected set; }
    [field: SerializeField]
    public float Regeneration { get; private set; }
    [field: SerializeField]
    public float MoveSpeed { get; private set; }
    [field: SerializeField]
    public float Power { get; private set; }
    [field: SerializeField]
    public float Resistance { get; private set; }
    [field: SerializeField]
    public float Scale { get; private set; }

    /// Extra stats beyond GDD
    [field: SerializeField] 
    public float LifeSteal { get; private set; }
    [field: SerializeField]
    public int Revives { get; private set; }
    [field: SerializeField]
    public float DashStrength { get; private set; }
    [field: SerializeField]
    public float PrimaryCooldownModifier { get; private set; }
    [field: SerializeField]
    public float SecondaryCooldownModifier { get; private set; }
    [field: SerializeField]
    public float DashCooldownModifier { get; private set; }
    [field: SerializeField]
    public float AttackRadius {  get; private set; }
    [field: SerializeField]
    public float AttackBuff { get; private set; }
    [field: SerializeField]
    public float AttackDebuff { get; private set; }
    [field: SerializeField]
    public ItemAbility Ability { get; private set; }
    private float primaryCooldownModification;
    private float secondaryCooldownModification;
    private float dashCooldownModification;

    /// <summary>
    /// Apply all the effects of this item to a stat module
    /// </summary>
    /// <param name="stats">Synced Stats of player</param>
    public void ApplyStatChanges(SyncedStats stats)
    {       
        //Base Stats
        stats.PassiveRegen.value += Regeneration;
        stats.MovementSpeed.value += MoveSpeed;
        stats.MaximumHealth.value += Health;
        stats.CurrentHealth.value = Mathf.Clamp(stats.CurrentHealth.value + Health, 1, stats.MaximumHealth.value);
        stats.AttackPower.value += Power;
        stats.Resistance.value += Resistance;
        stats.Lifesteal.value += LifeSteal;
        stats.Revives.value += Revives;

        stats.Scale.value += Scale;
        stats.GetComponentInChildren<HealthHandler>().transform.localScale = Vector3.one * (((stats.Scale.value - 1) / 10) + 1);

        stats.DashStrength.value += DashStrength;

        primaryCooldownModification = stats.PrimaryAbilityCooldown.value * PrimaryCooldownModifier;
        stats.PrimaryAbilityCooldown.value += primaryCooldownModification;
        secondaryCooldownModification = stats.SecondaryAbilityCooldown.value * SecondaryCooldownModifier;
        stats.SecondaryAbilityCooldown.value += secondaryCooldownModification;
        dashCooldownModification = stats.DashCooldown.value * DashCooldownModifier;
        stats.DashCooldown.value += dashCooldownModification;

        stats.AttackRadius.value += AttackRadius;
        stats.AttackBuff.value += AttackBuff;
        stats.AttackDebuff.value += AttackDebuff;
    }
    /// <summary>
    /// Remove all the effects of this item to a stat module
    /// </summary>
    /// <param name="stats">Synced Stats of player</param>
    public void RemoveStatChanges(SyncedStats stats)
    {
        stats.PassiveRegen.value -= Regeneration;
        stats.MovementSpeed.value -= MoveSpeed;
        stats.MaximumHealth.value -= Health;
        stats.CurrentHealth.value = Mathf.Clamp(stats.CurrentHealth.value, 1, stats.MaximumHealth.value);
        stats.AttackPower.value -= Power;
        stats.Resistance.value -= Resistance;
        stats.Lifesteal.value -= LifeSteal;
        stats.Revives.value -= Revives;

        stats.Scale.value -= Scale;
        stats.GetComponentInChildren<HealthHandler>().transform.localScale = Vector3.one * (((stats.Scale.value - 1) / 10) + 1);

        stats.DashStrength.value -= DashStrength;

        stats.PrimaryAbilityCooldown.value -= primaryCooldownModification;
        stats.SecondaryAbilityCooldown.value -= secondaryCooldownModification;
        stats.DashCooldown.value -= dashCooldownModification;

        stats.AttackRadius.value -= AttackRadius;
        stats.AttackBuff.value -= AttackBuff;
        stats.AttackDebuff.value -= AttackDebuff;
    }
}
}   