using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class InventoryHoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum InventoryHoverType { Equipment, Upgrade }
    public enum InventoryHoverSkew { Up, Down, Left, Right }
    public InventoryHoverType hoverType;
    public InventoryHoverSkew hoverSkew;
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
        InventoryHoverManager.OnMouseLoseFocus();
    }

    private void ShowTooltip()
    {
        InventoryHoverManager.OnMouseHover(hoverType, itemIndex, Input.mousePosition, hoverSkew);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);
        ShowTooltip();
    }
}