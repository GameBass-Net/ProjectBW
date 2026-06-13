/// <summary>
/// Project : Easy Build System
/// Class : DestructionBuildingStateEditor.cs
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
    [CustomEditor(typeof(DestructionBuildingState), true)]
    public class DestructionBuildingStateEditor : BaseInspectorEditor<DestructionBuildingState>
    {
        protected override void OnInspectorDraw()
        {
            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorHeader(target, "Enables the destruction of Building Parts that are already placed, either individually or in groups.");
            }

            EditorGUIExtended.Separator("Base State Settings", false);

            Properties.Draw("m_cancelStateAfterValidation", new GUIContent("Cancel State After Validation", "When enabled, automatically exits destruction mode after successfully destroying an object."));

            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable logging for destruction checks."));

            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorBottom();
            }
        }
    }
}