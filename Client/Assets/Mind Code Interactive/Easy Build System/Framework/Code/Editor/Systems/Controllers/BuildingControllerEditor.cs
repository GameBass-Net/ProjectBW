/// <summary>
/// Project : Easy Build System
/// Class : BuildingControllerEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers
{
    [CustomEditor(typeof(BuildingController))]
    public class BuildingControllerEditor : BaseInspectorEditor<BuildingController>
    {
        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable();
            ValidateComponents();
        }

        protected override void OnInspectorDraw()
        {
#if PRO_BUILD_SYSTEM
            ProUpgradeUtility.DrawControllerUpgradeBanner(Target);
#endif

            EditorGUIExtended.InspectorHeader(target,
                "Manages the logic of building states, views and Building Part selection.\n" +
                "Dispatches input actions to the active building state.\n" +
                "See the documentation for more information about this component.");

            DrawInputSection();
            DrawViewsSection();
            DrawStatesSection();
            DrawAudioSection();
            DrawCustomSections();

            EditorGUIExtended.InspectorBottom();
        }

        protected virtual void DrawCustomSections() { }

        private void DrawInputSection()
        {
            BuildingInput inputHandlerComponent = Target.GetComponent<BuildingInput>();

            EditorGUIExtended.DrawExpandableSection("Input Settings", "gamepad",
                "Configure the input bindings that each building state listens to.",
                () =>
                {
                    if (inputHandlerComponent != null)
                    {
                        UnityEditor.Editor inputEditor = GetOrCreateEditor(inputHandlerComponent);
                        inputEditor?.OnInspectorGUI();
                    }
                    else
                    {
                        EditorGUIExtended.HelpBox("BuildingInput component is required.", EditorGUIElements.MessageType.Error);
                    }
                }, false, true);
        }

        private void DrawViewsSection()
        {
            EditorGUIExtended.DrawExpandableSection("Views Settings", "camera",
                "Configure the camera views used to raycast and detect building targets in the scene.",
                () => BuildingControllerViewsEditor.Draw(Target, GetOrCreateEditor),
                false, true);
        }

        private void DrawStatesSection()
        {
            EditorGUIExtended.DrawExpandableSection("States Settings", "state",
                "Configure the building states that handle the logic for each building mode.",
                () => BuildingControllerStatesEditor.Draw(Target, GetOrCreateEditor),
                false, true);
        }

        private void DrawAudioSection()
        {
            EditorGUIExtended.DrawExpandableSection("Audio Settings", "audio",
                "Configure the audio clips played in response to building actions and mode transitions.",
                () =>
                {
                    EditorGUIExtended.Separator("Placement Audio", false);
                    using (EditorGUIExtended.IndentScope(1))
                    {
                        Properties.Draw("m_audioData.m_enterPlacementModeClip", new GUIContent("Enter Mode", "Played when entering placement mode."));
                        Properties.Draw("m_audioData.m_exitPlacementModeClip", new GUIContent("Exit Mode", "Played when exiting placement mode."));
                        Properties.Draw("m_audioData.m_placementValidClip", new GUIContent("Success", "Played when a part is successfully placed."));
                        Properties.Draw("m_audioData.m_placementFailedClip", new GUIContent("Failed", "Played when placement is blocked by a condition."));
                        Properties.Draw("m_audioData.m_cancelPlacementClip", new GUIContent("Cancelled", "Played when placement is cancelled."));
                    }

                    EditorGUIExtended.Separator("Destruction Audio");
                    using (EditorGUIExtended.IndentScope(1))
                    {
                        Properties.Draw("m_audioData.m_enterDestructionModeClip", new GUIContent("Enter Mode", "Played when entering destruction mode."));
                        Properties.Draw("m_audioData.m_exitDestructionModeClip", new GUIContent("Exit Mode", "Played when exiting destruction mode."));
                        Properties.Draw("m_audioData.m_destructionValidClip", new GUIContent("Success", "Played when a part is successfully destroyed."));
                        Properties.Draw("m_audioData.m_destructionFailedClip", new GUIContent("Failed", "Played when destruction is blocked by a condition."));
                        Properties.Draw("m_audioData.m_cancelDestructionClip", new GUIContent("Cancelled", "Played when destruction is cancelled."));
                    }

                    EditorGUIExtended.Separator("Adjustment Audio");
                    using (EditorGUIExtended.IndentScope(1))
                    {
                        Properties.Draw("m_audioData.m_enterAdjustmentModeClip", new GUIContent("Enter Mode", "Played when entering adjustment mode."));
                        Properties.Draw("m_audioData.m_exitAdjustmentModeClip", new GUIContent("Exit Mode", "Played when exiting adjustment mode."));
                        Properties.Draw("m_audioData.m_adjustmentValidClip", new GUIContent("Success", "Played when a part is successfully adjusted."));
                        Properties.Draw("m_audioData.m_adjustmentFailedClip", new GUIContent("Failed", "Played when adjustment is blocked by a condition."));
                        Properties.Draw("m_audioData.m_cancelAdjustmentClip", new GUIContent("Cancelled", "Played when adjustment is cancelled."));
                    }

                    EditorGUIExtended.Separator("Upgrade Audio");
                    using (EditorGUIExtended.IndentScope(1))
                    {
                        Properties.Draw("m_audioData.m_enterUpgradeModeClip", new GUIContent("Enter Mode", "Played when entering upgrade mode."));
                        Properties.Draw("m_audioData.m_exitUpgradeModeClip", new GUIContent("Exit Mode", "Played when exiting upgrade mode."));
                        Properties.Draw("m_audioData.m_upgradeValidClip", new GUIContent("Success", "Played when a part is successfully upgraded."));
                        Properties.Draw("m_audioData.m_upgradeFailedClip", new GUIContent("Failed", "Played when upgrade is blocked by a condition."));
                        Properties.Draw("m_audioData.m_cancelUpgradeClip", new GUIContent("Cancelled", "Played when upgrade is cancelled."));
                    }
                }, false, true);
        }

        private void ValidateComponents()
        {
            if (Target.BuildingInput != null)
            {
                Target.BuildingInput.hideFlags = HideFlags.HideInInspector;
            }

            bool dirty = false;

            if (Target.States != null)
            {
                BuildingState[] cleanedStates = Target.States.Where(s => s != null).ToArray();
                if (cleanedStates.Length != Target.States.Length) { Target.States = cleanedStates; dirty = true; }
            }

            if (Target.Views != null)
            {
                BuildingView[] cleanedViews = Target.Views.Where(v => v != null).ToArray();
                if (cleanedViews.Length != Target.Views.Length) { Target.Views = cleanedViews; dirty = true; }
            }

            dirty |= CleanupComponents(
                Target.GetComponents<BuildingState>(),
                new HashSet<BuildingState>((Target.States ?? Array.Empty<BuildingState>()).Where(s => s != null)));

            dirty |= CleanupComponents(
                Target.GetComponents<BuildingView>(),
                new HashSet<BuildingView>((Target.Views ?? Array.Empty<BuildingView>()).Where(v => v != null)));

            if (dirty)
            {
                EditorUtility.SetDirty(Target);
            }
        }

        private static bool CleanupComponents<T>(T[] all, HashSet<T> registered) where T : MonoBehaviour
        {
            bool dirty = false;
            foreach (T component in all)
            {
                if (registered.Contains(component))
                {
                    component.hideFlags = HideFlags.HideInInspector;
                }
                else
                {
                    DestroyImmediate(component, true);
                    dirty = true;
                }
            }
            return dirty;
        }

        internal static void ResetComponent(MonoBehaviour componentToReset)
        {
            if (componentToReset == null)
            {
                return;
            }

            GameObject temporaryGameObject = new GameObject("Temp");
            temporaryGameObject.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                MonoBehaviour temporaryComponentForReset = temporaryGameObject.AddComponent(componentToReset.GetType()) as MonoBehaviour;
                if (temporaryComponentForReset != null)
                {
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(temporaryComponentForReset), componentToReset);
                    EditorUtility.SetDirty(componentToReset);
                }
            }
            finally
            {
                DestroyImmediate(temporaryGameObject);
            }
        }
    }
}