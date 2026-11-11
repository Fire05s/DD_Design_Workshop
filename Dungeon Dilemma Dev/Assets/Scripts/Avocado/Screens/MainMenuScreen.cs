using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PurrNet;

namespace Avocado.Screens {
    public class MainMenuScreen : UiScreen {
        private const string MasterVolumeKey = "MasterVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string CameraShakeKey = "CameraShake";
        public override void Initialize(PlayerID owner) {
            base.Initialize(owner);

            float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 100f);      // Default to 100 (full volume)
            float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 100f);            // Default to 100 (full volume)
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 100f);        // Default to 100 (full volume)
            float savedCameraShake = PlayerPrefs.GetFloat(CameraShakeKey, 100f);        // Default to 100% (default shake)

            SoundFXManager.Instance?.MasterVolume(savedMasterVolume / 100f);
            MusicManager.Instance?.MasterVolume(savedMasterVolume / 100f);
            SoundFXManager.Instance.SFXVolume(savedSfxVolume / 100f);
            MusicManager.Instance.MusicVolume(savedMusicVolume / 100f);

            ListenForButtonPress("create_lobby_button", button => {
                UiManager.CloseScreen("MainMenuUI");
                UiManager.OpenScreen("LobbyUI", owner);
                // Include Lobby Creation Functionality
            });

            ListenForButtonPress("join_button", button => {
                UiManager.CloseScreen("MainMenuUI");
                UiManager.OpenScreen("LobbyUI", owner);
                // Include Lobby Joining Functionality
            });

            ListenForButtonPress("settings_button", button => {
                UiManager.CloseScreen("MainMenuUI");
                UiManager.OpenScreen("PreGameSettingsUI", owner);
            });

            ListenForButtonPress("quit_button", button => {
                Application.Quit();
            });
        }
    }
}