/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartRendererOffsetEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartRendererOffsetEditor
    {
        private static Rect s_windowRect = new Rect(10f, 30f, 350f, 180f);
        private static Transform s_ownerRoot;
        private static Transform s_target;
        private static RendererVariantData s_targetVariant;

        private static Vector3 s_defaultPosition;
        private static Vector3 s_defaultEulerAngles;
        private static Vector3 s_defaultScale;

        private static HandleMode s_handleMode = HandleMode.Position;
        private static Tool s_previousTool = Tool.None;

        private enum HandleMode { Position, Rotation, Scale }

        public static void Open(Transform ownerRootTransform, Transform targetTransform)
        {
            s_ownerRoot = ownerRootTransform;
            s_target = targetTransform;
            if (!s_target)
            {
                return;
            }

            BuildingPart part = s_ownerRoot ? s_ownerRoot.GetComponent<BuildingPart>() : null;
            if (part != null)
            {
                for (int variantIndex = 0; variantIndex < part.RendererSystem.Count; variantIndex++)
                {
                    RendererVariantData rendererVariantToCheck = part.RendererSystem.GetVariant(variantIndex);
                    if (rendererVariantToCheck != null && rendererVariantToCheck.Root == s_target)
                    {
                        s_targetVariant = rendererVariantToCheck;
                        break;
                    }
                }
            }

            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;

            s_defaultPosition = s_target.localPosition;
            s_defaultEulerAngles = s_target.localEulerAngles;
            s_defaultScale = s_target.localScale;

            s_previousTool = UnityEditor.Tools.current;
            UnityEditor.Tools.current = Tool.None;

            Selection.activeObject = s_target.gameObject;

            SceneView activeSceneView = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
            if (activeSceneView)
            {
                activeSceneView.Focus();
                EditorApplication.delayCall += () =>
                {
                    if (activeSceneView)
                    {
                        activeSceneView.FrameSelected();
                    }
                };
            }
        }

        public static void Close()
        {
            UnityEditor.Tools.current = s_previousTool;
            SceneView.duringSceneGui -= OnScene;
            s_target = null;
            s_ownerRoot = null;
            s_targetVariant = null;
        }

        private static void OnScene(SceneView sceneViewToRender)
        {
            if (!s_target)
            {
                Close();
                return;
            }

            switch (s_handleMode)
            {
                case HandleMode.Position:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPositionValue = Handles.PositionHandle(s_target.position, s_target.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(s_target, "Move Target");
                        s_target.position = newPositionValue;
                    }
                    break;

                case HandleMode.Rotation:
                    EditorGUI.BeginChangeCheck();
                    Quaternion newRotationValue = Handles.RotationHandle(s_target.rotation, s_target.position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(s_target, "Rotate Target");
                        s_target.rotation = newRotationValue;
                    }
                    break;

                case HandleMode.Scale:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newScaleValue = Handles.ScaleHandle(
                        s_target.localScale,
                        s_target.position,
                        s_target.rotation,
                        HandleUtility.GetHandleSize(s_target.position));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(s_target, "Scale Target");
                        s_target.localScale = newScaleValue;
                    }
                    break;
            }

            Handles.BeginGUI();
            int windowControlId = GUIUtility.GetControlID(FocusType.Passive);
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0f);
            s_windowRect = GUILayout.Window(windowControlId, s_windowRect, DrawWindowContent, string.Empty);
            GUI.backgroundColor = Color.white;
            Handles.EndGUI();
        }

        private static void DrawWindowContent(int windowIdParameter)
        {
            EditorGUIExtended.BeginBorderLayoutVertical();
            using (EditorGUIExtended.MarginScope())
            {
                EditorGUILayout.LabelField("Render Offset Editor", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Move, rotate, or scale the selected transform.");

                using (EditorGUIExtended.DisabledScope(!s_target))
                {
                    if (s_target)
                    {
                        int selectedToolbarIndex = (int)s_handleMode;
                        selectedToolbarIndex = GUILayout.Toolbar(
                            selectedToolbarIndex,
                            new[]
                            {
                                EditorGUIUtility.IconContent("MoveTool"),
                                EditorGUIUtility.IconContent("RotateTool"),
                                EditorGUIUtility.IconContent("ScaleTool")
                            },
                            GUILayout.Height(25f));
                        s_handleMode = (HandleMode)selectedToolbarIndex;

                        GUILayout.Space(1f);

                        EditorGUILayout.ObjectField("Target", s_target, typeof(Transform), true);

                        s_target.localPosition = EditorGUILayout.Vector3Field("Position Offset", s_target.localPosition);
                        s_target.localEulerAngles = EditorGUILayout.Vector3Field("Rotation Offset", s_target.localEulerAngles);
                        s_target.localScale = EditorGUILayout.Vector3Field("Scale Offset", s_target.localScale);

                        EditorGUILayout.Space();

                        if (GUILayout.Button("Save & Close", GUILayout.Height(20f)))
                        {
                            UnityEditor.Tools.current = s_previousTool;
                            SceneView.duringSceneGui -= OnScene;
                            EditorUtility.SetDirty(s_target);

                            BuildingPart part = s_ownerRoot ? s_ownerRoot.GetComponent<BuildingPart>() : null;
                            if (part != null && s_targetVariant != null)
                            {
                                Undo.RecordObject(part, "Refresh Bounds");
                                part.RendererSystem.RecalculateBounds(s_targetVariant);
                                EditorUtility.SetDirty(part);
                            }

                            if (s_ownerRoot)
                            {
                                Selection.activeGameObject = s_ownerRoot.gameObject;
                            }

                            SceneView.RepaintAll();
                            s_target = null;
                            s_ownerRoot = null;
                            s_targetVariant = null;
                        }

                        if (GUILayout.Button("Cancel", GUILayout.Height(20f)))
                        {
                            s_target.localPosition = s_defaultPosition;
                            s_target.localEulerAngles = s_defaultEulerAngles;
                            s_target.localScale = s_defaultScale;
                            UnityEditor.Tools.current = s_previousTool;
                            SceneView.duringSceneGui -= OnScene;
                            if (s_ownerRoot)
                            {
                                Selection.activeGameObject = s_ownerRoot.gameObject;
                            }

                            s_target = null;
                            s_ownerRoot = null;
                            s_targetVariant = null;
                        }
                    }
                    else
                    {
                        GUILayout.Label("No target selected!", EditorStyles.helpBox);
                    }
                }

                GUI.DragWindow();
            }
            EditorGUIExtended.EndBorderLayoutVertical();
        }
    }
}