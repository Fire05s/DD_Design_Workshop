using UnityEngine;

public class MainMenuSettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject settingsScreen;
    public void OpenSettings()
    {
        settingsScreen.SetActive(true);
    }
}