/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollectionEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Collections
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Collections
{
    [CustomEditor(typeof(BuildingCollection))]
    public sealed class BuildingCollectionEditor : BaseInspectorEditor<BuildingCollection>
    {
        private ReorderableList.Code.Editor.ReorderableList m_partReferencesList;
        private SerializedProperty m_partReferencesProperty;

        protected override void OnInspectorEnable()
        {
            m_partReferencesProperty = Properties.Get("m_partReferences");
            m_partReferencesList = new ReorderableList.Code.Editor.ReorderableList(m_partReferencesProperty, false);

            SyncAttachedCollections();
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "Stores a list of Building Part references into a named collection.\n" +
                "Simplifies Building Part reference selection and filtering in the inspector.\n" +
                "See the documentation for more information about this component.");

            DrawGeneralSection();

            EditorGUIExtended.InspectorBottom();
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure the collection name and manage the building parts included.",
                () =>
                {
                    Properties.Draw("m_name", new GUIContent("Collection Name", "Display name used to identify this collection."));

                    EditorGUILayout.Space();

                    m_partReferencesList?.Layout();

                    EditorGUILayout.Space();

                    EditorGUIExtended.DragAndDropArea("Drag & Drop Building Parts here to add to collection.",
                        HandlePartReferencesDropped, IsValidDraggedObject, true, true);
                }, false, true);
        }

        private void SyncAttachedCollections()
        {
            if (Target == null || m_partReferencesProperty == null || !m_partReferencesProperty.isArray)
            {
                return;
            }

            bool anyCollectionChanged = false;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < m_partReferencesProperty.arraySize; i++)
                {
                    string extractedPrefabId = m_partReferencesProperty.GetArrayElementAtIndex(i).stringValue;
                    if (string.IsNullOrEmpty(extractedPrefabId))
                    {
                        continue;
                    }

                    string resolvedAssetPath = AssetDatabase.GUIDToAssetPath(extractedPrefabId);
                    if (string.IsNullOrEmpty(resolvedAssetPath))
                    {
                        continue;
                    }

                    GameObject loadedPrefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(resolvedAssetPath);
                    if (!loadedPrefabGameObject)
                    {
                        continue;
                    }

                    BuildingPart foundBuildingPart = loadedPrefabGameObject.GetComponentInChildren<BuildingPart>(true);
                    if (!foundBuildingPart || foundBuildingPart.AttachedCollection == Target)
                    {
                        continue;
                    }

                    Undo.RecordObject(foundBuildingPart, "Sync Building Collection");
                    foundBuildingPart.AttachedCollection = Target;

                    EditorUtility.SetDirty(foundBuildingPart);
                    EditorUtility.SetDirty(loadedPrefabGameObject);
                    anyCollectionChanged = true;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            if (anyCollectionChanged)
            {
                EditorUtility.SetDirty(Target);
                AssetDatabase.SaveAssets();
            }
        }

        private bool HandlePartReferencesDropped(Object[] droppedObjectsArray)
        {
            int successfulAddCount = 0;

            foreach (Object droppedObjectToProcess in droppedObjectsArray)
            {
                if (!TryExtractBuildingPartPrefabId(droppedObjectToProcess, out string extractedPrefabIdFromObject))
                {
                    continue;
                }

                if (!AddUniquePrefabId(extractedPrefabIdFromObject))
                {
                    Debug.LogWarning("Skipped: Building part already exists in collection.", droppedObjectToProcess);
                    continue;
                }

                AssignCollectionToPart(extractedPrefabIdFromObject, Target);
                successfulAddCount++;
            }

            if (successfulAddCount > 0)
            {
                EditorUtility.SetDirty(Target);
                AssetDatabase.SaveAssets();
            }

            return successfulAddCount > 0;
        }

        private void AssignCollectionToPart(string prefabIdToAssign, BuildingCollection collectionToAssign)
        {
            string resolvedAssetPath = AssetDatabase.GUIDToAssetPath(prefabIdToAssign);
            if (string.IsNullOrEmpty(resolvedAssetPath))
            {
                return;
            }

            GameObject loadedPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(resolvedAssetPath);
            if (!loadedPrefabAsset)
            {
                return;
            }

            BuildingPart foundBuildingPartInPrefab = loadedPrefabAsset.GetComponentInChildren<BuildingPart>(true);
            if (!foundBuildingPartInPrefab || foundBuildingPartInPrefab.AttachedCollection == collectionToAssign)
            {
                return;
            }

            foundBuildingPartInPrefab.AttachedCollection = collectionToAssign;
            EditorUtility.SetDirty(foundBuildingPartInPrefab);
            EditorUtility.SetDirty(loadedPrefabAsset);
        }

        private bool IsValidDraggedObject(Object draggedObjectToValidate) =>
            TryExtractBuildingPartPrefabId(draggedObjectToValidate, out string _);

        private bool TryExtractBuildingPartPrefabId(Object draggedObjectToExtract, out string extractedPrefabId)
        {
            extractedPrefabId = null;

            if (!draggedObjectToExtract)
            {
                return false;
            }

            Object sourceObjectToUse = draggedObjectToExtract;
            if (draggedObjectToExtract is Component draggedComponentObject)
            {
                sourceObjectToUse = draggedComponentObject.gameObject;
            }

            BuildingPart extractedBuildingPart = GetBuildingPartFromObject(sourceObjectToUse);
            if (extractedBuildingPart != null && !string.IsNullOrEmpty(extractedBuildingPart.PrefabId))
            {
                extractedPrefabId = extractedBuildingPart.PrefabId;
                return true;
            }

            string resolvedAssetPath = GetObjectAssetPath(sourceObjectToUse);
            if (string.IsNullOrEmpty(resolvedAssetPath))
            {
                return false;
            }

            string resolvedAssetGuid = AssetDatabase.AssetPathToGUID(resolvedAssetPath);
            if (!string.IsNullOrEmpty(resolvedAssetGuid))
            {
                extractedPrefabId = resolvedAssetGuid;
                return true;
            }

            GameObject loadedPrefabFromAssetPath = AssetDatabase.LoadAssetAtPath<GameObject>(resolvedAssetPath);
            BuildingPart foundBuildingPartInPrefab = loadedPrefabFromAssetPath ? loadedPrefabFromAssetPath.GetComponentInChildren<BuildingPart>(true) : null;
            if (foundBuildingPartInPrefab != null && !string.IsNullOrEmpty(foundBuildingPartInPrefab.PrefabId))
            {
                extractedPrefabId = foundBuildingPartInPrefab.PrefabId;
                return true;
            }

            return false;
        }

        private BuildingPart GetBuildingPartFromObject(Object sourceObjectToExtractFrom)
        {
            if (sourceObjectToExtractFrom is GameObject gameObjectToExtract)
            {
                BuildingPart foundBuildingPart = gameObjectToExtract.GetComponentInChildren<BuildingPart>(true);
                if (!foundBuildingPart)
                {
                    GameObject prefabSourceGameObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObjectToExtract);
                    if (prefabSourceGameObject)
                    {
                        foundBuildingPart = prefabSourceGameObject.GetComponentInChildren<BuildingPart>(true);
                    }
                }

                return foundBuildingPart;
            }

            if (sourceObjectToExtractFrom is BuildingPart part)
            {
                return part;
            }

            return null;
        }

        private string GetObjectAssetPath(Object sourceObjectToGetPathFrom)
        {
            string resolvedAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sourceObjectToGetPathFrom);
            if (string.IsNullOrEmpty(resolvedAssetPath))
            {
                resolvedAssetPath = AssetDatabase.GetAssetPath(sourceObjectToGetPathFrom);
            }

            return resolvedAssetPath;
        }

        private bool AddUniquePrefabId(string prefabIdToAdd)
        {
            if (string.IsNullOrEmpty(prefabIdToAdd) || m_partReferencesProperty == null || !m_partReferencesProperty.isArray)
            {
                return false;
            }

            for (int i = 0; i < m_partReferencesProperty.arraySize; i++)
            {
                if (m_partReferencesProperty.GetArrayElementAtIndex(i).stringValue == prefabIdToAdd)
                {
                    return false;
                }
            }

            m_partReferencesProperty.InsertArrayElementAtIndex(m_partReferencesProperty.arraySize);
            m_partReferencesProperty.GetArrayElementAtIndex(m_partReferencesProperty.arraySize - 1).stringValue = prefabIdToAdd;
            m_partReferencesProperty.serializedObject.ApplyModifiedProperties();
            return true;
        }
    }
}