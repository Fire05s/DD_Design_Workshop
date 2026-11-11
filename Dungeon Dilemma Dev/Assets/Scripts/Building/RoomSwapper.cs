using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomSwapper : MonoBehaviour
{
    public static RoomSwapper Instance;
    [SerializeField] private RoomBuilder _roomBuilder;

    [Header("Selection Settings")]
    [SerializeField] private Color _selectionColor = Color.blue;

    private SyncedRoom _selectedRoom; // currently selected existing room
    private SyncedRoom _hoveredRoom; // room that player is hovering over

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void HandleRoomSwap(Vector2Int gridPos)
    {
        SyncedRoom clickedRoom = GridManager.Instance.GetRoom(gridPos);
        if (clickedRoom == null || clickedRoom == RoomBuilder.Instance?.StartRoom) return;

        if (_selectedRoom == null)
        {
            // Set first room selection
            _selectedRoom = clickedRoom;
            HighlightRoom(_selectedRoom, true);
        }
        else
        {
            // Swap the selected rooms
            HighlightRoom(_selectedRoom, false);
            SwapRoom(_selectedRoom, clickedRoom);
            _selectedRoom = null;
        }
    }
    public void UpdateHover(Vector2Int gridPos)
    {
        SyncedRoom room = GridManager.Instance.GetRoom(gridPos);

        // If hovering same room, do nothing
        if (room == _hoveredRoom) return;

        // Remove highlight from previous hover (if it's not selected)
        if (_hoveredRoom != null && _hoveredRoom != _selectedRoom)
        {
            HighlightRoom(_hoveredRoom, false);
        }

        _hoveredRoom = null;

        // Ignore start room
        if (room == RoomBuilder.Instance.StartRoom) return;

        _hoveredRoom = room;

        // Apply highlight to new hover (if it's not selected)
        if (_hoveredRoom != null && _hoveredRoom != _selectedRoom)
        {
            HighlightRoom(_hoveredRoom, true);
        }
    }

    public void ExitRoomSwapping()
    {
        if (_selectedRoom != null) HighlightRoom(_selectedRoom, false);
        if (_hoveredRoom != null) HighlightRoom(_hoveredRoom, false);
        _selectedRoom = null;
        _hoveredRoom = null;
    }

    private void SwapRoom(SyncedRoom firstRoom, SyncedRoom secondRoom)
    {
        if (firstRoom == null || secondRoom == null || firstRoom == secondRoom) return;

        // Store positions
        Vector2Int posA = firstRoom.GridPosition;
        Vector2Int posB = secondRoom.GridPosition;

        // Swap positions in world
        firstRoom.transform.position = GridManager.Instance.RoomGridToWorld(posB);
        secondRoom.transform.position = GridManager.Instance.RoomGridToWorld(posA);

        // Update dictionary
        GridManager.Instance.SetRoom(posA, secondRoom);
        GridManager.Instance.SetRoom(posB, firstRoom);

        // Update doors for both rooms
        _roomBuilder.UpdateDoors(firstRoom);
        _roomBuilder.UpdateDoors(secondRoom);

        // Update neighbors around both positions
        _roomBuilder.UpdateNeighbors(posA);
        _roomBuilder.UpdateNeighbors(posB);
    }

    private void HighlightRoom(SyncedRoom room, bool selected)
    {
        var tilemaps = room.GetComponentsInChildren<Tilemap>();
        var sprites = room.GetComponentsInChildren<SpriteRenderer>();

        foreach (var t in tilemaps)
        {
            t.color = selected ? _selectionColor : Color.white;
        }

        foreach (var s in sprites)
        {
            s.color = selected ? _selectionColor : Color.white;
        }
    }
}
