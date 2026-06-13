/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartPresetEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets
{
    [CustomEditor(typeof(BuildingPartPreset))]
    public sealed class BuildingPartPresetEditor : BaseInspectorEditor<BuildingPartPreset>
    {
        private ReorderableList.Code.Editor.ReorderableList m_behaviorsList;
        private ReorderableList.Code.Editor.ReorderableList m_conditionsList;
        private ReorderableList.Code.Editor.ReorderableList m_socketsList;

        protected override void OnInspectorEnable()
        {
            m_behaviorsList = new ReorderableList.Code.Editor.ReorderableList(Properties.Get("m_behaviorsData"), false);
            m_conditionsList = new ReorderableList.Code.Editor.ReorderableList(Properties.Get("m_conditionsData"), false);
            m_socketsList = new ReorderableList.Code.Editor.ReorderableList(Properties.Get("m_socketsData"), false);
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "Stores building part configuration including placement settings, behaviors, conditions and socket data.");

            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure preset name, category and source prefab.",
                () =>
                {
                    Properties.Draw("m_presetName", new GUIContent("Preset Name", "Display name for this preset."));
                    Properties.Draw("m_category", new GUIContent("Category", "Category this preset belongs to."));
                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        Properties.Draw("m_sourcePrefab", new GUIContent("Source Prefab", "Original prefab this preset was created from."));
                    }
                }, false, true);

            EditorGUIExtended.DrawExpandableSection("Cached Settings", "cache",
                "Manage placement settings, behaviors, conditions and sockets cached data.",
                () =>
                {
                    using (EditorGUIExtended.IndentScope())
                    {
                        Properties.Draw("m_placementSettings", new GUIContent("Placement Settings", "Cached placement configuration."));
                    }

                    GUILayout.Space(3f);
                    m_behaviorsList.Layout();
                    m_conditionsList.Layout();
                    m_socketsList.Layout();
                }, false, true);

            EditorGUIExtended.InspectorBottom();
        }
    }
}