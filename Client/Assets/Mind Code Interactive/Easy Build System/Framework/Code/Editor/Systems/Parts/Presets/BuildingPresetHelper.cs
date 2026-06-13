/// <summary>
/// Project : Easy Build System
/// Class : BuildingPresetHelper.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets
{
    public static class BuildingPresetHelper
    {
        public static List<BuildingPartPreset> GetAllPresetsInProject()
        {
            List<BuildingPartPreset> presetsFoundInProject = new List<BuildingPartPreset>();

#if UNITY_EDITOR
            string[] guidsToProcess = AssetDatabase.FindAssets("t:BuildingPartPreset");
            foreach (string assetGuid in guidsToProcess)
            {
                string assetPathResolved = AssetDatabase.GUIDToAssetPath(assetGuid);
                BuildingPartPreset loadedPreset = AssetDatabase.LoadAssetAtPath<BuildingPartPreset>(assetPathResolved);
                if (loadedPreset != null)
                {
                    presetsFoundInProject.Add(loadedPreset);
                }
            }
#endif

            return presetsFoundInProject;
        }

#if UNITY_EDITOR
        public static bool ApplyPreset(BuildingPart targetBuildingPart, BuildingPartPreset presetToApplyToPart)
        {
            if (targetBuildingPart == null || presetToApplyToPart == null)
            {
                return false;
            }

            if (!EditorUtility.DisplayDialog(
                "Apply Preset",
                $"Apply preset '{presetToApplyToPart.PresetName}'?\n\n" +
                "This action will override the current part configuration.\n" +
                "All existing placements, behaviors, conditions settings will be lost.",
                "Apply",
                "Cancel"))
            {
                return false;
            }

            bool shouldApplySocketsData = true;
            if (presetToApplyToPart.SocketsData != null && presetToApplyToPart.SocketsData.Count > 0)
            {
                int socketsUserChoice = EditorUtility.DisplayDialogComplex(
                    "Apply sockets?",
                    "This preset contains sockets.\n\nApply the socket structure?",
                    "Yes", "No", "Cancel");

                if (socketsUserChoice == 2)
                {
                    return false;
                }

                shouldApplySocketsData = (socketsUserChoice == 0);
            }

            Undo.RecordObject(targetBuildingPart, "Apply Preset '" + presetToApplyToPart.PresetName + "'");
            presetToApplyToPart.ApplyToPart(targetBuildingPart, shouldApplySocketsData);

            foreach (BuildingCondition buildingConditionComponent in targetBuildingPart.ConditionSystem.GetAllConditions())
            {
                if (buildingConditionComponent != null)
                {
                    Undo.RegisterCreatedObjectUndo(buildingConditionComponent, "Apply Preset '" + presetToApplyToPart.PresetName + "'");
                }
            }

            targetBuildingPart.CacheSystem.Refresh();
            targetBuildingPart.BehaviorSystem.Refresh();
            targetBuildingPart.ConditionSystem.Refresh();

            EditorUtility.SetDirty(targetBuildingPart);
            UnityEngine.Debug.Log(
                "Preset '" + presetToApplyToPart.PresetName +
                "' applied to building part '" + targetBuildingPart.Name + "'.");

            return true;
        }

        public static bool SavePreset(BuildingPart sourcePartToSave, BuildingPartPreset presetAssetToSaveTo, string customFileName = null, string folderPathToSaveIn = "Assets/Presets")
        {
            if (sourcePartToSave == null || presetAssetToSaveTo == null)
            {
                return false;
            }

            int userChoiceForSockets = EditorUtility.DisplayDialogComplex(
                "Include sockets?",
                "Do you want to also save the sockets settings?",
                "Yes",
                "No",
                "Cancel");

            if (userChoiceForSockets == 2)
            {
                return false;
            }

            bool shouldIncludeSocketsInSave = userChoiceForSockets == 0;

            presetAssetToSaveTo.ConfigureFromPart(sourcePartToSave, customFileName ?? (sourcePartToSave.name + "_Preset"), shouldIncludeSocketsInSave);

            if (!AssetDatabase.IsValidFolder(folderPathToSaveIn))
            {
                string[] folderPathParts = folderPathToSaveIn.Split('/');
                string currentPathBeingCreated = folderPathParts[0];
                for (int i = 1; i < folderPathParts.Length; i++)
                {
                    string nextFolderPathToCreate = currentPathBeingCreated + "/" + folderPathParts[i];
                    if (!AssetDatabase.IsValidFolder(nextFolderPathToCreate))
                    {
                        AssetDatabase.CreateFolder(currentPathBeingCreated, folderPathParts[i]);
                    }

                    currentPathBeingCreated = nextFolderPathToCreate;
                }
            }

            string finalAssetFileName = customFileName ?? (presetAssetToSaveTo.PresetName + ".asset");
            if (!finalAssetFileName.EndsWith(".asset"))
            {
                finalAssetFileName += ".asset";
            }

            string completeAssetPath = folderPathToSaveIn + "/" + finalAssetFileName;

            AssetDatabase.CreateAsset(presetAssetToSaveTo, completeAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }
#endif
    }
}