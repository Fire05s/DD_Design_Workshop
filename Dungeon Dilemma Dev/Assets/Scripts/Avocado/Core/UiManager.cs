using System.Collections.Generic;
using Avocado.Screens;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

namespace Avocado {
    /// <summary>
    /// Global UI Manager, use this to interact with Avocado at a high level, primarily with the ability to open
    /// UI Screens
    /// </summary>
    public static class UiManager {
        public static bool SUPPRESS_LOGGING = false;

        [DoNotSerialize]
        public static bool Initialized;
        public static Dictionary<string, UiScreen> ScreenNameMap = new Dictionary<string, UiScreen>();
        public static List<UiScreen> Screens = new List<UiScreen>();
        public static List<UiScreen> OpenScreens = new List<UiScreen>();

        /// <summary>
        /// Open UI Screen by name
        /// </summary>
        /// <param name="screenName">Case-insensitive name of UI Screen (will match the name of the Prefab in Resources/Avocado/Screens)</param>
        public static void OpenScreen(string screenName, PlayerID caller) {
            if (!Initialized) {
                AvoLog("Attempted to open UI (Screen) before Discovery Step.", true);
                return;
            }

            screenName = screenName.ToLower();

            if (ScreenNameMap.ContainsKey(screenName)) {
                GameObject g = GameObject.Instantiate<GameObject>(ScreenNameMap[screenName].gameObject);
                UiScreen screen = g.GetComponent<UiScreen>();
                
                screen.Initialize(caller);
                screen.PostInitialize();
                OpenScreens.Add(screen);
            }
        }
        
        /// <summary>
        /// Close UI Screen by name
        /// </summary>
        /// <param name="screenName">Case-insensitive name of UI Screen (will match the name of the Prefab in Resources/Avocado/Screens)</param>
        public static void CloseScreen(string screenName) {
            if (!Initialized) {
                AvoLog("Attempted to close UI (Screen) before Discovery Step.", true);
                return;
            }

            screenName = screenName.ToLower();
            List<UiScreen> destroyScreens = new List<UiScreen>();
            
            foreach (UiScreen screen in OpenScreens) {
                if (screen == null)
                {
                    destroyScreens.Add(screen);
                    continue;
                }
                if (screen.name.ToLower().Contains(screenName)) {
                    screen.Collapse();
                    
                    destroyScreens.Add(screen);        
                }
            }

            foreach (UiScreen screen in destroyScreens) {
                OpenScreens.Remove(screen);
                if (screen != null) GameObject.Destroy(screen.gameObject);
            }
            
            // AvoLog("Attempted to close UI (Screen) when screen " + screenName + " is not open.", true);
        }

        /// <summary>
        /// Global Initialization for UI Manager, <b>don't touch</b>
        /// </summary>
        public static void Initialize() {
            GameObject[] screenPrefabs = Resources.LoadAll<GameObject>("Avocado/Screens");
            
            foreach (GameObject screenObj in screenPrefabs) {
                UiScreen screenScript = screenObj.GetComponent<UiScreen>();

                if (screenScript == null) {
                    AvoLog("Failed to find UI (Screen) Script on prefab " + screenObj.name, true);
                }
                
                AvoLog("Registered " + screenObj.name, false);
                
                Screens.Add(screenScript);
                ScreenNameMap[screenObj.name.ToLower()] = screenScript;
            }

            Initialized = true;
        }

        /// <summary>
        /// Logic control for UI Logging
        /// </summary>
        /// <param name="msg">Message to log</param>
        /// <param name="warn">Whether it should be marked as a warning</param>
        public static void AvoLog(string msg, bool warn) {
            if (SUPPRESS_LOGGING) {
                return;
            }

            // if (!warn)
            //     Debug.Log("[Avocado]: " + msg);
            // else
            //     Debug.LogWarning("[Avocado WARN]: " + msg);
        }
    }
}