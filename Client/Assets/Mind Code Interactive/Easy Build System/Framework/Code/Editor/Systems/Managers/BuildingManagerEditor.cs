/// <summary>
/// Project : Easy Build System
/// Class : BuildingManagerEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
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

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    [CustomEditor(typeof(BuildingManager))]
    public class BuildingManagerEditor : BaseInspectorEditor<BuildingManager>
    {
        protected readonly List<Action> m_sections = new List<Action>();

        protected override void OnEnable()
        {
            base.OnEnable();
            m_sections.Clear();
            RegisterSections();
        }

        protected override void OnInspectorEnable()
        {
            if (Target.GlobalRules == null)
            {
                return;
            }

            RegisterChildrenListAccessor(managerTarget => managerTarget.GlobalRules.Cast<UnityEngine.Object>().ToList());

            BuildingRulesEditor.HideInInspector(Target.GlobalRules);
        }

        protected virtual void RegisterSections()
        {
            m_sections.Add(DrawGeneralSection);
            m_sections.Add(DrawGridSection);
            m_sections.Add(DrawPhysicsSection);
            m_sections.Add(DrawGroupingSection);
            m_sections.Add(DrawBatchingSection);
            m_sections.Add(DrawSavingSection);
            m_sections.Add(DrawRulesSection);
        }

        protected override void OnInspectorDraw()
        {
#if PRO_BUILD_SYSTEM
            ProUpgradeUtility.DrawManagerUpgradeBanner(Target);
#endif

            EditorGUIExtended.InspectorHeader(target,
                "Registers and manages all Building Parts, Sockets, Areas and Groups in the scene.\n" +
                "Exposes a registry for querying registered components by type, ID or proximity.\n" +
                "Handles the Building Physics, Batching, Grouping and Terrain sub-systems.\n" +
                "See the documentation for more information about this component.");

            foreach (Action section in m_sections)
            {
                section?.Invoke();
            }

            DrawDebugSection();

            EditorGUIExtended.InspectorBottom();
        }

        protected virtual void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure socket detection and core manager behavior.",
                () =>
                {
                    EditorGUIExtended.Separator("Building Socket Settings", false);

                    Properties.Draw("m_socketDetectionType",
                        new GUIContent("Socket Detection Type", "Method used to detect sockets during placement."));

                    using (EditorGUIExtended.IndentScope())
                    {
                        Properties.Draw("m_socketLayer",
                            new GUIContent("Socket Layer", "Layer used for socket detection raycasts."));
                    }
                }, false, true);
        }

        protected virtual void DrawGridSection()
        {
            EditorGUIExtended.DrawExpandableSection("Grid Settings", "grid",
                "Configure the cell-based grid system for snapping Building Parts during placement.",
                () => BuildingGridEditor.Draw(Properties, serializedObject, Target));
        }

        protected virtual void DrawPhysicsSection()
        {
            EditorGUIExtended.DrawExpandableSection("Physics Settings", "bounce",
                "Centralized physics system that batches stability checks across all Building Parts to optimize performance.",
                () => BuildingPhysicsEditor.Draw(Properties, serializedObject, Target));
        }

        protected virtual void DrawGroupingSection()
        {
            EditorGUIExtended.DrawExpandableSection("Grouping Settings", "pack_group",
                "Automatically assigns placed Building Parts to proximity-based groups.",
                () => BuildingGroupingEditor.Draw(Properties));
        }

        protected virtual void DrawBatchingSection()
        {
            EditorGUIExtended.DrawExpandableSection("Batching Settings", "performance",
                "Combines Building Parts within a group into a single mesh to reduce draw calls.",
                () => BuildingBatchingEditor.Draw(Properties));
        }

        protected virtual void DrawSavingSection()
        {
            EditorGUIExtended.DrawExpandableSection("Saving Settings", "save",
                "Configure the save system, storage provider and auto-save behavior.",
                () => BuildingSaveEditor.Draw(Properties, serializedObject, Target));
        }

        protected virtual void DrawRulesSection()
        {
            EditorGUIExtended.DrawExpandableSection("Rules Settings", "state",
                "Configure rules that apply globally to all building actions, regardless of any area.",
                () => BuildingRulesEditor.Draw(Target, Target.GlobalRules, GetOrCreateEditor));
        }

        protected virtual void DrawDebugSection()
        {
            EditorGUIExtended.DrawExpandableSection("Debug Settings", "cache",
                "Inspect the current manager state, registered objects and refresh the Building Part registry.",
                () =>
                {
                    EditorGUIExtended.Separator("Building Manager Statistics", false);
                    EditorGUILayout.LabelField("Total Registered Areas :", Target.GetRegisteredAreas.Count.ToString());
                    EditorGUILayout.LabelField("Total Registered Parts :", Target.GetRegisteredParts.Count.ToString());
                    EditorGUILayout.LabelField("Total Registered Sockets :", Target.GetRegisteredSockets.Count.ToString());
                    EditorGUILayout.LabelField("Total Registered Groups :", Target.GetRegisteredGroups.Count.ToString());

                    SerializedObject registrySerializedObject = new SerializedObject(BuildingPartRegistry.Instance);
                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        using (EditorGUIExtended.IndentScope(1))
                        {
                            EditorGUILayout.PropertyField(registrySerializedObject.FindProperty("m_partReferences"),
                                new GUIContent("Registered Part References", "All Building Parts available in the project registry."), true);
                        }
                    }

                    GUILayout.Space(3f);
                    if (GUILayout.Button("Refresh Building Parts Registry..."))
                    {
                        BuildingPartRegistry.Instance.RefreshRegistry();
                    }
                });
        }
    }
}