/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Groups
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Groups
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingGroup))]
    public class BuildingGroupEditor : BaseInspectorEditor<BuildingGroup>
    {
        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "Defines a group in the scene where Building Parts are organized together by proximity.\n" +
                "Updated automatically as Building Parts are placed or destroyed in the scene.\n" +
                "See the documentation for more information about this component.");

            DrawGeneralSection();
            DrawDebugSection();

            EditorGUIExtended.InspectorBottom();
        }

        protected virtual void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure the group behavior and view the Building Parts assigned to this group.",
                () =>
                {
                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        Properties.Draw("m_uniqueId", new GUIContent("Building Unique ID", "Unique identifier assigned to this group."));
                        using (EditorGUIExtended.IndentScope())
                        {
                            Properties.DrawArray("m_groupedParts", new GUIContent("Grouped Building Parts", "All Building Parts currently assigned to this group."));
                        }
                    }

                    Properties.Draw("m_dontDestroyGroup", new GUIContent("Dont Destroy Group", "Keep this group alive when it has no remaining Building Parts."));
                },
                false,
                true);
        }

        protected virtual void DrawDebugSection()
        {
            EditorGUIExtended.DrawExpandableSection("Debug Settings", "cache",
                "Inspect the current group state, statistics and configure debug rendering options.",
                () =>
                {
                    EditorGUIExtended.Separator("Unique Object Statistics", false);
                    EditorGUILayout.LabelField("Unique ID :", Target.UniqueId.ToString());
                    EditorGUILayout.LabelField("Is Registered :", Target.IsRegistered.ToString());

                    EditorGUIExtended.Separator("Building Group Statistics");
                    EditorGUILayout.LabelField("Is Dynamic :", Target.IsDynamic.ToString());
                    EditorGUILayout.LabelField("Is Batched :", Target.IsBatched.ToString());
                    EditorGUILayout.LabelField("Total Building Parts :", Target.GroupedParts.Count.ToString());

                    EditorGUIExtended.Separator("Rendering Settings");

                    Properties.Draw("m_debugFlags",
                        new GUIContent("Debug Draw Flags", "Controls where group boundaries are rendered in the editor."));
                },
                false,
                false);
        }
    }
}