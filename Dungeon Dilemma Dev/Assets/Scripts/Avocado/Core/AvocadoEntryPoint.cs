using System;
using System.Collections;
using ControllerLogic;
using PurrNet;
using UnityEngine;

namespace Avocado {
    /// <summary>
    /// Entrypoint for the UI System, should be placed on the player prefab, will only initialize system on
    /// a client-owned object
    /// </summary>
    [RequireComponent(typeof(SyncedStats))]
    public class AvocadoEntryPoint: NetworkBehaviour {
        public static AvocadoEntryPoint Singleton;
        [SerializeField] private string activeScreen;
        protected override void OnSpawned(bool asServer) {
            base.OnSpawned(asServer);
            SyncedStats s = this.GetComponent<SyncedStats>();

            if (isOwner && Singleton == null) {
                EventBridge.Initialize(s);
                UiManager.OpenScreen(activeScreen, owner.Value);
                Singleton = this;
            }
        }

        private IEnumerator WaitForStatsInitialization()
        {
            while (!GlobalController.Singleton.IsStatsReady()) yield return null;
            UiManager.OpenScreen(activeScreen, owner.Value);
        }
    }
}