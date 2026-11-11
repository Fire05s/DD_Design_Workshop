using Avocado;
using ControllerLogic;
using JetBrains.Annotations;
using NUnit.Framework;
using PurrNet;
using Sync_Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avocado.Screens {
    public class ChallengerScreen : UiScreen
    {
        private string MasterVolumeKey = "MasterVolume";
        private string SfxVolumeKey = "SfxVolume";
        private string MusicVolumeKey = "MusicVolume";
        private string CameraShakeKey = "CameraShake";
        private List<SyncedStats> teamStats = new List<SyncedStats>();
        private SyncedStats bossStats;
        private PlayerUiManager playerUi;
        private HealthHandler currentPlayerHealthHandler;
        private Coroutine flashRoutine;
        private readonly List<AvocadoImage> hurtCorners = new();
        private readonly List<(SyncedStats, Action<float>, Action<float>)> _bindings = new();
        private Color _flashColor = Color.red;
        private Color _deadColor = new Color(0.3f, 0.6f, 1f);
        private Color _overtimeColor = new Color(150/255f, 0/255f, 255/255f);
        private float _flashDuration = 0.15f;
        private float _lowHPThreshold = 0.25f;
        private float _pulseSpeed = 5f;
        private Dictionary<AvocadoSlider, (bool isPulsing, bool isFlashing, Color originalColor)> _sliderStates = new();
        private Volume globalVolume;
        private Vignette _vignette;
        private float _maxVignetteIntensity = 0.5f;
        private CanvasGroup _phaseChangeUIGroup;
        private Coroutine _phaseChangeCoroutine;
        private float _phaseChangeFadeDuration = 0.5f;
        private float _phaseChangeDisplayDuration = 2.0f;
        private DashState _thisDashState;

        private AvocadoImage primaryCooldown;
        private AvocadoImage secondaryCooldown;
        private AvocadoImage dashCooldown;
        private Dictionary<AvocadoImage, Coroutine> cooldowns = new();
        private PlayerAttack currentAttack;
        private AvocadoText phaseChangeTitle;
        private UnityEngine.UI.Image phaseChangeBar1;
        private UnityEngine.UI.Image phaseChangeBar2;
        private GameObject bossHealthBarObject;
        private GameObject timerObject;
        private GameObject _announcementGroup;
        private AvocadoText announcementText;
        private UnityEngine.UI.Image announcementBorder1;
        private UnityEngine.UI.Image announcementBorder2;
        private GameObject _shopIcon;
        private AvocadoText _dashCountText;

        public override void Initialize(PlayerID owner)
        {
            base.Initialize(owner);

            MasterVolumeKey += owner;
            SfxVolumeKey += owner;
            MusicVolumeKey += owner;
            CameraShakeKey += owner;

            GetStatReferences(owner);
            if (teamStats[0] == null)
            {
                return;
            }

            // Health bar and display binding
            for (int i = 0; i < 4; i++)
            {
                // Edit + bind health and character icons
                if (i >= teamStats.Count)
                {
                    DisablePlayerDisplay(i);
                    continue;
                }
                if (i == 0)
                {
                    // Owner
                    BindHealthSlider(teamStats[0], GetSliderRef("this_health_bar"), GetSliderRef($"this_armor"));
                    GetImageRef("this_character_icon").Edit(teamStats[0].Icon);
                }
                else
                {
                    // Other players
                    BindHealthSlider(teamStats[i], GetSliderRef($"other_health_bar{i}"), GetSliderRef($"other_armor{i}"));
                    GetImageRef($"other_character_icon{i}").Edit(teamStats[i].Icon);
                }
            }
            BindHealthSlider(bossStats, GetSliderRef("boss_health_bar"), GetSliderRef("boss_armor"), true);

            // Icon bindings
            playerUi = teamStats[0].GetComponentInChildren<PlayerUiManager>();
            if (playerUi == null)
            {
                return;
            }

            UpdateAbilityIcons(playerUi.GetIcons());
            playerUi.OnIconChanged += UpdateAbilityIcons;

            // Ability cooldowns
            primaryCooldown = GetImageRef("primary_icon_grayed");
            secondaryCooldown = GetImageRef("secondary_icon_grayed");
            dashCooldown = GetImageRef("dash_icon_grayed");
            primaryCooldown.SetFillAmount(0);
            secondaryCooldown.SetFillAmount(0);
            dashCooldown.SetFillAmount(0);

            playerUi.OnAttackChanged += BindAttackEvents;
            BindAttackEvents(playerUi.GetCurrentAttack());

            // Timer
            AvocadoText timerText = GetTextRef("timer_text");
            OnIntervalRefresh(1, () =>
            {
                float globalTime = GlobalController.Singleton.GetGlobalTime();
                int minutes = Mathf.FloorToInt(globalTime / 60f);
                int seconds = Mathf.FloorToInt(globalTime % 60f);
                timerText.Edit(string.Format("{0:00}:{1:00}", minutes, seconds));
                if (GlobalController.Singleton.WorldData.RoundState == RoundState.RaidOvertime)
                {
                    timerText.SetColor(Color.Lerp(Color.white, _overtimeColor, Mathf.Clamp01(globalTime / 30f)));
                }
                else
                {
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
                }
            });

            // Hurt Flash UI
            hurtCorners.Add(GetImageRef("hurt_top_left"));
            hurtCorners.Add(GetImageRef("hurt_top_right"));
            hurtCorners.Add(GetImageRef("hurt_bot_left"));
            hurtCorners.Add(GetImageRef("hurt_bot_right"));
            currentPlayerHealthHandler = teamStats[0].GetComponentInChildren<HealthHandler>();
            currentPlayerHealthHandler.OnAttacked += TriggerFlashAction;
            if (globalVolume == null)
            {
                globalVolume = FindFirstObjectByType<Volume>();
                if (globalVolume == null)
                    Debug.LogError("No Global Volume found. Vignette won't work.");
            }
            globalVolume.profile.TryGet(out _vignette);

            _phaseChangeUIGroup = GetElementRef("PhaseTitleUI").GetComponent<CanvasGroup>();
            phaseChangeTitle = GetTextRef("phase_title_text");
            phaseChangeBar1 = GetElementRef("phase_title_bar1").GetComponent<UnityEngine.UI.Image>();
            phaseChangeBar2 = GetElementRef("phase_title_bar2").GetComponent<UnityEngine.UI.Image>();
            bossHealthBarObject = GetElementRef("boss_health_bar");
            _phaseChangeUIGroup.alpha = 0f;
            if (GlobalController.Singleton != null && GlobalController.Singleton.WorldData != null)
            {
                GlobalController.Singleton.WorldData.RoundState.onChanged += OnRoundStateChanged;
            }

            timerObject = GetElementRef("TimerUI");

            _announcementGroup = GetElementRef("AnnouncementUI");
            announcementText = GetTextRef("announcement_text");
            announcementBorder1 = GetElementRef("announcement_bar1").GetComponent<UnityEngine.UI.Image>();
            announcementBorder2 = GetElementRef("announcement_bar2").GetComponent<UnityEngine.UI.Image>();

            _shopIcon = GetElementRef("ShopIcon");

            _thisDashState = teamStats[0].GetComponentInChildren<DashState>();
            _dashCountText = GetTextRef("dash_count");
        }

        private void Update()
        {
            // controls health bar's and vignette's flashing/pulsing
            float lerp = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            bool ownerIsLow = false;
            bool areAlliesDead = false;
            int statNumber = 0;
            foreach (var sliderState in _sliderStates)
            {
                AvocadoSlider slider = sliderState.Key;
                var state = sliderState.Value;

                // only pulse if the bar is low HP and NOT currently playing a damage flash
                if (state.isPulsing && !state.isFlashing)
                {
                    var fillImage = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>();
                    fillImage.color = Color.Lerp(state.originalColor, _flashColor, lerp);
                    if (slider == GetSliderRef("this_health_bar")) 
                    {
                        ownerIsLow = true;
                    }
                }
                else if (!state.isFlashing)
                {
                    // ensure color returns to normal if not flashing/pulsing
                    var fillImage = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (fillImage.color != state.originalColor)
                        fillImage.color = state.originalColor;
                }

                if (slider != GetSliderRef("boss_health_bar") && teamStats[statNumber].CurrentHealth <= 0f && slider != GetSliderRef("this_health_bar"))
                {
                    areAlliesDead = true;
                    GetImageRef($"other_character_icon{statNumber}").Edit(teamStats[statNumber].DeadIcon);
                }
                else if (slider != GetSliderRef("boss_health_bar") && slider != GetSliderRef("this_health_bar"))
                {
                    GetImageRef($"other_character_icon{statNumber}").Edit(teamStats[statNumber].Icon);
                }
                statNumber++;
            }

            UpdateScreenVignette(ownerIsLow, lerp); // pulse the screen vignette if this player is low

            // player dead info
            if (teamStats[0].CurrentHealth <= 0f)
            {
                _announcementGroup.SetActive(true);
                announcementText.Edit("You have died!\nHave an ally with a Revive stand on your <color=#70C8FE>Ghost</color> for 3 seconds to revive you!");
                announcementBorder1.color = _deadColor;
                announcementBorder2.color = _deadColor;
                GetImageRef("this_character_icon").Edit(teamStats[0].DeadIcon);
            }
            else if (areAlliesDead && teamStats[0].GetComponentInChildren<ChallengerReviveHandler>().ReviveAmount >= 0)
            {
                _announcementGroup.SetActive(true);
                if (teamStats[0].GetComponentInChildren<ChallengerReviveHandler>().ReviveAmount == 1)
                {
                    announcementText.Edit($"An ally has died!\nYou have {teamStats[0].GetComponentInChildren<ChallengerReviveHandler>().ReviveAmount} Revive. Revive them by standing on their <color=#70C8FE>Ghost</color> for 3 seconds!");
                }
                else
                {
                    announcementText.Edit($"An ally has died!\nYou have {teamStats[0].GetComponentInChildren<ChallengerReviveHandler>().ReviveAmount} Revives. Revive them by standing on their <color=#70C8FE>Ghost</color> for 3 seconds!");
                }
                announcementBorder1.color = _deadColor;
                announcementBorder2.color = _deadColor;
            }
            else {
                _announcementGroup.SetActive(false);
                GetImageRef("this_character_icon").Edit(teamStats[0].Icon);
            }

            // change UI based on phases
            if (GlobalController.Singleton.WorldData.RoundState == RoundState.Setup)
            {
                bossHealthBarObject.SetActive(false);
                timerObject.SetActive(true);
                _shopIcon.SetActive(true);
            }
            else if (GlobalController.Singleton.WorldData.RoundState == RoundState.Raid)
            {
                bossHealthBarObject.SetActive(false);
                timerObject.SetActive(true);
                _shopIcon.SetActive(false);
            }
            else if (GlobalController.Singleton.WorldData.RoundState == RoundState.RaidOvertime)
            {
                bossHealthBarObject.SetActive(false);
                timerObject.SetActive(true);
                _shopIcon.SetActive(false);
                _announcementGroup.SetActive(true);
                announcementText.Edit("The Dungeon <color=#9600FF><i>consumes</i></color> you...\nFind the Dungeon Boss immediately!");
                announcementBorder1.color = _overtimeColor;
                announcementBorder2.color = _overtimeColor;
            }
            else if (GlobalController.Singleton.WorldData.RoundState == RoundState.BossFight)
            {
                bossHealthBarObject.SetActive(true);
                timerObject.SetActive(false);
                _shopIcon.SetActive(false);
            }

            if (_thisDashState.MaximumDashes > 1 && _thisDashState.DashesLeft >= 1)
            {
                _dashCountText.ToggleVisibility(true);
                _dashCountText.Edit($"x{_thisDashState.DashesLeft}");
            }
            else
            {
                _dashCountText.ToggleVisibility(false);
            }
        }

        public override void Collapse()
        {
            base.Collapse();

            // Removes the health event bindings
            foreach (var (stats, onMax, onCurrent) in _bindings)
            {
                if (stats == null) continue;

                stats.MaximumHealth.onChanged -= onMax;
                stats.CurrentHealth.onChanged -= onCurrent;
            }

            _bindings.Clear();
            currentPlayerHealthHandler.OnAttacked -= TriggerFlashAction;

            if (playerUi != null)
            {
                playerUi.OnIconChanged -= UpdateAbilityIcons;
                playerUi.OnAttackChanged -= BindAttackEvents;
            }

            if (currentAttack != null)
            {
                currentAttack.OnPrimaryStarted -= HandlePrimaryCooldown;
                currentAttack.OnSecondaryStarted -= HandleSecondaryCooldown;
                currentAttack.OnDashStarted -= HandleDashCooldown;
            }

            _sliderStates.Clear();
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
                case RoundState.RaidOvertime:
                    phaseChangeTitle.Edit("Overtime");
                    phaseChangeTitle.SetColor(_overtimeColor);
                    phaseChangeBar1.color = _overtimeColor;
                    phaseChangeBar2.color = _overtimeColor;
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

        private void GetStatReferences(PlayerID owner)
        {
            teamStats.Clear();
            bossStats = null;

            SyncedStats ownerStat = null;

            foreach (SyncedStats stat in GlobalController.Singleton?.GetAllStats())
            {
                if (stat.owner == owner) ownerStat = stat;
                else if (stat.CompareTag("Boss")) bossStats = stat;
                else teamStats.Add(stat);
            }
            teamStats.Insert(0, ownerStat);
        }

        /// <summary>
        /// Binds the health updates to the sliders
        /// </summary>
        /// <param name="stats"> The player's stats. </param>
        /// <param name="slider"> The corresponding health slider. </param>
        private void BindHealthSlider(SyncedStats stats, AvocadoSlider slider, AvocadoSlider armorSlider, bool isBoss = false)
        {
            if (stats == null) return;

            slider.State.Disabled = true;
            slider.RawElement.maxValue = stats.MaximumHealth;
            slider.RawElement.value = stats.CurrentHealth;

            // cache the original color and initialize the state for this slider
            Color originalColor = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>().color;
            _sliderStates[slider] = (false, false, originalColor);

            Action<float> onMax = (newMax) => slider.RawElement.maxValue = newMax;

            Action<float> onCurrent = (newHealth) =>
            {
                // flash health bar if health dropped
                if (newHealth < slider.RawElement.value)
                {
                    StartCoroutine(IndividualFlashRoutine(slider));
                }

                slider.RawElement.value = newHealth;

                // update pulsing state for this slider depending on low HP threshold
                var state = _sliderStates[slider];
                state.isPulsing = (newHealth / stats.MaximumHealth) <= _lowHPThreshold;
                _sliderStates[slider] = state;

                if (stats.CurrentHealth <= stats.MaximumHealth / 2 && isBoss)
                {
                    _sliderStates[slider] = (state.isPulsing, state.isFlashing, new Color(219/255f, 125/255f, 38/255f));
                }
            };

            stats.MaximumHealth.onChanged += onMax;
            stats.CurrentHealth.onChanged += onCurrent;
            _bindings.Add((stats, onMax, onCurrent));

            armorSlider.SetNewMax((int)stats.MaximumHealth.value);
            armorSlider.SetNewValue((int)stats.Armor.value);
            stats.Armor.onChanged += val => armorSlider.SetNewValue((int)val);
        }

        /// <summary>
        /// Coroutine to flash a player's health bar
        /// </summary>
        /// <param name="slider"> The specific slider that is flashing. </param>
        /// <returns></returns>
        private IEnumerator IndividualFlashRoutine(AvocadoSlider slider)
        {
            if (!_sliderStates.ContainsKey(slider)) yield break;

            var fillImage = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>();
            var state = _sliderStates[slider];

            // mark as flashing
            state.isFlashing = true;
            _sliderStates[slider] = state;
            
            fillImage.color = _flashColor;

            yield return new WaitForSeconds(_flashDuration);

            // reset flash state
            state = _sliderStates[slider];
            state.isFlashing = false;
            _sliderStates[slider] = state;

            // if not pulsing, revert to original color
            if (!state.isPulsing)
            {
                fillImage.color = state.originalColor;
            }
        }

        private void DisablePlayerDisplay(int playerNum)
        {
            AvocadoImage charIcon = GetImageRef($"other_character_border{playerNum}");
            charIcon.SetActive(false);
        }

        /// <summary>
        /// Changes ability icons
        /// </summary>
        /// <param name="newIcons"> New icons to change to </param>
        private void UpdateAbilityIcons(List<Sprite> newIcons)
        {
            GetImageRef($"primary_icon").Edit(newIcons[0]);
            GetImageRef($"primary_icon_grayed").Edit(newIcons[0]);
            GetImageRef($"secondary_icon").Edit(newIcons[1]);
            GetImageRef($"secondary_icon_grayed").Edit(newIcons[1]);
            GetImageRef($"dash_icon").Edit(newIcons[2]);
            GetImageRef($"dash_icon_grayed").Edit(newIcons[2]);
        }

        /// <summary>
        /// Binds attack cooldown events
        /// </summary>
        /// <param name="attack"> The player attack state. </param>
        private void BindAttackEvents(PlayerAttack attack)
        {
            if (currentAttack != null)
            {
                currentAttack.OnPrimaryStarted -= HandlePrimaryCooldown;
                currentAttack.OnSecondaryStarted -= HandleSecondaryCooldown;
                currentAttack.OnDashStarted -= HandleDashCooldown;
            }

            currentAttack = attack;

            if (currentAttack == null) return;

            currentAttack.OnPrimaryStarted += HandlePrimaryCooldown;
            currentAttack.OnSecondaryStarted += HandleSecondaryCooldown;
            currentAttack.OnDashStarted += HandleDashCooldown;
        }
        private void HandlePrimaryCooldown(float duration)
        {
            StartCooldown(primaryCooldown, duration);
        }

        private void HandleSecondaryCooldown(float duration)
        {
            StartCooldown(secondaryCooldown, duration);
        }

        private void HandleDashCooldown(float duration)
        {
            StartCooldown(dashCooldown, duration);
        }

        private void StartCooldown(AvocadoImage img, float duration)
        {
            if (cooldowns.TryGetValue(img, out Coroutine running))
                StopCoroutine(running);

            cooldowns[img] = StartCoroutine(CooldownRoutine(img, duration));
        }

        /// <summary>
        /// Handles the cooldown of ability icons
        /// </summary>
        private IEnumerator CooldownRoutine(AvocadoImage image, float duration)
        {
            image.SetFillAmount(1f);

            float timer = duration;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                image.SetFillAmount(timer / duration);
                yield return null;
            }

            image.SetFillAmount(0f);
        }

        public void TriggerFlashAction(Vector2 source, bool isStatusEffect)
        {
            TriggerFlash();
        }

        public void TriggerFlash(float intensity = 0.8f, float duration = 0.25f)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(intensity, duration));
        }

        private IEnumerator FlashRoutine(float intensity, float duration)
        {
            float time = 0;
            foreach (var img in hurtCorners) img.ToggleVisibility(true);
            // Fade in
            while (time < duration)
            {
                time += Time.deltaTime;
                float a = Mathf.Lerp(0, intensity, time / duration);
                SetAlpha(a);
                yield return null;
            }

            // Fade out
            time = 0;
            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(intensity, 0, time / duration);
                SetAlpha(alpha);
                yield return null;
            }

            foreach (var img in hurtCorners) img.ToggleVisibility(false);
        }

        private void SetAlpha(float alpha)
        {
            foreach (var img in hurtCorners)
            {
                img.ChangeColor(new Color(img.ImageTintColor.r, img.ImageTintColor.b, img.ImageTintColor.g, alpha));
            }
        }

        private void UpdateScreenVignette(bool ownerIsLow, float lerp)
        {
            if (_vignette == null)
            {
                return;
            }

            if (teamStats[0].CurrentHealth <= 0f)
            {
                _vignette.intensity.value = _maxVignetteIntensity;
                _vignette.color.value = _deadColor;
            }
            else if (GlobalController.Singleton.WorldData.RoundState == RoundState.RaidOvertime)
            {
                // lerp between 0 and max intensity
                _vignette.intensity.value = Mathf.Lerp(0.1f, _maxVignetteIntensity, lerp);
                _vignette.color.value = _overtimeColor;
            }
            else if (ownerIsLow)
            {
                // lerp between 0 and max intensity
                _vignette.intensity.value = Mathf.Lerp(0.1f, _maxVignetteIntensity, lerp);
                _vignette.color.value = _flashColor;
            }
            else
            {
                // if the player is not low, return to normal
                _vignette.intensity.value = Mathf.MoveTowards(_vignette.intensity.value, 0, Time.deltaTime);
            }
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
    }
}