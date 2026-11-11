using ControllerLogic;
using Items;
using PlayerLogic.BossLogic;
using PurrNet;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public enum Direction { Up, Down, Left, Right }

public class SyncedRoom : NetworkBehaviour
{
    public Vector2Int GridPosition;

    public Sprite Thumbnail;

    public List<Door> doors;

    [Header("Buildable Limits")]
    public float MinionLimit;
    public float TrapLimit;

    public Dictionary<Vector2Int, GameObject> placedBuildables = new();
    public Dictionary<Vector2Int, Buildable> placedData = new();

    public float MinionCount { get; private set; }
    public float TrapCount { get; private set; }

    [SerializeField] private int _totalMinions;
    [SerializeField] private int _deadMinions;
    [SerializeField] private bool _encounterStarted;

    public Door GetDoor(Direction dir)
    {
        return doors.Find(d => d.direction == dir);
    }

    public bool HasDoor(Direction dir)
    {
        return GetDoor(dir) != null;
    }

    public GameObject GetBuildable(Vector2Int pos)
    {
        return placedBuildables[pos];
    }

    public bool IsBuildableOccupied(Vector2Int pos)
    {
        return placedBuildables.ContainsKey(pos);
    }

    public bool AtBuildableCapacity(Buildable buildable)
    {
        return (MinionCount + buildable.MinionCap > MinionLimit ||
                TrapCount + buildable.TrapCap > TrapLimit);
    }

    public void AddBuildable(Buildable data, GameObject newBuildable, Vector2Int pos)
    {
        placedBuildables[pos] = newBuildable;
        placedData[pos] = data;
        TrapCount += data.TrapCap;
        MinionCount += data.MinionCap;
        if (References.Instance.Minions.Items.Contains(data)) RegisterMinion(newBuildable);

        Debug.Log($"Room {gameObject.name}: Trap - {TrapCount} ; Minion - {MinionCount}");
    }

    public Buildable RemoveBuildable(Vector2Int pos)
    {
        if (placedBuildables.ContainsKey(pos) && placedData.ContainsKey(pos))
        {
            Buildable build = placedData[pos];
            TrapCount -= placedData[pos].TrapCap;
            MinionCount -= placedData[pos].MinionCap;
            if (References.Instance.Minions.Items.Contains(build)) UnregisterMinion(placedBuildables[pos]);

            placedBuildables.Remove(pos);
            placedData.Remove(pos);
            return build;
        }
        return null;
    }

    public void ResetMinionBuildables()
    {
        Debug.Log($"Place data: {placedData.Count}");
        _totalMinions = 0;
        foreach (KeyValuePair<Vector2Int, Buildable> entry in placedData)
        {
            if (References.Instance.Minions.Items.Contains(entry.Value))
            {
                if (placedBuildables.TryGetValue(entry.Key, out GameObject oldBuild)
                && oldBuild != null)
                {
                    if (oldBuild.TryGetComponent(out HealthHandler health))
                        health.OnDeath -= HandleMinionDeath;
                    Destroy(oldBuild);
                }

                float xPos = entry.Key.x + entry.Value.TileOffset.x;
                float yPos = entry.Key.y + entry.Value.TileOffset.y;
                Vector3 spawnLocation = new Vector3(xPos, yPos, 0); // center on tile

                GameObject build = Instantiate(entry.Value.Prefab, transform);
                build.transform.localPosition = spawnLocation;
                build.GetComponent<NetworkBehaviour>()?.GiveOwnership(DungeonBossBuildingController.Singleton.owner);

                placedBuildables[entry.Key] = build;
                RegisterMinion(build);
            }
        }
        _deadMinions = 0; // reset dead minion count
        _encounterStarted = false;
    }

    private void RegisterMinion(GameObject minion)
    {
        _totalMinions++; // track num of minions in room
        if (minion.TryGetComponent(out HealthHandler health))
        {
            health.OnDeath += HandleMinionDeath;
        }
    }

    private void UnregisterMinion(GameObject minion)
    {
        _totalMinions--; // track num of minions in room
        if (minion.TryGetComponent(out HealthHandler health))
        {
            health.OnDeath -= HandleMinionDeath;
        }
    }

    private void HandleMinionDeath()
    {
        if (_deadMinions < _totalMinions) _deadMinions++;
        if (_deadMinions >= _totalMinions)
        {
            UnlockDoors();
        }
    }
    public void PlayerEntered(Door entranceDoor)
    {
        if (_encounterStarted || _totalMinions == 0)
            return;

        _encounterStarted = true;

        foreach (Door door in doors)
        {
            if (door != entranceDoor)
            {
                door.SetLock(true);
            }
        }
    }
    private void UnlockDoors()
    {
        foreach (Door door in doors)
        {
            door.SetLock(false);
        }
    }
}
