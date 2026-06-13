/// <summary>
/// Project : Easy Build System
/// Class : BuildingGridEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingGridEditor
    {
        public static void Draw(PropertyCollection properties, SerializedObject serializedObject, BuildingManager target)
        {
            properties.Draw("m_gridSettings.m_enableGrid",
                new GUIContent("Enable Grid", "Enables the cell-based grid system for snapping Building Parts during placement."));

            if (!properties.Get("m_gridSettings.m_enableGrid").boolValue)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            DrawGeneralSettings(properties, serializedObject);
            DrawVisualSettings(properties, serializedObject, target);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (target.GridSystem != null)
                {
                    target.GridSystem.RefreshGrid();
                }
            }

            DrawDebugSettings(properties, target);
        }

        private static ReorderableList.Code.Editor.ReorderableList s_stagesList;

        private static void DrawGeneralSettings(PropertyCollection properties, SerializedObject serializedObject)
        {
            EditorGUIExtended.Separator("Grid Layout Settings");

            using (EditorGUIExtended.IndentScope())
            {
                SerializedProperty stagesProp = serializedObject.FindProperty("m_gridSettings.m_gridStages");

                if (stagesProp != null)
                {
                    if (s_stagesList == null || s_stagesList.Native?.serializedProperty != stagesProp)
                    {
                        bool hasStages = stagesProp.arraySize > 0;

                        s_stagesList = new ReorderableList.Code.Editor.ReorderableList(
                            stagesProp,
                            new ReorderableList.Code.Editor.NativeFunctionOptions(isDraggable: true, shouldDisplayHeader: true, shouldDisplayAddButton: !hasStages, shouldDisplayRemoveButton: true),
                            shouldUseFoldout: false);
                    }

                    s_stagesList.Layout();
                }
            }
        }

        private static void DrawVisualSettings(PropertyCollection properties, SerializedObject serializedObject, BuildingManager target)
        {
            EditorGUIExtended.Separator("Cell Visual Settings");

            using (EditorGUIExtended.IndentScope())
            {
                properties.Draw("m_gridSettings.m_gridVisibilityByMode",
                    new GUIContent("Grid Visibility By Mode", "Configure which building modes display the grid."), true);
                properties.Draw("m_gridSettings.m_cellVisualPrefab",
                    new GUIContent("Cell Visual Prefab", "Prefab instantiated at each cell position to represent the grid."));
                properties.Draw("m_gridSettings.m_cellVisualSize",
                    new GUIContent("Cell Visual Size", "Scale applied to each cell visual."));
                properties.Draw("m_gridSettings.m_cellFreeColor",
                    new GUIContent("Free Cell Color", "Color displayed on cells that are available for placement."));
                properties.Draw("m_gridSettings.m_cellOccupiedColor",
                    new GUIContent("Occupied Cell Color", "Color displayed on cells that are already occupied."));
                properties.Draw("m_gridSettings.m_cellHighlightColor",
                    new GUIContent("Highlight Cell Color", "Color displayed on cells currently under the placement preview."));
            }
        }

        private static void DrawDebugSettings(PropertyCollection properties, BuildingManager target)
        {
            EditorGUIExtended.Separator("Grid Statistics");

            GridStageData currentStageData = target.GridSystem.CurrentStage;
            int stageTotalCellsCount = currentStageData?.GridCells?.Length ?? 0;
            int stageOccupiedCellsCount = CountOccupiedCells(currentStageData);

            EditorGUILayout.LabelField("Has Generated Grid :", target.GridSystem.HasGeneratedGrid.ToString());
            EditorGUILayout.LabelField("Grid Cells Count :", stageTotalCellsCount.ToString());
            EditorGUILayout.LabelField("Occupied Cells Count :", stageOccupiedCellsCount.ToString());

            EditorGUIExtended.Separator("Rendering Settings");

            properties.Draw("m_gridSettings.m_debugFlags",
                new GUIContent("Debug Draw Flags", "Where the grid is allowed to draw."));
        }

        private static int CountOccupiedCells(GridStageData gridStageToCount)
        {
            if (gridStageToCount?.OccupiedCache == null)
            {
                return 0;
            }

            int totalOccupiedCount = 0;
            for (int cellIndex = 0; cellIndex < gridStageToCount.OccupiedCache.Length; cellIndex++)
            {
                if (gridStageToCount.OccupiedCache[cellIndex])
                {
                    totalOccupiedCount++;
                }
            }

            return totalOccupiedCount;
        }
    }
}