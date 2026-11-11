using PurrNet;
using Shop;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;
using System.Collections;

namespace Avocado.Screens
{
    public class InventoryScreen : UiScreen
    {
        [SerializeField] private Sprite defaultSlotIcon;
        [SerializeField] private AudioClip sellSFX;
        [SerializeField] private AudioClip buttonSFX;
        private List<Equipment> shownEquipment = new List<Equipment>();
        private List<Upgrade> shownUpgrade = new List<Upgrade>();
        private SyncedStats stats;
        private int selectedEquipmentIndex = -1;
        private bool allowSelling = false;
        private GameObject equipmentSlotBorder = null;
        private float pulseSpeed = 2f;
        private float maxScale = 1.05f;
        private float minScale = 0.95f;
        private Coroutine pulseRoutine;
        public override void Initialize(PlayerID owner)
        {
            base.Initialize(owner);

            GetElementRef("sell_warning_text").SetActive(false);
            GetElementRef("equipment_slot_border1").SetActive(false);
            GetElementRef("equipment_slot_border2").SetActive(false);
            GetElementRef("equipment_slot_border3").SetActive(false);
            GetElementRef("equipment_slot_border4").SetActive(false);
            GetElementRef("equipment_slot_border5").SetActive(false);

            ListenForButtonPress("equipment_slot1", GetButtonRef => {ToggleSellButton(0);});
            ListenForButtonPress("equipment_slot2", GetButtonRef => {ToggleSellButton(1);});
            ListenForButtonPress("equipment_slot3", GetButtonRef => {ToggleSellButton(2);});
            ListenForButtonPress("equipment_slot4", GetButtonRef => {ToggleSellButton(3);});
            ListenForButtonPress("equipment_slot5", GetButtonRef => {ToggleSellButton(4);});

            ListenForButtonPress("equipment_sell_button", GetButtonRef => {
                SellEquipment();
            });

            ListenForButtonPress("to_equipment_button", GetButtonRef => {
                SoundFXManager.Instance?.PlayLocalSound(buttonSFX, transform.position);
                UiManager.CloseScreen("InventoryShopUI");
                UiManager.OpenScreen("EquipmentShopUI", owner);
            });

            UpdateUpgrades();
        }
        void FixedUpdate()
        {
            EditText("gold_text",$"{ShopEntryPoint.localShopInstance.GetGold()}");
            UpdateEquipment();
            UpdateStats();
        }
        public void ToggleSellButton(int index)
        {
            if (selectedEquipmentIndex == index)
            {
                if (index >= shownEquipment.Count) {
                    return;
                }
                allowSelling = !allowSelling;
                if (allowSelling)
                {
                    GetElementRef("equipment_sell_button").GetComponent<Image>().color = Color.red;
                    equipmentSlotBorder = GetElementRef("equipment_slot_border" + (selectedEquipmentIndex + 1));
                    equipmentSlotBorder.SetActive(true);
                    StartPulse();
                    GetElementRef("sell_warning_text").SetActive(true);
                    EditText("sell_warning_text", $"({shownEquipment[selectedEquipmentIndex].Name} sells for ${(int)(shownEquipment[selectedEquipmentIndex].Cost * 0.5)})");
                }
                else
                {
                    GetElementRef("equipment_sell_button").GetComponent<Image>().color = Color.white;
                    equipmentSlotBorder.SetActive(false);
                    StopPulse();
                    GetElementRef("sell_warning_text").SetActive(false);
                }
            }
            else
            {
                if (index >= shownEquipment.Count) {
                    return;
                }
                if (selectedEquipmentIndex != -1)
                {
                    equipmentSlotBorder.SetActive(false);
                    StopPulse();
                }
                selectedEquipmentIndex = index;
                allowSelling = true;
                GetElementRef("equipment_sell_button").GetComponent<Image>().color = Color.red;
                equipmentSlotBorder = GetElementRef("equipment_slot_border" + (selectedEquipmentIndex + 1));
                equipmentSlotBorder.SetActive(true);
                StartPulse();
                GetElementRef("sell_warning_text").SetActive(true);
                EditText("sell_warning_text", $"(<color=#FFFF00>{shownEquipment[selectedEquipmentIndex].Name}</color> will <color=#FF0000>Sell</color> for <color=#00FF00>${(int)(shownEquipment[selectedEquipmentIndex].Cost * 0.5)}</color>)");
            }
        }

        public void StartPulse()
        {
            if (pulseRoutine == null)
            {
                pulseRoutine = StartCoroutine(PulseRoutine());
            }
        }
        public void StopPulse()
        {
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
                equipmentSlotBorder.transform.localScale = Vector3.one;
            }
        }
        private IEnumerator PulseRoutine()
        {
            while (true)
            {
                float lerpStep = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                lerpStep = Mathf.SmoothStep(0f, 1f, lerpStep);
                float currentScale = Mathf.Lerp(minScale, maxScale, lerpStep);
                equipmentSlotBorder.transform.localScale = Vector3.one * currentScale;
                yield return null;
            }
        }

        public void UpdateEquipment()
        {
            shownEquipment = ShopEntryPoint.localShopInstance.GetHeldEquipment();
            int eqA = shownEquipment.Count;
            for (int i = 0; i < 5; i++)
            {
                if (i < eqA)
                {
                    GetImageRef("equipment_slot" + (i + 1)).Edit(shownEquipment[i].Icon);
                }
                else
                {
                    GetImageRef("equipment_slot" + (i + 1)).Edit(defaultSlotIcon);
                }
            }
        }
        public void UpdateUpgrades()
        {
            shownUpgrade = ShopEntryPoint.localShopInstance.GetHeldUpgrades();
            int uA = shownUpgrade.Count;
            for (int i = 0; i < 5; i++)
            {
                if (i < uA)
                {
                    GetImageRef("upgrade_slot" + (i + 1)).Edit(shownUpgrade[i].Icon);
                }
                else
                {
                    GetImageRef("upgrade_slot" + (i + 1)).Edit(defaultSlotIcon);
                }
            }
        }
        public void UpdateStats()
        {
            stats = ShopEntryPoint.localShopInstance.GetStats();

            EditText("character_name_text", stats.Name.value);
            if (stats.Name.value == "Yarrow")
            {
                GetTextRef("character_name_text").SetColor(new Color(65/255f, 98/255f, 108/255f));
            }
            else if (stats.Name.value == "Amber")
            {
                GetTextRef("character_name_text").SetColor(new Color(186/255f, 106/255f, 54/255f));
            }
            else if (stats.Name.value == "Linay")
            {
                GetTextRef("character_name_text").SetColor(new Color(97/255f, 35/255f, 79/255f));
            }
            else if (stats.Name.value == "Ruru")
            {
                GetTextRef("character_name_text").SetColor(new Color(82/255f, 15/255f, 7/255f));
            }
            else if (stats.Name.value == "Yew")
            {
                GetTextRef("character_name_text").SetColor(new Color(29/255f, 51/255f, 21/255f));
            }
            GetImageRef("character_icon").Edit(stats.Icon);

            EditText("hp_value_text", ((int)stats.MaximumHealth.value).ToString());
            EditText("regeneration_value_text", ((int)stats.PassiveRegen.value).ToString());
            EditText("movespeed_value_text", ((int)stats.MovementSpeed.value).ToString());
            EditText("power_value_text", ((int)stats.AttackPower.value).ToString());
            EditText("resistance_value_text", ((int)stats.Resistance.value).ToString());
            EditText("scale_value_text", ((int)stats.Scale.value).ToString());
        }
        public void SellEquipment()
        {
            if (!allowSelling) { return; }
            bool attempt = ShopEntryPoint.localShopInstance.SellEquipment(shownEquipment[selectedEquipmentIndex]);
            if (attempt) {
                SoundFXManager.Instance?.PlayLocalSound(sellSFX, transform.position);
                GetElementRef("equipment_sell_button").GetComponent<Image>().color = Color.white;
                equipmentSlotBorder.SetActive(false);
                StopPulse();
                GetElementRef("sell_warning_text").SetActive(false);
            }
        }
    }
}