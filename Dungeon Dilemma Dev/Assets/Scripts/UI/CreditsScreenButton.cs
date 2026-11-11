using UnityEngine;

public class CreditsScreenButton : MonoBehaviour
{
    [SerializeField] private GameObject creditsScreen;
    public void OpenCredits()
    {
        creditsScreen.SetActive(true);
    }
}