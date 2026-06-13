/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartPresetLinkEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.IO;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartPresetLinkEditor
    {
        public static void Draw(BuildingPart target, PropertyCollection properties, SerializedObject serializedObject)
        {
            if (target == null || properties == null || serializedObject == null)
            {
                return;
            }

            SerializedProperty linkedPresetProperty = properties.Get("m_linkedPreset");
            BuildingPartPreset linkedPreset = linkedPresetProperty.objectReferenceValue as BuildingPartPreset;
            bool hasChanges = linkedPreset != null && linkedPreset.HasChangesFrom(target);
            bool isSource = IsSourcePrefab(target, linkedPreset);

            if (hasChanges)
            {
                EditorGUIExtended.HelpBox(
                    isSource
                        ? "This object has changes compared to the linked preset.\nClick \"Save to Preset\" to update the preset, or \"Revert from Preset\" to restore the preset values."
                        : "This object has overridden preset values.\nClick \"Apply Preset\" to sync back to the preset values.",
                    EditorGUIElements.MessageType.None
                );
            }

            if (isSource)
            {
                EditorGUIExtended.Label("This is the source prefab for this preset.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
                GUILayout.Space(5f);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(linkedPresetProperty, new GUIContent("Linked Preset :"));
            if (EditorGUI.EndChangeCheck())
            {
                BuildingPartPreset newPreset = linkedPresetProperty.objectReferenceValue as BuildingPartPreset;
                bool wasNull = linkedPreset == null;

                serializedObject.ApplyModifiedProperties();

                if (wasNull && newPreset != null)
                {
                    BuildingPartPreset presetToApply = newPreset;
                    EditorApplication.update += WaitForPickerClosed;

                    void WaitForPickerClosed()
                    {
                        if (EditorGUIUtility.GetObjectPickerControlID() == 0)
                        {
                            EditorApplication.update -= WaitForPickerClosed;
                            EditorApplication.delayCall += () => ApplyPreset(target, serializedObject, presetToApply, true);
                        }
                    }
                }
            }

            Texture2D saveIcon = Resources.Load<Texture2D>("Editor/Icons/save");
            Texture2D applyIcon = Resources.Load<Texture2D>("Editor/Icons/apply_file");
            Texture2D revertIcon = Resources.Load<Texture2D>("Editor/Icons/update_file");

            GUILayout.Space(3f);

            if (linkedPreset == null)
            {
                if (GUILayout.Button(new GUIContent(" Save As Preset...", saveIcon), GUILayout.Height(20f)))
                {
                    EditorApplication.delayCall += () => CreateNewPreset(target, serializedObject, linkedPresetProperty);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (isSource)
                {
                    using (EditorGUIExtended.DisabledScope(!hasChanges))
                    {
                        if (GUILayout.Button(new GUIContent(" Save to Preset", saveIcon), GUILayout.Height(20f)))
                        {
                            EditorApplication.delayCall += () => SaveToPreset(target, serializedObject, linkedPreset);
                        }

                        if (GUILayout.Button(new GUIContent(" Revert from Preset", revertIcon), GUILayout.Height(20f)))
                        {
                            EditorApplication.delayCall += () => ApplyPreset(target, serializedObject, linkedPreset, false);
                        }
                    }
                }
                else
                {
                    using (EditorGUIExtended.DisabledScope(!hasChanges))
                    {
                        if (GUILayout.Button(new GUIContent(" Apply Preset", applyIcon), GUILayout.Height(20f)))
                        {
                            EditorApplication.delayCall += () => ApplyPreset(target, serializedObject, linkedPreset, true);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUIExtended.Separator();
        }

        private static bool IsSourcePrefab(BuildingPart target, BuildingPartPreset preset)
        {
            if (preset == null || preset.SourcePrefab == null)
            {
                return false;
            }

            GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(target.gameObject)
                                         ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(target.gameObject);

            return prefabRoot == preset.SourcePrefab || target.gameObject == preset.SourcePrefab;
        }

        private static void CreateNewPreset(BuildingPart target, SerializedObject serializedObject, SerializedProperty linkedPresetProperty)
        {
            string defaultName = target.name + "_Preset";
            string presetPath = EditorUtility.SaveFilePanelInProject("Create Building Preset", defaultName, "asset", "Choose name and location");

            if (string.IsNullOrEmpty(presetPath))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(presetPath);
            string folderPath = Path.GetDirectoryName(presetPath).Replace("\\", "/");

            BuildingPartPreset newPreset = ScriptableObject.CreateInstance<BuildingPartPreset>();
            if (!BuildingPresetHelper.SavePreset(target, newPreset, fileName, folderPath))
            {
                Object.DestroyImmediate(newPreset);
                return;
            }

            newPreset.SourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(target.gameObject)
                                          ?? PrefabUtility.GetCorrespondingObjectFromOriginalSource(target.gameObject)
                                          ?? target.gameObject;

            Undo.RecordObject(target, "Link Preset");
            linkedPresetProperty.objectReferenceValue = newPreset;
            serializedObject.ApplyModifiedProperties();

            Selection.activeObject = newPreset;
            Debug.Log($"Created and linked preset '{fileName}'.");
        }

        private static void SaveToPreset(BuildingPart target, SerializedObject serializedObject, BuildingPartPreset preset)
        {
            bool hasSockets = target.CacheSystem.Sockets != null && target.CacheSystem.Sockets.Length > 0;
            bool includeSockets = false;

            if (hasSockets)
            {
                int choice = EditorUtility.DisplayDialogComplex("Save to Preset", "Do you want to include sockets in the preset?", "With Sockets", "Cancel", "Without Sockets");
                if (choice == 1)
                {
                    return;
                }

                includeSockets = choice == 0;
            }

            Undo.RecordObject(preset, "Save to Preset");
            preset.ConfigureFromPart(target, preset.PresetName, includeSockets);
            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
            serializedObject.Update();
            Debug.Log($"Saved changes to preset '{preset.PresetName}'{(includeSockets ? " (with sockets)" : "")}.");
        }

        private static void ApplyPreset(BuildingPart target, SerializedObject serializedObject, BuildingPartPreset preset, bool applySockets)
        {
            bool presetHasSockets = preset.SocketsData != null && preset.SocketsData.Count > 0;
            string assetPath = AssetDatabase.GetAssetPath(target.gameObject);
            bool isPrefabAsset = !string.IsNullOrEmpty(assetPath);
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target.gameObject);

            if (applySockets && presetHasSockets)
            {
                string message = isPrefabAsset
                    ? "Applying sockets to a prefab asset requires opening the prefab for editing.\nDo you want to include sockets?"
                    : isPrefabInstance
                        ? "Applying sockets to a prefab instance will unpack the prefab.\nDo you want to include sockets?"
                        : "Do you want to include sockets from the preset?";

                int choice = EditorUtility.DisplayDialogComplex("Apply Preset", message, "With Sockets", "Cancel", "Without Sockets");
                if (choice == 1)
                {
                    return;
                }

                applySockets = choice == 0;
            }

            if (isPrefabAsset && applySockets && presetHasSockets)
            {
                using (PrefabUtility.EditPrefabContentsScope editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
                {
                    BuildingPart prefabPart = editingScope.prefabContentsRoot.GetComponent<BuildingPart>();
                    if (prefabPart != null)
                    {
                        preset.ApplyToPart(prefabPart, true);
                        EditorUtility.SetDirty(prefabPart);
                    }
                }
                AssetDatabase.SaveAssets();
                serializedObject.Update();
            }
            else
            {
                if (isPrefabInstance && applySockets && presetHasSockets)
                {
                    PrefabUtility.UnpackPrefabInstance(target.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                }

                Undo.RecordObject(target, "Apply Preset");
                preset.ApplyToPart(target, applySockets);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);

                AssetDatabase.SaveAssets();
            }

            SceneView.RepaintAll();
            Debug.Log($"Applied preset '{preset.PresetName}'{(applySockets ? " (with sockets)" : "")}.");
        }
    }
}
