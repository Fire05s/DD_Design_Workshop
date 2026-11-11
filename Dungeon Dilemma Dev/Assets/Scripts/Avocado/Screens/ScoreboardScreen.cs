using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PurrNet;
using ControllerLogic;
using Steamworks;
using Unity.VisualScripting;
using Items;
using System.Collections;
using Sync_Modules;

namespace Avocado.Screens {
    public class ScoreboardScreen : UiScreen {
        [SerializeField] private Sprite gridSlot;
        [SerializeField] private Sprite upgradeGridSlot;
        private List<SyncedStats> sortedStats = new();

        public override void Initialize(PlayerID owner) {
            base.Initialize(owner);
            // Lives
            AvocadoImage challengerLife1 = GetImageRef("challenger_life1");
            AvocadoImage challengerLife2 = GetImageRef("challenger_life2");
            AvocadoImage challengerLife3 = GetImageRef("challenger_life3");
            AvocadoImage dbLife1 = GetImageRef("dungeon_boss_life1");
            AvocadoImage dbLife2 = GetImageRef("dungeon_boss_life2");
            AvocadoImage dbLife3 = GetImageRef("dungeon_boss_life3");

            // Challenger Placement Groups
            GameObject firstPlaceGroup = GetElementRef("FirstPlace");
            GameObject secondPlaceGroup = GetElementRef("SecondPlace");
            GameObject thirdPlaceGroup = GetElementRef("ThirdPlace");
            GameObject fourthPlaceGroup = GetElementRef("FourthPlace");

            // Scores
            AvocadoText firstScore = GetTextRef("first_place_score");
            AvocadoText secondScore = GetTextRef("second_place_score");
            AvocadoText thirdScore = GetTextRef("third_place_score");
            AvocadoText fourthScore = GetTextRef("fourth_place_score");


            // Character Icons
            AvocadoImage firstIcon = GetImageRef("first_place_character_icon");
            AvocadoImage secondIcon = GetImageRef("second_place_character_icon");
            AvocadoImage thirdIcon = GetImageRef("third_place_character_icon");
            AvocadoImage fourthIcon = GetImageRef("fourth_place_character_icon");
            AvocadoImage dbIcon = GetImageRef("dungeon_boss_icon");

            // First Place Slots
            AvocadoImage firstPlaceSlot1 = GetImageRef("first_place_slot1");
            AvocadoImage firstPlaceSlot2 = GetImageRef("first_place_slot2");
            AvocadoImage firstPlaceSlot3 = GetImageRef("first_place_slot3");
            AvocadoImage firstPlaceSlot4 = GetImageRef("first_place_slot4");
            AvocadoImage firstPlaceSlot5 = GetImageRef("first_place_slot5");
            GameObject firstPlaceSlotGameObject1 = GetElementRef("first_place_slot1");
            GameObject firstPlaceSlotGameObject2 = GetElementRef("first_place_slot2");
            GameObject firstPlaceSlotGameObject3 = GetElementRef("first_place_slot3");
            GameObject firstPlaceSlotGameObject4 = GetElementRef("first_place_slot4");
            GameObject firstPlaceSlotGameObject5 = GetElementRef("first_place_slot5");

            // Second Place Slots
            AvocadoImage secondPlaceSlot1 = GetImageRef("second_place_slot1");
            AvocadoImage secondPlaceSlot2 = GetImageRef("second_place_slot2");
            AvocadoImage secondPlaceSlot3 = GetImageRef("second_place_slot3");
            AvocadoImage secondPlaceSlot4 = GetImageRef("second_place_slot4");
            AvocadoImage secondPlaceSlot5 = GetImageRef("second_place_slot5");
            GameObject secondPlaceSlotGameObject1 = GetElementRef("second_place_slot1");
            GameObject secondPlaceSlotGameObject2 = GetElementRef("second_place_slot2");
            GameObject secondPlaceSlotGameObject3 = GetElementRef("second_place_slot3");
            GameObject secondPlaceSlotGameObject4 = GetElementRef("second_place_slot4");
            GameObject secondPlaceSlotGameObject5 = GetElementRef("second_place_slot5");

            // Third Place Slots
            AvocadoImage thirdPlaceSlot1 = GetImageRef("third_place_slot1");
            AvocadoImage thirdPlaceSlot2 = GetImageRef("third_place_slot2");
            AvocadoImage thirdPlaceSlot3 = GetImageRef("third_place_slot3");
            AvocadoImage thirdPlaceSlot4 = GetImageRef("third_place_slot4");
            AvocadoImage thirdPlaceSlot5 = GetImageRef("third_place_slot5");
            GameObject thirdPlaceSlotGameObject1 = GetElementRef("third_place_slot1");
            GameObject thirdPlaceSlotGameObject2 = GetElementRef("third_place_slot2");
            GameObject thirdPlaceSlotGameObject3 = GetElementRef("third_place_slot3");
            GameObject thirdPlaceSlotGameObject4 = GetElementRef("third_place_slot4");
            GameObject thirdPlaceSlotGameObject5 = GetElementRef("third_place_slot5");

            // Fourth Place Slots
            AvocadoImage fourthPlaceSlot1 = GetImageRef("fourth_place_slot1");
            AvocadoImage fourthPlaceSlot2 = GetImageRef("fourth_place_slot2");
            AvocadoImage fourthPlaceSlot3 = GetImageRef("fourth_place_slot3");
            AvocadoImage fourthPlaceSlot4 = GetImageRef("fourth_place_slot4");
            AvocadoImage fourthPlaceSlot5 = GetImageRef("fourth_place_slot5");
            GameObject fourthPlaceSlotGameObject1 = GetElementRef("fourth_place_slot1");
            GameObject fourthPlaceSlotGameObject2 = GetElementRef("fourth_place_slot2");
            GameObject fourthPlaceSlotGameObject3 = GetElementRef("fourth_place_slot3");
            GameObject fourthPlaceSlotGameObject4 = GetElementRef("fourth_place_slot4");
            GameObject fourthPlaceSlotGameObject5 = GetElementRef("fourth_place_slot5");

            // Dungeon Boss Slots
            AvocadoImage dbEquipmentSlot1 = GetImageRef("dungeon_boss_equipment1");
            AvocadoImage dbEquipmentSlot2 = GetImageRef("dungeon_boss_equipment2");
            AvocadoImage dbEquipmentSlot3 = GetImageRef("dungeon_boss_equipment3");
            AvocadoImage dbEquipmentSlot4 = GetImageRef("dungeon_boss_equipment4");
            AvocadoImage dbEquipmentSlot5 = GetImageRef("dungeon_boss_equipment5");
            AvocadoImage dbUpgradeSlot1 = GetImageRef("dungeon_boss_upgrade1");
            AvocadoImage dbUpgradeSlot2 = GetImageRef("dungeon_boss_upgrade2");
            AvocadoImage dbUpgradeSlot3 = GetImageRef("dungeon_boss_upgrade3");
            AvocadoImage dbUpgradeSlot4 = GetImageRef("dungeon_boss_upgrade4");
            AvocadoImage dbUpgradeSlot5 = GetImageRef("dungeon_boss_upgrade5");

            // Switch Buttons
            AvocadoButton switchButton1 = GetButtonRef("equipment_upgrade_switch_button1");
            AvocadoButton switchButton2 = GetButtonRef("equipment_upgrade_switch_button2");
            AvocadoButton switchButton3 = GetButtonRef("equipment_upgrade_switch_button3");
            AvocadoButton switchButton4 = GetButtonRef("equipment_upgrade_switch_button4");

            // Switch States
            bool firstPlaceEquipmentViewActive = true;
            bool secondPlaceEquipmentViewActive = true;
            bool thirdPlaceEquipmentViewActive = true;
            bool fourthPlaceEquipmentViewActive = true;

            // Update lives UI
            StartCoroutine(UpdateLivesCount());

            OnIntervalRefresh(0, () => {
                sortedStats = GlobalController.Singleton?.GetAllStats().OrderByDescending(player => (float)player.Score.value).ToList();

                if (sortedStats.Count < 5)
                {
                    fourthPlaceGroup.SetActive(false);
                }
                if (sortedStats.Count < 4)
                {
                    thirdPlaceGroup.SetActive(false);
                }
                if (sortedStats.Count < 3)
                {
                    secondPlaceGroup.SetActive(false);
                }

                int playerCounts = 0;
                for (int i = 0; i < sortedStats.Count; ++i)
                {
                    SyncedStats playerStats = sortedStats[i];
                    if (!playerStats.CompareTag("Boss")) {
                        if (playerCounts == 0)
                        {
                            firstScore.Edit(FormatScore(playerStats.Score.value));
                            firstIcon.Edit(playerStats.Icon);

                            SyncedInventory inventory = playerStats.GetComponent<SyncedInventory>();

                            if (firstPlaceEquipmentViewActive)
                            {
                                firstPlaceSlot1.Edit(inventory.HeldEquipment.ElementAtOrDefault(0)?.Icon ?? gridSlot);
                                firstPlaceSlot2.Edit(inventory.HeldEquipment.ElementAtOrDefault(1)?.Icon ?? gridSlot);
                                firstPlaceSlot3.Edit(inventory.HeldEquipment.ElementAtOrDefault(2)?.Icon ?? gridSlot);
                                firstPlaceSlot4.Edit(inventory.HeldEquipment.ElementAtOrDefault(3)?.Icon ?? gridSlot);
                                firstPlaceSlot5.Edit(inventory.HeldEquipment.ElementAtOrDefault(4)?.Icon ?? gridSlot);
                            }
                            else
                            {
                                firstPlaceSlot1.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(0)?.Icon ?? upgradeGridSlot);
                                firstPlaceSlot2.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(1)?.Icon ?? upgradeGridSlot);
                                firstPlaceSlot3.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(2)?.Icon ?? upgradeGridSlot);
                                firstPlaceSlot4.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(3)?.Icon ?? upgradeGridSlot);
                                firstPlaceSlot5.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(4)?.Icon ?? upgradeGridSlot);
                            }
                        }
                        else if (playerCounts == 1 && secondPlaceGroup.activeInHierarchy)
                        {
                            secondScore.Edit(FormatScore(playerStats.Score.value));
                            secondIcon.Edit(playerStats.Icon);
                            SyncedInventory inventory = playerStats.GetComponent<SyncedInventory>();

                            if (secondPlaceEquipmentViewActive)
                            {
                                secondPlaceSlot1.Edit(inventory.HeldEquipment.ElementAtOrDefault(0)?.Icon ?? gridSlot);
                                secondPlaceSlot2.Edit(inventory.HeldEquipment.ElementAtOrDefault(1)?.Icon ?? gridSlot);
                                secondPlaceSlot3.Edit(inventory.HeldEquipment.ElementAtOrDefault(2)?.Icon ?? gridSlot);
                                secondPlaceSlot4.Edit(inventory.HeldEquipment.ElementAtOrDefault(3)?.Icon ?? gridSlot);
                                secondPlaceSlot5.Edit(inventory.HeldEquipment.ElementAtOrDefault(4)?.Icon ?? gridSlot);
                            }
                            else
                            {
                                secondPlaceSlot1.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(0)?.Icon ?? upgradeGridSlot);
                                secondPlaceSlot2.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(1)?.Icon ?? upgradeGridSlot);
                                secondPlaceSlot3.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(2)?.Icon ?? upgradeGridSlot);
                                secondPlaceSlot4.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(3)?.Icon ?? upgradeGridSlot);
                                secondPlaceSlot5.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(4)?.Icon ?? upgradeGridSlot);
                            }
                        }
                        else if (playerCounts == 2 && thirdPlaceGroup.activeInHierarchy)
                        {
                            thirdScore.Edit(FormatScore(playerStats.Score.value));
                            thirdIcon.Edit(playerStats.Icon);
                            SyncedInventory inventory = playerStats.GetComponent<SyncedInventory>();

                            if (thirdPlaceEquipmentViewActive)
                            {
                                thirdPlaceSlot1.Edit(inventory.HeldEquipment.ElementAtOrDefault(0)?.Icon ?? gridSlot);
                                thirdPlaceSlot2.Edit(inventory.HeldEquipment.ElementAtOrDefault(1)?.Icon ?? gridSlot);
                                thirdPlaceSlot3.Edit(inventory.HeldEquipment.ElementAtOrDefault(2)?.Icon ?? gridSlot);
                                thirdPlaceSlot4.Edit(inventory.HeldEquipment.ElementAtOrDefault(3)?.Icon ?? gridSlot);
                                thirdPlaceSlot5.Edit(inventory.HeldEquipment.ElementAtOrDefault(4)?.Icon ?? gridSlot);
                            }
                            else
                            {
                                thirdPlaceSlot1.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(0)?.Icon ?? upgradeGridSlot);
                                thirdPlaceSlot2.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(1)?.Icon ?? upgradeGridSlot);
                                thirdPlaceSlot3.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(2)?.Icon ?? upgradeGridSlot);
                                thirdPlaceSlot4.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(3)?.Icon ?? upgradeGridSlot);
                                thirdPlaceSlot5.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(4)?.Icon ?? upgradeGridSlot);
                            }
                        }
                        else if (playerCounts == 3 && fourthPlaceGroup.activeInHierarchy)
                        {
                            fourthScore.Edit(FormatScore(playerStats.Score.value));
                            fourthIcon.Edit(playerStats.Icon);
                            SyncedInventory inventory = playerStats.GetComponent<SyncedInventory>();

                            if (fourthPlaceEquipmentViewActive)
                            {
                                fourthPlaceSlot1.Edit(inventory.HeldEquipment.ElementAtOrDefault(0)?.Icon ?? gridSlot);
                                fourthPlaceSlot2.Edit(inventory.HeldEquipment.ElementAtOrDefault(1)?.Icon ?? gridSlot);
                                fourthPlaceSlot3.Edit(inventory.HeldEquipment.ElementAtOrDefault(2)?.Icon ?? gridSlot);
                                fourthPlaceSlot4.Edit(inventory.HeldEquipment.ElementAtOrDefault(3)?.Icon ?? gridSlot);
                                fourthPlaceSlot5.Edit(inventory.HeldEquipment.ElementAtOrDefault(4)?.Icon ?? gridSlot);
                            }
                            else
                            {
                                fourthPlaceSlot1.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(0)?.Icon ?? upgradeGridSlot);
                                fourthPlaceSlot2.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(1)?.Icon ?? upgradeGridSlot);
                                fourthPlaceSlot3.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(2)?.Icon ?? upgradeGridSlot);
                                fourthPlaceSlot4.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(3)?.Icon ?? upgradeGridSlot);
                                fourthPlaceSlot5.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(4)?.Icon ?? upgradeGridSlot);
                            }
                        }

                        ++playerCounts;
                    }
                    else
                    {
                        dbIcon.Edit(playerStats.Icon);
                        SyncedInventory inventory = playerStats.GetComponent<SyncedInventory>();
                        dbEquipmentSlot1.Edit(inventory.HeldEquipment.ElementAtOrDefault(0)?.Icon ?? gridSlot);
                        dbEquipmentSlot2.Edit(inventory.HeldEquipment.ElementAtOrDefault(1)?.Icon ?? gridSlot);
                        dbEquipmentSlot3.Edit(inventory.HeldEquipment.ElementAtOrDefault(2)?.Icon ?? gridSlot);
                        dbEquipmentSlot4.Edit(inventory.HeldEquipment.ElementAtOrDefault(3)?.Icon ?? gridSlot);
                        dbEquipmentSlot5.Edit(inventory.HeldEquipment.ElementAtOrDefault(4)?.Icon ?? gridSlot);

                        dbUpgradeSlot1.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(0)?.Icon ?? upgradeGridSlot);
                        dbUpgradeSlot2.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(1)?.Icon ?? upgradeGridSlot);
                        dbUpgradeSlot3.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(2)?.Icon ?? upgradeGridSlot);
                        dbUpgradeSlot4.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(3)?.Icon ?? upgradeGridSlot);
                        dbUpgradeSlot5.Edit(inventory.ActiveUpgrades.ElementAtOrDefault(4)?.Icon ?? upgradeGridSlot);
                    }
                }
            });

            ListenForButtonPress("equipment_upgrade_switch_button1", button =>
            {
                if (firstPlaceEquipmentViewActive)
                {
                    firstPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    firstPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    firstPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    firstPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    firstPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                }
                else
                {
                    firstPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    firstPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    firstPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    firstPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    firstPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                }
                firstPlaceEquipmentViewActive = !firstPlaceEquipmentViewActive;
            });

            ListenForButtonPress("equipment_upgrade_switch_button2", button =>
            {
                if (secondPlaceEquipmentViewActive)
                {
                    secondPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    secondPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    secondPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    secondPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    secondPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                }
                else
                {
                    secondPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    secondPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    secondPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    secondPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    secondPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                }
                secondPlaceEquipmentViewActive = !secondPlaceEquipmentViewActive;
            });

            ListenForButtonPress("equipment_upgrade_switch_button3", button =>
            {
                if (thirdPlaceEquipmentViewActive)
                {
                    thirdPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    thirdPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    thirdPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    thirdPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    thirdPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                }
                else
                {
                    thirdPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    thirdPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    thirdPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    thirdPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    thirdPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                }
                thirdPlaceEquipmentViewActive = !thirdPlaceEquipmentViewActive;
            });

            ListenForButtonPress("equipment_upgrade_switch_button4", button =>
            {
                if (fourthPlaceEquipmentViewActive)
                {
                    fourthPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    fourthPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    fourthPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    fourthPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                    fourthPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Upgrade;
                }
                else
                {
                    fourthPlaceSlotGameObject1.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    fourthPlaceSlotGameObject2.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    fourthPlaceSlotGameObject3.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    fourthPlaceSlotGameObject4.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                    fourthPlaceSlotGameObject5.GetComponent<ScoreboardHoverTip>().hoverType = ScoreboardHoverTip.ScoreboardHoverType.Equipment;
                }
                fourthPlaceEquipmentViewActive = !fourthPlaceEquipmentViewActive;
            });
        }

        public string FormatScore(double number)
        {
            if (number >= 1_000_000_000)
                return (number / 1_000_000_000d).ToString("0.#") + "B";

            if (number >= 1_000_000)
                return (number / 1_000_000d).ToString("0.#") + "M";

            if (number >= 1_000)
                return (number / 1_000d).ToString("0.#") + "K";

            return number.ToString("0");
        }

        private IEnumerator UpdateLivesCount()
        {
            yield return new WaitForSeconds(0.1f); // wait one frame for lives to update

            SyncedWorld _world = GlobalController.Singleton.GetWorldData();
            for (int i = 1; i < 4; i++)
            {
                GetImageRef($"challenger_life{i}").ToggleVisibility(i <= _world.ChallengerLives.value);
                GetImageRef($"dungeon_boss_life{i}").ToggleVisibility(i <= _world.BossLives.value);
                GetImageRef($"challenger_broken_life{i}").ToggleVisibility(i > _world.ChallengerLives.value);
                GetImageRef($"dungeon_boss_broken_life{i}").ToggleVisibility(i > _world.BossLives.value);
            }
        }
    }
}