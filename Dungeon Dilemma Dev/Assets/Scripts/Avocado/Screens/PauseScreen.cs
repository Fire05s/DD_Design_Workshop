using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PurrNet;
using ControllerLogic;

namespace Avocado.Screens {
    public class PauseScreen : UiScreen {
        public override void Initialize(PlayerID owner) {
            base.Initialize(owner);

            ListenForButtonPress("resume_button", button => {
                UiManager.CloseScreen("PauseUI");
                GlobalController.Singleton.GetPlayer(owner).GetComponentInChildren<SyncedInput>().ResumeUIButton();
            });

            ListenForButtonPress("settings_button", button => {
                UiManager.CloseScreen("PauseUI");
                UiManager.OpenScreen("InGameSettingsUI", owner);
            });

            ListenForButtonPress("disconnect_button", button => {
                if (NetworkManager.main.isServer)
                {
                    NetworkManager.main.StopServer();
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainLobby");
                }
                else
                {
                    GlobalController.Singleton.RequestDisconnectServerRpc(owner);
                }
            });
        }
    }
}