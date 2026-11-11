using UnityEngine;
namespace Items {
[CreateAssetMenu(menuName = "Item/Upgrade")]
public class Upgrade : Item
{
    [Header("Stats Modifier")]
    [field: SerializeField]
    public float Health { get; private set; }
    [field: SerializeField]
    public float Regeneration { get; private set; }
    [field: SerializeField]
    public float MoveSpeed { get; private set; }
    [field: SerializeField]
    public float Power { get; private set; }
    [field: SerializeField]
    public float Resistance { get; private set; }
    [field: SerializeField]
    public float LifeSteal { get; private set; }
    [field: SerializeField]
    public float Scale { get; private set; }
    [field: SerializeField]
    public int Revives { get; private set; }
    [field: SerializeField]
    public float DashStrength { get; private set; }
    [field: SerializeField]
    public float PrimaryCooldown { get; private set; }
    [field: SerializeField]
    public float SecondaryCooldown { get; private set; }
    [field: SerializeField]
    public float DashCooldown { get; private set; }
    [field: SerializeField]
    public float AttackRadius { get; private set; }
    [field: SerializeField]
    public float AttackBuff { get; private set; }
    [field: SerializeField]
    public float AttackDebuff { get; private set; }
    [field: SerializeField]
    public ItemAbility Ability { get; private set; }

    // <summary>
    /// Apply all the effects of this item to a stat module
    /// </summary>
    /// <param name="stats"></param>
    public void ApplyStatChanges(SyncedStats stats)
    {
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

        stats.PrimaryAbilityCooldown.value += PrimaryCooldown;
        stats.SecondaryAbilityCooldown.value += SecondaryCooldown;
        stats.DashCooldown.value += DashCooldown;

        stats.AttackRadius.value += AttackRadius;
        stats.AttackBuff.value += AttackBuff;
        stats.AttackDebuff.value += AttackDebuff;
    }
}
}