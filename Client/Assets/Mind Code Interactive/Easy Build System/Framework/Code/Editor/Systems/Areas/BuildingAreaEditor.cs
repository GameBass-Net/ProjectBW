/// <summary>
/// Project : Easy Build System
/// Class : BuildingAreaEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Areas
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Areas
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingArea))]
    public class BuildingAreaEditor : BaseInspectorEditor<BuildingArea>
    {
        protected override void OnInspectorEnable()
        {
            if (Target.BuildingRules == null)
            {
                return;
            }

            RegisterChildrenListAccessor(areaTarget => areaTarget.BuildingRules.Cast<UnityEngine.Object>().ToList());

            BuildingRulesEditor.HideInInspector(Target.BuildingRules);
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "Defines an area in the scene where custom building rules are applied to Building Parts.\n" +
                "Any building action performed within its bounds is evaluated against a set of configurable Building Rules.\n" +
                "See the documentation for more information about this component.");

            DrawGeneralSection();
            DrawRulesSection();
            DrawDebugSection();

            EditorGUIExtended.InspectorBottom();
        }

        protected virtual void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure the area settings used to detect parts and apply rules.",
                () =>
                {
                    Properties.Draw("m_areaType", new GUIContent("Area Type", "Identifier used to tag and differentiate this area from others."));
                    Properties.Draw("m_areaPriority", new GUIContent("Area Priority", "Determines which area takes precedence when multiple areas overlap. Higher values win."));
                    Properties.Draw("m_areaShapeType", new GUIContent("Shape Type", "Defines the shape used to detect parts inside this area."));

                    if (Properties.Get("m_areaShapeType").enumValueIndex == (int)BuildingArea.ShapeType.Sphere)
                    {
                        Properties.Draw("m_areaSphereRadius", new GUIContent("Sphere Radius", "Radius of the spherical detection area."));
                    }
                    else
                    {
                        Properties.Draw("m_areaBounds", new GUIContent("Bounds Size", "Size of the box-shaped detection area."));
                    }

                    Properties.Draw("m_areaInclusionMode", new GUIContent("Inclusion Mode", "Defines whether a part must be fully inside the area or can partially overlap it."));
                }, false, true);
        }

        protected virtual void DrawRulesSection()
        {
            EditorGUIExtended.DrawExpandableSection("Rules Settings", "state",
                "Configure the rules that control and limit what can be built in this area.",
                () => BuildingRulesEditor.Draw(Target, Target.BuildingRules, GetOrCreateEditor));
        }

        protected virtual void DrawDebugSection()
        {
            EditorGUIExtended.DrawExpandableSection("Debug Settings",
                "cache",
                "View component state, statistics and manage debug rendering settings.",
                () =>
                {
                    EditorGUIExtended.Separator("Unique Object Statistics", false);
                    EditorGUILayout.LabelField("Unique ID :", Target.UniqueId);
                    EditorGUILayout.LabelField("Is Registered :", Target.IsRegistered ? "True" : "False");

                    EditorGUIExtended.Separator("Building Area Statistics");
                    EditorGUILayout.LabelField("Registered Parts :", Target.RegisteredParts.Count.ToString());

                    EditorGUIExtended.Separator("Rendering Settings");
                    Properties.Draw("m_debugFlags",
                        new GUIContent("Debug Draw Flags", "Where the area boundaries are allowed to draw."));
                }, false, false);
        }
    }
}