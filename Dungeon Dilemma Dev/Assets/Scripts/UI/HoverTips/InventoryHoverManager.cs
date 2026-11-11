using System;
using Items;
using Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHoverManager : MonoBehaviour
{
    [SerializeField] private Sprite hpSprite;
    [SerializeField] private Sprite regenSprite;
    [SerializeField] private Sprite spdSprite;
    [SerializeField] private Sprite pwrSprite;
    [SerializeField] private Sprite resSprite;
    [SerializeField] private Sprite scaleSprite;
    private const int TOOLTIP_XSKEW = 175;
    private const int TOOLTIP_YSKEW = 195;
    private const int TOOLTIP_SMALL_YSKEW = 160;

    // Equipment Tooltip
    private GameObject tooltipEquipmentGroup;
    private TextMeshProUGUI tooltipEquipmentName;
    private TextMeshProUGUI tooltipEquipmentDescription;

    // Upgrade Tooltip
    private GameObject tooltipUpgradeGroup;
    private TextMeshProUGUI tooltipUpgradeName;
    private TextMeshProUGUI tooltipUpgradeDescription;

    // Actions
    public static Action<InventoryHoverTip.InventoryHoverType, int, Vector3, InventoryHoverTip.InventoryHoverSkew> OnMouseHover;
    public static Action OnMouseLoseFocus;

    private void OnEnable()
    {
        tooltipEquipmentGroup = GameObject.Find("EquipmentTooltip");
        tooltipEquipmentName = GameObject.Find("eq_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipEquipmentDescription = GameObject.Find("equipment_ability_text").GetComponent<TextMeshProUGUI>();

        tooltipUpgradeGroup = GameObject.Find("UpgradeTooltip");
        tooltipUpgradeName = GameObject.Find("up_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipUpgradeDescription = GameObject.Find("upgrade_ability_text").GetComponent<TextMeshProUGUI>();

        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;

        HideTip();
    }

    private void OnDisable()
    {
        OnMouseHover -= ShowTip;
        OnMouseLoseFocus -= HideTip;
    }

    private void ShowTip(InventoryHoverTip.InventoryHoverType hoverType, int index, Vector3 mousePos, InventoryHoverTip.InventoryHoverSkew hoverSkew)
    {
        Vector2 tooltipPosition = Vector2.zero;
        switch (hoverSkew)
        {
            case InventoryHoverTip.InventoryHoverSkew.Up:
                if (hoverType == InventoryHoverTip.InventoryHoverType.Upgrade)
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y + TOOLTIP_SMALL_YSKEW);
                }
                else
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y + TOOLTIP_YSKEW);
                }
                break;
            case InventoryHoverTip.InventoryHoverSkew.Down:
                if (hoverType == InventoryHoverTip.InventoryHoverType.Upgrade)
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y - TOOLTIP_SMALL_YSKEW);
                }
                else
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y - TOOLTIP_YSKEW);
                }
                break;
            case InventoryHoverTip.InventoryHoverSkew.Left:
                tooltipPosition = new Vector2(mousePos.x - TOOLTIP_XSKEW, mousePos.y);
                break;
            case InventoryHoverTip.InventoryHoverSkew.Right:
                tooltipPosition = new Vector2(mousePos.x + TOOLTIP_XSKEW, mousePos.y);
                break;
        }
        if (hoverType == InventoryHoverTip.InventoryHoverType.Equipment && index < ShopEntryPoint.localShopInstance.GetHeldEquipment().Count)
        {
            Equipment curItem = ShopEntryPoint.localShopInstance.GetHeldEquipment()[index];

            tooltipEquipmentName.text = curItem.Name;
            tooltipEquipmentDescription.text = curItem.AbilityDescription;
            tooltipEquipmentGroup.SetActive(true);
            tooltipEquipmentGroup.transform.position = tooltipPosition;
            int statLine = 1;
            if (curItem.Health != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.gameObject.SetActive(true);
                statValueText.gameObject.SetActive(true);
                statIcon.gameObject.SetActive(true);

                statText.text = "Health";
                statValueText.text = curItem.Health.ToString();
                statIcon.sprite = hpSprite;
                statValueText.color = new Color(255/255f, 93/255f, 90/255f);
                statLine++;
            }
            if (curItem.Regeneration != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.gameObject.SetActive(true);
                statValueText.gameObject.SetActive(true);
                statIcon.gameObject.SetActive(true);

                statText.text = "Regeneration";
                statValueText.text = curItem.Regeneration.ToString();
                statIcon.sprite = regenSprite;
                statValueText.color = new Color(87/255f, 232/255f, 108/255f);
                statLine++;
            }
            if (curItem.MoveSpeed != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.alpha = 255f;
                statValueText.alpha = 255f;
                Color tempColor = statIcon.color;
                tempColor.a = 255f;
                statIcon.color = tempColor;

                statText.text = "Movespeed";
                statValueText.text = curItem.Health.ToString();
                statIcon.sprite = spdSprite;
                statValueText.color = new Color(214/255f, 214/255f, 84/255f);
                statLine++;
            }
            if (curItem.Power != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.alpha = 255f;
                statValueText.alpha = 255f;
                Color tempColor = statIcon.color;
                tempColor.a = 255f;
                statIcon.color = tempColor;

                statText.text = "Power";
                statValueText.text = curItem.Power.ToString();
                statIcon.sprite = pwrSprite;
                statValueText.color = new Color(255/255f, 83/255f, 255/255f);
                statLine++;
            }
            if (curItem.Resistance != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.alpha = 255f;
                statValueText.alpha = 255f;
                Color tempColor = statIcon.color;
                tempColor.a = 255f;
                statIcon.color = tempColor;

                statText.text = "Resistance";
                statValueText.text = curItem.Resistance.ToString();
                statIcon.sprite = resSprite;
                statValueText.color = new Color(115/255f, 115/255f, 255/255f);
                statLine++;
            }
            if (curItem.Scale != 0)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.alpha = 255f;
                statValueText.alpha = 255f;
                Color tempColor = statIcon.color;
                tempColor.a = 255f;
                statIcon.color = tempColor;

                statText.text = "Scale";
                statValueText.text = curItem.Scale.ToString();
                statIcon.sprite = scaleSprite;
                statValueText.color = new Color(84/255f, 215/255f, 215/255f);
                statLine++;
            }
            for (; statLine <= 4; statLine++)
            {
                TextMeshProUGUI statText = GameObject.Find("stat_text" + statLine).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI statValueText = GameObject.Find("stat_value_text" + statLine).GetComponent<TextMeshProUGUI>();
                Image statIcon = GameObject.Find("stat_icon" + statLine).GetComponent<Image>();

                statText.alpha = 0f;
                statValueText.alpha = 0f;
                Color tempColor = statIcon.color;
                tempColor.a = 0f;
                statIcon.color = tempColor;
            }
        }
        else if (hoverType == InventoryHoverTip.InventoryHoverType.Upgrade && index < ShopEntryPoint.localShopInstance.GetHeldUpgrades().Count) {
            Upgrade curItem = ShopEntryPoint.localShopInstance.GetHeldUpgrades()[index];
            tooltipUpgradeName.text = curItem.Name;
            tooltipUpgradeDescription.text = curItem.AbilityDescription;

            tooltipUpgradeGroup.SetActive(true);
            tooltipUpgradeGroup.transform.position = tooltipPosition;
        }
    }

    private void HideTip()
    {
        tooltipEquipmentGroup.SetActive(false);
        tooltipUpgradeGroup.SetActive(false);
    }
}