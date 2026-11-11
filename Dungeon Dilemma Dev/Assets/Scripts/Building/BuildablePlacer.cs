using ControllerLogic;
using Items;
using PlayerLogic.BossLogic;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BuildablePlacer : MonoBehaviour
{
    public static BuildablePlacer Instance;

    [Header("Preview Settings")]
    [SerializeField] private float _tiltAmount = 3f;
    [SerializeField] private float _previewMoveSpeed = 15f;
    [SerializeField] private Color _validColor = Color.green;
    [SerializeField] private Color _invalidColor = Color.red;

    [Header("Placement")]
    [SerializeField] private LayerMask _unbuildableMask;

    [Header("Count Display")]
    [SerializeField] private BuildableCountDisplay _countPrefab;

    [Header("SFX")]
    [SerializeField] private AudioClip _placementSound;

    private GameObject _buildablePreview;
    private BuildableCountDisplay _countDisplay;
    private Buildable _currentBuildable;
    private SpriteRenderer[] _previewRenderers;

    private SyncedInventory _bossInventory;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void InitializeBossInventory(SyncedInventory inventory)
    {
        _bossInventory = inventory;
    }

    /// <summary>
    /// Places the buildable into the corresponding room at the position
    /// </summary>
    /// <param name="buildable"> The buildable data containing prefab to place </param>
    /// <param name="pos"> Placement location </param>
    public void PlaceBuildable(Buildable buildable, Vector3 pos)
    {
        Vector2Int roomPos = GridManager.Instance.WorldToRoomGrid(pos);
        SyncedRoom room = GridManager.Instance.GetRoom(roomPos);
        if (room == null) return;
        Vector2Int buildablePos = GridManager.Instance.WorldToLocalBuildableGrid(pos, room);

        if (IsPlacementValid(buildable, pos))
        {
            float xPos = buildablePos.x + buildable.TileOffset.x;
            float yPos = buildablePos.y + buildable.TileOffset.y;
            Vector3 spawnLocation = new Vector3(xPos, yPos, 0); // center on tile

            GameObject build = Instantiate(buildable.Prefab, room.transform);
            SoundFXManager.Instance.PlayLocalSound(_placementSound, _bossInventory.transform.position);
            build.transform.localPosition = spawnLocation;
            build.GetComponent<NetworkBehaviour>()?.GiveOwnership(DungeonBossBuildingController.Singleton.owner);
            room.AddBuildable(buildable, build, buildablePos);
            _bossInventory.PlaceBuildable(buildable.Name);
        }
    }

    public void RemoveBuildable(Vector3 pos)
    {
        Vector2Int roomPos = GridManager.Instance.WorldToRoomGrid(pos);
        SyncedRoom room = GridManager.Instance.GetRoom(roomPos);
        if (room == null) return;
        Vector2Int buildablePos = GridManager.Instance.WorldToLocalBuildableGrid(pos, room);

        if (room.IsBuildableOccupied(buildablePos))
        {
            GameObject build = room.GetBuildable(buildablePos);
            Buildable buildable = room.RemoveBuildable(buildablePos);
            if (buildable != null) _bossInventory.RetrieveBuildable(buildable.Name);
            Destroy(build);
        }
    }

    /// <summary>
    /// Displays a preview of the selected buildable
    /// </summary>
    /// <param name="buildable"> Selected buildable </param>
    public void UpdateBuildablePreview(Buildable buildable, Vector3 targetPos)
    {
        if (_buildablePreview == null || _currentBuildable != buildable)
        {
            CreatePreview(buildable);
        }

        if (_countDisplay == null)
        {
            _countDisplay = Instantiate(_countPrefab);
        }

        Vector3 target = GetSnappedPosition(buildable, targetPos);

        // Smooth movement
        _buildablePreview.transform.position = Vector3.Lerp(
            _buildablePreview.transform.position,
            target,
            _previewMoveSpeed * Time.deltaTime
        );

        // Direction for tilt
        Vector3 direction = target - _buildablePreview.transform.position;

        float tiltZ = -direction.x * _tiltAmount;

        Quaternion targetRot = Quaternion.Euler(0, 0, tiltZ);

        _buildablePreview.transform.rotation = Quaternion.Lerp(
            _buildablePreview.transform.rotation,
            targetRot,
            10f * Time.deltaTime
        );

        Vector2Int roomPos = GridManager.Instance.WorldToRoomGrid(targetPos);
        SyncedRoom room = GridManager.Instance.GetRoom(roomPos);

        _countDisplay.transform.position = targetPos;
        if (room != null) _countDisplay.UpdateRoomCount(room.MinionCount, room.MinionLimit, room.TrapCount, room.TrapLimit);
        _countDisplay.UpdateAmountCount(_bossInventory.HeldBuildables[buildable.Name] - _bossInventory.PlacedBuildables[buildable.Name]);

        bool valid = (IsPlacementValid(buildable, targetPos));
        SetPreviewColor(valid ? _validColor : _invalidColor);
    }

    /// <summary>
    /// Handles exiting buildable placement mode
    /// </summary>
    public void ExitBuildablePlacement()
    {
        // TODO: clean up active features when exiting (like deleting existing previews)
        if(_buildablePreview != null)
        {
            Destroy(_buildablePreview);
        }
        if (_countDisplay != null)
        {
            Destroy(_countDisplay.gameObject);
        }
    }

    private void CreatePreview(Buildable buildable)
    {
        if (_buildablePreview != null)
            Destroy(_buildablePreview);

        _currentBuildable = buildable;
        _buildablePreview = Instantiate(buildable.Prefab);

        // Disable all colliders and anim so it doesn't interfere
        foreach (var col in _buildablePreview.GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        foreach (var anim in _buildablePreview.GetComponentsInChildren<Animator>())
            anim.enabled = false;

        // Disable minion AI
        NavMeshAgent agent = GetComponentInChildren<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Get all renderers to tint them
        _previewRenderers = _buildablePreview.GetComponentsInChildren<SpriteRenderer>();

        SetPreviewColor(_validColor);
    }

    private void SetPreviewColor(Color color)
    {
        if (_previewRenderers == null) return;

        foreach (var renderer in _previewRenderers)
        {
            Color c = color;
            c.a = 0.5f;
            renderer.color = c;
            renderer.sortingOrder = 100;
        }
    }

    private bool IsPlacementValid(Buildable buildable, Vector3 worldPos)
    {
        Vector2Int roomPos = GridManager.Instance.WorldToRoomGrid(worldPos);
        SyncedRoom room = GridManager.Instance.GetRoom(roomPos);

        if (room == null)
            return false;

        Vector2Int buildablePos = GridManager.Instance.WorldToLocalBuildableGrid(worldPos, room);

        // Already occupied
        if (room.IsBuildableOccupied(buildablePos))
            return false;

        // Room limit
        if (room.AtBuildableCapacity(buildable)) return false;

        // Not enough in inventory
        if (!_bossInventory.CanPlaceBuildable(buildable.Name)) return false;

        // Final snapped world position
        Vector3 snappedPos = GetSnappedPosition(buildable, worldPos);

        Collider2D hit = Physics2D.OverlapBox(
            snappedPos,
            buildable.Size - new Vector2(0.05f, 0.05f),
            0f,
            _unbuildableMask
        );
        return hit == null;
    }
    private Vector3 GetSnappedPosition(Buildable buildable, Vector3 worldPos)
    {
        Vector2Int gridPos = GridManager.Instance.WorldToBuildableGrid(worldPos);

        return new Vector3(
            gridPos.x + buildable.TileOffset.x,
            gridPos.y + buildable.TileOffset.y,
            0f
        );
    }

    private void OnDrawGizmos()
    {
        if (_buildablePreview == null) return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            GetSnappedPosition(_currentBuildable, _buildablePreview.transform.position),
            (Vector3Int)_currentBuildable.Size - new Vector3(0.05f, 0.05f)
        );
    }
}
