using NavMeshPlus.Components;
using PurrNet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class RoomBuilder : MonoBehaviour
{
    public static RoomBuilder Instance;

    [Header("Rooms")]
    public List<SyncedRoom> StartRooms = new List<SyncedRoom>();
    public List<SyncedRoom> AvailableRooms = new List<SyncedRoom>();
    public List<SyncedRoom> BossRooms = new List<SyncedRoom>();

    public SyncedRoom RoomOption1 { get; private set; }
    public SyncedRoom RoomOption2 { get; private set; }

    [Header("Preview Settings")]
    [SerializeField] private float _tiltAmount = 3f;
    [SerializeField] private float _previewMoveSpeed = 15f;
    [SerializeField] private Color _validColor = Color.green;
    [SerializeField] private Color _invalidColor = Color.red;

    private SyncedRoom previewInstance;
    private SyncedRoom currentPreviewPrefab;
    private Tilemap[] previewTilemaps;

    public SyncedRoom StartRoom { get; private set; }
    public SyncedRoom BossRoom { get; private set; }

    Direction[] allowedDirs = new Direction[]
    {
        Direction.Up,
        Direction.Left,
        Direction.Right
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ChooseRoomOptions();
    }

    public void GenerateInitialLayout(int roomCount)
    {
        List<Vector2Int> placedPositions = new();

        // First room at origin
        Vector2Int startPos = Vector2Int.zero;
        SyncedRoom initialRoom = StartRooms[Random.Range(0, StartRooms.Count)];
        StartRoom = PlaceRoom(initialRoom, startPos);
        placedPositions.Add(startPos);
        Direction dir = Direction.Up;

        while (placedPositions.Count < roomCount)
        {
            // Get most recent room
            Vector2Int basePos = placedPositions[placedPositions.Count - 1];

            Vector2Int newPos = basePos + RoomDirectionHelper.ToVector(dir);

            if (!GridManager.Instance.IsOccupied(newPos))
            {
                SyncedRoom newRoom = null;
                if (placedPositions.Count == roomCount - 1)
                {
                    // Last room = boss room
                    newRoom = BossRooms[Random.Range(0, BossRooms.Count)];
                    if (newRoom == null) return;
                    BossRoom = PlaceRoom(newRoom, newPos);
                }
                else
                {
                    newRoom = AvailableRooms[Random.Range(0, AvailableRooms.Count)];
                    if (newRoom == null) return;
                    PlaceRoom(newRoom, newPos);
                }
                placedPositions.Add(newPos);
            }

            // Pick new direction
            dir = allowedDirs[Random.Range(0, allowedDirs.Length)];
        }

        UpdateSpawnpoints();
    }

    public SyncedRoom PlaceRoom(SyncedRoom roomPrefab, Vector2Int pos)
    {
        if (!CanPlaceRoom(pos)) return null;

        Vector3 worldPos = GridManager.Instance.RoomGridToWorld(pos);

        SyncedRoom newRoom = Instantiate(roomPrefab, worldPos, Quaternion.identity).GetComponent<SyncedRoom>();
        GridManager.Instance.SetRoom(pos, newRoom);

        // Update this room's doors
        UpdateDoors(newRoom);

        // Update neighbors' doors
        UpdateNeighbors(pos);

        return newRoom;
    }
    public void UpdateRoomPreview(SyncedRoom roomPrefab, Vector2Int gridPos)
    {
        if (previewInstance == null || currentPreviewPrefab != roomPrefab)
            CreatePreview(roomPrefab);

        Vector3 targetPos = GridManager.Instance.RoomGridToWorld(gridPos);

        // Smooth movement
        previewInstance.transform.position = Vector3.Lerp(
            previewInstance.transform.position,
            targetPos,
            _previewMoveSpeed * Time.deltaTime
        );

        // Direction for tilt
        Vector3 direction = targetPos - previewInstance.transform.position;

        float tiltZ = -direction.x * _tiltAmount;

        Quaternion targetRot = Quaternion.Euler(0, 0, tiltZ);

        previewInstance.transform.rotation = Quaternion.Lerp(
            previewInstance.transform.rotation,
            targetRot,
            10f * Time.deltaTime
        );

        bool valid = CanPlaceRoom(gridPos);
        SetPreviewColor(valid ? _validColor : _invalidColor);
    }

    public void UpdateAllDoors()
    {
        foreach (var room in GridManager.Instance.GetAllRooms())
        {
            UpdateDoors(room);
        }
    }

    public void UpdateDoors(SyncedRoom room)
    {
        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            Debug.Log($"Checking {dir.ToString()} for {room.name}");
            Door door = room.GetDoor(dir);
            if (door == null) continue;

            Vector2Int neighborPos = room.GridPosition + RoomDirectionHelper.ToVector(dir);
            SyncedRoom neighbor = GridManager.Instance.GetRoom(neighborPos);

            if (neighbor != null)
            {
                Debug.Log($"Found {dir.ToString()} neighbor for {room.name}");
                // Check if neighbor has matching door
                bool neighborHasDoor = neighbor.HasDoor(RoomDirectionHelper.Opposite(dir));

                door.SetOpen(neighborHasDoor);
            }
            else
            {
                // No neighbor? close it
                Debug.Log($"{room.name} don't have {dir.ToString()} neighbor");
                door.SetOpen(false);
            }
        }
    }

    public void UpdateNeighbors(Vector2Int originPos)
    {
        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2Int neighborPos = originPos + RoomDirectionHelper.ToVector(dir);

            if (GridManager.Instance.IsOccupied(neighborPos))
            {
                SyncedRoom neighbor = GridManager.Instance.GetRoom(neighborPos);
                UpdateDoors(neighbor);
            }
        }
    }

    public void ExitRoomBuilding()
    {
        if (previewInstance != null) Destroy(previewInstance.gameObject);
    }

    private bool CanPlaceRoom(Vector2Int pos)
    {
        // Spot must be empty
        if (GridManager.Instance.IsOccupied(pos))
            return false;

        // First room can be anywhere
        if (GridManager.Instance.RoomCount == 0)
            return true;

        // Future rooms must have at least one neighbor
        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            Vector2Int neighborPos = pos + RoomDirectionHelper.ToVector(dir);

            if (GridManager.Instance.IsOccupied(neighborPos))
                return true;
        }

        return false;
    }

    private void CreatePreview(SyncedRoom roomPrefab)
    {
        if (previewInstance != null)
            Destroy(previewInstance.gameObject);

        currentPreviewPrefab = roomPrefab;
        previewInstance = Instantiate(roomPrefab);

        // Disable all colliders so it doesn't interfere
        foreach (var col in previewInstance.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Get all renderers to tint them
        previewTilemaps = previewInstance.GetComponentsInChildren<Tilemap>();

        SetPreviewColor(_validColor);
    }

    private void SetPreviewColor(Color color)
    {
        if (previewTilemaps == null) return;

        foreach (var tilemap in previewTilemaps)
        {
            Color c = color;
            c.a = 0.5f;
            tilemap.color = c;
            tilemap.GetComponent<TilemapRenderer>().sortingOrder += 1;
        }
    }

    public void ChooseRoomOptions()
    {
        if (AvailableRooms.Count < 2)
        {
            Debug.LogWarning("Not enough rooms to pick two unique options.");
            RoomOption1 = RoomOption2 = AvailableRooms[0];
            return;
        }

        RoomOption1 = AvailableRooms[Random.Range(0, AvailableRooms.Count)];

        do
        {
            RoomOption2 = AvailableRooms[Random.Range(0, AvailableRooms.Count)];
        }
        while (RoomOption2 == RoomOption1);

    }

    public void UpdateSpawnpoints()
    {
        SetSpawnServerRpc("Boss Spawnpoint", BossRoom.GridPosition);
        //SetSpawnServerRpc("Raid Start Point", StartRoom.GridPosition);
    }

    [ServerRpc]
    private static void SetSpawnServerRpc(string name, Vector2Int location)
    {
        GameObject spawn = GameObject.Find(name);
        Vector3 roomPos = GridManager.Instance.GridToWorldRoomCenter(location);
        if (spawn != null) spawn.transform.position = roomPos;
        else Debug.LogWarning($"Can't find {name}");
    }
}
