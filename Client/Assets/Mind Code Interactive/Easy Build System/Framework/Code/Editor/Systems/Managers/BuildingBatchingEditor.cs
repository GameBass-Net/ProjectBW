/// <summary>
/// Project : Easy Build System
/// Class : BuildingBatchingEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingBatchingEditor
    {
        public static void Draw(PropertyCollection properties)
        {
            bool isGroupingEnabled = properties.Get("m_groupingSettings.m_enablePartsGrouping").boolValue;

            if (!isGroupingEnabled)
            {
                EditorGUIExtended.HelpBox("Batching requires Building Grouping to be enabled.", EditorGUIElements.MessageType.Info);
                properties.Get("m_batchingSettings.m_enableBatching").boolValue = false;
            }

            using (EditorGUIExtended.DisabledScope(!isGroupingEnabled))
            {
                properties.Draw("m_batchingSettings.m_enableBatching",
                    new GUIContent("Enable Batching", "Combines Building Parts within a group into a single mesh to reduce draw calls."));

                if (properties.Get("m_batchingSettings.m_enableBatching").boolValue)
                {
                    using (EditorGUIExtended.IndentScope())
                    {
                        properties.Draw("m_batchingSettings.m_autoBatching",
                            new GUIContent("Auto Batching", "Automatically batches and unbatches groups based on distance to the active build zone."));
                        properties.Draw("m_batchingSettings.m_batchingDistance",
                            new GUIContent("Batching Distance", "Distance threshold within which groups are unbatched during building."));
                    }
                }
            }
        }
    }
}