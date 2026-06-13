/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingPhysicsEditor
    {
        public static void Draw(PropertyCollection properties, SerializedObject serializedObject, BuildingManager target)
        {
            EditorGUI.BeginChangeCheck();

            properties.Draw("m_physicsSettings.m_enablePhysics",
                new GUIContent("Enable Physics", "Enables the centralized physics system for all collapse condition checks."));

            if (!properties.Get("m_physicsSettings.m_enablePhysics").boolValue)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                return;
            }

            using (EditorGUIExtended.IndentScope())
            {
                properties.Draw("m_physicsSettings.m_checkInterval",
                    new GUIContent("Check Interval", "Time in seconds between each batch of physics checks. Lower = more responsive but heavier."));

                properties.Draw("m_physicsSettings.m_maxChecksPerFrame",
                    new GUIContent("Max Checks Per Frame", "Maximum number of parts checked per frame. The system cycles through all registered parts over multiple frames. Higher = faster full coverage but more CPU per frame."));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}