using Avocado;
using PurrNet;
using UnityEngine;

public class PlayerContainer : NetworkBehaviour
{
    [Header("Character References")]
    public GameObject CharacterInstance;

    [ObserversRpc(runLocally: true)]
    public void SpawnCharacter(GameObject character, Vector2 location, string tag)
    {
        if (CharacterInstance != null || !isOwner) return;

        CharacterInstance = Instantiate(character, location, Quaternion.identity, transform);
        // Give ownership of character to same owner
        CharacterInstance.GetComponent<NetworkBehaviour>().GiveOwnership(owner, propagateToChildren: true);
        this.tag = tag;
        ConfirmSpawnServerRpc();
    }

    [TargetRpc]
    public void SetCharacterPosition(PlayerID id, Vector2 newPosition)
    {
        CharacterInstance.transform.position = newPosition;
    }

    [ServerRpc]
    private void ConfirmSpawnServerRpc()
    {
        CharacterSelection selection = FindFirstObjectByType<CharacterSelection>();
        selection.NotifySpawnConfirmed(owner.Value);
    }
}
