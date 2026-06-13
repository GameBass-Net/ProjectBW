/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupingEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingGroupingEditor
    {
        public static void Draw(PropertyCollection properties)
        {
            properties.Draw("m_groupingSettings.m_enablePartsGrouping",
                new GUIContent("Enable Grouping", "Automatically assigns placed Building Parts to proximity-based groups."));

            if (properties.Get("m_groupingSettings.m_enablePartsGrouping").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    properties.Draw("m_groupingSettings.m_groupPartNeighborDistance",
                        new GUIContent("Neighbor Distance", "Maximum distance between two Building Parts to be considered part of the same group."));

                    properties.Draw("m_groupingSettings.m_defaultPivotMode",
                        new GUIContent("Default Pivot Mode", "Pivot mode applied when a group is created or a part is added to a group."));
                }
            }
        }
    }
}