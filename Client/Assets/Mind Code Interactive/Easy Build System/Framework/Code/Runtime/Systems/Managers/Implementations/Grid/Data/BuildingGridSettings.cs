/// <summary>
/// Project : Easy Build System
/// Class : BuildingGridSettings.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
{
    [Serializable]
    public class BuildingGridSettings
    {
        [SerializeField] private bool m_enableGrid;
        [SerializeField]
        private GridStageData[] m_gridStages = new GridStageData[1]
        {
            new GridStageData { Name = "Default", Rows = 10, Columns = 10, CellSize = 1f }
        };
        [SerializeField] private GameObject m_cellVisualPrefab;
        [SerializeField, Min(0f)] private float m_cellVisualSize = 0.09f;
        [SerializeField, ColorUsage(true, true)] private Color m_cellFreeColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField, ColorUsage(true, true)] private Color m_cellOccupiedColor = new Color(1f, 0f, 0f, 0.5f);
        [SerializeField, ColorUsage(true, true)] private Color m_cellHighlightColor = new Color(1f, 1f, 0f, 0.5f);
        [SerializeField]
        private GridModeVisualEntry[] m_gridVisibilityByMode = new GridModeVisualEntry[]
        {
            new GridModeVisualEntry { Mode = BuildingMode.None,        ShowGridVisuals = true },
            new GridModeVisualEntry { Mode = BuildingMode.Placement,   ShowGridVisuals = true },
            new GridModeVisualEntry { Mode = BuildingMode.Adjustment,  ShowGridVisuals = true },
            new GridModeVisualEntry { Mode = BuildingMode.Destruction, ShowGridVisuals = true }
        };
        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.SceneView;

        public bool EnableGrid { get => m_enableGrid; set => m_enableGrid = value; }

        public GridStageData[] GridStages { get => m_gridStages; set => m_gridStages = value; }

        public GameObject CellVisualPrefab { get => m_cellVisualPrefab; set => m_cellVisualPrefab = value; }

        public float CellVisualSize { get => m_cellVisualSize; set => m_cellVisualSize = value; }

        public Color CellFreeColor { get => m_cellFreeColor; set => m_cellFreeColor = value; }

        public Color CellOccupiedColor { get => m_cellOccupiedColor; set => m_cellOccupiedColor = value; }

        public Color CellHighlightColor { get => m_cellHighlightColor; set => m_cellHighlightColor = value; }

        public GridModeVisualEntry[] GridVisibilityByMode { get => m_gridVisibilityByMode; set => m_gridVisibilityByMode = value; }

        public DebugRenderer.ViewFlags DebugFlags { get => m_debugFlags; set => m_debugFlags = value; }

        public GridStageData CurrentStage => m_gridStages != null && m_gridStages.Length > 0 ? m_gridStages[0] : null;
    }
}