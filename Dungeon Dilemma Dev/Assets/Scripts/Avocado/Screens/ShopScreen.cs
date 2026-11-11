using Items;
using PurrNet;
using Shop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Avocado.Screens {
    public class ShopScreen: UiScreen {
        [SerializeField] private Sprite hpSprite;
        [SerializeField] private Sprite regenSprite;
        [SerializeField] private Sprite spdSprite;
        [SerializeField] private Sprite pwrSprite;
        [SerializeField] private Sprite resSprite;
        [SerializeField] private Sprite scaleSprite;
        [SerializeField] private Sprite cdSprite;
        [SerializeField] private Sprite mcSprite;
        [SerializeField] private Sprite tcSprite;
        [SerializeField] private Sprite paSprite;
        [SerializeField] CanvasGroup dialogCanvasGroup;
        [SerializeField] AudioClip purchaseSFX;
        [SerializeField] AudioClip rerollSFX;
        [SerializeField] AudioClip buttonSFX;
        private List<ShopItem> inShop = new List<ShopItem>();
        private AvocadoButton rerollRope;
        private GameObject shopkeeper;
        private Vector3 shopkeeperOrigin;
        private AvocadoText dialogText;
        private int lastSlotInspected = -1;
        private bool lastSlotConfirmShown = false;
        private float shopkeeperTalkingSpeed = 0.05f;
        private bool shopkeeperTalking = false;
        private bool shopkeeperBobbing = false;
        private float waitBeforeFade = 2.0f;
        private float fadeDuration = 1.0f;
        public override void Initialize(PlayerID owner)
        {
            base.Initialize(owner);

            ListenForButtonPress("reroll_button", button => {Reroll();});

            rerollRope = GetButtonRef("reroll_button");
            shopkeeper = GetElementRef("shopkeeper");
            shopkeeperOrigin = shopkeeper.transform.position;
            dialogText = GetTextRef("dialog_text");
            DisplayShop();

            GetElementRef("confirm_1").SetActive(false);
            GetElementRef("confirm_2").SetActive(false);
            GetElementRef("confirm_3").SetActive(false);
            GetElementRef("confirm_4").SetActive(false);

            GetElementRef("AlertIcon").SetActive(!ShopEntryPoint.localShopInstance.upgradePicked);

            ListenForButtonPress("equipment_slot_button_1", button=> {
                ToggleBuyView(0);
            });
            ListenForButtonPress("equipment_slot_button_2", button=> {
                ToggleBuyView(1);
            });
            ListenForButtonPress("equipment_slot_button_3", button=> {
                ToggleBuyView(2);
            });
            ListenForButtonPress("equipment_slot_button_4", button=> {
                ToggleBuyView(3);
            });

            ListenForButtonPress("buy_button_1", button=> {
                int attempt = ShopEntryPoint.localShopInstance.BuyShopItem(0);
                if (attempt == 1) {
                    SoundFXManager.Instance?.PlayLocalSound(purchaseSFX, transform.position);
                    GetElementRef("shop_slot_1").SetActive(false);
                    ShopEntryPoint.localShopInstance.purchasedEquipment[0] = true;
                    StartBuyDialog();
                }
                else if (attempt == 2)
                {
                    StartDialog("You don't have enough Gold to purchase that item.");
                }
                else if (attempt == 3)
                {
                    StartDialog("You already have 5 Equipment.");
                }
            });
            ListenForButtonPress("buy_button_2", button=> {
                int attempt = ShopEntryPoint.localShopInstance.BuyShopItem(1);
                if (attempt == 1) {
                    SoundFXManager.Instance?.PlayLocalSound(purchaseSFX, transform.position);
                    GetElementRef("shop_slot_2").SetActive(false);
                    ShopEntryPoint.localShopInstance.purchasedEquipment[1] = true;;
                    StartBuyDialog();
                }
                else if (attempt == 2)
                {
                    StartDialog("You don't have enough Gold to purchase that item.");
                }
                else if (attempt == 3)
                {
                    StartDialog("You already have 5 Equipment.");
                }
            });
            ListenForButtonPress("buy_button_3", button=> {
                int  attempt = ShopEntryPoint.localShopInstance.BuyShopItem(2);
                if (attempt == 1) {
                    SoundFXManager.Instance?.PlayLocalSound(purchaseSFX, transform.position);
                    GetElementRef("shop_slot_3").SetActive(false);
                    ShopEntryPoint.localShopInstance.purchasedEquipment[2] = true;
                    StartBuyDialog();
                }
                else if (attempt == 2)
                {
                    StartDialog("You don't have enough Gold to purchase that item.");
                }
                else if (attempt == 3)
                {
                    StartDialog("You already have 5 Equipment.");
                }
            });
            ListenForButtonPress("buy_button_4", button=> {
                int attempt = ShopEntryPoint.localShopInstance.BuyShopItem(3);
                if (attempt == 1) {
                    GetElementRef("shop_slot_4").SetActive(false);
                    ShopEntryPoint.localShopInstance.purchasedEquipment[3] = true;
                    StartBuyDialog();
                }
                else if (attempt == 2)
                {
                    StartDialog("You don't have enough Gold to purchase that item.");
                }
                else if (attempt == 3)
                {
                    StartDialog("You already have 5 Equipment.");
                }
            });

            ListenForButtonPress("to_inventory_button", GetButtonRef => {
                SoundFXManager.Instance?.PlayLocalSound(buttonSFX, transform.position);
                UiManager.CloseScreen("EquipmentShopUI");
                UiManager.OpenScreen("InventoryShopUI", owner);
            });
            ListenForButtonPress("to_upgrades_button", GetButtonRef => {
                SoundFXManager.Instance?.PlayLocalSound(buttonSFX, transform.position);
                UiManager.CloseScreen("EquipmentShopUI");
                UiManager.OpenScreen("UpgradeShopUI", owner);
            });
        }
        void FixedUpdate()
        {
            EditText("gold_text",$"{ShopEntryPoint.localShopInstance.GetGold()}");
            EditText("reroll_cost_text",$"Reroll Cost: {ShopEntryPoint.localShopInstance.GetRerollCost()}");
            EditText("free_rerolls_text",$"Free Rerolls: {ShopEntryPoint.localShopInstance.GetFreeRerolls()}");
            if (shopkeeperTalking && !shopkeeperBobbing)
            {
                StartCoroutine(TalkingShopkeeper());
            }
            else if (!shopkeeperTalking)
            {
                shopkeeperBobbing = false;
                StopAllCoroutines();
                shopkeeper.transform.position = shopkeeperOrigin;
            }

            if (ShopEntryPoint.localShopInstance.CanRerollShop())
            {
                rerollRope.State.Disabled = false;
                bool emptyShop = true;
                for (int i = 0; i < inShop.Count; i++)
                {
                    if (!ShopEntryPoint.localShopInstance.purchasedEquipment[i])
                    {
                        emptyShop = false;
                    }
                }
                if (emptyShop)
                {
                    GetElementRef("reroll_outline").SetActive(true);
                }
                else
                {
                    GetElementRef("reroll_outline").SetActive(false);
                }
            }
            else
            {
                rerollRope.State.Disabled = true;
                GetElementRef("reroll_outline").SetActive(false);
            }
        }
        public void DisplayShop()
        {
            inShop = ShopEntryPoint.localShopInstance.InitialRollShop();
            if (ShopEntryPoint.localShopInstance.IsBoss)
            {
                StartDialog(References.Instance.ShopDialog.DBShopOpenDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.DBShopOpenDialogData.Count)]
                    .DialogLine);
            }
            else
            {
                StartDialog(References.Instance.ShopDialog.CShopOpenDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.CShopOpenDialogData.Count)]
                    .DialogLine);
            }
            PopulateShop();
        }
        public void ToggleBuyView(int index)
        {
            if (lastSlotInspected == index)
            {
                lastSlotConfirmShown = !lastSlotConfirmShown;
                GetElementRef("confirm_" + (index + 1)).SetActive(lastSlotConfirmShown);
            }
            else
            {
                lastSlotInspected = index;
                lastSlotConfirmShown = true;
                for (int i = 0; i < 4; i++)
                {
                    if (i >= inShop.Count || i == index)
                    {
                        continue;
                    }
                    GetElementRef("confirm_" + (i + 1)).SetActive(false);
                }
                GetElementRef("confirm_" + (index + 1)).SetActive(true);
                if (ShopEntryPoint.localShopInstance.CanBuyShopItem(index))
                {
                    GetElementRef("buy_button_" + (index + 1)).GetComponent<Image>().color = Color.green;
                }
                else
                {
                    GetElementRef("buy_button_" + (index + 1)).GetComponent<Image>().color = Color.gray;
                }
            }
        }
        public void Reroll()
        {
            if (!ShopEntryPoint.localShopInstance.CanRerollShop())
            {
                StartDialog("You don't have enough Gold to Reroll the Shop.");
                return;
            }
            SoundFXManager.Instance?.PlayLocalSound(rerollSFX, transform.position);
            inShop = ShopEntryPoint.localShopInstance.RerollShop();
            for (int i = 0; i < 4; i++)
            {
                if (i >= inShop.Count)
                {
                    GetElementRef("shop_slot_" + (i + 1)).SetActive(false);
                    continue;
                }
                ShopEntryPoint.localShopInstance.purchasedEquipment[i] = false;
                GetElementRef("shop_slot_" + (i + 1)).SetActive(true);
                GetElementRef("confirm_" + (i + 1)).SetActive(false);
            }
            PopulateShop();
        }
        public void PopulateShop()
        {
            int statLine;
            ShopItem curItem;
            for (int i = 0; i < 4; i++)
            {
                if (i >= inShop.Count)
                {
                    GetElementRef("shop_slot_" + (i + 1)).SetActive(false);
                    return;
                }
                curItem = inShop[i];
                if (ShopEntryPoint.localShopInstance.purchasedEquipment[i])
                {
                    GetElementRef("shop_slot_" + (i + 1)).SetActive(false);
                    continue;
                }
                GetImageRef("equipment_icon_" + (i + 1)).Edit(curItem.image);
                EditText("equipment_title_" + (i + 1), curItem.name);
                EditText("equipment_ability_text_" + (i + 1), curItem.ability);
                EditText("equipment_flavor_text_" + (i + 1), curItem.flavortext);
                EditText("equipment_cost_" + (i + 1), $"${curItem.cost - (int)(curItem.cost * ShopEntryPoint.localShopInstance.discount)}");
                statLine = 1;
                if (curItem.HP != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Health");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(hpSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.HP.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(255/255f, 93/255f, 90/255f));
                    statLine++;
                }
                if (curItem.Regen != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Regeneration");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(regenSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Regen.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(87/255f, 232/255f, 108/255f));
                    statLine++;
                }
                if (curItem.Speed != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Movespeed");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(spdSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Speed.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(214/255f, 214/255f, 84/255f));
                    statLine++;
                }
                if (curItem.Power != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Power");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(pwrSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Power.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(255/255f, 83/255f, 255/255f));
                    statLine++;
                }
                if (curItem.Resistance != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Resistance");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(resSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Resistance.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(115/255f, 115/255f, 255/255f));
                    statLine++;
                }
                if (curItem.Scale != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Scale");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(scaleSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Scale.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(84/255f, 215/255f, 215/255f));
                    statLine++;
                }
                if (curItem.Cooldown != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Cooldown");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(cdSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.Cooldown.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(214/255f, 214/255f, 84/255f));
                    statLine++;
                }
                if (curItem.MinionCap != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Minion Cap");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(mcSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.MinionCap.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(255/255f, 174/255f, 103/255f));
                    statLine++;
                }
                if (curItem.TrapCap != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Trap Cap");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(tcSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.TrapCap.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(255/255f, 174/255f, 103/255f));
                    statLine++;
                }
                if (curItem.PurchaseAmount != 0)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(true);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(true);
                    EditText("stat_text" + statLine + "_" + (i + 1), "Amount");
                    GetImageRef("stat_icon" + statLine + "_" + (i + 1)).Edit(paSprite);
                    EditText("stat_value_text" + statLine + "_" + (i + 1), curItem.PurchaseAmount.ToString());
                    GetTextRef("stat_text" + statLine + "_" + (i + 1)).SetColor(new Color(57/255f, 60/255f, 95/255f));
                    statLine++;
                }
                for (; statLine <= 4; statLine++)
                {
                    GetElementRef("stat_text" + statLine + "_" + (i + 1)).SetActive(false);
                    GetElementRef("stat_icon" + statLine + "_" + (i + 1)).SetActive(false);
                    GetElementRef("stat_value_text" + statLine + "_" + (i + 1)).SetActive(false);
                }
            }
        }

        public void StartBuyDialog()
        {
            if (ShopEntryPoint.localShopInstance.IsBoss)
            {
                StartDialog(References.Instance.ShopDialog.DBShopBuyDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.DBShopBuyDialogData.Count)]
                    .DialogLine);
            }
            else
            {
                StartDialog(References.Instance.ShopDialog.CShopBuyDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.CShopBuyDialogData.Count)]
                    .DialogLine);
            }
        }
        public void StartDialog(string sentence)
        {
            StopAllCoroutines();
            shopkeeperBobbing = false;
            StartCoroutine(TypeSentence(sentence));
        }

        IEnumerator TypeSentence(string sentence)
        {
            shopkeeperTalking = true;
            dialogCanvasGroup.alpha = 1f;
            string curDialog = "";
            foreach (char letter in sentence.ToCharArray())
            {
                curDialog += letter;
                dialogText.Edit(curDialog);
                yield return new WaitForSeconds(shopkeeperTalkingSpeed);
            }
            shopkeeperTalking = false;

            yield return new WaitForSeconds(waitBeforeFade);

            float counter = 0f;
            while (counter < fadeDuration)
            {
                counter += Time.deltaTime;
                dialogCanvasGroup.alpha = Mathf.Lerp(1f, 0f, counter / fadeDuration);
                yield return null;
            }
        }
        IEnumerator TalkingShopkeeper()
        {
            shopkeeperBobbing = true;
            yield return new WaitForSeconds(shopkeeperTalkingSpeed * 1.5f);
            shopkeeper.transform.position += new Vector3(0, 3, 0);
            yield return new WaitForSeconds(shopkeeperTalkingSpeed * 1.5f);
            shopkeeper.transform.position += new Vector3(0, -6, 0);
            yield return new WaitForSeconds(shopkeeperTalkingSpeed * 1.5f);
            shopkeeper.transform.position += new Vector3(0, 3, 0);
            shopkeeperBobbing = false;
        }
    }
}