/// <summary>
/// Project : Easy Build System
/// Class : BuildingGridSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Helpers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid
{
    public class BuildingGridSystem : BuildingManagerSubSystem, IDebuggable
    {
        private const string DEFAULT_CELL_PREFAB_PATH = "Grids/Grid_Cell";

        protected BuildingGridSettings m_settings;
        protected BuildingGridRenderer m_renderer;
        protected bool m_hasGeneratedGrid;

        private readonly Dictionary<BuildingPart, Vector3> m_lastValidPositions = new Dictionary<BuildingPart, Vector3>();
        private readonly HashSet<Vector2Int> m_changedCellsBuffer = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> m_singleCellBuffer = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> m_lastHighlightedCells = new HashSet<Vector2Int>();

        public BuildingGridSettings Settings => m_settings;
        public BuildingGridRenderer Renderer => m_renderer;
        public GridStageData CurrentStage => m_settings.CurrentStage;
        public bool HasGeneratedGrid => m_hasGeneratedGrid;
        public Dictionary<BuildingPart, Vector3> LastValidPositions => m_lastValidPositions;

        public BuildingGridSystem(BuildingManager manager, BuildingGridSettings settings)
        {
            m_manager = manager;
            m_settings = settings;

            if (m_settings.CellVisualPrefab == null)
            {
                m_settings.CellVisualPrefab = Resources.Load<GameObject>(DEFAULT_CELL_PREFAB_PATH);
            }

            m_renderer = new BuildingGridRenderer(settings);
        }

        public override void Initialize()
        {
            if (!m_settings.EnableGrid)
            {
                return;
            }

            DebugRendererManager.Register(this);

            EventPublisher.Subscribe<BuildingControllerEvent.BuildModeChangedEventArgs>(HandleBuildModeChanged);
            EventPublisher.Subscribe<BuildingStateEvent.PlacedEventArgs>(HandlePartPlaced);
            EventPublisher.Subscribe<BuildingStateEvent.DestroyedEventArgs>(HandlePartDestroyed);
            EventPublisher.Subscribe<BuildingStateEvent.AdjustmentStartedEventArgs>(HandleAdjustmentStarted);
            EventPublisher.Subscribe<BuildingStateEvent.AdjustmentEndedEventArgs>(HandleAdjustmentEnded);

            if (!m_hasGeneratedGrid)
            {
                InitializeGrid();
            }
        }

        public override void Shutdown()
        {
            DebugRendererManager.Unregister(this);

            if (Application.isPlaying)
            {
                EventPublisher.Unsubscribe<BuildingControllerEvent.BuildModeChangedEventArgs>(HandleBuildModeChanged);
                EventPublisher.Unsubscribe<BuildingStateEvent.PlacedEventArgs>(HandlePartPlaced);
                EventPublisher.Unsubscribe<BuildingStateEvent.DestroyedEventArgs>(HandlePartDestroyed);
                EventPublisher.Unsubscribe<BuildingStateEvent.AdjustmentStartedEventArgs>(HandleAdjustmentStarted);
                EventPublisher.Unsubscribe<BuildingStateEvent.AdjustmentEndedEventArgs>(HandleAdjustmentEnded);
            }

            ClearGrid();
        }

        public override void Update()
        {
            if (!m_settings.EnableGrid || !m_hasGeneratedGrid)
            {
                return;
            }

            UpdateDynamicOccupancyCells();
            UpdateHighlightedCells();
            m_renderer.UpdateInstancedMode(CurrentStage);
            m_renderer.RenderInstanced();
        }

        #region IDebuggable

        public bool DebugEnabled => m_manager != null && m_settings.EnableGrid;

        public DebugRenderer.ViewFlags DebugFlags
        {
            get => m_settings.DebugFlags;
            set => m_settings.DebugFlags = value;
        }

        public bool RequireSelection => false;

        public void OnDebugRender()
        {
            if (CurrentStage == null || CurrentStage.GridCells == null)
            {
                return;
            }

            float cellSize = CurrentStage.CellSize;

            for (int x = 0; x < CurrentStage.Rows; x++)
            {
                for (int z = 0; z < CurrentStage.Columns; z++)
                {
                    GridCellData cell = GetCell(x, z);
                    if (cell == null)
                    {
                        continue;
                    }

                    Color wireColor = cell.IsOccupied ? Color.red : Color.green;
                    DebugRenderer.DrawWireCube(cell.CellPosition, new Vector3(cellSize, 0.01f, cellSize), wireColor, 0f, 1f, false);
                }
            }

            HashSet<Vector2Int> occupancyCells = GetAllPreviewOccupancyCells();
            foreach (Vector2Int cellIndex in occupancyCells)
            {
                GridCellData cell = GetCell(cellIndex.x, cellIndex.y);
                if (cell == null)
                {
                    continue;
                }

                Color fillColor = cell.IsOccupied ? new Color(1f, 0f, 0f, 0.1f) : new Color(0f, 1f, 0f, 0.1f);
                Color wireColor = cell.IsOccupied ? Color.red : Color.green;

                DebugRenderer.DrawCube(cell.CellPosition, new Vector3(cellSize, 0.01f, cellSize), Quaternion.identity, Vector3.one, fillColor, 0f, false);
                DebugRenderer.DrawWireCube(cell.CellPosition, new Vector3(cellSize, 0.01f, cellSize), Quaternion.identity, Vector3.one, wireColor, 0f, 1f, false);
            }
        }

        #endregion

        #region Grid Management

        protected virtual void InitializeGrid()
        {
            if (m_hasGeneratedGrid)
            {
                return;
            }

            GridStageData[] stages = m_settings.GridStages;
            if (stages == null)
            {
                return;
            }

            for (int i = 0; i < stages.Length; i++)
            {
                GridStageData stage = stages[i];

                InitializeCaches(stage);
                stage.GridCells = new GridCellData[stage.Rows * stage.Columns];

                for (int x = 0; x < stage.Rows; x++)
                {
                    for (int z = 0; z < stage.Columns; z++)
                    {
                        int cellIndex = x * stage.Columns + z;
                        float cachedHeight = stage.HeightsCache[cellIndex];
                        bool isOccupied = stage.OccupiedCache[cellIndex];

                        Vector3 cellWorldPos = CalculateCellWorldPosition(stage, x, z, cachedHeight);
                        if (cellWorldPos == Vector3.negativeInfinity)
                        {
                            continue;
                        }

                        stage.SetCell(x, z, new GridCellData(cellWorldPos, isOccupied));
                    }
                }
            }

            m_renderer.UpdateAllCellColors(CurrentStage);
            m_hasGeneratedGrid = true;
        }

        protected virtual void ClearGrid()
        {
            m_hasGeneratedGrid = false;
            m_renderer.ClearVisuals();

            GridStageData[] stages = m_settings.GridStages;
            if (stages != null)
            {
                foreach (GridStageData stage in stages)
                {
                    stage.GridCells = null;
                }
            }
        }

        public void RefreshGrid()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                m_renderer.SetVisible(true);
                ClearGrid();
                InitializeGrid();
                m_renderer.MarkDirty();
            };
#else
            m_renderer.SetVisible(true);
            ClearGrid();
            InitializeGrid();
            m_renderer.MarkDirty();
#endif
        }

        #endregion

        #region Cell Access

        public GridCellData GetCell(int x, int z)
        {
            GridStageData stage = CurrentStage;
            return stage == null || stage.GridCells == null ||
                   x < 0 || x >= stage.Rows || z < 0 || z >= stage.Columns
                ? null
                : stage.GetCell(x, z);
        }

        public GridCellData GetCell(Vector3 position)
        {
            GridStageData stage = CurrentStage;
            if (stage == null || stage.GridCells == null)
            {
                return null;
            }

            Vector3 localPos = m_manager.transform.InverseTransformPoint(position);
            Vector3 pivotOffset = GridHelper.GetCenterPivotOffset(stage);
            localPos.x -= pivotOffset.x;
            localPos.z -= pivotOffset.z;

            float cellSize = stage.CellSize;

            if (localPos.x < 0f || localPos.x >= stage.Columns * cellSize ||
                localPos.z < 0f || localPos.z >= stage.Rows * cellSize)
            {
                return null;
            }

            return GetCell(Mathf.FloorToInt(localPos.x / cellSize), Mathf.FloorToInt(localPos.z / cellSize));
        }

        #endregion

        #region Cell Highlight

        public void SetHighlightedCells(HashSet<Vector2Int> cells)
        {
            bool changed = false;

            if (cells == null || cells.Count == 0)
            {
                if (m_lastHighlightedCells.Count > 0)
                {
                    m_lastHighlightedCells.Clear();
                    changed = true;
                }
            }
            else
            {
                if (!cells.SetEquals(m_lastHighlightedCells))
                {
                    m_lastHighlightedCells.Clear();
                    foreach (Vector2Int cell in cells)
                    {
                        m_lastHighlightedCells.Add(cell);
                    }

                    changed = true;
                }
            }

            if (changed)
            {
                m_renderer.SetHighlightedCells(m_lastHighlightedCells);
            }
        }

        public void SetHighlightedCellsFromWorldPosition(Vector3 worldPosition, BuildingPart part)
        {
            if (part == null || CurrentStage == null)
            {
                SetHighlightedCells(null);
                return;
            }

            BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
            if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
            {
                SetHighlightedCells(null);
                return;
            }

            SetHighlightedCells(GridHelper.GetOccupancyCells(
                CurrentStage,
                worldPosition,
                part.transform.rotation,
                placementSettings.PreviewCellSizeX,
                placementSettings.PreviewCellSizeZ,
                m_manager.transform));
        }

        public void ClearHighlightedCells()
        {
            SetHighlightedCells(null);
        }

        #endregion

        #region Part Occupancy

        public bool PlacePart(BuildingPart part)
        {
            GridStageData stage = CurrentStage;
            if (stage == null || part == null || !IsScenePart(part) || m_manager == null)
            {
                return false;
            }

            BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
            if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
            {
                return true;
            }

            HashSet<Vector2Int> occupancyCells = GridHelper.GetOccupancyCells(
                stage,
                part.transform.position,
                part.transform.rotation,
                placementSettings.PreviewCellSizeX,
                placementSettings.PreviewCellSizeZ,
                m_manager.transform);

            if (occupancyCells == null || occupancyCells.Count == 0)
            {
                return false;
            }

            foreach (Vector2Int cellIndex in occupancyCells)
            {
                GridCellData cell = GetCell(cellIndex.x, cellIndex.y);
                if (cell == null || cell.IsOccupied)
                {
                    return false;
                }
            }

            foreach (Vector2Int cellIndex in occupancyCells)
            {
                GridCellData cell = GetCell(cellIndex.x, cellIndex.y);
                if (cell == null)
                {
                    continue;
                }

                int cacheIndex = cellIndex.x * stage.Columns + cellIndex.y;
                if (stage.OccupiedCache != null && cacheIndex >= 0 && cacheIndex < stage.OccupiedCache.Length)
                {
                    stage.OccupiedCache[cacheIndex] = true;
                }

                cell.IsOccupied = true;

                m_singleCellBuffer.Clear();
                m_singleCellBuffer.Add(cellIndex);
                m_renderer.UpdateCellColors(m_singleCellBuffer, stage);
            }

            return true;
        }

        public void RemovePart(BuildingPart part)
        {
            GridStageData stage = CurrentStage;
            if (stage == null || part == null || !IsScenePart(part) || m_manager == null)
            {
                return;
            }

            BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
            if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
            {
                return;
            }

            HashSet<Vector2Int> occupancyCells = GridHelper.GetOccupancyCells(
                stage,
                part.transform.position,
                part.transform.rotation,
                placementSettings.PreviewCellSizeX,
                placementSettings.PreviewCellSizeZ,
                m_manager.transform);

            if (occupancyCells == null || occupancyCells.Count == 0)
            {
                return;
            }

            foreach (Vector2Int cellIndex in occupancyCells)
            {
                GridCellData cell = GetCell(cellIndex.x, cellIndex.y);
                if (cell == null)
                {
                    continue;
                }

                int cacheIndex = cellIndex.x * stage.Columns + cellIndex.y;
                if (stage.OccupiedCache != null && cacheIndex >= 0 && cacheIndex < stage.OccupiedCache.Length)
                {
                    stage.OccupiedCache[cacheIndex] = false;
                }

                cell.IsOccupied = false;

                m_singleCellBuffer.Clear();
                m_singleCellBuffer.Add(cellIndex);
                m_renderer.UpdateCellColors(m_singleCellBuffer, stage);
            }
        }

        public HashSet<Vector2Int> GetAllPreviewOccupancyCells(bool includePreviews = false)
        {
            HashSet<Vector2Int> occupancyCells = new HashSet<Vector2Int>();

            if (CurrentStage == null)
            {
                return occupancyCells;
            }

            foreach (BuildingPart part in m_manager.GetRegisteredParts)
            {
                if (part == null)
                {
                    continue;
                }

                BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
                if (!placementSettings.PreviewUseGridSnapping)
                {
                    continue;
                }

                if (!includePreviews && part.State == BuildingPart.BuildingState.Placement)
                {
                    continue;
                }

                occupancyCells.UnionWith(GridHelper.GetOccupancyCells(
                    CurrentStage,
                    part.transform.position,
                    part.transform.rotation,
                    placementSettings.PreviewCellSizeX,
                    placementSettings.PreviewCellSizeZ,
                    m_manager.transform));
            }

            return occupancyCells;
        }

        public HashSet<Vector2Int> GetPreviewOccupancyCellsForPart(BuildingPart part)
        {
            if (part == null || m_manager == null || !IsScenePart(part))
            {
                return new HashSet<Vector2Int>();
            }

            BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
            if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
            {
                return new HashSet<Vector2Int>();
            }

            return GridHelper.GetOccupancyCells(
                CurrentStage,
                part.transform.position,
                part.transform.rotation,
                placementSettings.PreviewCellSizeX,
                placementSettings.PreviewCellSizeZ,
                m_manager.transform);
        }

        #endregion

        #region Grid Validation

        public bool AreOccupancyCellsWithinGrid(BuildingPart part, Vector3 position, Quaternion rotation)
        {
            if (part == null || CurrentStage == null || CurrentStage.GridCells == null || !IsScenePart(part))
            {
                return false;
            }

            BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
            if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
            {
                return true;
            }

            HashSet<Vector2Int> occupancyCells = GridHelper.GetOccupancyCells(
                CurrentStage,
                position,
                rotation,
                placementSettings.PreviewCellSizeX,
                placementSettings.PreviewCellSizeZ,
                m_manager.transform);

            if (occupancyCells == null || occupancyCells.Count == 0)
            {
                return false;
            }

            foreach (Vector2Int cellIndex in occupancyCells)
            {
                if (!GridHelper.IsInsideGridBounds(cellIndex.x, cellIndex.y, CurrentStage.Rows, CurrentStage.Columns))
                {
                    return false;
                }

                GridCellData cell = GetCell(cellIndex.x, cellIndex.y);
                if (cell != null && cell.IsOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsScenePart(BuildingPart part)
        {
            if (part == null)
            {
                return false;
            }

            return part.gameObject.scene.IsValid() && part.gameObject.scene.isLoaded;
        }

        #endregion

        #region Visibility

        public void SetGridVisualsVisible(bool visible)
        {
            m_renderer.SetVisible(visible);
        }

        #endregion

        #region Event Handlers

        private void HandleBuildModeChanged(BuildingControllerEvent.BuildModeChangedEventArgs args)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            bool gridVisible = false;

            GridModeVisualEntry[] entries = m_settings.GridVisibilityByMode;
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].Mode == args.Mode)
                    {
                        gridVisible = entries[i].ShowGridVisuals;
                        break;
                    }
                }
            }

            SetGridVisualsVisible(gridVisible);
        }

        private void HandlePartPlaced(BuildingStateEvent.PlacedEventArgs args)
        {
            if (!Application.isPlaying || args?.Part == null || args.Part.State != BuildingPart.BuildingState.Placed)
            {
                return;
            }

            PlacePart(args.Part);
        }

        private void HandlePartDestroyed(BuildingStateEvent.DestroyedEventArgs args)
        {
            if (!Application.isPlaying || args?.Part == null || args.Part.State == BuildingPart.BuildingState.Placement)
            {
                return;
            }

            RemovePart(args.Part);
        }

        private void HandleAdjustmentStarted(BuildingStateEvent.AdjustmentStartedEventArgs args)
        {
            if (!Application.isPlaying || args?.Part == null)
            {
                return;
            }

            RemovePart(args.Part);
        }

        private void HandleAdjustmentEnded(BuildingStateEvent.AdjustmentEndedEventArgs args)
        {
            if (!Application.isPlaying || args?.Part == null)
            {
                return;
            }

            if (args.Part.State == BuildingPart.BuildingState.Placed)
            {
                PlacePart(args.Part);
            }
        }

        #endregion

        #region Private Methods

        protected void UpdateDynamicOccupancyCells()
        {
            GridStageData stage = CurrentStage;
            if (stage == null || stage.GridCells == null || stage.OccupiedCache == null)
            {
                return;
            }

            HashSet<Vector2Int> dynamicOccupancyCells = GetAllPreviewOccupancyCells();
            m_changedCellsBuffer.Clear();

            int totalCells = stage.OccupiedCache.Length;

            for (int x = 0; x < stage.Rows; x++)
            {
                for (int z = 0; z < stage.Columns; z++)
                {
                    int cellIndex = x * stage.Columns + z;
                    if (cellIndex < 0 || cellIndex >= totalCells)
                    {
                        continue;
                    }

                    bool newOccupiedState = stage.OccupiedCache[cellIndex] || dynamicOccupancyCells.Contains(new Vector2Int(x, z));

                    GridCellData cell = GetCell(x, z);
                    if (cell != null && cell.IsOccupied != newOccupiedState)
                    {
                        cell.IsOccupied = newOccupiedState;
                        m_changedCellsBuffer.Add(new Vector2Int(x, z));
                    }
                }
            }

            if (m_changedCellsBuffer.Count > 0)
            {
                m_renderer.UpdateCellColors(m_changedCellsBuffer, stage);
            }
        }

        protected void UpdateHighlightedCells()
        {
            if (CurrentStage == null)
            {
                return;
            }

            foreach (BuildingPart part in m_manager.GetRegisteredParts)
            {
                if (part == null || part.State != BuildingPart.BuildingState.Placement)
                {
                    continue;
                }

                BuildingPlacementSettings placementSettings = part.PlacementSystem.Settings;
                if (placementSettings == null || !placementSettings.PreviewUseGridSnapping)
                {
                    continue;
                }

                SetHighlightedCellsFromWorldPosition(part.transform.position, part);
                return;
            }

            ClearHighlightedCells();
        }

        protected virtual Vector3 CalculateCellWorldPosition(GridStageData stage, int x, int z, float cachedHeight)
        {
            if (m_manager == null || stage == null)
            {
                return Vector3.negativeInfinity;
            }

            Vector3 pivotOffset = GridHelper.GetCenterPivotOffset(stage);
            Vector3 localPos = new Vector3(
                x * stage.CellSize + pivotOffset.x + stage.CellSize / 2f,
                cachedHeight + stage.Height,
                z * stage.CellSize + pivotOffset.z + stage.CellSize / 2f);

            return m_manager.transform.TransformPoint(localPos);
        }

        private void InitializeCaches(GridStageData stage)
        {
            int totalCells = stage.Rows * stage.Columns;
            stage.HeightsCache = new float[totalCells];
            stage.OccupiedCache = new bool[totalCells];

            for (int i = 0; i < totalCells; i++)
            {
                stage.HeightsCache[i] = 0f;
                stage.OccupiedCache[i] = false;
            }
        }

        #endregion
    }
}