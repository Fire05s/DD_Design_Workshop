using UnityEngine;

public class BuildableInventory : MonoBehaviour
{
    private SyncedInventory _mainInventory;
    private void Start()
    {
        _mainInventory = GetComponentInParent<SyncedInventory>();
    }


}
