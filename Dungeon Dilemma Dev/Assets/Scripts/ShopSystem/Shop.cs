using Items;
using System.Collections.Generic;
using System.Linq;
using PurrNet;
using Priestess;
using System.Diagnostics;
using NUnit.Framework.Internal.Execution;

namespace Shop
{
    public class Shop
    {
        protected int _numShopSlots;
        protected int _rerollCost;
        protected int _freeRerolls;
        protected float _discount;
        protected float _sellRefund;

        protected List<Equipment> AvailableEquipment = new List<Equipment>();
        /// <summary>
        /// A cache of items currently on display in the shop
        /// </summary>
        public List<Item> CurrentlyInShop = new List<Item>();
        /// <summary>
        /// A cache of all items seen last roll
        /// </summary>
        protected List<Item> LastSeenItems = new List<Item>();

        public Shop()
        {
            _numShopSlots = 3;
            _rerollCost = 100;
            _freeRerolls = 0;
            _discount = 0;
            _sellRefund = 0.5f;
            AvailableEquipment = References.Instance.ChallengerEquipment.Items;
        }
        /// <summary>
        /// Increases the size of shop by 1 (max 4)
        /// </summary>
        public void IncreaseShopSize() { _numShopSlots = 4; }
        /// <summary>
        /// Lowers or raises the cost of reroll
        /// </summary>
        /// <param name="val">+ for add, - for subtract cost</param>
        public void SetRerollCost(int val) {_rerollCost = val;}
        /// <summary>
        /// Increase the free rerolls available
        /// </summary>
        /// <param name="val">Num to add</param>
        public void SetFreeRerolls(int val) {_freeRerolls = val;}
        /// <summary>
        /// Change the cost discount
        /// </summary>
        /// <param name="val">will be added to current discount</param>
        public void SetDiscount(float val) {_discount = val;}
        /// <summary>
        /// Checks if the player can reroll
        /// </summary>
        /// <param name="stats"></param>
        /// <returns></returns>
        public bool CanReroll(SyncedStats stats)
        {
            if (_freeRerolls > 0) {return true;}
            else if (stats.Gold.value >= _rerollCost && stats.Gold.value > 0) {return true;}
            return false;
        }
        public virtual void Roll(SyncedInventory inv)
        {
            LastSeenItems = new List<Item>(CurrentlyInShop);
            CurrentlyInShop.Clear();
            for (int i=0; i<_numShopSlots; i++)
            {
                CurrentlyInShop.Add(AddEquipment(inv));
            }
        }
        protected Item AddEquipment(SyncedInventory inv)
        {
            if (AvailableEquipment.Count == 0) return null;
            var validOptions = AvailableEquipment.Where(e => !LastSeenItems.Contains(e) && !CurrentlyInShop.Contains(e) && !inv.HeldEquipment.Contains(e)).ToList();
            if (validOptions.Count == 0) 
                return AvailableEquipment[UnityEngine.Random.Range(0, AvailableEquipment.Count)];
            return validOptions[UnityEngine.Random.Range(0, validOptions.Count)];
        }
        public void ProcessReroll(SyncedStats stats)
        {
            if (_freeRerolls > 0)
            {
                _freeRerolls--;
            }
            else
            {
                ModifyGold(stats, -1 * _rerollCost);
            }
        }
        public void EndShopSession()
        {
            FlushItemCache();
        }
        protected virtual void FlushItemCache()
        {
            LastSeenItems.Clear();
        }
        public bool SellEquipment(Equipment equipment, SyncedInventory inv, SyncedStats stats, CombatManager cm)
        {
            if (!inv.IsItemInInventory(equipment)) {return false;}
            inv.RemoveItem(equipment);
            cm.RemoveItem(equipment);
            Shop.ModifyGold(stats, (int)(equipment.Cost * _sellRefund));
            return true;
        }
        public bool CanBuyItem(int index, SyncedInventory inv, SyncedStats stat)
        {
            if (stat.Gold.value < CurrentlyInShop[index].Cost - (int)(CurrentlyInShop[index].Cost * _discount))
            {
                return false;
            }
            else if (inv.HeldEquipment.Count >= inv.MaxEquipmentAmount && CurrentlyInShop[index] is Equipment)
            {
                return false;
            }
            return true;
        }
        public virtual int BuyItem(int index, SyncedInventory inv, SyncedStats stats, CombatManager cm)
        {
            if (stats.Gold.value < CurrentlyInShop[index].Cost - (int)(CurrentlyInShop[index].Cost * _discount))
            {
                return 2; // attempting to buy item, but not enough money
            }
            else if (inv.HeldEquipment.Count >= inv.MaxEquipmentAmount && CurrentlyInShop[index] is Equipment)
            {
                return 3; // attempting to buy item, but full on equipment
            }
            Item purchasedItem = CurrentlyInShop[index];
            ModifyGold(stats, -1 * (purchasedItem.Cost - (int)(CurrentlyInShop[index].Cost * _discount)));
            if (purchasedItem is Equipment)
            {
                inv.AddItem(purchasedItem);
                cm.RegisterItem(purchasedItem);
            }
            else if (purchasedItem is Buildable bu)
            {
                inv.AddItem(bu, bu.PurchaseAmount);
            }
            return 1; // successfully bought item
        }
        public List<ShopItem> GetCurrentlyInShop()
        {
            List<ShopItem> newList = new List<ShopItem>();
            foreach (Item item in CurrentlyInShop)
            {
                if (item is Equipment eq)
                {
                    newList.Add(new ShopItem(item.Name, item.Cost, item.FlavorText, item.AbilityDescription, item.Icon, eq.Health, eq.Regeneration, eq.MoveSpeed, eq.Power, eq.Resistance, eq.Scale, 0, 0, 0, 0));
                }
                else if (item is Buildable b){
                    newList.Add(new ShopItem(item.Name, item.Cost, item.FlavorText, item.AbilityDescription, item.Icon, b.Health, 0, 0, b.Power, 0, 0, b.Cooldown, b.MinionCap, b.TrapCap, b.PurchaseAmount));
                }
            }
            return newList;
        }
        public int GetFreeRerolls()
        {
            return _freeRerolls;
        }
        public int GetRerollCost()
        {
            return _rerollCost;
        }
        [ServerRpc]
        private static void ModifyGold(SyncedStats stats, int amount)
        {
            stats.Gold.value += amount;
        }
    }
    public class BossShop : Shop
    {
        protected List<Buildable> AvailableMinions = new List<Buildable>();
        protected List<Buildable> AvailableTraps = new List<Buildable>();
        protected Dictionary<string, int> _weights = new Dictionary<string, int>();
        public BossShop() : base()
        {
            _weights.Add("Equipment", 1);
            _weights.Add("Minions", 1);
            _weights.Add("Traps", 1);
            AvailableEquipment = References.Instance.BossEquipment.Items;
            AvailableMinions = References.Instance.Minions.Items;
            AvailableTraps = References.Instance.Traps.Items;
        }
        /// <summary>
        /// Change the weights of items in the shop. Applicable only to Dungeon Boss.
        /// </summary>
        /// <param name="weightEquipment">Add to Equipment Weight. Default is 0</param>
        /// <param name="weightMinions">Add to Minion Weight. Default is 0</param>
        /// <param name="weightTraps">Add to Trap Weight. Default is 0</param>
        public void ModifyWeights(int weightEquipment=0, int weightMinions=0, int weightTraps=0)
        {
            _weights["Equipment"] += weightEquipment;
            _weights["Minions"] += weightMinions;
            _weights["Traps"] += weightTraps;
        }
        public void ResetWeights()
        {
            _weights["Equipment"] = 1;
            _weights["Minions"] = 1;
            _weights["Traps"] = 1;
        }
        public override void Roll(SyncedInventory inv)
        {
            LastSeenItems = new List<Item>(CurrentlyInShop);
            CurrentlyInShop.Clear();
            for (int i=0; i<_numShopSlots; i++)
            {
                int _type = UnityEngine.Random.Range(0, _weights["Equipment"]+_weights["Minions"]+_weights["Traps"]);
                if (_type < _weights["Equipment"])
                {
                    CurrentlyInShop.Add(AddEquipment(inv));
                }
                else if (_type >= _weights["Equipment"] && _type < _weights["Equipment"]+_weights["Minions"])
                {
                    CurrentlyInShop.Add(AddMinion());
                }
                else
                {
                    CurrentlyInShop.Add(AddTrap());
                }
            }
        }
        private Item AddMinion()
        {
            if (AvailableMinions.Count == 0) return null;
            var validOptions = AvailableMinions.Where(e => !LastSeenItems.Contains(e) && !CurrentlyInShop.Contains(e)).ToList();
            if (validOptions.Count == 0) 
                return AvailableMinions[UnityEngine.Random.Range(0, AvailableMinions.Count)];
            return validOptions[UnityEngine.Random.Range(0, validOptions.Count)];
        }
        private Item AddTrap()
        {
            if (AvailableTraps.Count == 0) return null;
            var validOptions = AvailableTraps.Where(e => !LastSeenItems.Contains(e) && !CurrentlyInShop.Contains(e)).ToList();
            if (validOptions.Count == 0) 
                return AvailableTraps[UnityEngine.Random.Range(0, AvailableTraps.Count)];
            return validOptions[UnityEngine.Random.Range(0, validOptions.Count)];
        }
    }
}