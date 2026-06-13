/// <summary>
/// Project : Easy Build System
/// Class : BuildingCommonConditionEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingCommonCondition))]
    public class BuildingCommonConditionEditor : BaseInspectorEditor<BuildingCommonCondition>
    {
        protected override void OnInspectorDraw()
        {
            DrawPlacementSection();
            DrawDebugSection();
        }

        private void DrawPlacementSection()
        {
            Properties.Draw("m_enablePlacement", new GUIContent("Enable Placement", "Allow this building part to be placed in the scene."));
            Properties.Draw("m_enableDestruction", new GUIContent("Enable Destruction", "Allow this building part to be destroyed after placement."));
            Properties.Draw("m_enableAdjustment", new GUIContent("Enable Adjustment", "Allow this building part to be adjusted after placement."));
            Properties.Draw("m_enableUpgrading", new GUIContent("Enable Upgrading", "Allow this building part to be upgraded to other variants."));
            Properties.Draw("m_requireSocket", new GUIContent("Require Socket", "This building part must be attached to a socket to be placed."));
            Properties.Draw("m_ignoreAreas", new GUIContent("Ignore Areas", "Ignore building area restrictions for this building part."));
        }

        private void DrawDebugSection()
        {
            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Display debug information in the console."));
        }
    }
}