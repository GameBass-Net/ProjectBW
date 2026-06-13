/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainConditionEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Conditions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingTerrainCondition))]
    public class BuildingTerrainConditionEditor : BaseInspectorEditor<BuildingTerrainCondition>
    {
        protected override void OnInspectorEnable()
        {
            SetDebugEnabled(true);
        }

        protected override void OnInspectorDisable()
        {
            SetDebugEnabled(false);
        }

        private void SetDebugEnabled(bool enabled)
        {
            foreach (Object target in targets)
            {
                BuildingTerrainCondition condition = target as BuildingTerrainCondition;
                if (condition != null && condition.EnableDebug != enabled)
                {
                    condition.EnableDebug = enabled;
                }
            }
        }

        protected override void OnInspectorDraw()
        {
            DrawTreesProximitySection();
            DrawTexturesSection();
            DrawDebugSection();
        }

        private void DrawTreesProximitySection()
        {
            Properties.Draw("m_checkTreesProximity", new GUIContent("Check Trees Proximity", "Enable detection of nearby trees blocking placement."));

            if (Properties.Get("m_checkTreesProximity").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    Properties.Draw("m_treesDetectionRadius", new GUIContent("Detection Radius", "Distance to detect trees around this building part."));
                    Properties.Draw("m_treesDeniedIndex", new GUIContent("Denied Tree Prototypes", "Tree prototypes that will block placement."));
                }
            }
        }

        private void DrawTexturesSection()
        {
            Properties.Draw("m_checkTextures", new GUIContent("Check Textures", "Enable terrain texture validation for placement."));

            if (Properties.Get("m_checkTextures").boolValue)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    Properties.Draw("m_deniedTextures", new GUIContent("Denied Textures", "Terrain textures that will block placement."));
                }
            }
        }

        private void DrawDebugSection()
        {
            EditorGUIExtended.Separator("Debug Settings");
            Properties.Draw("m_showLogs", new GUIContent("Show Logs", "Display debug information in the console."));
            Properties.Draw("m_debugFlags", new GUIContent("Debug Draw Flags", "Where the terrain condition gizmos are allowed to draw."));
        }
    }
}