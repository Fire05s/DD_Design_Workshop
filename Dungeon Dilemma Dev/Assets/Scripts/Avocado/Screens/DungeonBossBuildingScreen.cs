using System.Collections.Generic;
using ControllerLogic;
using Items;
using PlayerLogic.BossLogic;
using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using Buildable = Items.Buildable;
using Sync_Modules;
using System.Collections;

namespace Avocado.Screens {
    public class DungeonBossBuildingScreen : UiScreen
    {
        private string MasterVolumeKey = "MasterVolume";
        private string SfxVolumeKey = "SfxVolume";
        private string MusicVolumeKey = "MusicVolume";
        private string CameraShakeKey = "CameraShake";
        private bool RoomOptionsDisabled = false;
        private bool PlacementModeEnabled = true;
        private CanvasGroup _phaseChangeUIGroup;
        private Coroutine _phaseChangeCoroutine;
        private float _phaseChangeFadeDuration = 0.5f;
        private float _phaseChangeDisplayDuration = 2.0f;
        private SyncedInventory _inventory;
        
        private List<AvocadoImage> _inventorySlots = new();
        private List<AvocadoText> _amountTexts = new();
        private List<Buildable> _inventoryBuildables = new();
        private AvocadoText phaseChangeTitle;
        private Image phaseChangeBar1;
        private Image phaseChangeBar2;

        // Training Dummy Health Bar
        private AvocadoSlider _tdHealthBar;
        private AvocadoText _tdLevel;
        private AvocadoText _tdPayout;
        private TDHealthHandler _tdHealthHandler;
        private Image _tdHealthBarFill;
        private Color _tdOriginalBarColor;
        private bool _tdIsFlashing = false;
        private bool _tdIsPulsing = false;
        private Coroutine _tdFlashRoutine;
        private float _tdFlashDuration = 0.25f;
        private float _tdLowHPThreshold = 0.25f;
        private float _tdPulseSpeed = 5f;
        private Color _tdFlashColor = Color.red;
        private Color _tdHealColor;
        private Color _tdPulseColor;
        
        public override void Initialize(PlayerID owner) {
            base.Initialize(owner);

            MasterVolumeKey += owner;
            SfxVolumeKey += owner;
            MusicVolumeKey += owner;
            CameraShakeKey += owner;

            _inventory = GlobalController.Singleton.GetPlayer(owner).gameObject.GetComponent<SyncedInventory>();
            
            for (int i = 1; i <= 20; i++) {
                _inventoryBuildables.Add(null);
                _inventorySlots.Add(GetImageRef("inventory_slot" + i));
                int a = i;
                
                ListenForButtonPress("inventory_slot" + i, button => {
                    if (_inventoryBuildables[a - 1] != null) {
                        DungeonBossBuildingController.Singleton.SelectBuildable(_inventoryBuildables[a - 1]);
                    }
                });

                _amountTexts.Add(GetTextRef("inventory_slot_num" + i));
            }

            GameObject placementMenu = GetElementRef("PlacementUI");
            GameObject roomBuildingMenu = GetElementRef("RoomBuildingUI");
            
            // Switch between modes
            AvocadoButton placementModeButton = GetButtonRef("placement_ui_button");
            AvocadoButton roomBuildingButton = GetButtonRef("room_building_ui_button");
            
            ListenForButtonPress("room_building_ui_button", button => {
                if (PlacementModeEnabled) {
                    PlacementModeEnabled = false;
                    DungeonBossBuildingController.Singleton.SwitchMode(DungeonBossBuildingController.Singleton.CurrentRoomMode);
                    roomBuildingMenu.SetActive(true);
                    placementMenu.SetActive(false);
                    roomBuildingButton.State.Disabled = true;
                    placementModeButton.State.Disabled = false;
                }
            });

            ListenForButtonPress("placement_ui_button", button => {
                if (!PlacementModeEnabled)
                {
                    PlacementModeEnabled = true;
                    DungeonBossBuildingController.Singleton.SwitchMode(DungeonBossBuildingController.BuildMode.BuildablePlacement);
                    placementMenu.SetActive(true);
                    roomBuildingMenu.SetActive(false);
                    UpdatePlacementMenu(owner);
                    placementModeButton.State.Disabled = true;
                    roomBuildingButton.State.Disabled = false;
                }
            });

            ListenForButtonPress("end_setup_button", button => {
                GlobalController.Singleton.SetGlobalTime(5f);
                button.State.Disabled = true;
                GetTextRef("end_setup_text").SetColor(new Color(180/255f, 180/255f, 180/255f));
            });

            // Room slots
            AvocadoImage slot1 = GetImageRef("room_slot1");
            AvocadoImage slot2 = GetImageRef("room_slot2");
            AvocadoImage slotBG1 = GetImageRef("room_slot_bg1");
            AvocadoImage slotBG2 = GetImageRef("room_slot_bg2");

            slot1.RuntimeElement.sprite = RoomBuilder.Instance.RoomOption1.Thumbnail;
            slot2.RuntimeElement.sprite = RoomBuilder.Instance.RoomOption2.Thumbnail;

            AvocadoText roomPlacementTip = GetTextRef("room_placement_tip");

            ListenForButtonPress("room_slot1", button => {
                if (RoomOptionsDisabled) return;
                DungeonBossBuildingController.Singleton.SelectRoom(RoomBuilder.Instance.RoomOption1);
            });

            ListenForButtonPress("room_slot2", button => {
                if (RoomOptionsDisabled) return;
                DungeonBossBuildingController.Singleton.SelectRoom(RoomBuilder.Instance.RoomOption2);
            });

            PlacementModeEnabled = true;
            DungeonBossBuildingController.Singleton.SwitchMode(DungeonBossBuildingController.BuildMode.BuildablePlacement);
            placementMenu.SetActive(true);
            roomBuildingMenu.SetActive(false);
            UpdatePlacementMenu(owner);
            placementModeButton.State.Disabled = true;
            roomBuildingButton.State.Disabled = false;

            AvocadoText timerText = GetTextRef("timer_text");
            OnIntervalRefresh(1, () =>
            {
                // timer ui
                float globalTime = GlobalController.Singleton.GetGlobalTime();
                int minutes = Mathf.FloorToInt(globalTime / 60f);
                int seconds = Mathf.FloorToInt(globalTime % 60f);
                timerText.Edit(string.Format("{0:00}:{1:00}", minutes, seconds));
                if (globalTime < 10)
                {
                    timerText.SetColor(Color.red);
                }
                else if (globalTime < 30)
                {
                    timerText.SetColor(Color.yellow);
                }
                else
                {
                    timerText.SetColor(Color.white);
                }

                // update room placement ui
                if (DungeonBossBuildingController.Singleton.CurrentMode
                    != DungeonBossBuildingController.BuildMode.RoomBuild) {
                    slot1.ToggleVisibility(false);
                    slot2.ToggleVisibility(false);
                    slotBG1.ToggleVisibility(false);
                    slotBG2.ToggleVisibility(false);
                    roomPlacementTip.ToggleVisibility(false);

                    RoomOptionsDisabled = true;
                }
                else
                {
                    slot1.ToggleVisibility(true);
                    slot2.ToggleVisibility(true);
                    slotBG1.ToggleVisibility(true);
                    slotBG2.ToggleVisibility(true);
                    roomPlacementTip.ToggleVisibility(true);

                    RoomOptionsDisabled = false;
                }

                if (DungeonBossBuildingController.Singleton.CurrentRoomMode 
                    == DungeonBossBuildingController.BuildMode.RoomBuild && PlacementModeEnabled)
                {
                    GetElementRef("AlertIcon").SetActive(true);
                }
                else
                {
                    GetElementRef("AlertIcon").SetActive(false);
                }

                // buildable amount display update
                UpdateAmountDisplay();
            });

            _phaseChangeUIGroup = GetElementRef("PhaseTitleUI").GetComponent<CanvasGroup>();
            phaseChangeTitle = GetTextRef("phase_title_text");
            phaseChangeBar1 = GetElementRef("phase_title_bar1").GetComponent<Image>();
            phaseChangeBar2 = GetElementRef("phase_title_bar2").GetComponent<Image>();
            _phaseChangeUIGroup.alpha = 0f;
            if (GlobalController.Singleton != null && GlobalController.Singleton.WorldData != null)
            {
                GlobalController.Singleton.WorldData.RoundState.onChanged += OnRoundStateChanged;
            }

            // Training Dummy Health Bar Setup
            _tdHealthBar = GetSliderRef("td_health_bar");
            _tdLevel = GetTextRef("training_dummy_level");
            _tdPayout = GetTextRef("training_dummy_payout");

            _tdHealthHandler = GlobalController.Singleton.TDHealthHandler;
            if (_tdHealthHandler != null)
            {
                // Get the health bar fill image from the slider
                _tdHealthBar.State.Disabled = true;
                _tdHealthBar.RawElement.maxValue = _tdHealthHandler.Stats.MaximumHealth.value;
                _tdHealthBar.RawElement.value = _tdHealthHandler.Stats.CurrentHealth.value;
                
                _tdHealthBarFill = _tdHealthBar.RawElement.fillRect.GetComponent<Image>();
                if (_tdHealthBarFill != null)
                {
                    _tdOriginalBarColor = _tdHealthBarFill.color;
                    _tdHealColor = _tdHealthHandler.HealColor;
                    _tdPulseColor = _tdHealthHandler.PulseColor;
                }

                // Bind health updates
                _tdHealthHandler.Stats.CurrentHealth.onChanged += UpdateTDHealthBar;
                _tdHealthHandler.Stats.MaximumHealth.onChanged += UpdateTDMaxHealthBar;
                _tdHealthHandler.OnAttacked += TriggerTDDamageFlash;

                // Update initial values
                UpdateTDHealthBar(_tdHealthHandler.Stats.CurrentHealth.value);
                UpdateTDMaxHealthBar(_tdHealthHandler.Stats.MaximumHealth.value);
            }

            // Bind level and payout updates
            GlobalController.Singleton.TDLevel.onChanged += UpdateTDLevel;
            UpdateTDLevel(GlobalController.Singleton.TDLevel.value);
        }

        private void Update()
        {
            // Handle training dummy health bar pulsing when health is low
            if (_tdHealthHandler != null && _tdHealthBar != null && _tdHealthBarFill != null)
            {
                // Always update the health bar value to ensure it stays in sync
                // _tdHealthBar.RawElement.value = _tdHealthHandler.Stats.CurrentHealth.value;
                // LayoutRebuilder.MarkLayoutForRebuild(_tdHealthBar.RawElement.GetComponent<RectTransform>());

                float healthPercent = Mathf.Clamp01(_tdHealthHandler.Stats.CurrentHealth.value / _tdHealthHandler.Stats.MaximumHealth.value);

                if (_tdIsPulsing && !_tdIsFlashing)
                {
                    float lerp = (Mathf.Sin(Time.time * _tdPulseSpeed) + 1f) / 2f;
                    _tdHealthBarFill.color = Color.Lerp(_tdOriginalBarColor, _tdPulseColor, lerp);
                }
                else if (!_tdIsFlashing && _tdHealthBarFill.color != _tdOriginalBarColor)
                {
                    // Reset to original color if not flashing
                    _tdHealthBarFill.color = _tdOriginalBarColor;
                }

                // Update pulsing state based on current health
                if (healthPercent <= _tdLowHPThreshold && !_tdIsFlashing)
                {
                    _tdIsPulsing = true;
                }
                else if (healthPercent > _tdLowHPThreshold && !_tdIsFlashing)
                {
                    _tdIsPulsing = false;
                }
            }
        }

        private void UpdatePlacementMenu(PlayerID id) {
            int i = 0;
            
            foreach (var b in _inventory.HeldBuildables.Keys) {
                _inventorySlots[i].ToggleVisibility(true);
                Buildable buildable = GetBuildableReference(b);
                if (buildable == null) continue;
                _inventorySlots[i].Edit(buildable.Icon);
                _inventoryBuildables[i] = buildable;

                _amountTexts[i].Edit(_inventory.GetBuildableAmount(buildable.Name).ToString());
                i++;
            }

            for (int j = i; j < _inventorySlots.Count; j++) {
                _inventorySlots[j].ToggleVisibility(false);
                _amountTexts[j].ToggleVisibility(false);
            }
        }

        private void UpdateAmountDisplay()
        {
            int i = 0;
            foreach (var b in _inventory.HeldBuildables.Keys)
            {
                Buildable buildable = GetBuildableReference(b);
                if (buildable == null) continue;
                _amountTexts[i].Edit(_inventory.GetBuildableAmount(buildable.Name).ToString());
                i++;
            }
        }

        private Buildable GetBuildableReference(string name)
        {
            Buildable foundBuildable = null;

            // Search through Traps list
            foundBuildable = References.Instance.Traps.Items.Find(x => x.Name == name);
            if (foundBuildable != null) return foundBuildable;

            // Search through Minion list
            foundBuildable = References.Instance.Minions.Items.Find(x => x.Name == name);
            
            return foundBuildable;
        }

        private void OnRoundStateChanged(RoundState newState)
        {
            if (newState == RoundState.PostRound)
            {
                return;
            }

            UpdateSettings();
            TriggerPhaseAnnouncement(newState);
        }

        private void TriggerPhaseAnnouncement(RoundState newState)
        {
            if (_phaseChangeUIGroup == null) return;

            if (_phaseChangeCoroutine != null) {
                StopCoroutine(_phaseChangeCoroutine);
            }

            _phaseChangeCoroutine = StartCoroutine(PhaseAnnouncementRoutine(newState));
        }

        private IEnumerator PhaseAnnouncementRoutine(RoundState newState)
        {
            switch (newState) {
                case RoundState.Setup:
                    phaseChangeTitle.Edit("Setup Phase");
                    phaseChangeTitle.SetColor(Color.yellow);
                    phaseChangeBar1.color = Color.yellow;
                    phaseChangeBar2.color = Color.yellow;
                    break;
                case RoundState.Raid:
                    phaseChangeTitle.Edit("Raid Phase");
                    phaseChangeTitle.SetColor(new Color(255/255f, 125/255f, 0/255f));
                    phaseChangeBar1.color = new Color(255/255f, 125/255f, 0/255f);
                    phaseChangeBar2.color = new Color(255/255f, 125/255f, 0/255f);
                    break;
                case RoundState.BossFight:
                    phaseChangeTitle.Edit("Boss Phase");
                    phaseChangeTitle.SetColor(Color.red);
                    phaseChangeBar1.color = Color.red;
                    phaseChangeBar2.color = Color.red;
                    break;
            }

            float elapsed = 0f;

            // fade in
            while (elapsed < _phaseChangeFadeDuration)
            {
                elapsed += Time.deltaTime;
                _phaseChangeUIGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / _phaseChangeFadeDuration);
                yield return null;
            }
            _phaseChangeUIGroup.alpha = 1f;

            // stay visible
            yield return new WaitForSeconds(_phaseChangeDisplayDuration);

            // fade out
            elapsed = 0f;
            while (elapsed < _phaseChangeFadeDuration)
            {
                elapsed += Time.deltaTime;
                _phaseChangeUIGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _phaseChangeFadeDuration);
                yield return null;
            }
            _phaseChangeUIGroup.alpha = 0f;
        }

        private void UpdateSettings()
        {
            float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 100f);  // Default to 100 (full volume)
            float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 100f);        // Default to 100 (full volume)
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 100f);    // Default to 100 (full volume)
            float savedCameraShake = PlayerPrefs.GetFloat(CameraShakeKey, 100f);    // Default to 100% (default shake)
            SoundFXManager.Instance?.MasterVolume(savedMasterVolume / 100f);
            MusicManager.Instance?.MasterVolume(savedMasterVolume / 100f);
            SoundFXManager.Instance?.SFXVolume(savedSfxVolume / 100f);
            MusicManager.Instance?.MusicVolume(savedMusicVolume / 100f);
            PlayerPrefs.SetFloat(MasterVolumeKey, savedMasterVolume);
            PlayerPrefs.SetFloat(SfxVolumeKey, savedSfxVolume);
            PlayerPrefs.SetFloat(MusicVolumeKey, savedMusicVolume);
            PlayerPrefs.SetFloat(CameraShakeKey, savedCameraShake);
            PlayerPrefs.Save();
        }

        private void UpdateTDMaxHealthBar(float newMax)
        {
            if (_tdHealthBar != null)
            {
                _tdHealthBar.RawElement.maxValue = newMax;
                _tdHealthBar.RawElement.value = _tdHealthHandler.Stats.CurrentHealth.value;
            }
        }

        /// <summary>
        /// Updates the training dummy health bar display and handles pulsing state.
        /// </summary>
        private void UpdateTDHealthBar(float newHealth)
        {
            if (_tdHealthHandler == null) return;

            float healthPercent = Mathf.Clamp01(newHealth / _tdHealthHandler.Stats.MaximumHealth.value);
            if (newHealth > _tdHealthBar.RawElement.value) {
                if (_tdFlashRoutine != null)
                {
                    StopCoroutine(_tdFlashRoutine);
                }
                _tdFlashRoutine = StartCoroutine(FlashTDHealthBarRoutine(_tdHealColor));
            }
            _tdHealthBar.RawElement.value = newHealth;

            // Update pulsing state based on current health
            if (healthPercent <= _tdLowHPThreshold && !_tdIsFlashing)
            {
                _tdIsPulsing = true;
            }
            else if (healthPercent > _tdLowHPThreshold)
            {
                _tdIsPulsing = false;
                if (!_tdIsFlashing && _tdHealthBarFill != null)
                {
                    _tdHealthBarFill.color = _tdOriginalBarColor;
                }
            }
        }

        /// <summary>
        /// Triggers the damage flash effect on the training dummy health bar.
        /// </summary>
        private void TriggerTDDamageFlash(Vector2 source, bool isStatusEffect)
        {
            if (_tdFlashRoutine != null)
            {
                StopCoroutine(_tdFlashRoutine);
            }
            _tdFlashRoutine = StartCoroutine(FlashTDHealthBarRoutine(Color.red));
        }

        /// <summary>
        /// Coroutine to flash the training dummy health bar between a color and the original color.
        /// </summary>
        private IEnumerator FlashTDHealthBarRoutine(Color flashColor)
        {
            _tdIsFlashing = true;
            float elapsed = 0f;

            if (_tdHealthBarFill == null) yield break;

            // Flash to the damage color
            while (elapsed < _tdFlashDuration)
            {
                elapsed += Time.deltaTime;
                _tdHealthBarFill.color = Color.Lerp(flashColor, _tdOriginalBarColor, elapsed / _tdFlashDuration);
                yield return null;
            }
            _tdHealthBarFill.color = _tdOriginalBarColor;
            _tdIsFlashing = false;

            // Check if we should start pulsing (low health)
            if (_tdHealthHandler != null && _tdHealthHandler.Stats.CurrentHealth.value / _tdHealthHandler.Stats.MaximumHealth.value <= _tdLowHPThreshold)
            {
                _tdIsPulsing = true;
            }
        }

        /// <summary>
        /// Updates the training dummy level display and calculates the payout.
        /// </summary>
        private void UpdateTDLevel(int newLevel)
        {
            _tdLevel.Edit($"Level {newLevel}");

            // Calculate payout based on level
            int goldPayout = GlobalController.Singleton.TDBasePayout * (int)System.Math.Pow(2, newLevel - 1);
            _tdPayout.Edit($"{goldPayout} Gold Payout");
        }
    }
}