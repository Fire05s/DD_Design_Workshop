using System;
using System.Collections;
using Avocado;
using ControllerLogic;
using Items;
using PurrNet;
using UnityEngine;

namespace PlayerLogic.BossLogic {
    public class DungeonBossBuildingController: NetworkBehaviour {
        public static DungeonBossBuildingController Singleton;
        
        [SerializeField] private SyncedInput _input;
        [SerializeField] private SyncedStats _stats;
        [SerializeField] private BuildingCamera _cameraController;
        //private RoomInstance _inspectionRoom;
        public enum BuildMode
        {
            RoomBuild,
            RoomSwap,
            BuildablePlacement,
            Exit
        }
        private BuildMode currentMode = BuildMode.RoomBuild;
        public BuildMode CurrentMode => currentMode;

        public BuildMode CurrentRoomMode { get; private set; }

        private bool _isBuildingEnabled;
        private SyncedRoom _selectedRoom;
        private Buildable _selectedBuildable;
        public bool controlsActive;

        private void Awake() {
            Singleton = this;
        }

        private void Start()
        {
            if (_stats == null)
            {
                _stats = GetComponentInParent<SyncedStats>();
            }
            SetInventoryReference(owner.Value, _stats);
            if (!isOwner) return;
            if (!GlobalController.Singleton.IsDebugLevel) RoomBuilder.Instance.GenerateInitialLayout(4);
        }

        private void OnEnable()
        {
            if (!isOwner) return;
            SwitchMode(BuildMode.RoomBuild);
            _cameraController.enabled = true;
            _isBuildingEnabled = true;
            CurrentRoomMode = BuildMode.RoomBuild;
            RoomBuilder.Instance.UpdateAllDoors();
            GridManager.Instance.RemoveNavMeshes();
            GridManager.Instance.RespawnMinions();
        }

        private void OnDisable() {
            DisableBuilding();
            _cameraController.enabled = false;

            // choose new options for next round
            RoomBuilder.Instance.ChooseRoomOptions();
        }

        private void Update() {
            if (!isOwner || !_isBuildingEnabled) return;
            
            if (_stats == null) {
                _stats = GetComponentInParent<SyncedStats>();
            }

            Vector3 mouseWorld = _input.GetMouseWorldPosition();
            mouseWorld.z = 0;

            Vector2Int gridPos = GridManager.Instance.WorldToRoomGrid(mouseWorld);

            UpdatePreview(gridPos, mouseWorld);
            if (Input.GetMouseButtonDown(0) && controlsActive)
            {
                HandleInput(gridPos, mouseWorld, true);
            }
            else if (Input.GetMouseButtonDown(1) && controlsActive)
            {
                HandleInput(gridPos, mouseWorld, false);
            }
        }

        private void HandleInput(Vector2Int gridPos, Vector3 mousePos, bool isRightClick)
        {
            switch (currentMode)
            {
                case BuildMode.RoomBuild:
                    if (_selectedRoom == null || !isRightClick) return;
                    if (RoomBuilder.Instance.PlaceRoom(_selectedRoom, gridPos) != null)
                    {
                        SwitchMode(BuildMode.RoomSwap);
                        CurrentRoomMode = BuildMode.RoomSwap;
                    }
                    break;
                case BuildMode.RoomSwap:
                    if (!isRightClick) return;
                    RoomSwapper.Instance.HandleRoomSwap(gridPos);
                    RoomBuilder.Instance.UpdateSpawnpoints();
                    break;
                case BuildMode.BuildablePlacement:
                    if (_selectedBuildable == null) return;
                    if (isRightClick) BuildablePlacer.Instance.PlaceBuildable(_selectedBuildable, mousePos);
                    else BuildablePlacer.Instance.RemoveBuildable(mousePos);
                    break;
            }
        }

        private void UpdatePreview(Vector2Int gridPos, Vector3 mousePos)
        {
            switch (currentMode)
            {
                case BuildMode.RoomBuild:
                    if (_selectedRoom == null) return;
                    RoomBuilder.Instance.UpdateRoomPreview(_selectedRoom, gridPos);
                    break;
                case BuildMode.RoomSwap:
                    RoomSwapper.Instance.UpdateHover(gridPos);
                    break;
                case BuildMode.BuildablePlacement:
                    if (_selectedBuildable == null) return;
                    BuildablePlacer.Instance.UpdateBuildablePreview(_selectedBuildable, mousePos);
                    break;
            }
        }


        /// <summary>
        /// Handles exiting and switching build mode.
        /// </summary>
        /// <param name="newMode"> The mode to switch to. </param>
        public void SwitchMode(BuildMode newMode)
        {
            if (currentMode == newMode) return;
            switch (currentMode)
            {
                case BuildMode.RoomBuild:
                    RoomBuilder.Instance.ExitRoomBuilding();
                    break;
                case BuildMode.RoomSwap:
                    RoomSwapper.Instance.ExitRoomSwapping();
                    break;
                case BuildMode.BuildablePlacement:
                    BuildablePlacer.Instance.ExitBuildablePlacement();
                    break;
                case BuildMode.Exit:
                    break;
            }
            currentMode = newMode;
        }

        public void SelectBuildable(Buildable buildable) {
            _selectedBuildable = buildable;
        }
        
        public void SelectRoom(SyncedRoom data) {
            StartCoroutine(WaitSelectRoom(data));
        }

        public void DisableBuilding()
        {
            if (!_isBuildingEnabled) return;
            _isBuildingEnabled = false;
            SwitchMode(BuildMode.Exit);
            _selectedRoom = null;
            _selectedBuildable = null;

            RoomBuilder.Instance.UpdateAllDoors();
            GridManager.Instance.BuildNavMeshes();
        }

        private IEnumerator WaitSelectRoom(SyncedRoom data)
        {
            // prevent clicking and placing in the same frame
            yield return new WaitForSeconds(0.1f);

            _selectedRoom = data;
        }

        [TargetRpc]
        private void SetInventoryReference(PlayerID boss, SyncedStats stats)
        {
            BuildablePlacer.Instance.InitializeBossInventory(stats.GetComponent<SyncedInventory>());
        }
    }
}