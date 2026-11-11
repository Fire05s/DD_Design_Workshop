using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsManager : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string CameraShakeKey = "CameraShake";
    private const string SettingsViewKey = "SettingsView";
    [SerializeField] private GameObject generalSettingsGroup;
    [SerializeField] private GameObject controlsSettingsGroup;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider cameraShakeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeValue;
    [SerializeField] private TextMeshProUGUI sfxVolumeValue;
    [SerializeField] private TextMeshProUGUI musicVolumeValue;
    [SerializeField] private TextMeshProUGUI cameraShakeValue;
    private GameObject settingsScreen;

    void Start()
    {
        float savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 100f);      // Default to 100 (full volume)
        float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 100f);            // Default to 100 (full volume)
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 100f);        // Default to 100 (full volume)
        float savedCameraShake = PlayerPrefs.GetFloat(CameraShakeKey, 100f);        // Default to 100% (default shake)
        int savedSettingsView = PlayerPrefs.GetInt(SettingsViewKey, 1);             // Default to 1 (general settings); 2 = controls settings
        PlayerPrefs.SetFloat(MasterVolumeKey, savedMasterVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, savedSfxVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, savedMusicVolume);
        PlayerPrefs.SetFloat(CameraShakeKey, savedCameraShake);
        PlayerPrefs.SetFloat(SettingsViewKey, savedSettingsView);
        PlayerPrefs.Save();

        // Master Volume
        masterVolumeSlider.minValue = 0;
        masterVolumeSlider.maxValue = 100;
        masterVolumeSlider.value = savedMasterVolume;
        masterVolumeValue.text = ((int)savedMasterVolume).ToString();
        SoundFXManager.Instance?.MasterVolume(savedMasterVolume / 100f);
        MusicManager.Instance?.MasterVolume(savedMasterVolume / 100f);

        // SFX Volume
        sfxVolumeSlider.minValue = 0;
        sfxVolumeSlider.maxValue = 100;
        sfxVolumeSlider.value = savedSfxVolume;
        sfxVolumeValue.text = ((int)savedSfxVolume).ToString();
        SoundFXManager.Instance?.SFXVolume(savedSfxVolume / 100f);

        // Music Volume
        musicVolumeSlider.minValue = 0;
        musicVolumeSlider.maxValue = 100;
        musicVolumeSlider.value = savedMusicVolume;
        musicVolumeValue.text = ((int)savedMusicVolume).ToString();
        MusicManager.Instance?.MusicVolume(savedMusicVolume / 100f);

        // Camera Shake
        cameraShakeSlider.minValue = 0;
        cameraShakeSlider.maxValue = 200;
        cameraShakeSlider.value = savedCameraShake;
        cameraShakeValue.text = $"{(int)savedCameraShake}%";

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

        settingsScreen = gameObject;
        settingsScreen.SetActive(false);
    }

    public void BackButton()
    {
        settingsScreen.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void GeneralSettingsButton()
    {
        controlsSettingsGroup.SetActive(false);
        generalSettingsGroup.SetActive(true);
        PlayerPrefs.SetInt(SettingsViewKey, 1);
        PlayerPrefs.Save();
    }

    public void ControlsButton()
    {
        generalSettingsGroup.SetActive(false);
        controlsSettingsGroup.SetActive(true);
        PlayerPrefs.SetInt(SettingsViewKey, 2);
        PlayerPrefs.Save();
    }

    public void MasterVolumeSlider()
    {
        float volume = masterVolumeSlider.value;
        SoundFXManager.Instance?.MasterVolume(volume / 100f);
        MusicManager.Instance?.MasterVolume(volume / 100f);
        masterVolumeValue.text = ((int)volume).ToString();

        // Save the volume to PlayerPrefs
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SFXVolumeSlider()
    {
        float volume = sfxVolumeSlider.value;
        SoundFXManager.Instance?.SFXVolume(volume / 100f);
        sfxVolumeValue.text = ((int)volume).ToString();

        // Save the volume to PlayerPrefs
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void MusicVolumeSlider()
    {
        float volume = musicVolumeSlider.value;
        MusicManager.Instance?.MusicVolume(volume / 100f);
        musicVolumeValue.text = ((int)volume).ToString();

        // Save the volume to PlayerPrefs
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void CameraShakeSlider()
    {
        float shakePreference = cameraShakeSlider.value;
        cameraShakeValue.text = $"{(int)shakePreference}%";

        // Save the shake preference to PlayerPrefs
        PlayerPrefs.SetFloat(CameraShakeKey, shakePreference);
        PlayerPrefs.Save();
    }
}