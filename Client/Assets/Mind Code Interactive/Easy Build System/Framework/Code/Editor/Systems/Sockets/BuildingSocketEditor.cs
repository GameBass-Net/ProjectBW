/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocketEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Sockets
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Sockets
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingSocket))]
    public class BuildingSocketEditor : BaseInspectorEditor<BuildingSocket>
    {
        private enum HandleMode
        {
            Position,
            Rotation,
            Scale
        }

        private int m_activeSnappingPointIndex = -1;
        private Tool m_previousTool;
        private HandleMode m_activeHandleMode = HandleMode.Position;
        private readonly Dictionary<int, int> m_previewIndices = new Dictionary<int, int>();

        private const string k_ShowHandlesKey = "EasyBuildSystem_ShowSocketHandles";

        private bool ShowSocketHandles => EditorPrefs.GetBool(k_ShowHandlesKey, true);

        protected override void OnInspectorDisable()
        {
            bool anyHadPreview = false;
            foreach (BuildingSocket socket in TargetsTyped)
            {
                if (socket != null && socket.HasPreview())
                {
                    socket.ClearPreview();
                    anyHadPreview = true;
                }
            }

            if (anyHadPreview)
            {
                m_activeSnappingPointIndex = -1;
                UnityEditor.Tools.current = m_previousTool;
                SceneView.RepaintAll();
            }
        }

        private bool TargetsShareSnappingPoint(int pointIndex)
        {
            if (TargetsTyped == null || TargetsTyped.Length == 0)
            {
                return false;
            }

            BuildingSocket primary = TargetsTyped[0];
            if (primary == null || primary.SnappingPoints == null || pointIndex >= primary.SnappingPoints.Count)
            {
                return false;
            }

            if (TargetsTyped.Length == 1)
            {
                return true;
            }

            SocketSnapData reference = primary.SnappingPoints[pointIndex];

            for (int i = 1; i < TargetsTyped.Length; i++)
            {
                BuildingSocket socket = TargetsTyped[i];
                if (socket == null || socket.SnappingPoints == null || pointIndex >= socket.SnappingPoints.Count)
                {
                    return false;
                }

                SocketSnapData snap = socket.SnappingPoints[pointIndex];

                if (snap.MatchBy != reference.MatchBy)
                {
                    return false;
                }

                if (reference.MatchBy == BuildingSocket.MatchType.Category && snap.Category != reference.Category)
                {
                    return false;
                }

                if (reference.MatchBy == BuildingSocket.MatchType.Reference && snap.PartReference != reference.PartReference)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AnyTargetHasPreview()
        {
            foreach (BuildingSocket socket in TargetsTyped)
            {
                if (socket != null && socket.HasPreview())
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearAllTargetPreviews()
        {
            foreach (BuildingSocket socket in TargetsTyped)
            {
                if (socket != null && socket.HasPreview())
                {
                    socket.ClearPreview();
                }
            }
        }

        private void CreateAllTargetPreviews(BuildingPart part)
        {
            if (part == null)
            {
                return;
            }

            foreach (BuildingSocket socket in TargetsTyped)
            {
                if (socket != null)
                {
                    socket.CreatePreview(part);
                }
            }
        }

        private void OnSceneGUI()
        {
            if (m_activeSnappingPointIndex >= 0 && m_activeSnappingPointIndex < Target.SnappingPoints.Count)
            {
                UpdateSnappingPointPreview();
                HandleSnappingPointTransformEdit();
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoad]
        private static class SocketHandlesDrawer
        {
            static SocketHandlesDrawer()
            {
                SceneView.duringSceneGui += DrawAllSocketHandles;
            }

            private static void DrawAllSocketHandles(SceneView sceneView)
            {
                if (!Handles.ShouldRenderGizmos())
                {
                    return;
                }

                if (!EditorPrefs.GetBool("EasyBuildSystem_ShowSocketHandles", true))
                {
                    return;
                }

#if UNITY_2020_1_OR_NEWER
                UnityEditor.SceneManagement.PrefabStage prefabStage =
                    UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
    UnityEditor.Experimental.SceneManagement.PrefabStage prefabStage =
        UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif

                bool isPrefabStage = prefabStage != null;

                if (!isPrefabStage && BuildingManager.Instance == null)
                {
                    return;
                }

                HashSet<BuildingPart> processedParts = new HashSet<BuildingPart>();

                foreach (GameObject selectedObject in Selection.gameObjects)
                {
                    if (selectedObject == null)
                    {
                        continue;
                    }

                    BuildingPart selectedPart = selectedObject.GetComponentInParent<BuildingPart>();
                    if (selectedPart == null)
                    {
                        continue;
                    }

                    if (!processedParts.Add(selectedPart))
                    {
                        continue;
                    }

                    if (isPrefabStage && selectedPart.gameObject.scene != prefabStage.scene)
                    {
                        continue;
                    }

                    BuildingSocket[] allSockets = selectedPart.GetComponentsInChildren<BuildingSocket>(true);
                    if (allSockets == null || allSockets.Length == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < allSockets.Length; i++)
                    {
                        BuildingSocket socket = allSockets[i];
                        if (socket == null || !socket.isActiveAndEnabled)
                        {
                            continue;
                        }

                        DrawSocketHandle(socket);
                    }
                }
            }

            private static void DrawSocketHandle(BuildingSocket socket)
            {
                Vector3 socketPosition = socket.Position;
                float size = HandleUtility.GetHandleSize(socketPosition);
                float handleRadius = size * 0.15f;
                bool isSocketSelected = System.Array.IndexOf(Selection.gameObjects, socket.gameObject) >= 0;
                bool isHandleHovered = Vector2.Distance(HandleUtility.WorldToGUIPoint(socketPosition), Event.current.mousePosition) < 30f;

                if (isSocketSelected)
                {
                    Handles.color = new Color(0f, 1f, 1f, 0.6f);
                    Handles.SphereHandleCap(0, socketPosition, Quaternion.identity, handleRadius, EventType.Repaint);

                    Vector3 forwardDirectionEnd = socketPosition + socket.transform.forward * size * 0.65f;
                    Handles.DrawLine(socketPosition, forwardDirectionEnd);
                    Handles.ArrowHandleCap(0, forwardDirectionEnd, socket.transform.rotation, size * 0.65f, EventType.Repaint);
                }
                else
                {
                    Handles.color = new Color(0f, 1f, 1f, isHandleHovered ? 0.3f : 0.15f);
                    Handles.SphereHandleCap(0, socketPosition, Quaternion.identity, handleRadius, EventType.Repaint);
                }

                float radius = socket.SocketRadius;
                Handles.color = new Color(0f, 1f, 1f, 1f);
                Handles.DrawWireDisc(socketPosition, Vector3.up, radius);
                Handles.DrawWireDisc(socketPosition, Vector3.right, radius);
                Handles.DrawWireDisc(socketPosition, Vector3.forward, radius);

                Handles.color = new Color(0f, 1f, 1f, isHandleHovered ? 0.3f : 0.15f);
                if (Handles.Button(socketPosition, Quaternion.identity, handleRadius, handleRadius, Handles.SphereHandleCap))
                {
                    Selection.activeObject = socket;
                    EditorGUIUtility.PingObject(socket);
                }

                if (isHandleHovered)
                {
                    HandleUtility.Repaint();
                }
            }
        }
#endif

        protected override void OnInspectorDraw()
        {
#if PRO_BUILD_SYSTEM
            ProUpgradeUtility.DrawSocketUpgradeBanner(Target);
#endif
            EditorGUIExtended.InspectorHeader(target,
                "Defines a socket in the scene where Building Parts can attach to each other.\n" +
                "Each socket holds multiple snapping points with independent match rules per point.\n" +
                "See the documentation for more information about this component.");

            DrawGeneralSection();
            DrawSnappingSection();
            DrawDebugSection();

            EditorGUIExtended.InspectorBottom();
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure the socket type, priority and detection radius.",
                () =>
                {
                    Properties.Draw("m_socketType", new GUIContent("Socket Type", "Identifier used to match compatible Building Parts to this socket."));
                    Properties.Draw("m_socketProperty", new GUIContent("Socket Priority", "Determines which socket takes precedence when multiple are detected simultaneously."));
                    Properties.Draw("m_socketRadius", new GUIContent("Socket Radius", "Detection radius within which Building Parts can snap to this socket."));
                    OnDrawGeneralSettingsExtra();
                }, false, true);
        }

        protected virtual void OnDrawGeneralSettingsExtra() { }

        private void DrawSnappingSection()
        {
            EditorGUIExtended.DrawExpandableSection("Snapping Settings", "link",
                "Configure snapping points that define where and how Building Parts attach to this socket.",
                () =>
                {
                    SerializedProperty snappingPointsProperty = serializedObject.FindProperty("m_snappingPoints");

                    if (snappingPointsProperty.arraySize == 0)
                    {
                        EditorGUILayout.Space(1f);
                        EditorGUIExtended.Label("Add snapping points to define where Building Parts can attach to this socket.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
                    }

                    for (int pointIndex = 0; pointIndex < snappingPointsProperty.arraySize; pointIndex++)
                    {
                        DrawSnappingPoint(snappingPointsProperty, pointIndex);
                    }

                    EditorGUIExtended.Separator();

                    if (GUILayout.Button("Add Snapping Point..."))
                    {
                        Undo.RecordObject(Target, "Add Snapping Point");
                        snappingPointsProperty.InsertArrayElementAtIndex(snappingPointsProperty.arraySize);
                        SerializedProperty newSnappingPoint = snappingPointsProperty.GetArrayElementAtIndex(snappingPointsProperty.arraySize - 1);
                        newSnappingPoint.FindPropertyRelative("m_matchBy").enumValueIndex = (int)BuildingSocket.MatchType.Category;
                        newSnappingPoint.FindPropertyRelative("m_category").stringValue = string.Empty;
                        newSnappingPoint.FindPropertyRelative("m_partReference").stringValue = string.Empty;
                        newSnappingPoint.FindPropertyRelative("m_positionOffset").vector3Value = Vector3.zero;
                        newSnappingPoint.FindPropertyRelative("m_rotationOffset").vector3Value = Vector3.zero;
                        newSnappingPoint.FindPropertyRelative("m_scaleOffset").vector3Value = Vector3.one;
                        serializedObject.ApplyModifiedProperties();
                    }
                }, false, true);
        }

        private void DrawDebugSection()
        {
            EditorGUIExtended.DrawExpandableSection("Debug Settings", "cache",
                "Inspect the current socket state, statistics and configure debug rendering options.",
                () =>
                {
                    EditorGUIExtended.Separator("Unique Object Statistics", false);
                    EditorGUILayout.LabelField("Unique ID :", Target.UniqueId.ToString());
                    EditorGUILayout.LabelField("Is Registered :", Target.IsRegistered.ToString());

                    EditorGUIExtended.Separator("Building Socket Statistics");
                    EditorGUILayout.LabelField("Is Enabled :", Target.IsEnabled().ToString());
                    EditorGUILayout.LabelField("Parent Building Part :", Target.ParentPart != null ? Target.ParentPart.name : "None");
                    EditorGUILayout.LabelField("Attached Building Part :", Target.AttachedPart != null ? Target.AttachedPart.name : "None");

                    EditorGUIExtended.Separator("Rendering Settings");
                    Properties.Draw("m_debugFlags",
                        new GUIContent("Debug Draw Flags", "Controls where socket boundaries are rendered in the editor."));
                    EditorPrefs.SetBool(k_ShowHandlesKey, EditorGUILayout.Toggle(
                        new GUIContent("Show Socket Handles", "Toggles the visibility of socket handles in the scene view."),
                        ShowSocketHandles));
                }, false, false);
        }

        private void DrawSnappingPoint(SerializedProperty snappingPointsProperty, int pointIndex)
        {
            bool sharesAcrossTargets = TargetsShareSnappingPoint(pointIndex);

            if (!sharesAcrossTargets && targets.Length > 1)
            {
                foreach (BuildingSocket socket in TargetsTyped)
                {
                    if (socket == null || socket.SnappingPoints == null || pointIndex >= socket.SnappingPoints.Count)
                    {
                        return;
                    }
                }
            }

            SerializedProperty snappingPointProperty = snappingPointsProperty.GetArrayElementAtIndex(pointIndex);
            if (snappingPointProperty == null)
            {
                return;
            }

            SerializedProperty matchTypeProperty = snappingPointProperty.FindPropertyRelative("m_matchBy");
            if (matchTypeProperty == null || matchTypeProperty.hasMultipleDifferentValues)
            {
                if (matchTypeProperty?.hasMultipleDifferentValues ?? false)
                {
                    EditorGUIExtended.HelpBox("Cannot edit snapping points with different match types.", EditorGUIElements.MessageType.Warning);
                }

                return;
            }

            string displayName = GetSnappingPointDisplayName(snappingPointProperty);
            string categoryName = snappingPointProperty.FindPropertyRelative("m_category")?.stringValue ?? "";
            string matchDescription = (matchTypeProperty.enumValueIndex == (int)BuildingSocket.MatchType.Category)
                ? "[Type] " + categoryName
                : "[Reference] " + displayName;

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent("[" + pointIndex + "] Snapping Point - " + matchDescription),
                string.Empty,
                () => DrawSnappingPointInspector(pointIndex, sharesAcrossTargets),
                menu => BuildSnappingPointContextMenu(snappingPointsProperty, menu, pointIndex),
                false
            );
        }

        private string GetSnappingPointDisplayName(SerializedProperty snappingPointProperty)
        {
            string partReferencePrefabId = snappingPointProperty.FindPropertyRelative("m_referencePrefab")?.stringValue;
            if (!string.IsNullOrEmpty(partReferencePrefabId))
            {
                BuildingPart referencedPart = BuildingManager.Instance?.GetPartByPrefabId(partReferencePrefabId);
                return referencedPart != null ? referencedPart.Name : partReferencePrefabId;
            }

            return "";
        }

        private void DrawSnappingPointInspector(int pointIndex, bool sharesAcrossTargets)
        {
            EditorGUI.BeginChangeCheck();
            SerializedProperty snappingPointSnapProp = Properties["m_snappingPoints"].GetArrayElementAtIndex(pointIndex);
            snappingPointSnapProp.DrawRelative("m_matchBy", new GUIContent("Match Type", "How to match compatible parts: by category or by specific part reference."));

            SerializedProperty updatedMatchTypeProperty = snappingPointSnapProp.GetRelative("m_matchBy");
            if (updatedMatchTypeProperty != null && updatedMatchTypeProperty.enumValueIndex == (int)BuildingSocket.MatchType.Category)
            {
                snappingPointSnapProp.DrawRelative("m_category", new GUIContent("Category", "The category of parts that can snap to this point."));
            }
            else
            {
                snappingPointSnapProp.DrawRelative("m_partReference", new GUIContent("Part Reference", "The specific part that can snap to this point."));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUIExtended.Separator();

            bool isPreviewActive = AnyTargetHasPreview() && m_activeSnappingPointIndex == pointIndex;

            if (isPreviewActive)
            {
                if (!EditorHelper.IsInPrefabIsolation(Target.gameObject))
                {
                    ConditionResult conditionResult = Target.PreviewPart.ConditionSystem.EvaluateConditions(BuildingMode.Placement);
                    if (!conditionResult.IsValid)
                    {
                        EditorGUIExtended.ColoredLabel(conditionResult.Reason, Color.red, EditorGUILabels.LabelType.Bold, EditorGUILabels.LabelAlignment.Center);
                        GUILayout.Space(5f);
                    }
                }
                else
                {
                    EditorGUIExtended.HelpBox("Real-time building conditions unavailable in Prefab Isolation Mode.", EditorGUIElements.MessageType.Warning);
                    GUILayout.Space(1f);
                }

                GUILayout.BeginHorizontal();
                GUIContent[] handleModeContents = new GUIContent[]
                {
                    new GUIContent(" Position", EditorGUIUtility.IconContent("MoveTool").image, "Edit the position offset."),
                    new GUIContent(" Rotation", EditorGUIUtility.IconContent("RotateTool").image, "Edit the rotation offset."),
                    new GUIContent(" Scale", EditorGUIUtility.IconContent("ScaleTool").image, "Edit the scale offset.")
                };
                m_activeHandleMode = (HandleMode)GUILayout.Toolbar((int)m_activeHandleMode, handleModeContents, GUILayout.Height(20f));
                GUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }

            SerializedProperty offsetSnapProp = Properties["m_snappingPoints"].GetArrayElementAtIndex(pointIndex);
            offsetSnapProp.DrawRelative("m_positionOffset", new GUIContent("Position Offset", "Local position offset from the socket origin."));
            offsetSnapProp.DrawRelative("m_rotationOffset", new GUIContent("Rotation Offset", "Local rotation offset in degrees."));
            offsetSnapProp.DrawRelative("m_scaleOffset", new GUIContent("Scale Offset", "Local scale multiplier for the snapped part."));

            EditorGUILayout.Separator();

            if (!sharesAcrossTargets && TargetsTyped.Length > 1)
            {
                EditorGUIExtended.HelpBox("Selected sockets have different match data on this snapping point. Editing is disabled.", EditorGUIElements.MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(!sharesAcrossTargets && TargetsTyped.Length > 1))
            {
                if (Target.SnappingPoints[pointIndex].MatchBy == BuildingSocket.MatchType.Category)
                {
                    DrawCategorySnappingPointControls(pointIndex);
                }
                else
                {
                    DrawReferenceSnappingPointControls(pointIndex, isPreviewActive);
                }
            }
        }

        private void DrawCategorySnappingPointControls(int pointIndex)
        {
            BuildingPart[] categoryPartsArray = BuildingManager.Instance.GetPartsByCategory(Target.SnappingPoints[pointIndex].Category);
            BuildingCollection parentCollection = Target.ParentPart ? Target.ParentPart.AttachedCollection : null;
            categoryPartsArray = PrioritizeCollection(categoryPartsArray, parentCollection);

            if (categoryPartsArray != null && categoryPartsArray.Length > 0)
            {
                if (!m_previewIndices.ContainsKey(pointIndex))
                {
                    m_previewIndices[pointIndex] = 0;
                }

                int totalCategoryParts = categoryPartsArray.Length;
                int currentCategoryIndex = m_previewIndices[pointIndex] % totalCategoryParts;

                GUILayout.BeginHorizontal();

                GUIContent leftArrowIcon = new GUIContent(Resources.Load<Texture2D>("Editor/Icons/left_arrow"));
                GUIContent rightArrowIcon = new GUIContent(Resources.Load<Texture2D>("Editor/Icons/right_arrow"));
                GUIStyle iconButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageOnly,
                    fixedWidth = 30,
                    fixedHeight = 18,
                    padding = new RectOffset(1, 1, 4, 3)
                };

                if (GUILayout.Button(leftArrowIcon, iconButtonStyle))
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                    m_previewIndices[pointIndex] = (currentCategoryIndex - 1 + totalCategoryParts) % totalCategoryParts;
                    m_activeSnappingPointIndex = pointIndex;
                    m_previousTool = UnityEditor.Tools.current;
                    UnityEditor.Tools.current = Tool.None;
                }

                BuildingPart part = categoryPartsArray[currentCategoryIndex];
                string partName = part == null ? "Missing Part"
                                     : part.Equals(null) ? "Missing Part"
                                     : part.name;

                bool isPreviewActive = AnyTargetHasPreview() && m_activeSnappingPointIndex == pointIndex;
                string buttonLabel = (isPreviewActive ? "Save" : "Edit") + $" ({partName})";

                if (EditorGUIExtended.StateButton(buttonLabel, isPreviewActive, GUILayout.ExpandWidth(true)))
                {
                    if (isPreviewActive)
                    {
                        m_activeSnappingPointIndex = -1;
                        ClearAllTargetPreviews();
                        UnityEditor.Tools.current = m_previousTool;
                    }
                    else
                    {
                        m_activeSnappingPointIndex = pointIndex;
                        m_previousTool = UnityEditor.Tools.current;
                        UnityEditor.Tools.current = Tool.None;
                    }

                    EnableGizmosForPreview();
                }

                if (GUILayout.Button(rightArrowIcon, iconButtonStyle))
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                    m_previewIndices[pointIndex] = (currentCategoryIndex + 1) % totalCategoryParts;
                    m_activeSnappingPointIndex = pointIndex;
                    m_previousTool = UnityEditor.Tools.current;
                    UnityEditor.Tools.current = Tool.None;
                }

                GUILayout.EndHorizontal();
            }
        }

        private void DrawReferenceSnappingPointControls(int pointIndex, bool isPreviewActive)
        {
            if (EditorGUIExtended.StateButton(isPreviewActive ? "Save Snapping Settings" : "Edit Snapping Settings...", isPreviewActive, GUILayout.ExpandWidth(true)))
            {
                if (isPreviewActive)
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                    UnityEditor.Tools.current = m_previousTool;
                }
                else
                {
                    m_activeSnappingPointIndex = pointIndex;
                    m_previousTool = UnityEditor.Tools.current;
                    UnityEditor.Tools.current = Tool.None;
                }
            }
        }

        private void BuildSnappingPointContextMenu(SerializedProperty snappingPointsProperty, GenericMenu menu, int pointIndex)
        {
            bool canMoveUp = pointIndex > 0;
            bool canMoveDown = pointIndex < snappingPointsProperty.arraySize - 1;

            EditorContextMenus.AddMoveUpItem(menu, canMoveUp, () =>
            {
                if (AnyTargetHasPreview())
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                }

                serializedObject.Update();
                snappingPointsProperty.MoveArrayElement(pointIndex, pointIndex - 1);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.AddMoveDownItem(menu, canMoveDown, () =>
            {
                if (AnyTargetHasPreview())
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                }

                serializedObject.Update();
                snappingPointsProperty.MoveArrayElement(pointIndex, pointIndex + 1);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddDuplicateItem(menu, () =>
            {
                serializedObject.Update();
                snappingPointsProperty.InsertArrayElementAtIndex(pointIndex);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.AddCopyPasteForType(
                menu,
                typeof(SocketSnapData),
                () =>
                {
                    EditorContextMenus.SetJsonClipboard(
                        typeof(SocketSnapData),
                        JsonUtility.ToJson(Target.SnappingPoints[pointIndex]));
                },
                jsonData =>
                {
                    SocketSnapData source = JsonUtility.FromJson<SocketSnapData>(jsonData);
                    SocketSnapData target = Target.SnappingPoints[pointIndex];

                    target.MatchBy = source.MatchBy;
                    target.Category = source.Category;
                    target.PartReference = source.PartReference;
                    target.PositionOffset = source.PositionOffset;
                    target.RotationOffset = source.RotationOffset;
                    target.ScaleOffset = source.ScaleOffset;

                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddRemoveComponentItem(menu, Target, () =>
            {
                if (AnyTargetHasPreview() && m_activeSnappingPointIndex == pointIndex)
                {
                    m_activeSnappingPointIndex = -1;
                    ClearAllTargetPreviews();
                }

                serializedObject.Update();
                snappingPointsProperty.DeleteArrayElementAtIndex(pointIndex);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });
        }

        private void HandleSnappingPointTransformEdit()
        {
            SocketSnapData currentSnappingPoint = Target.SnappingPoints[m_activeSnappingPointIndex];
            Vector3 worldPositionOffset = Target.transform.TransformPoint(currentSnappingPoint.PositionOffset);
            Quaternion worldRotationOffset = Quaternion.Euler(currentSnappingPoint.RotationOffset);
            Vector3 worldScaleOffset = currentSnappingPoint.ScaleOffset;

            EditorGUI.BeginChangeCheck();

            switch (m_activeHandleMode)
            {
                case HandleMode.Position:
                    worldPositionOffset = Handles.PositionHandle(worldPositionOffset, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObjects(TargetsTyped, "Move Snapping Point");
                        Vector3 newLocalOffset = Target.transform.InverseTransformPoint(worldPositionOffset);

                        foreach (BuildingSocket socket in TargetsTyped)
                        {
                            if (socket == null || socket.SnappingPoints == null || m_activeSnappingPointIndex >= socket.SnappingPoints.Count)
                            {
                                continue;
                            }

                            socket.SnappingPoints[m_activeSnappingPointIndex].PositionOffset = newLocalOffset;
                            EditorUtility.SetDirty(socket);
                        }
                    }
                    break;

                case HandleMode.Rotation:
                    worldRotationOffset = Handles.RotationHandle(worldRotationOffset, worldPositionOffset);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObjects(TargetsTyped, "Rotate Snapping Point");
                        Vector3 newRotationEuler = worldRotationOffset.eulerAngles;

                        foreach (BuildingSocket socket in TargetsTyped)
                        {
                            if (socket == null || socket.SnappingPoints == null || m_activeSnappingPointIndex >= socket.SnappingPoints.Count)
                            {
                                continue;
                            }

                            socket.SnappingPoints[m_activeSnappingPointIndex].RotationOffset = newRotationEuler;
                            EditorUtility.SetDirty(socket);
                        }
                    }
                    break;

                case HandleMode.Scale:
                    Vector3 newWorldScale = Handles.ScaleHandle(worldScaleOffset, worldPositionOffset, Quaternion.identity, HandleUtility.GetHandleSize(worldPositionOffset));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObjects(TargetsTyped, "Scale Snapping Point");

                        foreach (BuildingSocket socket in TargetsTyped)
                        {
                            if (socket == null || socket.SnappingPoints == null || m_activeSnappingPointIndex >= socket.SnappingPoints.Count)
                            {
                                continue;
                            }

                            socket.SnappingPoints[m_activeSnappingPointIndex].ScaleOffset = newWorldScale;
                            EditorUtility.SetDirty(socket);
                        }
                    }
                    break;
            }
        }

        private void UpdateSnappingPointPreview()
        {
            if (TargetsTyped == null || TargetsTyped.Length == 0)
            {
                return;
            }

            SocketSnapData currentSnappingPoint = Target.SnappingPoints[m_activeSnappingPointIndex];

            BuildingPart partToPreview = null;

            if (currentSnappingPoint.MatchBy == BuildingSocket.MatchType.Reference)
            {
                partToPreview = BuildingManager.Instance.GetPartByPrefabId(currentSnappingPoint.PartReference);
            }
            else
            {
                BuildingPart[] categoryParts = BuildingManager.Instance.GetPartsByCategory(currentSnappingPoint.Category);
                BuildingCollection parentCollection = Target.ParentPart ? Target.ParentPart.AttachedCollection : null;
                categoryParts = PrioritizeCollection(categoryParts, parentCollection);

                if (categoryParts != null && categoryParts.Length > 0)
                {
                    if (!m_previewIndices.ContainsKey(m_activeSnappingPointIndex))
                    {
                        m_previewIndices[m_activeSnappingPointIndex] = 0;
                    }

                    partToPreview = categoryParts[m_previewIndices[m_activeSnappingPointIndex] % categoryParts.Length];
                }
            }

            if (partToPreview == null)
            {
                return;
            }

            foreach (BuildingSocket socket in TargetsTyped)
            {
                if (socket == null)
                {
                    continue;
                }

                if (!socket.HasPreview())
                {
                    socket.CreatePreview(partToPreview);
                }

                if (!socket.HasPreview())
                {
                    continue;
                }

                socket.PreviewPart.transform.SetParent(socket.transform);
                socket.PreviewPart.Move(currentSnappingPoint, socket.transform);

                if (EditorHelper.IsInPrefabIsolation(socket.gameObject))
                {
                    socket.PreviewPart.PlacementSystem.UpdatePreview(true, BuildingPart.BuildingState.Placement);
                }
                else
                {
                    ConditionResult conditionResult = socket.PreviewPart.ConditionSystem.EvaluateConditions(BuildingMode.Placement);
                    socket.PreviewPart.PlacementSystem.UpdatePreview(conditionResult.IsValid, BuildingPart.BuildingState.Placement);
                }
            }
        }

        private void EnableGizmosForPreview()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.drawGizmos = true;
                sceneView.Repaint();
            }
        }

        private BuildingPart[] PrioritizeCollection(BuildingPart[] partsArray, BuildingCollection collection)
        {
            if (partsArray == null || partsArray.Length == 0 || collection == null)
            {
                return partsArray;
            }

            HashSet<string> collectionPartReferences = new HashSet<string>(collection.PartReferences ?? System.Array.Empty<string>());
            List<BuildingPart> partsInCollection = new List<BuildingPart>();
            List<BuildingPart> partsNotInCollection = new List<BuildingPart>();

            foreach (BuildingPart part in partsArray)
            {
                string partPrefabId = part ? part.PrefabId : null;
                if (!string.IsNullOrEmpty(partPrefabId) && collectionPartReferences.Contains(partPrefabId))
                {
                    partsInCollection.Add(part);
                }
                else
                {
                    partsNotInCollection.Add(part);
                }
            }

            if (partsNotInCollection.Count == 0)
            {
                return partsArray;
            }

            partsInCollection.AddRange(partsNotInCollection);
            return partsInCollection.ToArray();
        }
    }
}