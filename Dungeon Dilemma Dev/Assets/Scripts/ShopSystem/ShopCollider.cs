using UnityEngine;

public class ShopCollider : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        col.gameObject.GetComponentInChildren<PlayerUiManager>().canOpenShop=true;
    }
    void OnTriggerExit2D(Collider2D col)
    {
        col.gameObject.GetComponentInChildren<PlayerUiManager>().canOpenShop=false;
    }
}
