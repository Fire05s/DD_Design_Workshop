using UnityEngine;

[CreateAssetMenu(fileName = "ShopDialogueData", menuName = "Dialogue/Shop")]
public class ShopDialogData : ScriptableObject
{
    [field: SerializeField]
    public string DialogLine { get; private set; }
    [field: SerializeField]
    public bool CShopOpenLine { get; private set; }
    [field: SerializeField]
    public bool CShopBuyLine { get; private set; }
    [field: SerializeField]
    public bool DBShopOpenLine { get; private set; }
    [field: SerializeField]
    public bool DBShopBuyLine { get; private set; }
    [field: SerializeField]
    public bool CUpgShopOpenLine { get; private set; }
    [field: SerializeField]
    public bool CUpgShopSelectLine { get; private set; }
    [field: SerializeField]
    public bool DBUpgShopOpenLine { get; private set; }
    [field: SerializeField]
    public bool DBUpgShopSelectLine { get; private set; }
}