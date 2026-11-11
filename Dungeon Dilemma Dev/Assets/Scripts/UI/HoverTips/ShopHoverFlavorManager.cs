using System;
using System.Collections.Generic;
using System.Linq;
using Avocado.Screens;
using ControllerLogic;
using Items;
using Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopHoverFlavorManager : MonoBehaviour
{
    private const int TOOLTIP_XSKEW = 175;
    private const int TOOLTIP_YSKEW = 160;
    private List<SyncedStats> sortedStats = new();

    // Equipment Tooltip
    private GameObject tooltipItemGroup;
    private TextMeshProUGUI tooltipItemName;
    private TextMeshProUGUI tooltipFlavorText;

    // Actions
    public static Action<ShopHoverTip.ShopHoverType, int, Vector3, ShopHoverTip.ShopHoverSkew> OnMouseHover;
    public static Action OnMouseLoseFocus;

    private void OnEnable()
    {
        tooltipItemGroup = GameObject.Find("ItemFlavorTooltip");
        tooltipItemName = GameObject.Find("item_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipFlavorText = GameObject.Find("item_flavor_text").GetComponent<TextMeshProUGUI>();

        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;

        HideTip();
    }

    private void OnDisable()
    {
        OnMouseHover -= ShowTip;
        OnMouseLoseFocus -= HideTip;
    }

    private void ShowTip(ShopHoverTip.ShopHoverType hoverType, int index, Vector3 mousePos, ShopHoverTip.ShopHoverSkew hoverSkew)
    {
        sortedStats = GlobalController.Singleton?.GetAllStats().OrderByDescending(player => (float)player.Score.value).ToList();
        Vector2 tooltipPosition = Vector2.zero;

        switch (hoverSkew)
        {
            case ShopHoverTip.ShopHoverSkew.Up:
                tooltipPosition = new Vector2(mousePos.x, mousePos.y + TOOLTIP_YSKEW);
                break;
            case ShopHoverTip.ShopHoverSkew.Down:
                tooltipPosition = new Vector2(mousePos.x, mousePos.y - TOOLTIP_YSKEW);
                break;
            case ShopHoverTip.ShopHoverSkew.Left:
                tooltipPosition = new Vector2(mousePos.x - TOOLTIP_XSKEW, mousePos.y);
                break;
            case ShopHoverTip.ShopHoverSkew.Right:
                tooltipPosition = new Vector2(mousePos.x + TOOLTIP_XSKEW, mousePos.y);
                break;
        }

        ShopItem curItem;
        if (hoverType == ShopHoverTip.ShopHoverType.Equipment)
        {
            curItem = ShopEntryPoint.localShopInstance.shop.GetCurrentlyInShop()[index];
        }
        else
        {
            if (ShopEntryPoint.localShopInstance.upgradePicked)
            {
                curItem = ShopEntryPoint.localShopInstance.chosenUpgrade;
            }
            else
            {
                curItem = ShopEntryPoint.localShopInstance.upgradeShop.GetCurrentlyInShop()[index];
            }
        }
        tooltipItemName.text = curItem.name;
        tooltipFlavorText.text = curItem.flavortext;
        tooltipItemGroup.SetActive(true);
        tooltipItemGroup.transform.position = tooltipPosition;
    }

    private void HideTip()
    {
        tooltipItemGroup.SetActive(false);
    }
}