using UnityEngine;
using PurrNet;

namespace Avocado.Screens {
    public class PreGameSettingsScreen : UiScreen {
        private const string MasterVolumeKey = "MasterVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string CameraShakeKey = "CameraShake";
        private const string SettingsViewKey = "SettingsView";
        public override void Initialize(PlayerID owner) {
            base.Initialize(owner);

            GameObject generalSettingsGroup = GetElementRef("GeneralSettings");
            GameObject controlsSettingsGroup = GetElementRef("ControlsSettings");

            AvocadoSlider masterVolumeSlider = GetSliderRef("master_volume_slider");
            AvocadoSlider sfxVolumeSlider = GetSliderRef("sfx_volume_slider");
            AvocadoSlider musicVolumeSlider = GetSliderRef("music_volume_slider");
            AvocadoSlider cameraShakeSlider = GetSliderRef("camera_shake_slider");

            AvocadoText masterVolumeValue = GetTextRef("master_volume_value");
            AvocadoText sfxVolumeValue = GetTextRef("sfx_volume_value");
            AvocadoText musicVolumeValue = GetTextRef("music_volume_value");
            AvocadoText cameraShakeValue = GetTextRef("camera_shake_value");

            float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 100f);      // Default to 100 (full volume)
            float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 100f);            // Default to 100 (full volume)
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 100f);        // Default to 100 (full volume)
            float savedCameraShake = PlayerPrefs.GetFloat(CameraShakeKey, 100f);        // Default to 100% (default shake)
            int savedSettingsView = PlayerPrefs.GetInt(SettingsViewKey, 1);             // Default to 1 (general settings); 2 = controls settings

            // Master Volume
            masterVolumeSlider.RawElement.minValue = 0;
            masterVolumeSlider.RawElement.maxValue = 100;
            masterVolumeSlider.RawElement.value = savedMasterVolume;
            masterVolumeValue.Edit(((int)savedMasterVolume).ToString());
            SoundFXManager.Instance?.MasterVolume(savedMasterVolume / 100f);
            MusicManager.Instance?.MasterVolume(savedMasterVolume / 100f);

            // SFX Volume
            sfxVolumeSlider.RawElement.minValue = 0;
            sfxVolumeSlider.RawElement.maxValue = 100;
            sfxVolumeSlider.RawElement.value = savedSfxVolume;
            sfxVolumeValue.Edit(((int)savedSfxVolume).ToString());
            SoundFXManager.Instance?.SFXVolume(savedSfxVolume / 100f);

            // Music Volume
            musicVolumeSlider.RawElement.minValue = 0;
            musicVolumeSlider.RawElement.maxValue = 100;
            musicVolumeSlider.RawElement.value = savedMusicVolume;
            musicVolumeValue.Edit(((int)savedMusicVolume).ToString());
            MusicManager.Instance?.MusicVolume(savedMusicVolume / 100f);

            // Camera Shake
            cameraShakeSlider.RawElement.minValue = 0;
            cameraShakeSlider.RawElement.maxValue = 200;
            cameraShakeSlider.RawElement.value = savedCameraShake;
            cameraShakeValue.Edit($"{(int)savedCameraShake}%");

            if (savedSettingsView == 1)
            {
                controlsSettingsGroup.SetActive(false);
                generalSettingsGroup.SetActive(true);
            }
            else if (savedSettingsView == 2)
            {
                generalSettingsGroup.SetActive(false);
                controlsSettingsGroup.SetActive(true);
            }

            ListenForButtonPress("back_button", button =>
            {
                UiManager.CloseScreen("PreGameSettingsUI");
                // UiManager.OpenScreen("MainMenuUI", owner);
            });

            ListenForButtonPress("quit_button", button =>
            {
                Application.Quit();
            });

            ListenForButtonPress("general_settings_button", button =>
            {
                controlsSettingsGroup.SetActive(false);
                generalSettingsGroup.SetActive(true);
                PlayerPrefs.SetInt(SettingsViewKey, 1);
                PlayerPrefs.Save();
            });
            ListenForButtonPress("controls_button", button =>
            {
                generalSettingsGroup.SetActive(false);
                controlsSettingsGroup.SetActive(true);
                PlayerPrefs.SetInt(SettingsViewKey, 2);
                PlayerPrefs.Save();
            });

            ListenForSliderInteract("master_volume_slider", slider =>
            {
                float volume = masterVolumeSlider.RawElement.value;
                SoundFXManager.Instance.MasterVolume(volume / 100f);
                MusicManager.Instance.MasterVolume(volume / 100f);
                masterVolumeValue.Edit(((int)volume).ToString());

                // Save the volume to PlayerPrefs
                PlayerPrefs.SetFloat(MasterVolumeKey, volume);
                PlayerPrefs.Save();
            });

            ListenForSliderInteract("sfx_volume_slider", slider =>
            {
                float volume = sfxVolumeSlider.RawElement.value;
                SoundFXManager.Instance.SFXVolume(volume / 100f);
                sfxVolumeValue.Edit(((int)volume).ToString());

                // Save the volume to PlayerPrefs
                PlayerPrefs.SetFloat(SfxVolumeKey, volume);
                PlayerPrefs.Save();
            });

            ListenForSliderInteract("music_volume_slider", slider =>
            {
                float volume = musicVolumeSlider.RawElement.value;
                MusicManager.Instance.MusicVolume(volume / 100f);
                musicVolumeValue.Edit(((int)volume).ToString());

                // Save the volume to PlayerPrefs
                PlayerPrefs.SetFloat(MusicVolumeKey, volume);
                PlayerPrefs.Save();
            });
            ListenForSliderInteract("camera_shake_slider", slider =>
            {
                float shakePreference = cameraShakeSlider.RawElement.value;
                cameraShakeValue.Edit($"{(int)shakePreference}%");

                // Save the shake preference to PlayerPrefs
                PlayerPrefs.SetFloat(CameraShakeKey, shakePreference / 100f);
                PlayerPrefs.Save();
            });
        }
    }
}