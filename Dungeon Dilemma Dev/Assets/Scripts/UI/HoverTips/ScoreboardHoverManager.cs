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

public class ScoreboardHoverManager : MonoBehaviour
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
    private List<SyncedStats> sortedStats = new();

    // Equipment Tooltip
    private GameObject tooltipEquipmentGroup;
    private TextMeshProUGUI tooltipEquipmentName;
    private TextMeshProUGUI tooltipEquipmentDescription;

    // Upgrade Tooltip
    private GameObject tooltipUpgradeGroup;
    private TextMeshProUGUI tooltipUpgradeName;
    private TextMeshProUGUI tooltipUpgradeDescription;

    // Score Tooltip
    private GameObject tooltipScoreGroup;
    private TextMeshProUGUI tooltipScoreValue;
    private TextMeshProUGUI damageDealtValue;
    private TextMeshProUGUI damageBlockedValue;
    private TextMeshProUGUI healedValue;
    private TextMeshProUGUI supportValue;
    private TextMeshProUGUI bossHitsValue;

    // Actions
    public static Action<ScoreboardHoverTip.ScoreboardHoverType, int, int, Vector3, ScoreboardHoverTip.ScoreboardHoverSkew> OnMouseHover;
    public static Action OnMouseLoseFocus;

    private void OnEnable()
    {
        tooltipEquipmentGroup = GameObject.Find("EquipmentTooltip");
        tooltipEquipmentName = GameObject.Find("eq_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipEquipmentDescription = GameObject.Find("equipment_ability_text").GetComponent<TextMeshProUGUI>();

        tooltipUpgradeGroup = GameObject.Find("UpgradeTooltip");
        tooltipUpgradeName = GameObject.Find("up_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipUpgradeDescription = GameObject.Find("upgrade_ability_text").GetComponent<TextMeshProUGUI>();

        tooltipScoreGroup = GameObject.Find("ScoreTooltip");
        tooltipScoreValue = GameObject.Find("score_value_text").GetComponent<TextMeshProUGUI>();
        damageDealtValue = GameObject.Find("dd_value_text").GetComponent<TextMeshProUGUI>();
        damageBlockedValue = GameObject.Find("blocked_value_text").GetComponent<TextMeshProUGUI>();
        healedValue = GameObject.Find("healed_value_text").GetComponent<TextMeshProUGUI>();
        supportValue = GameObject.Find("support_value_text").GetComponent<TextMeshProUGUI>();
        bossHitsValue = GameObject.Find("boss_hits_value_text").GetComponent<TextMeshProUGUI>();

        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;

        HideTip();
    }

    private void OnDisable()
    {
        OnMouseHover -= ShowTip;
        OnMouseLoseFocus -= HideTip;
    }

    private void ShowTip(ScoreboardHoverTip.ScoreboardHoverType hoverType, int placement, int index, Vector3 mousePos, ScoreboardHoverTip.ScoreboardHoverSkew hoverSkew)
    {
        sortedStats = GlobalController.Singleton?.GetAllStats().OrderByDescending(player => (float)player.Score.value).ToList();
        Vector2 tooltipPosition = Vector2.zero;
        if (placement == 4)
        {
            placement = sortedStats.Count - 1;
        }

        switch (hoverSkew)
        {
            case ScoreboardHoverTip.ScoreboardHoverSkew.Up:
                if (hoverType == ScoreboardHoverTip.ScoreboardHoverType.Score || hoverType == ScoreboardHoverTip.ScoreboardHoverType.Upgrade)
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y + TOOLTIP_SMALL_YSKEW);
                }
                else
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y + TOOLTIP_YSKEW);
                }
                break;
            case ScoreboardHoverTip.ScoreboardHoverSkew.Down:
                if (hoverType == ScoreboardHoverTip.ScoreboardHoverType.Score || hoverType == ScoreboardHoverTip.ScoreboardHoverType.Upgrade)
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y - TOOLTIP_SMALL_YSKEW);
                }
                else
                {
                    tooltipPosition = new Vector2(mousePos.x, mousePos.y - TOOLTIP_YSKEW);
                }
                break;
            case ScoreboardHoverTip.ScoreboardHoverSkew.Left:
                tooltipPosition = new Vector2(mousePos.x - TOOLTIP_XSKEW, mousePos.y);
                break;
            case ScoreboardHoverTip.ScoreboardHoverSkew.Right:
                tooltipPosition = new Vector2(mousePos.x + TOOLTIP_XSKEW, mousePos.y);
                break;
        }

        if (hoverType == ScoreboardHoverTip.ScoreboardHoverType.Equipment && index < sortedStats[placement].GetComponent<SyncedInventory>().HeldEquipment.Count)
        {
            Equipment curItem = sortedStats[placement].GetComponent<SyncedInventory>().HeldEquipment[index];

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
                statValueText.text = curItem.MoveSpeed.ToString();
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
        else if (hoverType == ScoreboardHoverTip.ScoreboardHoverType.Upgrade && index < sortedStats[placement].GetComponent<SyncedInventory>().ActiveUpgrades.Count) {
            Upgrade curItem = sortedStats[placement].GetComponent<SyncedInventory>().ActiveUpgrades[index];
            tooltipUpgradeName.text = curItem.Name;
            tooltipUpgradeDescription.text = curItem.AbilityDescription;

            tooltipUpgradeGroup.SetActive(true);
            tooltipUpgradeGroup.transform.position = tooltipPosition;
        }
        else if (hoverType == ScoreboardHoverTip.ScoreboardHoverType.Score)
        {
            SyncedStats curStats = sortedStats[placement];
            if (placement == sortedStats.Count - 1) // DB Special "Score"
            {
                tooltipScoreValue.text = "Yew has no Score";
            }
            else
            {
                tooltipScoreValue.text = FormatScore(curStats.Score.value);
            }
            damageDealtValue.text = FormatScore(curStats.DamageDealt.value);
            damageBlockedValue.text = FormatScore(curStats.DamageBlocked.value);
            healedValue.text = FormatScore(curStats.RegeneratedHealth.value);
            supportValue.text = FormatScore(curStats.SupportPoints.value);
            bossHitsValue.text = FormatScore(curStats.BossHits.value);

            tooltipScoreGroup.SetActive(true);
            tooltipScoreGroup.transform.position = tooltipPosition;
        }
    }

    private void HideTip()
    {
        tooltipEquipmentGroup.SetActive(false);
        tooltipUpgradeGroup.SetActive(false);
        tooltipScoreGroup.SetActive(false);
    }

    public string FormatScore(double number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000d).ToString("0.#") + "B";

        if (number >= 1_000_000)
            return (number / 1_000_000d).ToString("0.#") + "M";

        if (number >= 1_000)
            return (number / 1_000d).ToString("0.#") + "K";

        return number.ToString("0");
    }
}