using UnityEngine;
using System.Collections.Generic;
using Items;
using System.Linq;

namespace Shop{
    public class UpgradePicker
    {
        private List<Upgrade> AvailableUpgrades = new List<Upgrade>();
        private List<Upgrade> CurrentlyInShop = new List<Upgrade>();
        private Dictionary<Upgrade, int> CachedUpgrades = new Dictionary<Upgrade, int>();
        private int _shopSize = 3;
        private bool _canPick;
        private List<bool> RerollableUpgrades = new List<bool>(){ true, true, true };

        public UpgradePicker(List<Upgrade> availUpg)
        {
            AvailableUpgrades = availUpg;
        }
        public bool CanPickUpgrade()
        {
            return _canPick;
        }
        public void AddSelection()
        {
            _canPick = true; 
        }
        public void InitialRoll(SyncedInventory inv)
        {
            CurrentlyInShop.Clear();
            for(int i=0; i<_shopSize; i++)
            {
                CurrentlyInShop.Add(AddUpgrade(inv));
            }
        }
        /// <summary>
        /// Returns whether the input upgrade can be rerolled or not
        /// </summary>
        /// <param name="index"> int index of the upgrade slot </param>
        /// <returns> true if the upgrade can be rerolled; false if it can't be rerolled yet </returns>
        public bool CanReroll(int index)
        {
            return RerollableUpgrades[index];
        }
        public void Reroll(int index, SyncedInventory inv)
        {
            if (!CanReroll(index)) { return; }
            RerollableUpgrades[index] = false;
            CurrentlyInShop[index] = AddUpgrade(inv);
            AddCache(CurrentlyInShop[index]);
        }
        public bool ChooseUpgrade(int index, SyncedInventory inv, Priestess.CombatManager cm)
        {
            if (index > _shopSize) {return false; }
            inv.AddItem(CurrentlyInShop[index]);
            cm.RegisterItem(CurrentlyInShop[index]);
            _canPick = false;
            CurrentlyInShop.Clear();
            return true;
        }
        private Upgrade AddUpgrade(SyncedInventory inv)
        {
            if (AvailableUpgrades.Count == 0) return null;
            var validOptions = AvailableUpgrades.Where(u => !CachedUpgrades.ContainsKey(u) && !CurrentlyInShop.Contains(u) && !inv.ActiveUpgrades.Contains(u)).ToList();
            if (validOptions.Count == 0) 
                return AvailableUpgrades[UnityEngine.Random.Range(0, AvailableUpgrades.Count)];
            return validOptions[UnityEngine.Random.Range(0, validOptions.Count)];
        }
        public void ResetRerollableUpgrades()
        {
            RerollableUpgrades = new List<bool>(){ true, true, true };
        }
        private void AddCache(Upgrade up)
        {
            if (CachedUpgrades.Count==3)
            {
                CachedUpgrades.Remove(up);
            }
            CachedUpgrades[up] = 1;
        }
        public void EndSession()
        {
            CachedUpgrades.Clear();
        }
        public List<ShopItem> GetCurrentlyInShop()
        {
            List<ShopItem> newList = new List<ShopItem>();
            foreach (Upgrade u in CurrentlyInShop)
            {
                newList.Add(new ShopItem(u.Name, u.Cost, u.FlavorText, u.AbilityDescription, u.Icon, u.Health, u.Regeneration, u.MoveSpeed, u.Power, u.Resistance, u.Scale, 0, 0, 0, 0));
            }
            return newList;
        }
    }
}