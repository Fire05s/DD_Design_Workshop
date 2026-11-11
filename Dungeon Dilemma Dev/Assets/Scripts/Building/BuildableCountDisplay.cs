using TMPro;
using UnityEngine;

public class BuildableCountDisplay : MonoBehaviour
{
    [Header("Text Component")]
    [SerializeField] private TextMeshPro _minionText;
    [SerializeField] private TextMeshPro _trapText;
    [SerializeField] private TextMeshPro _amountText;

    [Header("Colors")]
    [SerializeField] private Color _validColor;
    [SerializeField] private Color _invalidColor;

    public void UpdateRoomCount(float minionInRoom, float minionLimit, float trapInRoom, float trapLimit)
    {
        _minionText.text = $"{minionInRoom}/{minionLimit}";
        _minionText.color = (minionInRoom < minionLimit) ? _validColor : _invalidColor;

        _trapText.text = $"{trapInRoom}/{trapLimit}";
        _trapText.color = (trapInRoom < trapLimit) ? _validColor : _invalidColor;
    }

    public void UpdateAmountCount(float currentAmount)
    {
        _amountText.text = $"x{currentAmount}";
        _amountText.color = (currentAmount > 0) ? Color.white : _invalidColor;
    }
}
