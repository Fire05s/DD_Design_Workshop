using UnityEngine;

public class CreditsScreenManager : MonoBehaviour
{
    private GameObject creditsScreen;

    void Start()
    {
        creditsScreen = gameObject;
        creditsScreen.SetActive(false);
    }

    public void BackButton()
    {
        creditsScreen.SetActive(false);
    }
}