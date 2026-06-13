/// <summary>
/// Project : Easy Build System
/// Class : BuildingViewEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
{
    [CustomEditor(typeof(BuildingView), true)]
    public class BuildingViewEditor : BaseInspectorEditor<BuildingView>
    {
        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.Separator("Raycast Settings", false);

            Properties.Draw("m_raycastCamera", new GUIContent("Raycast Camera", "Camera used for raycast operations in this view."));

            EditorGUI.BeginChangeCheck();
            Properties.Draw("m_raycastDistance", new GUIContent("Raycast Distance", "Maximum distance for raycast operations."));
            LayerMask convertedRaycastLayerMask = EditorGUILayout.MaskField(
                new GUIContent("Raycast Layer", "Layer mask that defines which objects can be detected by raycasts."),
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(Target.RaycastLayer),
                InternalEditorUtility.layers
            );
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target, "Change Raycast Layer");
                Target.RaycastLayer = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(convertedRaycastLayerMask);
                EditorUtility.SetDirty(Target);
            }

            Properties.Draw("m_raycastOffset", new GUIContent("Raycast Offset", "Local offset applied to the raycast origin position."));

            EditorGUIExtended.Separator("Distance Settings");

            Properties.Draw("m_constrainValidDistance", new GUIContent("Constrain Distance", "Enable distance constraints between the view origin and the target position."));

            if (Properties.Get("m_constrainValidDistance").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    Properties.Draw("m_minValidDistance", new GUIContent("Min Valid Distance", "Minimum allowed distance from the view origin to the target position."));
                    Properties.Draw("m_maxValidDistance", new GUIContent("Max Valid Distance", "Maximum allowed distance from the view origin to the target position."));
                }
            }

            EditorGUIExtended.Separator("Snap Settings");

            Properties.Draw("m_snapRadius", new GUIContent("Snap Radius", "Radius used to find sockets around the target position."));

            Properties.Draw("m_snapMaxAngle", new GUIContent("Snap Max Angle", "Maximum allowed angle between the view direction and the socket direction."));

            Properties.Draw("m_snapObstructionCheck", new GUIContent("Snap Obstruction Check", "Require a clear line of sight to the socket before snapping."));

            if (Properties.Get("m_snapObstructionCheck").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    EditorGUI.BeginChangeCheck();
                    LayerMask convertedObstructionLayerMask = EditorGUILayout.MaskField(
                        new GUIContent("Snap Obstruction Layers", "Layers considered as obstructing line of sight to sockets."),
                        InternalEditorUtility.LayerMaskToConcatenatedLayersMask(Target.SnapObstructionLayers),
                        InternalEditorUtility.layers
                    );
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target, "Change Obstruction Layers");
                        Target.SnapObstructionLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(convertedObstructionLayerMask);
                        EditorUtility.SetDirty(Target);
                    }
                }
            }
        }
    }
}