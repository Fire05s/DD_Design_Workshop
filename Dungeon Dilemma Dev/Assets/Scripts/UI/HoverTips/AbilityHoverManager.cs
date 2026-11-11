using System;
using Items;
using Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHoverManager : MonoBehaviour
{
    private const int TOOLTIP_XSKEW = 175;
    private const int TOOLTIP_YSKEW = 195;

    // Equipment Tooltip
    private GameObject tooltipAbilityGroup;
    private TextMeshProUGUI tooltipAbilityName;
    private TextMeshProUGUI tooltipSubtitleName;
    private TextMeshProUGUI tooltipAbilityDescription;

    // Actions
    public static Action<Vector3, Image> OnMouseHover;
    public static Action OnMouseLoseFocus;

    private void OnEnable()
    {
        tooltipAbilityGroup = GameObject.Find("AbilityTooltip");
        tooltipAbilityName = GameObject.Find("ab_tooltip_name_text").GetComponent<TextMeshProUGUI>();
        tooltipSubtitleName = GameObject.Find("ab_tooltip_subtitle_text").GetComponent<TextMeshProUGUI>();
        tooltipAbilityDescription = GameObject.Find("ab_text").GetComponent<TextMeshProUGUI>();

        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;

        HideTip();
    }

    private void OnDisable()
    {
        OnMouseHover -= ShowTip;
        OnMouseLoseFocus -= HideTip;
    }

    private void ShowTip(Vector3 mousePos, Image icon)
    {
        Vector2 tooltipPosition = new Vector2(mousePos.x - TOOLTIP_XSKEW, mousePos.y + TOOLTIP_YSKEW);

        // Yarrow's Abilities
        if (icon.sprite.name == "MeleePrimary")
        {
            tooltipAbilityName.text = "<color=#52C6F5>The Rebel's Blade</color>";
            tooltipSubtitleName.text = "Primary";
            tooltipAbilityDescription.text = "Slash in the direction of your Mouse, dealing damage (1.5x <color=#FF53FF>Power</color>) to all enemies hit.";
        }
        else if (icon.sprite.name == "MeleeSecondary")
        {
            tooltipAbilityName.text = "<color=#52C6F5>Sharpened Blade</color>";
            tooltipSubtitleName.text = "Secondary";
            tooltipAbilityDescription.text = "Sharpen your blade, empowering yourself to multiply your damage by 33% of your <color=#FF53FF>Power</color> for 10 seconds.";
        }
        else if (icon.sprite.name == "MeleeDash")
        {
            tooltipAbilityName.text = "<color=#52C6F5>Steel Toe</color>";
            tooltipSubtitleName.text = "Dash";
            tooltipAbilityDescription.text = "Invincible dash where you’re moving, dealing damage (0.75x <color=#FF53FF>Power</color>) to all enemies in your path.";
        }
        // Amber's Abilities
        if (icon.sprite.name == "TankPrimary")
        {
            tooltipAbilityName.text = "<color=#F59C57>The Bulwark's Axe</color>";
            tooltipSubtitleName.text = "Primary";
            tooltipAbilityDescription.text = "Slash in the direction of your Mouse, dealing damage (1x <color=#FF53FF>Power</color>) to all enemies hit.";
        }
        else if (icon.sprite.name == "TankSecondary")
        {
            tooltipAbilityName.text = "<color=#F59C57>Shield Up!</color>";
            tooltipSubtitleName.text = "Secondary";
            tooltipAbilityDescription.text = "Raise your Shield, doubling your <color=#7373FF>Resistance</color>. You move <color=#D6D654>15% Slower</color> while protected.";
        }
        else if (icon.sprite.name == "TankDash")
        {
            tooltipAbilityName.text = "<color=#F59C57>Protective Dive</color>";
            tooltipSubtitleName.text = "Dash";
            tooltipAbilityDescription.text = "Invincible dash where you’re moving, giving Armor (0.75x <color=#FF53FF>Power</color>) to yourself and all Allies in your path.";
        }
        // Linay's Abilities
        else if (icon.sprite.name == "Range_Ability1")
        {
            tooltipAbilityName.text = "<color=#C155FF>The Lone Arrow</color>";
            tooltipSubtitleName.text = "Primary";
            tooltipAbilityDescription.text = "Shoot an arrow in the direction of your Mouse, dealing damage (1.5x <color=#FF53FF>Power</color>) to the first enemy hit.";
        }
        else if (icon.sprite.name == "Range_Ability2")
        {
            tooltipAbilityName.text = "<color=#C155FF>Concentration</color>";
            tooltipSubtitleName.text = "Secondary";
            tooltipAbilityDescription.text = "Concentrate and reduce the duration of all of your cooldowns by 33% of your <color=#FF53FF>Power</color> and increase your <color=#D6D654>Movespeed</color> by 15% of your <color=#FF53FF>Power</color> for 10 seconds.";
        }
        else if (icon.sprite.name == "Range_Ability3")
        {
            tooltipAbilityName.text = "<color=#C155FF>Swift Step</color>";
            tooltipSubtitleName.text = "Dash";
            tooltipAbilityDescription.text = "Invincible dash where you’re moving. You can store up to two Dashes.";
        }
        // Ruru's Abilities
        else if (icon.sprite.name == "MagePrimarySpark")
        {
            tooltipAbilityName.text = "<color=#E66161>The Flame's Spark</color>";
            tooltipSubtitleName.text = "Primary 1";
            tooltipAbilityDescription.text = "Cast a slow fireball in the direction of your Mouse that explodes on contact, dealing damage (1.5x <color=#FF53FF>Power</color>) to all enemies hit.";
        }
        else if (icon.sprite.name == "MagePrimaryCone")
        {
            tooltipAbilityName.text = "<color=#E66161>The Flame's Burst</color>";
            tooltipSubtitleName.text = "Primary 2";
            tooltipAbilityDescription.text = "Cast a cone of fire in the direction of your Mouse, dealing damage (1.25x <color=#FF53FF>Power</color>) to all enemies hit.";
        }
        else if (icon.sprite.name == "MageSecondary")
        {
            tooltipAbilityName.text = "<color=#E66161>Spell Swap</color>";
            tooltipSubtitleName.text = "Secondary";
            tooltipAbilityDescription.text = "Switch your Primary Spell and enhance yourself to multiply your damage by 33% of your <color=#FF53FF>Power</color> for 2 seconds.";
        }
        else if (icon.sprite.name == "MageDash")
        {
            tooltipAbilityName.text = "<color=#E66161>Infernal Hop</color>";
            tooltipSubtitleName.text = "Dash";
            tooltipAbilityDescription.text = "Invincible dash where you’re moving and leave behind a molten ground that deals damage (0.5x <color=#FF53FF>Power</color>) to enemies that walk through.";
        }
        // Yew's Phase 1 Abilities
        else if (icon.sprite.name == "primary_phase_1")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Splinter Slash</color>";
            tooltipSubtitleName.text = "Primary (Phase 1)";
            tooltipAbilityDescription.text = "Stand still and summon spikes (1.5x <color=#FF53FF>Power</color>) in the direction of your Mouse after a delay, damaging all Challengers in the area.";
        }
        else if (icon.sprite.name == "secondary_phase_1")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Rooted Rift</color>";
            tooltipSubtitleName.text = "Secondary (Phase 1)";
            tooltipAbilityDescription.text = "Stand still and summon spikes (1x <color=#FF53FF>Power</color>) around yourself after a delay, damaging all Challengers in the area.";
        }
        else if (icon.sprite.name == "dash_phase_1")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Leafy Lash</color>";
            tooltipSubtitleName.text = "Dash (Phase 1)";
            tooltipAbilityDescription.text = "Stand still and invincible dash in the direction of your Mouse after a delay, damaging (1x <color=#FF53FF>Power</color>) all Challengers in your path.";
        }
        // Yew's Phase 2 Abilities
        if (icon.sprite.name == "primary_phase_2")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Splinter Split</color>";
            tooltipSubtitleName.text = "Primary (Phase 2)";
            tooltipAbilityDescription.text = "Stand still and summon spikes (1.5x <color=#FF53FF>Power</color>) and fire a thorn (1.25x <color=#FF53FF>Power</color>) in the direction of your Mouse after a delay. (x2)";
        }
        else if (icon.sprite.name == "secondary_phase_2")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Rooted Rupture</color>";
            tooltipSubtitleName.text = "Secondary (Phase 2)";
            tooltipAbilityDescription.text = "Stand still and summon spikes (1x <color=#FF53FF>Power</color>) and fire thorns (1.25x <color=#FF53FF>Power</color>) around yourself after a delay. (2x)";
        }
        else if (icon.sprite.name == "dash_phase_2")
        {
            tooltipAbilityName.text = "<color=#6FC74F>Leafy Languish</color>";
            tooltipSubtitleName.text = "Dash (Phase 2)";
            tooltipAbilityDescription.text = "Stand still and invincible dash in the direction of your Mouse after a delay, damaging (1x <color=#FF53FF>Power</color>) and crippling all Challengers in your path.";
        }
        else if (icon.sprite.name == "boss-gaze-icon")
        {
            tooltipAbilityName.text = "<color=#6FC74F>The Boss' Gaze</color>";
            tooltipSubtitleName.text = "Spell";
            tooltipAbilityDescription.text = "Send a magical slam where you are looking, damaging (1x <color=#FF53FF>Power</color>) all Challengers caught in the spell.";
        }
        tooltipAbilityGroup.SetActive(true);
        tooltipAbilityGroup.transform.position = tooltipPosition;
    }

    private void HideTip()
    {
        tooltipAbilityGroup.SetActive(false);
    }
}