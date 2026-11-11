using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ScoreboardHoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ScoreboardHoverType { Equipment, Upgrade, Score }
    public enum ScoreboardHoverSkew { Up, Down, Left, Right }
    public ScoreboardHoverType hoverType;
    public ScoreboardHoverSkew hoverSkew;
    public int placement;
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
        ScoreboardHoverManager.OnMouseLoseFocus();
    }

    private void ShowTooltip()
    {
        ScoreboardHoverManager.OnMouseHover(hoverType, placement, itemIndex, Input.mousePosition, hoverSkew);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeToWait);
        ShowTooltip();
    }
}