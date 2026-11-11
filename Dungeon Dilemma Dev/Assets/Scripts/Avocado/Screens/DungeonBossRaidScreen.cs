using Avocado;
using ControllerLogic;
using PurrNet;
using Sync_Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avocado.Screens
{
    public class DungeonBossRaidScreen : UiScreen
    {
        private string MasterVolumeKey = "MasterVolume";
        private string SfxVolumeKey = "SfxVolume";
        private string MusicVolumeKey = "MusicVolume";
        private string CameraShakeKey = "CameraShake";
        private List<SyncedStats> teamStats = new List<SyncedStats>();
        private SyncedStats bossStats;
        private readonly List<(SyncedStats, Action<float>, Action<float>)> _bindings = new();
        private Color _flashColor = Color.red;
        private Color _overtimeColor = new Color(150/255f, 0/255f, 255/255f);
        private float _flashDuration = 0.15f;
        private float _lowHPThreshold = 0.25f;
        private float _pulseSpeed = 5f;
        private CanvasGroup _phaseChangeUIGroup;
        private Coroutine _phaseChangeCoroutine;
        private float _phaseChangeFadeDuration = 0.5f;
        private float _phaseChangeDisplayDuration = 2.0f;
        private Dictionary<AvocadoSlider, (bool isPulsing, bool isFlashing, Color originalColor)> _sliderStates = new();
        private AvocadoText phaseChangeTitle;
        private UnityEngine.UI.Image phaseChangeBar1;
        private UnityEngine.UI.Image phaseChangeBar2;
        private BossGaze _bossGaze;
        private AvocadoImage _bossGazeAbilityIcon;

        public override void Initialize(PlayerID owner)
        {
            base.Initialize(owner);

            MasterVolumeKey += owner;
            SfxVolumeKey += owner;
            MusicVolumeKey += owner;
            CameraShakeKey += owner;

            GetStatReferences(owner);
            if (bossStats == null)
            {
                return;
            }

            // Health bar and display binding
            for (int i = 1; i < 5; i++)
            {
                // Edit + bind health and character icons
                if (i > teamStats.Count)
                {
                    DisablePlayerDisplay(i);
                    continue;
                }
                else
                {
                    // Other players
                    BindHealthSlider(teamStats[i - 1], GetSliderRef($"other_health_bar{i}"), GetSliderRef($"other_armor{i}"));
                    GetImageRef($"other_character_icon{i}").Edit(teamStats[i - 1].Icon);
                }
            }

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

            _phaseChangeUIGroup = GetElementRef("PhaseTitleUI").GetComponent<CanvasGroup>();
            phaseChangeTitle = GetTextRef("phase_title_text");
            phaseChangeBar1 = GetElementRef("phase_title_bar1").GetComponent<UnityEngine.UI.Image>();
            phaseChangeBar2 = GetElementRef("phase_title_bar2").GetComponent<UnityEngine.UI.Image>();
            _phaseChangeUIGroup.alpha = 0f;
            if (GlobalController.Singleton != null && GlobalController.Singleton.WorldData != null)
            {
                GlobalController.Singleton.WorldData.RoundState.onChanged += OnRoundStateChanged;
                
            }

            List<SyncedStats> allStats = GlobalController.Singleton.GetAllStats();
            foreach (SyncedStats stat in allStats)
            {
                if (stat.Name.value == "Yew")
                {
                    _bossGaze = stat.GetComponentInChildren<BossGaze>();
                }
            }
            _bossGazeAbilityIcon = GetImageRef("boss_gaze_icon_grayed");
        }

        private void Update()
        {
            // controls health bar's flashing/pulsing
            float lerp = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f;
            foreach (var sliderState in _sliderStates)
            {
                AvocadoSlider slider = sliderState.Key;
                var state = sliderState.Value;

                // Only pulse if the bar is low HP and NOT currently playing a damage flash
                if (state.isPulsing && !state.isFlashing)
                {
                    var fillImage = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>();
                    fillImage.color = Color.Lerp(state.originalColor, _flashColor, lerp);
                }
                else if (!state.isFlashing)
                {
                    // ensure color returns to normal if not flashing/pulsing
                    var fillImage = slider.RawElement.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (fillImage.color != state.originalColor)
                        fillImage.color = state.originalColor;
                }
            }

            int statNumber = 1;
            foreach (SyncedStats stat in teamStats)
            {
                if (stat.CurrentHealth <= 0f)
                {
                    GetImageRef($"other_character_icon{statNumber}").Edit(stat.DeadIcon);
                }
                else
                {
                    GetImageRef($"other_character_icon{statNumber}").Edit(stat.Icon);
                }
                statNumber++;
            }

            _bossGazeAbilityIcon.SetFillAmount(_bossGaze.RemainingCooldown / _bossGaze.AttackCooldown);
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

            foreach (SyncedStats stat in GlobalController.Singleton?.GetAllStats())
            {
                if (stat.CompareTag("Boss")) bossStats = stat;
                else teamStats.Add(stat);
            }
        }

        /// <summary>
        /// Binds the health updates to the sliders
        /// </summary>
        /// <param name="stats"> The player's stats. </param>
        /// <param name="slider"> The corresponding health slider. </param>
        private void BindHealthSlider(SyncedStats stats, AvocadoSlider slider, AvocadoSlider armorSlider)
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

            // mark this bar as flashing
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
