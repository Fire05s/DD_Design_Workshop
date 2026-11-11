using ControllerLogic;
using NavMeshPlus.Components;
using PurrNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the room data for the entire grid.
/// </summary>
public class GridManager : MonoBehaviour
{
    public float roomSize = 25f;
    public static GridManager Instance;

    private SyncDictionary<Vector2Int, SyncedRoom> rooms = new();

    public int RoomCount => rooms.Count;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    /// <summary>
    /// Converts world coords to grid coords
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector2Int WorldToRoomGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / roomSize),
            Mathf.FloorToInt(worldPos.y / roomSize)
        );
    }

    /// <summary>
    /// Converts grid coords to world coords
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector3 RoomGridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * roomSize,
            gridPos.y * roomSize,
            0
        );
    }

    public Vector3 GridToWorldRoomCenter(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.x * roomSize) + roomSize / 2,
            (gridPos.y * roomSize) + roomSize / 2,
            0
        );
    }

    public Vector2Int WorldToBuildableGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );
    }

    public Vector2Int WorldToLocalBuildableGrid(Vector3 worldPos, SyncedRoom room)
    {
        Vector3 localPos = worldPos - room.transform.position;

        return new Vector2Int(
            Mathf.FloorToInt(localPos.x),
            Mathf.FloorToInt(localPos.y)
        );
    }

    /// <summary>
    /// Checks if room spot is occupied
    /// </summary>
    /// <param name="pos"> Room position. </param>
    /// <returns> True if a room occupies the spot, false otherwise. </returns>
    public bool IsOccupied(Vector2Int pos) => rooms.ContainsKey(pos);

    /// <summary>
    /// Retrieves the room occupying the position
    /// </summary>
    /// <param name="pos"> Position to check. </param>
    /// <returns> Room data. </returns>
    public SyncedRoom GetRoom(Vector2Int pos)
    {
        rooms.TryGetValue(pos, out SyncedRoom room);
        return room;
    }

    /// <summary>
    /// Assigns the room data at the location.
    /// </summary>
    /// <param name="pos"> Grid location (not world pos) </param>
    /// <param name="room"></param>
    public void SetRoom(Vector2Int pos, SyncedRoom room)
    {
        rooms[pos] = room;
        room.GridPosition = pos;
    }

    /// <summary>
    /// Deletes all rooms from the grid.
    /// </summary>
    public void Clear()
    {
        foreach (SyncedRoom room in rooms.Values)
        {
            if (room != null) Destroy(room.gameObject);
        }

        rooms.Clear();
    }

    public void BuildNavMeshes()
    {
        foreach (var room in rooms.Values)
        {
            BuildNavRpc(room);
        }
    }

    public void RemoveNavMeshes()
    {
        foreach (var room in rooms.Values)
        {
            RemoveNavRpc(room);
        }
    }

    public void RespawnMinions()
    {
        foreach (var room in rooms.Values)
        {
            RespawnMinionInRoom(room);
        }
    }

    [ObserversRpc(runLocally: true)]
    private static void BuildNavRpc(SyncedRoom room)
    {
        if (room == null) return;

        NavMeshSurface navSurface = room.GetComponentInChildren<NavMeshSurface>();

        if (navSurface != null)
        {
            if (!navSurface.isActiveAndEnabled) navSurface.enabled = true;
            room.StartCoroutine(RebuildNavMesh(navSurface));
        }
    }

    private static IEnumerator RebuildNavMesh(NavMeshSurface navSurface)
    {
        yield return null;

        Physics2D.SyncTransforms();

        navSurface.RemoveData();

        yield return null;

        navSurface.BuildNavMesh();
    }

    [ObserversRpc(runLocally: true)]
    private static void RemoveNavRpc(SyncedRoom room)
    {
        if (room == null) return;

        NavMeshSurface navSurface = room.GetComponentInChildren<NavMeshSurface>();

        if (navSurface != null)
        {
            navSurface.RemoveData();
            if (navSurface.isActiveAndEnabled) navSurface.enabled = false;
        }
    }
    private static void RespawnMinionInRoom(SyncedRoom room)
    {
        room.ResetMinionBuildables();
    }

    public List<SyncedRoom> GetAllRooms()
    {
        return rooms.Values.ToList();
    }
}
