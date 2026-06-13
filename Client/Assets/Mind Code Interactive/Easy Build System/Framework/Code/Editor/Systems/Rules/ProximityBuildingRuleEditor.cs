/// <summary>
/// Project : Easy Build System
/// Class : ProximityBuildingRuleEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Rules
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Rules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ProximityBuildingRule))]
    public class ProximityBuildingRuleEditor : BaseInspectorEditor<ProximityBuildingRule>
    {
        protected override void OnInspectorDraw()
        {
            using (EditorGUIExtended.IndentScope())
            {
                Properties.Draw("m_placementSettings", new GUIContent("Placement Settings", "Proximity constraints applied during placement."));
                Properties.Draw("m_destructionSettings", new GUIContent("Destruction Settings", "Proximity constraints applied during destruction."));
                Properties.Draw("m_adjustmentSettings", new GUIContent("Adjustment Settings", "Proximity constraints applied during adjustment."));
            }
        }
    }
}
