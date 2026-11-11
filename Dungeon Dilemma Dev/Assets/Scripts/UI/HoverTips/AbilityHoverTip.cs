using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class AbilityHoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    private float timeToWait = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        AbilityHoverManager.OnMouseLoseFocus();
    }

    private void ShowTooltip()
    {
        AbilityHoverManager.OnMouseHover(Input.mousePosition, icon);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);
        ShowTooltip();
    }
}