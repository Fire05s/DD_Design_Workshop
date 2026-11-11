using Avocado;
using ControllerLogic;
using PurrNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Avocado.Screens
{
    public class SelectionScreen : UiScreen
    {
        private string BaseMasterVolumeKey = "MasterVolume";
        private string BaseSfxVolumeKey = "SfxVolume";
        private string BaseMusicVolumeKey = "MusicVolume";
        private string BaseCameraShakeKey = "CameraShake";
        private string MasterVolumeKey = "MasterVolume";
        private string SfxVolumeKey = "SfxVolume";
        private string MusicVolumeKey = "MusicVolume";
        private string CameraShakeKey = "CameraShake";
        [Header("Colors")]
        [SerializeField] private Color _defaultTextColor;
        [SerializeField] private Color _readyTextColor;
        [SerializeField] private Color _readyButtonColor;
        [SerializeField] private Color _unreadyButtonColor;

        [Header("Audio")]
        [SerializeField] private AudioClip _selectAudio;

        private CharacterSelection _selection;
        public override void Initialize(PlayerID owner)
        {
            base.Initialize(owner);

            MasterVolumeKey += owner;
            SfxVolumeKey += owner;
            MusicVolumeKey += owner;
            CameraShakeKey += owner;

            UpdateSettings();

            _selection = FindFirstObjectByType<CharacterSelection>();

            ListenForButtonPress("yew_button", button => {
                _selection.SetCharacterServerRpc(owner, 0);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
            });
            ListenForButtonPress("yarrow_button", button => {
                _selection.SetCharacterServerRpc(owner, 1);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
            });
            ListenForButtonPress("amber_button", button => {
                _selection.SetCharacterServerRpc(owner, 2);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
            });
            ListenForButtonPress("linay_button", button => {
                _selection.SetCharacterServerRpc(owner, 3);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
            });
            ListenForButtonPress("ruru_button", button => {
                _selection.SetCharacterServerRpc(owner, 4);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
            });

            ListenForButtonPress("ready_button", button => {
                _selection.SetReadyServerRpc(owner);
                SoundFXManager.Instance?.PlayLocalSound(_selectAudio, transform.position);
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

            AvocadoText yewPlayerText = GetTextRef("yew_player_text");
            AvocadoText yarrowPlayerText = GetTextRef("yarrow_player_text");
            AvocadoText amberPlayerText = GetTextRef("amber_player_text");
            AvocadoText linayPlayerText = GetTextRef("linay_player_text");
            AvocadoText ruruPlayerText = GetTextRef("ruru_player_text");

            Dictionary<AvocadoText, int> playerReadyTexts = new Dictionary<AvocadoText, int>()
            {
                {yewPlayerText, 0},
                {yarrowPlayerText, 1},
                {amberPlayerText, 2},
                {linayPlayerText, 3},
                {ruruPlayerText, 4},
            };

            GetTextRef("this_player_text").Edit($"(You are Player {owner.id})");

            OnIntervalRefresh(0, () => {
                Dictionary<PlayerID, int> playerSelection = _selection.GetPlayersSelection().ToDictionary();
                Dictionary<int, PlayerID> characterToPlayer = playerSelection.ToDictionary(x => x.Value, x => x.Key);

                foreach (KeyValuePair<AvocadoText, int> textEntry in playerReadyTexts)
                {
                    if (characterToPlayer.ContainsKey(textEntry.Value))
                    {
                        SetPlayerField(textEntry.Key, characterToPlayer[textEntry.Value], owner);
                    }
                    else
                    {
                        textEntry.Key.ToggleVisibility(false);
                    }
                }
            });
        }

        private void SetPlayerField(AvocadoText text, PlayerID player, PlayerID owner)
        {
            text.ToggleVisibility(true);
            text.Edit(player.id.ToString());
            if (_selection.IsPlayerReady(player))
            {
                text.SetColor(_readyTextColor);
                if (player != owner) return;
                GetButtonRef("ready_button").RawElement.GetComponent<Image>().color = _unreadyButtonColor;
                GetTextRef("ready_text").Edit("Unready");
            }
            else
            {
                text.SetColor(_defaultTextColor);
                if (player != owner) return;
                GetButtonRef("ready_button").RawElement.GetComponent<Image>().color = _readyButtonColor;
                GetTextRef("ready_text").Edit("Ready");
            }
        }

        private void UpdateSettings()
        {
            float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, PlayerPrefs.GetFloat(BaseMasterVolumeKey, 100f));   // Default to 100 (full volume)
            float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, PlayerPrefs.GetFloat(BaseSfxVolumeKey, 100f));            // Default to 100 (full volume)
            float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, PlayerPrefs.GetFloat(BaseMusicVolumeKey, 100f));      // Default to 100 (full volume)
            float savedCameraShake = PlayerPrefs.GetFloat(CameraShakeKey, PlayerPrefs.GetFloat(BaseCameraShakeKey, 100f));      // Default to 100% (default shake)
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
