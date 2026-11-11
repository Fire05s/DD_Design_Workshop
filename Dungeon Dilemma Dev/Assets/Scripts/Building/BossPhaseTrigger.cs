using ControllerLogic;
using PurrNet;
using Sync_Modules;
using UnityEngine;

public class BossPhaseTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GlobalController.Singleton.WorldData.RoundState != RoundState.BossFight &&
            collision.CompareTag("Challenger"))
        {
            GlobalController.Singleton.ActivateBossFight(); // End current phase
            TeleportPlayers(collision.transform.position);
        }
    }

    private void TeleportPlayers(Vector3 pos)
    {
        foreach (SyncedStats stat in GlobalController.Singleton.GetAllStats())
        {
            if (stat.CompareTag("Boss")) continue;
            PlayerContainer container = stat.GetComponent<PlayerContainer>();
            TeleportTargetRpc(stat.owner.Value, container, pos);
        }
    }

    [TargetRpc]
    private static void TeleportTargetRpc(PlayerID id, PlayerContainer container, Vector3 pos)
    {
        container.SetCharacterPosition(id, pos);
    }
}
