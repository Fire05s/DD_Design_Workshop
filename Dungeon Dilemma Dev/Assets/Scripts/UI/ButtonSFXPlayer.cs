using UnityEngine;

public class ButtonSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip buttonSFX;
    public void PlayButtonSFX()
    {
        SoundFXManager.Instance.PlayLocalSound(buttonSFX, transform.position);
    }
}