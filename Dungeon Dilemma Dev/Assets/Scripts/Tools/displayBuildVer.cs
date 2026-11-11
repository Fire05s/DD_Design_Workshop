using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayBuildVer : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI vernumtext = GetComponent<TextMeshProUGUI>();
        vernumtext.text = Application.version;
    }
}
