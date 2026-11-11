using Items;
using PurrNet;
using Shop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Avocado.Screens {
    public class UpgradeScreen: UiScreen {
        [SerializeField] CanvasGroup dialogCanvasGroup;
        [SerializeField] AudioClip upgradeSelectSFX;
        [SerializeField] AudioClip upgradeRerollSFX;
        [SerializeField] AudioClip buttonSFX;
        List<ShopItem> shownUpgrades = new List<ShopItem>();
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

            shopkeeper = GetElementRef("shopkeeper");
            shopkeeperOrigin = shopkeeper.transform.position;
            dialogText = GetTextRef("dialog_text");

            GetElementRef("confirm_1").SetActive(false);
            GetElementRef("confirm_2").SetActive(false);
            GetElementRef("confirm_3").SetActive(false);

            ListenForButtonPress("to_equipment_button", GetButtonRef => {
                SoundFXManager.Instance?.PlayLocalSound(buttonSFX, transform.position);
                UiManager.CloseScreen("UpgradeShopUI");
                UiManager.OpenScreen("EquipmentShopUI", owner);
            });
            
            ListenForButtonPress("upgrade_reroll_button1", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeRerollSFX, transform.position);
                RerollSlot(0);
            });
            ListenForButtonPress("upgrade_reroll_button2", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeRerollSFX, transform.position);
                RerollSlot(1);
            });
            ListenForButtonPress("upgrade_reroll_button3", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeRerollSFX, transform.position);
                RerollSlot(2);
            });

            ListenForButtonPress("upgrade_slot_button1", button=> {
                if (!ShopEntryPoint.localShopInstance.upgradePicked) {
                    SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                    ToggleSelectView(0);
                }
            });
            ListenForButtonPress("upgrade_slot_button2", button=> {
                if (!ShopEntryPoint.localShopInstance.upgradePicked) {
                    SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                    ToggleSelectView(1);
                }
            });
            ListenForButtonPress("upgrade_slot_button3", button=> {
                if (!ShopEntryPoint.localShopInstance.upgradePicked) {
                    SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                    ToggleSelectView(2);
                }
            });

            ListenForButtonPress("select_button_1", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                PickUpgrade(0);
                StartUpgradeSelectedDialog();
            });

            ListenForButtonPress("select_button_2", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                PickUpgrade(1);
                StartUpgradeSelectedDialog();
            });

            ListenForButtonPress("select_button_3", button=> {
                SoundFXManager.Instance?.PlayLocalSound(upgradeSelectSFX, transform.position);
                PickUpgrade(2);
                StartUpgradeSelectedDialog();
            });

            shownUpgrades = ShopEntryPoint.localShopInstance.InitialRollUpgrade();
            PopulateUpgrades();
            if (ShopEntryPoint.localShopInstance.upgradePicked)
            {
                StartDialog("You have already selected this Upgrade for this round.");
            }
            else if (ShopEntryPoint.localShopInstance.IsBoss)
            {
                StartDialog(References.Instance.ShopDialog.DBUpgShopOpenDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.DBUpgShopOpenDialogData.Count)]
                    .DialogLine);
            }
            else
            {
                StartDialog(References.Instance.ShopDialog.CUpgShopOpenDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.CUpgShopOpenDialogData.Count)]
                    .DialogLine);
            }
        }
        void FixedUpdate()
        {
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
        }
        private void PopulateUpgrades()
        {
            if (ShopEntryPoint.localShopInstance.upgradePicked)
            {
                for (int i = 0; i < 3; i++)
                {
                    GetElementRef("upgrade_reroll_button" + (i + 1)).SetActive(false);
                    if (i == 1)
                    {
                        GetElementRef("UpgradeSlot" + (i + 1)).SetActive(true);
                        continue;
                    }
                    GetElementRef("UpgradeSlot" + (i + 1)).SetActive(false);
                }
                GetImageRef("upgrade_icon2").Edit(ShopEntryPoint.localShopInstance.chosenUpgrade.image);
                EditText("upgrade_title2", ShopEntryPoint.localShopInstance.chosenUpgrade.name);
                EditText("upgrade_ability2", ShopEntryPoint.localShopInstance.chosenUpgrade.ability);
                EditText("upgrade_flavor2", ShopEntryPoint.localShopInstance.chosenUpgrade.flavortext);
            }
            else
            {
                ShopItem curItem;
                for (int i = 0; i < 3; i++)
                {
                    curItem = shownUpgrades[i];
                    GetImageRef("upgrade_icon" + (i + 1)).Edit(curItem.image);
                    EditText("upgrade_title" + (i + 1), curItem.name);
                    EditText("upgrade_ability" + (i + 1), curItem.ability);
                    EditText("upgrade_flavor" + (i + 1), curItem.flavortext);
                    if (ShopEntryPoint.localShopInstance.rerolledUpgrades[i])
                    {
                        GetButtonRef("upgrade_reroll_button" + (i + 1)).State.Disabled = true;
                        GetElementRef("upgrade_reroll_button" + (i + 1)).GetComponent<Image>().color = Color.gray;
                    }
                }
            }
        }
        
        public void ToggleSelectView(int index)
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
                for (int i = 0; i < 3; i++)
                {
                    GetElementRef("confirm_" + (i + 1)).SetActive(false);
                }
                GetElementRef("confirm_" + (index + 1)).SetActive(true);
            }
        }

        private void RerollSlot(int index)
        {
            shownUpgrades = ShopEntryPoint.localShopInstance.RerollUpgrade(index);
            ShopEntryPoint.localShopInstance.rerolledUpgrades[index] = true;
            GetButtonRef("upgrade_reroll_button" + (index + 1)).State.Disabled = true;
            GetElementRef("upgrade_reroll_button" + (index + 1)).GetComponent<Image>().color = Color.gray;
            GetElementRef("confirm_" + (index + 1)).SetActive(false);
            PopulateUpgrades();
        }
        private void PickUpgrade(int index)
        {
            ShopEntryPoint.localShopInstance.BuyUpgradeItem(index);
            ShopEntryPoint.localShopInstance.upgradePicked = true;
            ShopEntryPoint.localShopInstance.chosenUpgrade = shownUpgrades[index];
            for (int i = 0; i < 3; i++)
            {
                GetElementRef("upgrade_reroll_button" + (i + 1)).SetActive(false);
                if (i == index)
                {
                    GetElementRef("confirm_" + (i + 1)).SetActive(false);
                    continue;
                }
                GetElementRef("UpgradeSlot" + (i + 1)).SetActive(false);
            }
        }

        public void StartUpgradeSelectedDialog()
        {
            if (ShopEntryPoint.localShopInstance.IsBoss)
            {
                StartDialog(References.Instance.ShopDialog.DBUpgShopSelectDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.DBUpgShopSelectDialogData.Count)]
                    .DialogLine);
            }
            else
            {
                StartDialog(References.Instance.ShopDialog.CUpgShopSelectDialogData[
                    UnityEngine.Random.Range(0, References.Instance.ShopDialog.CUpgShopSelectDialogData.Count)]
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