using ControllerLogic;
using PurrNet;
using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public class SyncedStats : NetworkBehaviour {
    [Header("Raid")]
    public SyncVar<bool> IsAlive = new();
    public SyncVar<float> CurrentHealth = new();
    [Header("Combat")]
    public SyncVar<float> PassiveRegen = new();
    public SyncVar<float> AttackPower = new();
    public SyncVar<float> Armor = new();
    public SyncVar<float> Resistance = new();
    public SyncVar<float> AttackRadius = new();
    public SyncVar<float> MaximumHealth = new();
    public SyncVar<float> AttackBuff = new();
    public SyncVar<float> AttackDebuff = new();
    public SyncVar<float> Lifesteal = new();
    public SyncVar<int> Revives = new();
    [Header("Movement")]
    public SyncVar<float> MovementSpeed = new();
    public SyncVar<float> DashStrength = new();
    public SyncVar<float> Scale = new();
    [Header("Cooldowns")]
    public SyncVar<float> PrimaryAbilityCooldown = new();
    public SyncVar<float> SecondaryAbilityCooldown = new();
    public SyncVar<float> DashCooldown = new();
    [Header("Currency")]
    public SyncVar<int> Gold = new();
    [Header("Character")]
    public SyncVar<string> Name = new();
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public Sprite DeadIcon { get; private set; }
    [Header("Score")]
    public SyncVar<float> Score = new();
    public SyncVar<float> DamageDealt = new();
    public SyncVar<float> DamageBlocked = new();
    public SyncVar<float> RegeneratedHealth = new();
    public SyncVar<float> SupportPoints = new();
    public SyncVar<float> BossHits = new();

    public enum AttackType { Primary, Secondary }

    /// <summary>
    /// Populates the current StatSync instance with data from a scriptable object.
    /// </summary>
    /// <returns>A bool specifiying if the function successfully ran as server</returns>
    public bool SetupStatValues(StatsData stats) {
        if (!isServer) { return false; }
        PassiveRegen.value = stats.PassiveRegen;
        AttackPower.value = stats.BasePower;
        Armor.value = stats.BaseArmor;
        Resistance.value = stats.BaseResist;
        AttackRadius.value = stats.AttackRadius;
        MaximumHealth.value = stats.BaseHealth;
        MovementSpeed.value = stats.Speed;
        DashStrength.value = stats.DashForce;
        Scale.value = stats.Scale;
        Revives.value = stats.BaseRevives;
        PrimaryAbilityCooldown.value = stats.PrimaryAbilityCooldown;
        SecondaryAbilityCooldown.value = stats.SecondaryAbilityCooldown;
        DashCooldown.value = stats.DashCooldown;
        Name.value = stats.Name;
        Icon = stats.Icon;
        DeadIcon = stats.DeadIcon;

        if (Name.value == "Yew")
        {
            Score.value = -1;
        }

        SetNonSyncStats(stats);

        CurrentHealth.value = stats.BaseHealth;
        return true;
    }

    public float GetCooldown(AttackType type)
    {
        switch (type)
        {
            case AttackType.Primary:
                return PrimaryAbilityCooldown.value;
            case AttackType.Secondary:
                return SecondaryAbilityCooldown.value;
            default:
                return 0;
        };
    }

    /// <summary>
    /// Sets the values for non-synced variables on all clients
    /// These variables are read-only, meant to be updated once during setup
    /// </summary>
    [ObserversRpc(excludeSender: true)]
    public void SetNonSyncStats(StatsData stats)
    {
        Icon = stats.Icon;
        DeadIcon = stats.DeadIcon;  
    }

    [ServerRpc]
    public void AddArmor(float armorValue, float maxArmor = -1)
    {
        if (maxArmor != -1)
        {
            Armor.value = Mathf.Clamp(Armor.value + armorValue, 0, maxArmor);
        }
        else
        {
            Armor.value += armorValue;
        }
    }

    [ServerRpc]
    public void AddMaxHP (float hpValue)
    {
        MaximumHealth.value += hpValue;
        CurrentHealth.value = Mathf.Clamp(CurrentHealth.value + hpValue, 1, MaximumHealth.value);
    }

    [ServerRpc]
    public void AddRegen (float regenValue)
    {
        PassiveRegen.value += regenValue;
    }

    [ServerRpc]
    public void AddSpeed (float spdValue)
    {
        MovementSpeed.value += spdValue;
    }

    [ServerRpc]
    public void AddPower (float pwrValue)
    {
        AttackPower.value += pwrValue;
    }

    [ServerRpc]
    public void AddResist (float resValue)
    {
        Resistance.value += resValue;
    }

    [ServerRpc]
    public void AddScale (float scaleValue)
    {
        Scale.value += scaleValue;
        GetComponentInChildren<HealthHandler>().transform.localScale = Vector3.one * (((Scale.value - 1) / 10) + 1);
    }

    [ServerRpc] 
    public void ModifyGold(int amount)
    {
        Gold.value += amount;
    }
}