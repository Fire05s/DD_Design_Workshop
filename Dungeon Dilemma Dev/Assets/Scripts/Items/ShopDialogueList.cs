using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ItemReference {
[CreateAssetMenu(menuName = "Reference/ShopDialogueList")]
public class ShopDialogList : ScriptableObject
{
    [field: SerializeField] public List<ShopDialogData> ShopDialogueDatas = new List<ShopDialogData>();
    public List<ShopDialogData> CShopOpenDialogData => ShopDialogueDatas.Where(el => el.CShopOpenLine).ToList();
    public List<ShopDialogData> CShopBuyDialogData => ShopDialogueDatas.Where(el => el.CShopBuyLine).ToList();
    public List<ShopDialogData> DBShopOpenDialogData => ShopDialogueDatas.Where(el => el.DBShopOpenLine).ToList();
    public List<ShopDialogData> DBShopBuyDialogData => ShopDialogueDatas.Where(el => el.DBShopBuyLine).ToList();
    public List<ShopDialogData> CUpgShopOpenDialogData => ShopDialogueDatas.Where(el => el.CUpgShopOpenLine).ToList();
    public List<ShopDialogData> CUpgShopSelectDialogData => ShopDialogueDatas.Where(el => el.CUpgShopSelectLine).ToList();
    public List<ShopDialogData> DBUpgShopOpenDialogData => ShopDialogueDatas.Where(el => el.DBUpgShopOpenLine).ToList();
    public List<ShopDialogData> DBUpgShopSelectDialogData => ShopDialogueDatas.Where(el => el.DBUpgShopSelectLine).ToList();
}
}