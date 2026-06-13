/// <summary>
/// Project : Easy Build System
/// Class : PlacementBuildingStateEditor.cs
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
    [CustomEditor(typeof(PlacementBuildingState), true)]
    public class PlacementBuildingStateEditor : BaseInspectorEditor<PlacementBuildingState>
    {
        protected override void OnInspectorDraw()
        {
            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorHeader(target,
                    "Enables the placement of new Building Parts in the scene, either individually or in groups.");
            }

            EditorGUIExtended.Separator("Base State Settings", false);

            Properties.Draw("m_cancelStateAfterValidation", new GUIContent("Cancel State After Validation", "Automatically exits placement mode after placing an object."));

            EditorGUIExtended.Separator("Placement Settings");
            Properties.Draw("m_snapOnlyIfValid", new GUIContent("Snap Only If Valid", "Only snap to sockets when placement conditions (e.g. collisions) are valid."));
            Properties.Draw("m_avoidOccupiedCells", new GUIContent("Avoid Occupied Cells", "Prevents placing objects in cells that are already occupied."));
            Properties.Draw("m_lockRotation", new GUIContent("Lock Rotation", "Prevents rotation of objects during placement."));
            Properties.Draw("m_preservePreviewRotation", new GUIContent("Preserve Preview Rotation", "Keeps the last preview rotation and reuses it for newly created previews."));
            Properties.Draw("m_invertPreviewRotation", new GUIContent("Invert Preview Rotation", "Applies a 180° Y rotation to newly created previews (when preserve rotation isn't taking precedence)."));

            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable logging for placement checks."));

            if (Selection.activeObject == Target)
            {
                EditorGUIExtended.InspectorBottom();
            }
        }
    }
}