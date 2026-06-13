/// <summary>
/// Project : Easy Build System
/// Class : AdjustmentBuildingStateEditor.cs
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
    [CustomEditor(typeof(AdjustmentBuildingState), true)]
    public class AdjustmentBuildingStateEditor : BaseInspectorEditor<AdjustmentBuildingState>
    {
        protected override void OnInspectorDraw()
        {
            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorHeader(target,
                    "Enables the adjustment of Building Parts that are already placed, either individually or in groups.");
            }

            EditorGUIExtended.Separator("Base State Settings", false);

            Properties.Draw("m_cancelStateAfterValidation", new GUIContent("Cancel State After Validation",
                "When enabled, automatically exits adjustment mode after successfully adjusting an object."));

            EditorGUIExtended.Separator("Adjustment Settings");

            Properties.Draw("m_resetRotationOnAdjust", new GUIContent("Reset Rotation On Adjust",
                "When enabled, resets the rotation to identity when starting an adjustment instead of keeping the current rotation."));

            EditorGUIExtended.Separator("Debug Settings");

            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable logging for adjustment checks."));

            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorBottom();
            }
        }
    }
}