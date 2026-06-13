/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartPlacementEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartPlacementEditor
    {
        public static void Draw(
            BuildingPart target,
            PropertyCollection properties,
            SerializedObject serializedObject,
            ref BuildingPart.BuildingState originalState,
            ref bool isPreviewingInEditor)
        {
            if (target == null || properties == null || serializedObject == null)
            {
                return;
            }

            EditorGUIExtended.Separator("Materials Settings", false);
            properties.Draw("m_placementSystem.m_settings.m_enablePreviewMaterial", new GUIContent("Enable Preview Material", "Enables a material effect applied to the Building Part during placement preview."));
            if (properties.Get("m_placementSystem.m_settings.m_enablePreviewMaterial").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewMaterialTransition", new GUIContent("Preview Material Transition", "Animation type used when transitioning the preview material."));
                    if (properties.Get("m_placementSystem.m_settings.m_previewMaterialTransition").enumValueIndex == (int)MaterialTransitionType.Pulse)
                    {
                        using (EditorGUIExtended.IndentScope())
                        {
                            properties.Draw("m_placementSystem.m_settings.m_pulseMinAlpha", new GUIContent("Pulse Min Alpha", "Minimum alpha value during the pulse animation."));
                            properties.Draw("m_placementSystem.m_settings.m_pulseMaxAlpha", new GUIContent("Pulse Max Alpha", "Maximum alpha value during the pulse animation."));
                            properties.Draw("m_placementSystem.m_settings.m_pulseFrequency", new GUIContent("Pulse Frequency", "Speed of the pulse animation cycle."));
                        }
                    }
                    using (EditorGUIExtended.IndentScope())
                    {
                        properties.DrawArray("m_placementSystem.m_settings.m_previewStateMaterials", new GUIContent("State Materials", "Material applied to the Building Part for each building state."));
                    }

                    using (EditorGUIExtended.DisabledScope(!target.gameObject.activeInHierarchy))
                    {
                        string[] allLabels = Enum.GetNames(typeof(BuildingPart.BuildingState));
                        List<string> stateLabels = new List<string>();
                        List<BuildingPart.BuildingState> stateValues = new List<BuildingPart.BuildingState>();

                        for (int i = 0; i < allLabels.Length; i++)
                        {
                            BuildingPart.BuildingState state = (BuildingPart.BuildingState)i;
                            if (state != BuildingPart.BuildingState.None)
                            {
                                stateLabels.Add(allLabels[i]);
                                stateValues.Add(state);
                            }
                        }

                        int currentIndex = stateValues.IndexOf(target.State);
                        if (currentIndex < 0)
                        {
                            currentIndex = 0;
                        }

                        int chosen = EditorGUILayout.Popup(new GUIContent("Preview", "Preview the Building Part in a specific building state."), currentIndex, stateLabels.ToArray());
                        BuildingPart.BuildingState chosenState = stateValues[chosen];

                        if (chosenState != target.State)
                        {
                            if (chosenState == BuildingPart.BuildingState.Placed)
                            {
                                target.PlacementSystem.RestoreMaterials();
                                target.SetState(BuildingPart.BuildingState.Placed);
                                isPreviewingInEditor = false;
                            }
                            else
                            {
                                if (!isPreviewingInEditor)
                                {
                                    originalState = target.State;
                                    isPreviewingInEditor = true;
                                }
                                target.PlacementSystem.ApplyPreviewMaterials(chosenState);
                                target.PlacementSystem.UpdateMaterialsColor(chosenState);
                                target.SetState(chosenState);
                            }
                        }
                    }
                }
            }

            EditorGUIExtended.Separator("State GameObjects Settings");
            using (EditorGUIExtended.IndentScope())
            {
                properties.Draw("m_placementSystem.m_settings.m_stateGameObjects", new GUIContent("State GameObjects", "GameObjects activated or deactivated based on the current building state."), true);
            }

            EditorGUIExtended.Separator("Position Settings");
            properties.Draw("m_placementSystem.m_settings.m_previewOffsetPosition", new GUIContent("Preview Offset Position", "Position offset applied to the Building Part during preview."));

            properties.Draw("m_placementSystem.m_settings.m_previewClampPosition", new GUIContent("Clamp Position", "Restricts the preview position within defined bounds."));
            if (properties.Get("m_placementSystem.m_settings.m_previewClampPosition").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewClampMinPosition", new GUIContent("Clamp Min Position", "Minimum allowed position during preview."));
                    properties.Draw("m_placementSystem.m_settings.m_previewClampMaxPosition", new GUIContent("Clamp Max Position", "Maximum allowed position during preview."));
                }
            }

            properties.Draw("m_placementSystem.m_settings.m_previewRoundMovement", new GUIContent("Round Movement", "Snaps preview movement to fixed position steps."));
            if (properties.Get("m_placementSystem.m_settings.m_previewRoundMovement").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewRoundCellSize", new GUIContent("Round Movement Size", "Step size used when rounding the preview position."));
                }
            }

            properties.Draw("m_placementSystem.m_settings.m_previewSmoothMovement", new GUIContent("Smooth Movement", "Smoothly interpolates the preview position each frame."));
            if (properties.Get("m_placementSystem.m_settings.m_previewSmoothMovement").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewMovementSmoothSpeed", new GUIContent("Smooth Speed", "Interpolation speed applied to preview movement."));
                    properties.Draw("m_placementSystem.m_settings.m_previewSnappingPositionThreshold", new GUIContent("Snapping Position Threshold", "Distance threshold below which position snapping is applied."));
                    properties.Draw("m_placementSystem.m_settings.m_previewSnappingRotationThreshold", new GUIContent("Snapping Rotation Threshold", "Angle threshold below which rotation snapping is applied."));
                }
            }

            EditorGUIExtended.Separator("Rotation Settings");
            properties.Draw("m_placementSystem.m_settings.m_previewRotationStep", new GUIContent("Rotation Step", "Angle increment applied on each rotate action."));

            properties.Draw("m_placementSystem.m_settings.m_previewClampRotation", new GUIContent("Clamp Rotation", "Restricts the preview rotation within defined bounds."));
            if (properties.Get("m_placementSystem.m_settings.m_previewClampRotation").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewClampMinRotation", new GUIContent("Clamp Min Rotation", "Minimum allowed rotation during preview."));
                    properties.Draw("m_placementSystem.m_settings.m_previewClampMaxRotation", new GUIContent("Clamp Max Rotation", "Maximum allowed rotation during preview."));
                }
            }

            properties.Draw("m_placementSystem.m_settings.m_previewAllowSnappedRotation", new GUIContent("Allow Snapped Rotation", "Snaps the preview rotation to fixed angle steps."));
            if (properties.Get("m_placementSystem.m_settings.m_previewAllowSnappedRotation").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewSnappedRotationStep", new GUIContent("Snapped Rotation Step", "Angle step used when snapped rotation is enabled."));
                }
            }

            EditorGUIExtended.Separator("Surface Alignment");
            properties.Draw("m_placementSystem.m_settings.m_previewSurfaceAlignment", new GUIContent("Surface Alignment", "Aligns the Building Part to the surface normal on placement."));
            if (properties.Get("m_placementSystem.m_settings.m_previewSurfaceAlignment").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewSurfaceAlignmentAxis", new GUIContent("Alignment Axis", "Local axis used to align with the surface normal."));
                }
            }

            EditorGUIExtended.Separator("Grounding Settings");
            properties.Draw("m_placementSystem.m_settings.m_previewForceGrounding", new GUIContent("Force Grounding", "Forces the Building Part to stay grounded on a surface."));
            if (properties.Get("m_placementSystem.m_settings.m_previewForceGrounding").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewGroundingLayer", new GUIContent("Grounding Layer", "Layer used to detect the ground surface."));
                    properties.Draw("m_placementSystem.m_settings.m_previewGroundingElevation", new GUIContent("Grounding Elevation", "Raises the Building Part above the detected ground surface."));
                    if (properties.Get("m_placementSystem.m_settings.m_previewGroundingElevation").boolValue)
                    {
                        using (EditorGUIExtended.IndentScope())
                        {
                            properties.Draw("m_placementSystem.m_settings.m_previewGroundingElevationStartRatio", new GUIContent("Elevation Start Ratio", "Ratio along the bounds used as the elevation start point."));
                            properties.Draw("m_placementSystem.m_settings.m_previewGroundingElevationMaxHeight", new GUIContent("Elevation Max Height", "Maximum height the Building Part can be elevated above ground."));
                        }
                    }
                }
            }

            EditorGUIExtended.Separator("Grid Settings");
            properties.Draw("m_placementSystem.m_settings.m_previewUseGridSnapping", new GUIContent("Use Grid Snapping", "Snaps the Building Part to the nearest grid cell during placement."));
            if (properties.Get("m_placementSystem.m_settings.m_previewUseGridSnapping").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_placementSystem.m_settings.m_previewCellSize", new GUIContent("Cell Size", "Size of each grid cell used for snapping."));
                    properties.Draw("m_placementSystem.m_settings.m_previewCellPivot", new GUIContent("Cell Pivot", "Pivot point within the cell used for alignment."));
                    properties.Draw("m_placementSystem.m_settings.m_previewLockToGrid", new GUIContent("Lock To Grid", "Keeps the preview locked to the grid. Falls back to the last valid cell."));
                }
            }

            EditorGUIExtended.Separator("Direction Indicator");
            EditorGUI.BeginChangeCheck();
            properties.Draw("m_placementSystem.m_settings.m_previewUseDirectionIndicator", new GUIContent("Use Direction Indicator", "Shows a directional indicator prefab during placement preview."));
            bool directionIndicatorChanged = EditorGUI.EndChangeCheck();

            if (properties.Get("m_placementSystem.m_settings.m_previewUseDirectionIndicator").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    SerializedProperty prefabProperty = properties.Get("m_placementSystem.m_settings.m_previewDirectionIndicatorPrefab");
                    EditorGUI.BeginChangeCheck();
                    properties.Draw("m_placementSystem.m_settings.m_previewDirectionIndicatorPrefab", new GUIContent("Prefab", "Prefab instantiated as the direction indicator."));
                    bool prefabPropertyChanged = EditorGUI.EndChangeCheck();

                    EditorGUI.BeginChangeCheck();
                    properties.Draw("m_placementSystem.m_settings.m_previewDirectionIndicatorPosition", new GUIContent("Position", "Local position offset of the direction indicator."));
                    properties.Draw("m_placementSystem.m_settings.m_previewDirectionIndicatorRotation", new GUIContent("Rotation", "Local rotation offset of the direction indicator."));
                    properties.Draw("m_placementSystem.m_settings.m_previewDirectionIndicatorScale", new GUIContent("Scale", "Local scale applied to the direction indicator."));
                    bool transformPropertyChanged = EditorGUI.EndChangeCheck();

                    if (prefabPropertyChanged || transformPropertyChanged)
                    {
                        serializedObject.ApplyModifiedProperties();
                        if (target.PlacementSystem.HasDirectionIndicator())
                        {
                            target.PlacementSystem.ClearDirectionIndicator();
                        }

                        if (prefabProperty.objectReferenceValue != null && target.gameObject.scene.IsValid())
                        {
                            target.PlacementSystem.SetupDirectionIndicator();
                        }

                        SceneView.RepaintAll();
                    }

                    bool hasDirectionIndicator = target.PlacementSystem.HasDirectionIndicator();
                    using (EditorGUIExtended.DisabledScope(prefabProperty.objectReferenceValue == null || !target.gameObject.scene.IsValid()))
                    {
                        EditorGUILayout.Separator();
                        if (GUILayout.Button(hasDirectionIndicator ? "Clear Direction Indicators" : "Show Direction Indicators"))
                        {
                            if (hasDirectionIndicator)
                            {
                                target.PlacementSystem.ClearDirectionIndicator();
                            }
                            else
                            {
                                target.PlacementSystem.SetupDirectionIndicator();
                            }

                            SceneView.RepaintAll();
                        }
                    }
                }
            }
            else if (directionIndicatorChanged)
            {
                serializedObject.ApplyModifiedProperties();
                if (target.PlacementSystem.HasDirectionIndicator())
                {
                    target.PlacementSystem.ClearDirectionIndicator();
                    SceneView.RepaintAll();
                }
            }
        }
    }
}
