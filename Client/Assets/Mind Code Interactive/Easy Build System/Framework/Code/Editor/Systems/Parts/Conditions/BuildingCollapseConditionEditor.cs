/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollapseConditionEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingCollapseCondition))]
    public class BuildingCollapseConditionEditor : BaseInspectorEditor<BuildingCollapseCondition>
    {
        private ReorderableList.Code.Editor.ReorderableList m_requireTypesList;

        protected override void OnInspectorEnable()
        {
            SerializedProperty requireTypesProperty = Properties.Get("m_requireTypes");
            m_requireTypesList = new ReorderableList.Code.Editor.ReorderableList(requireTypesProperty, false);

            SetDebugEnabled(true);
        }

        protected override void OnInspectorDisable()
        {
            m_requireTypesList = null;

            SetDebugEnabled(false);
        }

        private void SetDebugEnabled(bool enabled)
        {
            foreach (Object target in targets)
            {
                BuildingCollapseCondition condition = target as BuildingCollapseCondition;
                if (condition != null && condition.EnableDebug != enabled)
                {
                    condition.EnableDebug = enabled;
                }
            }
        }

        protected override void OnInspectorDraw()
        {
            DrawSupportSection();
            DrawFallingSection();
            DrawDebugSection();
        }

        private void DrawSupportSection()
        {
            EditorGUIExtended.Separator("Support Settings", false);

            Properties.Draw("m_requireStablePlacement",
                new GUIContent("Require Stable Placement", "If enabled, the part must have valid support to be considered stable and placeable."));
            Properties.Draw("m_boundsPosition",
                new GUIContent("Bounds Position", "Local position offset of the support detection box relative to part transform."));
            Properties.Draw("m_boundsSize",
                new GUIContent("Bounds Size", "Dimensions of the support detection box used for stability checks."));
            Properties.Draw("m_supportLayerMask",
                new GUIContent("Support Layer Mask", "Physics layers considered as valid support surfaces."));

            Properties.Draw("m_requireAnyColliderSupport",
                new GUIContent("Require Any Collider Support", "If enabled, any collider in Support Layer Mask counts as support (even non-BuildingPart objects)."));

            if (!Properties.Get("m_requireAnyColliderSupport").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    Properties.Draw("m_requireBuildingPart",
                        new GUIContent("Require Building Part", "If enabled, support must come from another BuildingPart (optionally filtered by required types)."));

                    if (Target.RequireBuildingPart)
                    {
                        m_requireTypesList?.Layout();
                    }
                }
            }
        }

        private void DrawFallingSection()
        {
            EditorGUIExtended.Separator("Falling Settings");
            Properties.Draw("m_fallPhysicsTime",
                new GUIContent("Fall Physics Time", "Duration (seconds) before falling part is destroyed."));
            Properties.Draw("m_fallPhysicsMass",
                new GUIContent("Fall Physics Mass", "Rigidbody mass applied when part enters falling state."));
            Properties.Draw("m_fallPhysicsDrag",
                new GUIContent("Fall Physics Drag", "Rigidbody drag applied when part enters falling state."));
            Properties.Draw("m_fallPhysicMaterial",
                new GUIContent("Fall Physic Material", "Physics material applied to colliders during falling (optional)."));
        }

        private void DrawDebugSection()
        {
            if (Application.isPlaying)
            {
                EditorGUIExtended.Separator("Runtime Status");
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Toggle(new GUIContent("Has Support", "True if the last stability check detected valid support."), Target.HasSupport);
                    EditorGUILayout.Toggle(new GUIContent("Is Falling", "True if the part has entered falling state."), Target.IsFalling);
                }
            }

            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Enable verbose debug logs for this condition."));
            Properties.Draw("m_debugFlags", new GUIContent("Debug Draw Flags", "Where the collapse bounds gizmos are allowed to draw."));
        }
    }
}