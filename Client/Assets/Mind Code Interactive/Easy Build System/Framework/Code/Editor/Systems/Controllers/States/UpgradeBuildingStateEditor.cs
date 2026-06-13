/// <summary>
/// Project : Easy Build System
/// Class : UpgradeBuildingStateEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.States
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.States
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UpgradeBuildingState), true)]
    public class UpgradeBuildingStateEditor : BaseInspectorEditor<UpgradeBuildingState>
    {
        protected override void OnInspectorDraw()
        {
            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorHeader(target, "Enables the upgrade of Building Parts that are already placed.");
            }

            EditorGUIExtended.Separator("Base State Settings", false);

            Properties.Draw("m_cancelStateAfterValidation", new GUIContent("Cancel State After Validation", "When enabled, automatically exits upgrade mode after successfully upgrading an object."));

            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable logging for upgrade checks."));

            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorBottom();
            }
        }
    }
}