using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PurrNet;
using Items;
using Priestess;
using ControllerLogic;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

namespace Shop {
    public class ShopEntryPoint : MonoBehaviour
    {
        [SerializeField] private SyncedStats _stats;
        [SerializeField] private SyncedInventory _inventory;
        [SerializeField] private CombatManager _combat;
        public bool IsBoss;
        public Shop shop;
        public UpgradePicker upgradeShop;
        public List<bool> purchasedEquipment = new List<bool>() { false, false, false, false };
        public bool upgradePicked;
        public ShopItem chosenUpgrade;
        public List<bool> rerolledUpgrades = new List<bool>() { false, false, false };
        public List<ShopItem> inEquipmentShop = new List<ShopItem>();
        public List<ShopItem> inUpgradeShop = new List<ShopItem>();
        public static ShopEntryPoint localShopInstance;
        public float discount;
        private int freeRerolls;

        private void Awake()
        {
            localShopInstance = this;
        }

        /// <summary>
        /// Gets all component references and sets initial values based on static data
        /// </summary>
        public void InitiateShop(SyncedStats stats, SyncedInventory inventory, CombatManager combat, bool isBoss)
        {   
            _stats = stats;
            _inventory = inventory;
            _combat = combat;
            IsBoss = isBoss;
            upgradePicked = false;
            chosenUpgrade = null;
            discount = 0f;
            freeRerolls = 0;

            if (inventory.MaxEquipmentAmount == 0) { inventory.MaxEquipmentAmount = 5; }
            if (inventory.MaxUpgradeAmount == 0 ) { inventory.MaxUpgradeAmount = 5; }
            if (!IsBoss)
            {
                shop = new Shop();
                upgradeShop = new UpgradePicker(References.Instance.ChallengerUpgrades.Items);
            }
            else
            {
                shop = new BossShop();
                upgradeShop = new UpgradePicker(References.Instance.BossUpgrades.Items);
            }

            inEquipmentShop = InitialRollShop();
            inUpgradeShop = InitialRollUpgrade();
        }
        /// <summary>
        /// Change the number of slots in the shop.
        /// </summary>
        public void ModifyShopSlots() {shop.IncreaseShopSize();}
        /// <summary>
        /// Change the reroll cost in the shop.
        /// </summary>
        /// <param name="newCost">The new reroll cost. Default is 0</param>
        public void ModifyRerollCost(int newCost = 0){ shop.SetRerollCost(newCost); }
        /// <summary>
        /// Change the number of free rerolls in the shop.
        /// </summary>
        /// <param name="additionalFreeRerolls">Add to the number of free rerolls. Default is 0</param>
        public void SetFreeRerolls(int additionalFreeRerolls = 0){ freeRerolls = additionalFreeRerolls; shop.SetFreeRerolls(freeRerolls); }
        public void SetPriceDiscount(float newDiscount = 0) { discount = newDiscount; shop.SetDiscount(newDiscount); }
        public void ModifyWeights(int eq, int min, int trap) { if (shop is BossShop bs) {bs.ModifyWeights(eq, min, trap);} }
        /// <summary>
        /// Roll items in the shop for the first time
        /// </summary>
        /// <returns>A tuple consisting of the number of items and a list of the items</returns>
        public List<ShopItem> InitialRollShop()
        {
            return shop.GetCurrentlyInShop();
        }
        public List<ShopItem> InitialRollUpgrade()
        {
            return upgradeShop.GetCurrentlyInShop();
        }
        public bool CanRerollShop()
        {
            return shop.CanReroll(_stats);
        }
        public List<ShopItem> RerollShop()
        {
            if (!shop.CanReroll(_stats)) { return shop.GetCurrentlyInShop(); }
            shop.ProcessReroll(_stats);
            shop.Roll(_inventory);
            return shop.GetCurrentlyInShop();
        }
        public List<ShopItem> RerollUpgrade(int index)
        {
            upgradeShop.Reroll(index, _inventory);
            return upgradeShop.GetCurrentlyInShop();
        }
        public int GetGold()
        {
            return _stats.Gold.value;
        }
        /// <summary>
        /// Purchase an item from the shop
        /// </summary>
        /// <param name="index">Index of item to purchase</param>
        /// <returns>T/F if the transaction succeeded</returns>
        public int BuyShopItem(int index)
        {
            return shop.BuyItem(index, _inventory, _stats, _combat);
        }
        public bool CanBuyShopItem(int index)
        {
            return shop.CanBuyItem(index, _inventory, _stats);
        }
        public bool BuyUpgradeItem(int index)
        {
            return upgradeShop.ChooseUpgrade(index, _inventory, _combat);
        }
        public bool SellEquipment(Equipment equipment)
        {
            return shop.SellEquipment(equipment, _inventory, _stats, _combat);
        }
        public int GetFreeRerolls()
        {
            return shop.GetFreeRerolls();
        }
        public int GetRerollCost()
        {
            return shop.GetRerollCost();
        }
        /// <summary>
        /// Start a new setup session, call this once per round
        /// </summary>
        private void NewSetupSession()
        {
            shop.Roll(_inventory);
            upgradeShop.InitialRoll(_inventory);
            upgradeShop.AddSelection();
        }
        public IEnumerator StartNewSetupSession()
        {
            while (shop == null) yield return null;
            NewSetupSession();
        }
        /// <summary>
        /// End the current setup session, call this as the phase switches to raid
        /// </summary>
        public void EndSetupSession()
        {
            shop.EndShopSession();
            upgradeShop.EndSession();
            purchasedEquipment = new List<bool>() { false, false, false, false };
            upgradePicked = false;
            chosenUpgrade = null;
            rerolledUpgrades = new List<bool>() { false, false, false };
            inEquipmentShop = new List<ShopItem>();
            inUpgradeShop = new List<ShopItem>();
            upgradeShop.ResetRerollableUpgrades();
            if (shop is BossShop bs) { bs.ResetWeights(); }
        }
        public List<Equipment> GetHeldEquipment() {
            return _inventory.HeldEquipment.ToList();
        }
        public List<Upgrade> GetHeldUpgrades()
        {
            return _inventory.ActiveUpgrades.ToList();
        }
        public Dictionary<string, int> GetHeldBuildables()
        {
            return _inventory.HeldBuildables.ToDictionary();
        }
        public SyncedStats GetStats()
        {
            return _stats;
        }
    }
}