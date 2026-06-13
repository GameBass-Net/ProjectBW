/// <summary>
/// Project : Easy Build System
/// Class : TypeBuildingRuleEditor.cs
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
    [CustomEditor(typeof(CategoryFilterBuildingRule))]
    public class TypeBuildingRuleEditor : BaseInspectorEditor<CategoryFilterBuildingRule>
    {
        protected override void OnInspectorDraw()
        {
            using (EditorGUIExtended.IndentScope())
            {
                Properties.Draw("m_allowedForPlacement", new GUIContent("Allowed For Placement", "Part categories permitted for placement. Empty = all allowed."));
                Properties.Draw("m_allowedForDestruction", new GUIContent("Allowed For Destruction", "Part categories permitted for destruction. Empty = all allowed."));
                Properties.Draw("m_allowedForAdjustment", new GUIContent("Allowed For Adjustment", "Part categories permitted for adjustment. Empty = all allowed."));
            }
        }
    }
}