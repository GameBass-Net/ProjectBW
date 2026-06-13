/// <summary>
/// Project : Mind Code Interactive
/// Class : HeightBuildingRuleEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Rules
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Rules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HeightBuildingRule))]
    public class HeightBuildingRuleEditor : BaseInspectorEditor<HeightBuildingRule>
    {
        protected override void OnInspectorDraw()
        {
            Properties.Draw("m_placementHeightRange", new GUIContent("Placement Height Range", "Minimum and maximum altitude allowed for placement."));
            Properties.Draw("m_destructionHeightRange", new GUIContent("Destruction Height Range", "Minimum and maximum altitude allowed for destruction."));
            Properties.Draw("m_adjustmentHeightRange", new GUIContent("Adjustment Height Range", "Minimum and maximum altitude allowed for adjustment."));
        }
    }
}