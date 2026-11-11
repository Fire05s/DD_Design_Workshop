using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ShopHoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ShopHoverType { Equipment, Upgrade }
    public enum ShopHoverSkew { Up, Down, Left, Right }
    public ShopHoverType hoverType;
    public ShopHoverSkew hoverSkew;
    public int itemIndex;
    private float timeToWait = 0.1f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        ShopHoverFlavorManager.OnMouseLoseFocus();
    }

    private void ShowTooltip()
    {
        ShopHoverFlavorManager.OnMouseHover(hoverType, itemIndex, Input.mousePosition, hoverSkew);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);
        ShowTooltip();
    }
}