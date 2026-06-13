/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionConditionEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingCollisionCondition))]
    public class BuildingCollisionConditionEditor : BaseInspectorEditor<BuildingCollisionCondition>
    {
        protected override void OnInspectorEnable()
        {
            SetDebugEnabled(true);
        }

        protected override void OnInspectorDisable()
        {
            SetDebugEnabled(false);
        }

        private void SetDebugEnabled(bool enabled)
        {
            foreach (Object target in targets)
            {
                BuildingCollisionCondition condition = target as BuildingCollisionCondition;
                if (condition != null && condition.EnableDebug != enabled)
                {
                    condition.EnableDebug = enabled;
                }
            }
        }

        protected override void OnInspectorDraw()
        {
            DrawGeneralSection();
            DrawDebugSection();
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.Separator("Collision Bounds", false);

            Properties.Draw("m_requirementEvaluationMode",
                new GUIContent("Requirement Mode", "Determines how requirements are evaluated: all must match or at least one matches."));

            SerializedProperty boundsArrayProperty = Properties.Get("m_collisionBounds");

            if (boundsArrayProperty.arraySize == 0)
            {
                EditorGUILayout.Separator();
                EditorGUIExtended.Label("Collision bounds list is empty.",
                    EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }
            else
            {
                EditorGUILayout.Separator();
                for (int i = 0; i < boundsArrayProperty.arraySize; i++)
                {
                    DrawBoundElement(i, boundsArrayProperty);
                }
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Add Collision Bounds..."))
            {
                AddBound(boundsArrayProperty);
            }
        }

        private void DrawBoundElement(int elementIndex, SerializedProperty boundsArrayProperty)
        {
            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent($"[{elementIndex + 1}] Collision Bounds", "Define collision detection bounds for this building part."),
                string.Empty,
                () => DrawBoundProperties(boundsArrayProperty.GetArrayElementAtIndex(elementIndex)),
                menu => DrawBoundsContextMenu(menu, elementIndex, boundsArrayProperty),
                false);
        }

        private void DrawBoundProperties(SerializedProperty boundProperty)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUIExtended.Separator("Collision Bounds Settings", false);

            boundProperty.DrawRelative("m_center", new GUIContent("Collision Bounds Center", "Center position of the collision detection box."));
            boundProperty.DrawRelative("m_size", new GUIContent("Collision Bounds Size", "Dimensions of the collision detection box."));

            boundProperty.DrawRelative("m_collisionLayer", new GUIContent("Collision Layer", "Physics layers to check for collisions."));
            boundProperty.DrawRelative("m_collisionTolerance", new GUIContent("Collision Tolerance", "Tolerance for collision detection (0-1). Gizmo: Green box."));
            boundProperty.DrawRelative("m_snappingCollisionTolerance", new GUIContent("Snapping Collision Tolerance", "Tolerance when snapped to sockets (0-1). Gizmo: Yellow box."));

            boundProperty.DrawRelative("m_preventOverlapping", new GUIContent("Prevent Overlapping", "Prevent overlapping with other building parts."));

            if (boundProperty.FindPropertyRelative("m_preventOverlapping").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    boundProperty.DrawRelative("m_overlappingCenter", new GUIContent("Overlapping Bounds Center", "Center position of the overlap detection box."));
                    boundProperty.DrawRelative("m_overlappingSize", new GUIContent("Overlapping Bounds Size", "Dimensions of the overlap detection box."));
                    boundProperty.DrawRelative("m_overlappingTolerance", new GUIContent("Overlapping Tolerance", "Scale factor applied to overlapping size (0-1)."));
                    boundProperty.DrawRelative("m_ignoreOverlappingTypes", new GUIContent("Ignore Types", "Building categories to ignore during overlapping checks."));
                }
            }

            boundProperty.DrawRelative("m_requireCollision", new GUIContent("Require Collision", "Placement requires collision with objects on specified layers."));
            boundProperty.DrawRelative("m_requireTerrain", new GUIContent("Require Terrain", "Placement requires direct contact with terrain."));
            boundProperty.DrawRelative("m_ignoreNestedCollision", new GUIContent("Ignore Nested Collision", "Ignore collisions with parent socket parts."));

            using (EditorGUIExtended.IndentScope())
            {
                boundProperty.DrawRelative("m_ignoreTags", new GUIContent("Ignore Tags", "Tags to exclude from collision detection."));
                boundProperty.DrawRelative("m_ignoreBuildingTypes", new GUIContent("Ignore Building Types", "Building part categories to exclude from collision."));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawDebugSection()
        {
            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable debug logging for collision events."));
            Properties.Draw("m_debugFlags", new GUIContent("Debug Draw Flags", "Where the collision bounds gizmos are allowed to draw."));
        }

        private void DrawBoundsContextMenu(GenericMenu contextMenuToPopulate, int boundIndex, SerializedProperty boundsArrayProperty)
        {
            EditorContextMenus.AddCopyPasteForType(
                contextMenuToPopulate,
                typeof(BuildingCollisionBoundsData),
                () =>
                {
                    EditorContextMenus.SetJsonClipboard(
                        typeof(BuildingCollisionBoundsData),
                        JsonUtility.ToJson(Target.CollisionBounds[boundIndex]));
                    Debug.Log($"Collision bounds copied at index {boundIndex} on '{Target.name}'.");
                },
                jsonData =>
                {
                    serializedObject.Update();
                    SerializedProperty boundElement = boundsArrayProperty.GetArrayElementAtIndex(boundIndex);
                    BuildingCollisionBoundsData sourceDataToCopy = JsonUtility.FromJson<BuildingCollisionBoundsData>(jsonData);

                    boundElement.FindPropertyRelative("m_center").vector3Value = sourceDataToCopy.Center;
                    boundElement.FindPropertyRelative("m_size").vector3Value = sourceDataToCopy.Size;
                    boundElement.FindPropertyRelative("m_collisionTolerance").floatValue = sourceDataToCopy.CollisionTolerance;
                    boundElement.FindPropertyRelative("m_snappingCollisionTolerance").floatValue = sourceDataToCopy.SnappingCollisionTolerance;
                    boundElement.FindPropertyRelative("m_collisionLayer").intValue = sourceDataToCopy.CollisionLayer.value;
                    boundElement.FindPropertyRelative("m_preventOverlapping").boolValue = sourceDataToCopy.PreventOverlapping;
                    boundElement.FindPropertyRelative("m_overlappingCenter").vector3Value = sourceDataToCopy.OverlappingCenter;
                    boundElement.FindPropertyRelative("m_overlappingSize").vector3Value = sourceDataToCopy.OverlappingSize;
                    boundElement.FindPropertyRelative("m_overlappingTolerance").floatValue = sourceDataToCopy.OverlappingTolerance;
                    boundElement.FindPropertyRelative("m_requireCollision").boolValue = sourceDataToCopy.RequireCollision;
                    boundElement.FindPropertyRelative("m_requireTerrain").boolValue = sourceDataToCopy.RequireTerrain;
                    boundElement.FindPropertyRelative("m_ignoreNestedCollision").boolValue = sourceDataToCopy.IgnoreNestedCollision;

                    SerializedProperty ignoreTagsProperty = boundElement.FindPropertyRelative("m_ignoreTags");
                    if (sourceDataToCopy.IgnoreTags != null)
                    {
                        ignoreTagsProperty.arraySize = sourceDataToCopy.IgnoreTags.Length;
                        for (int i = 0; i < sourceDataToCopy.IgnoreTags.Length; i++)
                        {
                            ignoreTagsProperty.GetArrayElementAtIndex(i).stringValue = sourceDataToCopy.IgnoreTags[i];
                        }
                    }
                    else
                    {
                        ignoreTagsProperty.arraySize = 0;
                    }

                    SerializedProperty ignoreBuildingTypesProperty = boundElement.FindPropertyRelative("m_ignoreBuildingTypes");
                    if (sourceDataToCopy.IgnoreBuildingTypes != null)
                    {
                        ignoreBuildingTypesProperty.arraySize = sourceDataToCopy.IgnoreBuildingTypes.Length;
                        for (int i = 0; i < sourceDataToCopy.IgnoreBuildingTypes.Length; i++)
                        {
                            ignoreBuildingTypesProperty.GetArrayElementAtIndex(i).stringValue = sourceDataToCopy.IgnoreBuildingTypes[i];
                        }
                    }
                    else
                    {
                        ignoreBuildingTypesProperty.arraySize = 0;
                    }

                    SerializedProperty ignoreOverlappingTypesProperty = boundElement.FindPropertyRelative("m_ignoreOverlappingTypes");
                    if (sourceDataToCopy.IgnoreOverlappingTypes != null)
                    {
                        ignoreOverlappingTypesProperty.arraySize = sourceDataToCopy.IgnoreOverlappingTypes.Length;
                        for (int i = 0; i < sourceDataToCopy.IgnoreOverlappingTypes.Length; i++)
                        {
                            ignoreOverlappingTypesProperty.GetArrayElementAtIndex(i).stringValue = sourceDataToCopy.IgnoreOverlappingTypes[i];
                        }
                    }
                    else
                    {
                        ignoreOverlappingTypesProperty.arraySize = 0;
                    }

                    serializedObject.ApplyModifiedProperties();
                    Debug.Log($"Collision bounds pasted to index {boundIndex} on '{Target.name}'.");
                });

            EditorContextMenus.AddItem(contextMenuToPopulate, "Reset Default Bounds", () =>
            {
                serializedObject.Update();
                SerializedProperty boundElement = boundsArrayProperty.GetArrayElementAtIndex(boundIndex);

                BuildingRendererSystem rendererSystem = Target?.Part?.RendererSystem;
                Bounds defaultBoundsValue = rendererSystem?.Active != null
                    ? rendererSystem.Active.GetLocalBounds()
                    : new Bounds(Vector3.zero, Vector3.one);

                boundElement.FindPropertyRelative("m_center").vector3Value = defaultBoundsValue.center;
                boundElement.FindPropertyRelative("m_size").vector3Value = defaultBoundsValue.size;
                boundElement.FindPropertyRelative("m_overlappingCenter").vector3Value = defaultBoundsValue.center;
                boundElement.FindPropertyRelative("m_overlappingSize").vector3Value = defaultBoundsValue.size;

                serializedObject.ApplyModifiedProperties();
                Debug.Log($"Collision bounds reset to renderer bounds at index {boundIndex}.");
            });

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddResetItem(contextMenuToPopulate, () =>
            {
                ResetBound(boundsArrayProperty.GetArrayElementAtIndex(boundIndex));
            });

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddRemoveComponentItem(contextMenuToPopulate, Target, () =>
            {
                serializedObject.Update();
                boundsArrayProperty.DeleteArrayElementAtIndex(boundIndex);
                serializedObject.ApplyModifiedProperties();
            });
        }

        private void AddBound(SerializedProperty boundsArrayProperty)
        {
            boundsArrayProperty.arraySize++;
            SerializedProperty boundElement = boundsArrayProperty.GetArrayElementAtIndex(boundsArrayProperty.arraySize - 1);
            ResetBound(boundElement);
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetBound(SerializedProperty boundElement)
        {
            BuildingRendererSystem rendererSystem = Target?.Part?.RendererSystem;
            Bounds defaultBoundsValue = rendererSystem?.Active != null
                ? rendererSystem.Active.GetLocalBounds()
                : new Bounds(Vector3.zero, Vector3.one);

            boundElement.FindPropertyRelative("m_center").vector3Value = defaultBoundsValue.center;
            boundElement.FindPropertyRelative("m_size").vector3Value = defaultBoundsValue.size;
            boundElement.FindPropertyRelative("m_collisionTolerance").floatValue = 0.95f;
            boundElement.FindPropertyRelative("m_snappingCollisionTolerance").floatValue = 0.95f;
            boundElement.FindPropertyRelative("m_collisionLayer").intValue = 1 << 0;
            boundElement.FindPropertyRelative("m_preventOverlapping").boolValue = false;
            boundElement.FindPropertyRelative("m_overlappingCenter").vector3Value = defaultBoundsValue.center;
            boundElement.FindPropertyRelative("m_overlappingSize").vector3Value = defaultBoundsValue.size;
            boundElement.FindPropertyRelative("m_overlappingTolerance").floatValue = 1f;
            boundElement.FindPropertyRelative("m_requireCollision").boolValue = false;
            boundElement.FindPropertyRelative("m_requireTerrain").boolValue = false;
            boundElement.FindPropertyRelative("m_ignoreNestedCollision").boolValue = false;

            SerializedProperty ignoreTagsProperty = boundElement.FindPropertyRelative("m_ignoreTags");
            ignoreTagsProperty.arraySize = 0;

            SerializedProperty ignoreBuildingTypesProperty = boundElement.FindPropertyRelative("m_ignoreBuildingTypes");
            ignoreBuildingTypesProperty.arraySize = 0;

            SerializedProperty ignoreOverlappingTypesProperty = boundElement.FindPropertyRelative("m_ignoreOverlappingTypes");
            ignoreOverlappingTypesProperty.arraySize = 0;

            serializedObject.ApplyModifiedProperties();
        }
    }
}