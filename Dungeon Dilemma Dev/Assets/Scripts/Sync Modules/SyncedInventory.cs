using System;
using System.Collections.Generic;
using System.Linq;
// using BuildingSystem.Models;
using UnityEngine;

using PurrNet;

using Items;
using Buildable = Items.Buildable;

public class SyncedInventory : NetworkBehaviour
{
    private Priestess.CombatManager _combat;
    [SerializeField] private SyncedStats _stats;
    public int MaxEquipmentAmount = 5;
    public SyncList<Equipment> HeldEquipment = new();
    public SyncList<Equipment> SoldEquipment = new();
    public int MaxUpgradeAmount = 5;
    public SyncList<Upgrade> ActiveUpgrades = new();
    public SyncDictionary<string,int> HeldBuildables = new();
    public SyncDictionary<string,int> PlacedBuildables = new();

    /// <summary>
    /// Add an item to the inventory. Has duplicate protection.
    /// </summary>
    /// <param name="item">Item object or child</param>
    /// <returns>If operation succeeded/failed</returns>
    public void AddItem(Item item, int amount=0)
    {
        if (item is Equipment)
        {
            AddEquipment((Equipment)item);
        }
        else if (item is Upgrade)
        {
            AddUpgrade((Upgrade)item);
        }
        else if (item is Buildable)
        {
            AddBuildable(item.Name, amount);
        }
    }
    /// <summary>
    /// Remove equipment from the inventory. Contains type checking.
    /// </summary>
    /// <param name="item">Item object or child</param>
    public void RemoveItem(Item item)
    {
        if (item is Equipment)
        {
            RemoveEquipment((Equipment)item);
        }
        else if (item is Buildable)
        {
            RemoveBuildable(item.Name);
        }
    }
    /// <summary>
    /// Checks if the inventory contains an equipment item
    /// </summary>
    /// <param name="item">Any equipment object</param>
    /// <returns>T/F if inventory has item</returns>
    public bool IsItemInInventory(Equipment item)
    {
        if (HeldEquipment.Contains(item)) {return true;}
        else { return false;}
    }
    /// <summary>
    /// Checks if the inventory contains an upgrade item
    /// </summary>
    /// <param name="item">Any upgrade object</param>
    /// <returns>T/F if inventory has item</returns>
    public bool IsItemInInventory(Upgrade item)
    {
        if (ActiveUpgrades.Contains(item)) {return true;}
        else { return false;}
    }
    /// <summary>
    /// Checks if the inventory contains a buildable item
    /// </summary>
    /// <param name="item">Any buildable object</param>
    /// <returns>T/F if inventory has item</returns>
    public bool IsItemInInventory(Buildable item)
    {
        if (HeldBuildables.Keys.Contains(item.Name) && HeldBuildables[item.Name]!=0) { return true; }
        else { return false; }
    }
    [ServerRpc]
    private void AddEquipment(Equipment equipment)
    {
        if (HeldEquipment.Count >= MaxEquipmentAmount) { return; }
        // AddEquipmentToInventory(HeldEquipment, equipment);
        HeldEquipment.Add(equipment);
        // ApplyEquipmentStats(_stats, equipment);
        equipment.ApplyStatChanges(_stats);
        if (_combat!=null) {_combat.RegisterItem(equipment);}
    }
    [ServerRpc]
    private void RemoveEquipment(Equipment equipment)
    {
        if (!HeldEquipment.Contains(equipment)) { return; }
        equipment.RemoveStatChanges(_stats);
        HeldEquipment.Remove(equipment);
        SoldEquipment.Add(equipment);
        if (_combat!=null) {_combat.RemoveItem(equipment);}
    }
    [ServerRpc]
    private void AddUpgrade(Upgrade upgrade)
    {
        if (ActiveUpgrades.Count >= MaxUpgradeAmount) { return; }
        ActiveUpgrades.Add(upgrade);
        upgrade.ApplyStatChanges(_stats);
        if (_combat!=null) {_combat.RegisterItem(upgrade);}
    }
    [ServerRpc]
    private void AddBuildable(string buildableName, int amount)
    {
        if (amount==0) {return;}

        if (!HeldBuildables.ContainsKey(buildableName)) {
            HeldBuildables.Add(buildableName, amount);
            PlacedBuildables.Add(buildableName, 0);
        } else {
            HeldBuildables[buildableName] += amount;
        }
    }

    [ServerRpc]
    private void RemoveBuildable(string buildableName)
    {
        if (HeldBuildables.Keys.Contains(buildableName) && HeldBuildables[buildableName]>0)
        {
            HeldBuildables[buildableName]--;
        }
    }

    // BUILDABLE PLACEMENT
    public bool CanPlaceBuildable(string buildableName)
    {
        return (PlacedBuildables.ContainsKey(buildableName) && HeldBuildables.ContainsKey(buildableName) &&
                PlacedBuildables[buildableName] < HeldBuildables[buildableName]);
    }

    [ServerRpc]
    public void PlaceBuildable(string buildableName)
    {
        if (!PlacedBuildables.ContainsKey(buildableName)) return;
        if (PlacedBuildables[buildableName] < HeldBuildables[buildableName])
        {
            PlacedBuildables[buildableName]++;
        }
    }

    [ServerRpc]
    public void RetrieveBuildable(string buildableName)
    {
        if (!PlacedBuildables.ContainsKey(buildableName)) return;
        if (PlacedBuildables[buildableName] > 0)
        {
            PlacedBuildables[buildableName]--;
        }
    }

    /// <summary>
    /// Retrieves the amount of buildables available to be placed
    /// </summary>
    /// <param name="buildableName"></param>
    /// <returns></returns>
    public int GetBuildableAmount(string buildableName)
    {
        return (HeldBuildables[buildableName] - PlacedBuildables[buildableName]);
    }
}