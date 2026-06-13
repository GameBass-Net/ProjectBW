/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingPartCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections;
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingPartCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/Components/Create Building Part...";
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/Components/Create Building Part...";
        public const int PRIORITY = 11;
        public const string PREVIEW_MATERIAL_PATH = "Materials/PreviewMaterial";

        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            if (Selection.gameObjects.Length == 0)
            {
                return;
            }

            string lastSavedPath = EditorPrefs.GetString("MindCodeInteractive.EasyBuildSystem.LastBuildingPartExportPath", "Assets/");

            foreach (GameObject sourceGameObject in Selection.gameObjects)
            {
                if (sourceGameObject.GetComponentInParent<BuildingPart>() != null)
                {
                    continue;
                }

                string selectedSavePath = EditorUtility.SaveFilePanelInProject(
                    "Easy Build System - Define a save path...",
                    sourceGameObject.name, "prefab", "", lastSavedPath);

                if (string.IsNullOrEmpty(selectedSavePath))
                {
                    continue;
                }

                string selectedDirectoryPath = Path.GetDirectoryName(selectedSavePath);
                EditorPrefs.SetString("MindCodeInteractive.EasyBuildSystem.LastBuildingPartExportPath", selectedDirectoryPath);

                GameObject createdRootGameObject = CreateRootGameObject(sourceGameObject, selectedSavePath);
                BuildingPart createdBuildingPart = SetupBuildingPart(createdRootGameObject);

                EditorCoroutines.StartCoroutine(CreateBuildingPartRoutine(createdRootGameObject, createdBuildingPart, selectedSavePath));
            }
        }

        private GameObject CreateRootGameObject(GameObject sourceGameObject, string savePath)
        {
            string extractedFileName = Path.GetFileNameWithoutExtension(savePath);
            GameObject createdRootGameObject = new GameObject(extractedFileName);
            Undo.RegisterCreatedObjectUndo(createdRootGameObject, "Create BuildingPart Root");

            Transform rootTransform = createdRootGameObject.transform;
            Transform sourceTransform = sourceGameObject.transform;

            rootTransform.SetParent(sourceTransform.parent, true);
            rootTransform.position = sourceTransform.position;
            rootTransform.rotation = sourceTransform.rotation;
            rootTransform.localScale = sourceTransform.localScale;
            rootTransform.SetSiblingIndex(sourceTransform.GetSiblingIndex());

            Undo.SetTransformParent(sourceTransform, rootTransform, "Reparent Under Root");
            sourceTransform.localPosition = Vector3.zero;
            sourceTransform.localRotation = Quaternion.identity;
            sourceTransform.localScale = Vector3.one;

            return createdRootGameObject;
        }

        private BuildingPart SetupBuildingPart(GameObject rootGameObject)
        {
            BuildingPart createdBuildingPart = Undo.AddComponent<BuildingPart>(rootGameObject);
            createdBuildingPart.Name = rootGameObject.name;
            return createdBuildingPart;
        }

        private IEnumerator CreateBuildingPartRoutine(GameObject rootGameObject, BuildingPart part, string savePath)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Easy Build System", "Step 1/4: Enabling mesh read/write...", 0.1f);
                yield return new EditorCoroutines.EditorWaitForSeconds(0.15f);
                EnableMeshReadWrite(rootGameObject);

                EditorUtility.DisplayProgressBar("Easy Build System", "Step 2/4: Initializing building part...", 0.3f);
                yield return new EditorCoroutines.EditorWaitForSeconds(0.15f);
                part.RendererSystem.BuildVariantsFromRenderers();
                AddRequiredConditions(part);
                SetupPreviewMaterial(part);

                EditorUtility.DisplayProgressBar("Easy Build System", "Step 3/4: Saving prefab asset...", 0.6f);
                yield return new EditorCoroutines.EditorWaitForSeconds(0.15f);
                string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(savePath);
                GameObject savedPrefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(rootGameObject, uniqueAssetPath, InteractionMode.UserAction);

                EditorUtility.DisplayProgressBar("Easy Build System", "Step 4/4: Finalizing setup...", 0.85f);
                yield return new EditorCoroutines.EditorWaitForSeconds(0.15f);
                PrefabUtility.ApplyPrefabInstance(rootGameObject, InteractionMode.UserAction);

                BuildingPartRegistry.Instance.RefreshRegistry();

                EditorGUIUtility.PingObject(savedPrefabAsset);
                Selection.activeGameObject = savedPrefabAsset;
                SceneView.lastActiveSceneView?.FrameSelected();

                Debug.Log("Building Part \"" + rootGameObject.name + "\" successfully created.");
                EditorSceneManager.MarkAllScenesDirty();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void EnableMeshReadWrite(GameObject rootGameObject)
        {
            MeshFilter[] allMeshFiltersInHierarchy = rootGameObject.GetComponentsInChildren<MeshFilter>(true);

            foreach (MeshFilter meshFilterComponent in allMeshFiltersInHierarchy)
            {
                if (!meshFilterComponent || !meshFilterComponent.sharedMesh)
                {
                    continue;
                }

                Mesh sharedMeshAsset = meshFilterComponent.sharedMesh;

                if (sharedMeshAsset.isReadable)
                {
                    continue;
                }

                string meshAssetPath = AssetDatabase.GetAssetPath(sharedMeshAsset);

                if (string.IsNullOrEmpty(meshAssetPath))
                {
                    continue;
                }

                ModelImporter meshModelImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;

                if (meshModelImporter == null)
                {
                    continue;
                }

                if (!meshModelImporter.isReadable)
                {
                    meshModelImporter.isReadable = true;
                    meshModelImporter.SaveAndReimport();
                }
            }
        }

        private void AddRequiredConditions(BuildingPart part)
        {
            if (!part)
            {
                return;
            }

            TypeCache.TypeCollection derivedConditionTypes = TypeCache.GetTypesDerivedFrom<BuildingCondition>();

            for (int i = 0; i < derivedConditionTypes.Count; i++)
            {
                Type conditionTypeToCheck = derivedConditionTypes[i];

                if (conditionTypeToCheck == null || conditionTypeToCheck.IsAbstract)
                {
                    continue;
                }

                BuildingConditionAttribute conditionAttribute =
                    (BuildingConditionAttribute)Attribute.GetCustomAttribute(conditionTypeToCheck, typeof(BuildingConditionAttribute), false);

                if (conditionAttribute == null || !conditionAttribute.IsRequired)
                {
                    continue;
                }

                part.ConditionSystem.AddCondition(conditionTypeToCheck);
            }
        }

        private void SetupPreviewMaterial(BuildingPart part)
        {
            Material defaultPreviewMaterial = Resources.Load<Material>(PREVIEW_MATERIAL_PATH);
            if (!defaultPreviewMaterial)
            {
                return;
            }

            BuildingPlacementSettings placementConfigSettings = part.PlacementSystem.Settings;

            foreach (PreviewStateMaterialData previewMaterialConfig in placementConfigSettings.PreviewStateMaterials)
            {
                if (previewMaterialConfig != null)
                {
                    previewMaterialConfig.MaterialMode = MaterialMode.ReplaceMaterial;
                    previewMaterialConfig.CustomMaterial = defaultPreviewMaterial;
                }
            }
        }
    }
}