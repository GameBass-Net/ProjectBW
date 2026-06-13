/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartRegistry.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
{
    public class BuildingPartRegistry : ScriptableObject
    {
        private static BuildingPartRegistry s_instance;

        [SerializeField] private List<BuildingPartReference> m_partReferences = new List<BuildingPartReference>();

        private Dictionary<string, BuildingPart> m_partsByPrefabId;
        private Dictionary<string, BuildingPart[]> m_partsByCategory;

#if UNITY_EDITOR
        private readonly Dictionary<string, BuildingPart> m_partsByAssetPath =
            new Dictionary<string, BuildingPart>(StringComparer.Ordinal);
#endif

        public static BuildingPartRegistry Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Resources.Load<BuildingPartRegistry>("BuildingPartRegistry");
                }

                return s_instance;
            }
        }

        public List<BuildingPartReference> PartReferences => m_partReferences;

        public BuildingPart GetPartByPrefabId(string prefabId)
        {
            if (string.IsNullOrEmpty(prefabId))
            {
                return null;
            }

            if (m_partsByPrefabId == null)
            {
                BuildPrefabIdCache();
            }

            m_partsByPrefabId.TryGetValue(prefabId, out BuildingPart result);
            return result;
        }

        public BuildingPart[] GetPartsByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return null;
            }

            if (m_partsByCategory == null)
            {
                BuildCategoryCache();
            }

            m_partsByCategory.TryGetValue(category, out BuildingPart[] result);
            return result;
        }

        public void ClearCache()
        {
            m_partsByPrefabId = null;
            m_partsByCategory = null;
        }

        private void BuildPrefabIdCache()
        {
            m_partsByPrefabId = new Dictionary<string, BuildingPart>(StringComparer.Ordinal);

            for (int i = 0; i < m_partReferences.Count; i++)
            {
                BuildingPartReference reference = m_partReferences[i];
                if (reference == null || reference.BuildingParts == null)
                {
                    continue;
                }

                BuildingPart[] parts = reference.BuildingParts;
                for (int j = 0; j < parts.Length; j++)
                {
                    BuildingPart part = parts[j];
                    if (part == null || string.IsNullOrEmpty(part.PrefabId))
                    {
                        continue;
                    }

                    if (!m_partsByPrefabId.ContainsKey(part.PrefabId))
                    {
                        m_partsByPrefabId.Add(part.PrefabId, part);
                    }
                }
            }
        }

        private void BuildCategoryCache()
        {
            m_partsByCategory = new Dictionary<string, BuildingPart[]>(StringComparer.Ordinal);

            for (int i = 0; i < m_partReferences.Count; i++)
            {
                BuildingPartReference reference = m_partReferences[i];
                if (reference != null && !string.IsNullOrEmpty(reference.Category))
                {
                    m_partsByCategory[reference.Category] = reference.BuildingParts;
                }
            }
        }

#if UNITY_EDITOR

        public void RefreshRegistry()
        {
            m_partReferences.Clear();
            ClearCache();
            m_partsByAssetPath.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            Dictionary<string, List<BuildingPart>> partsByCategory =
                new Dictionary<string, List<BuildingPart>>(StringComparer.Ordinal);

            try
            {
                AssetDatabase.StartAssetEditing();

                int totalGuids = guids.Length;
                for (int i = 0; i < totalGuids; i++)
                {
                    string guid = guids[i];
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    float progress = totalGuids > 0 ? (i + 1f) / totalGuids : 1f;
                    EditorUtility.DisplayProgressBar(
                        "Refreshing Building Part Registry",
                        "Scanning: " + assetPath,
                        progress);

                    GameObject prefabAsset;
                    try
                    {
                        prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                    catch
                    {
                        continue;
                    }

                    if (prefabAsset == null)
                    {
                        continue;
                    }

                    PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(prefabAsset);
                    if (prefabType == PrefabAssetType.NotAPrefab)
                    {
                        continue;
                    }

                    if (!prefabAsset.TryGetComponent(out BuildingPart part) || part == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(part.PrefabId) || part.PrefabId != guid)
                    {
                        part.SetPrefabId(guid);
                        EditorUtility.SetDirty(part);
                    }

                    string partCategory = string.IsNullOrEmpty(part.Category)
                        ? "Default"
                        : part.Category;

                    if (!partsByCategory.TryGetValue(partCategory, out List<BuildingPart> categoryList))
                    {
                        categoryList = new List<BuildingPart>();
                        partsByCategory.Add(partCategory, categoryList);
                    }

                    categoryList.Add(part);

                    m_partsByAssetPath[assetPath] = part;
                }

                List<string> sortedCategories = new List<string>(partsByCategory.Keys);
                sortedCategories.Sort(StringComparer.Ordinal);

                for (int i = 0; i < sortedCategories.Count; i++)
                {
                    string category = sortedCategories[i];
                    List<BuildingPart> categoryParts = partsByCategory[category];
                    m_partReferences.Add(new BuildingPartReference(category, categoryParts));
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
        }

        internal bool ContainsAssetPath(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && m_partsByAssetPath.ContainsKey(assetPath);
        }

        internal bool RegisterPart(BuildingPart part, string assetPath)
        {
            if (part == null || string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(part.PrefabId) || part.PrefabId != guid)
            {
                part.SetPrefabId(guid);
                EditorUtility.SetDirty(part);
            }

            UnregisterPartInternal(part);

            string category = string.IsNullOrEmpty(part.Category)
                ? "Default"
                : part.Category;

            int referenceIndex = m_partReferences.FindIndex(r => r.Category == category);

            if (referenceIndex >= 0)
            {
                List<BuildingPart> parts =
                    new List<BuildingPart>(m_partReferences[referenceIndex].BuildingParts);

                if (!parts.Contains(part))
                {
                    parts.Add(part);
                    m_partReferences[referenceIndex] =
                        new BuildingPartReference(category, parts);
                }
            }
            else
            {
                m_partReferences.Add(
                    new BuildingPartReference(category, new List<BuildingPart> { part }));
            }

            m_partsByAssetPath[assetPath] = part;
            return true;
        }

        internal bool UnregisterPartByAssetPath(string assetPath)
        {
            if (!m_partsByAssetPath.TryGetValue(assetPath, out BuildingPart part))
            {
                return false;
            }

            m_partsByAssetPath.Remove(assetPath);
            return UnregisterPartInternal(part);
        }

        private bool UnregisterPartInternal(BuildingPart part)
        {
            bool removed = false;

            for (int i = m_partReferences.Count - 1; i >= 0; i--)
            {
                BuildingPartReference reference = m_partReferences[i];
                BuildingPart[] source = reference.BuildingParts;

                int index = Array.IndexOf(source, part);
                if (index < 0)
                {
                    continue;
                }

                List<BuildingPart> parts = new List<BuildingPart>(source);
                parts.RemoveAt(index);

                if (parts.Count == 0)
                {
                    m_partReferences.RemoveAt(i);
                }
                else
                {
                    m_partReferences[i] =
                        new BuildingPartReference(reference.Category, parts);
                }

                removed = true;
            }

            return removed;
        }

#endif
    }
}