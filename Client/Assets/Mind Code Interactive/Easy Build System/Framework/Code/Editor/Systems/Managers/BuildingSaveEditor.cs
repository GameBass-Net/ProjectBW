/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingSaveEditor
    {
        public static void Draw(PropertyCollection properties, SerializedObject serializedObject, BuildingManager target)
        {
            EditorGUI.BeginChangeCheck();

            properties.Draw("m_saveSettings.m_enableSaving",
                new GUIContent("Enable Saving", "Enables the save system for placed Building Parts."));

            if (!properties.Get("m_saveSettings.m_enableSaving").boolValue)
            {
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                return;
            }

            using (EditorGUIExtended.IndentScope())
            {
                properties.Draw("m_saveSettings.m_saveProvider",
                    new GUIContent("Save Provider", "Storage backend used to read and write save data."));
                properties.Draw("m_saveSettings.m_saveMode",
                    new GUIContent("Save Mode", "Defines when and how Building Parts are saved."));
                properties.Draw("m_saveSettings.m_autoSave",
                    new GUIContent("Auto Save", "Automatically saves at a regular interval while in play mode."));

                if (properties.Get("m_saveSettings.m_autoSave").boolValue)
                {
                    using (EditorGUIExtended.IndentScope())
                    {
                        properties.Draw("m_saveSettings.m_autoSaveInterval",
                            new GUIContent("Auto Save Interval", "Time in seconds between each automatic save."));

                        SerializedProperty autoSaveIntervalProperty = properties.Get("m_saveSettings.m_autoSaveInterval");
                        if (autoSaveIntervalProperty.floatValue < 1f)
                        {
                            autoSaveIntervalProperty.floatValue = 1f;
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUIExtended.Separator();

            EditorGUIExtended.DrawExpandableSection("Save Slot", "",
                string.Empty,
                () =>
                {
                    if (!target.SaveSystem.HasSaveData())
                    {
                        EditorGUILayout.Separator();
                        EditorGUIExtended.Label("This slot is empty. Use 'Save Buildings' to create a save.",
                            EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
                        EditorGUILayout.Separator();
                        return;
                    }

                    DrawSlotMetadata(target);
                    EditorGUIExtended.Separator();
                    DrawSlotActions(target);
                },
                false);
        }

        private static void DrawSlotMetadata(BuildingManager target)
        {
            BuildingSaveMetaData saveMetaData = target.SaveSystem.GetSaveMetaData();

            if (saveMetaData == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Building Parts :", saveMetaData.BuildingCount.ToString());
            EditorGUILayout.LabelField("Scene Name :", saveMetaData.SceneName);
            EditorGUILayout.LabelField("Last Saved :", saveMetaData.SaveTime.ToString("yyyy-MM-dd HH:mm"));
            EditorGUILayout.LabelField("Version :", saveMetaData.SaverVersion);
        }

        private static void DrawSlotActions(BuildingManager target)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete Save"))
            {
                if (!EditorUtility.DisplayDialog("Delete Save Data",
                    "Delete all save data?\nThis action cannot be undone.",
                    "Delete", "Cancel"))
                {
                    return;
                }

                target.SaveSystem.DeleteSave();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}