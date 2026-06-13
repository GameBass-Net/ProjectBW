/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartRegistryPostProcessor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

#if UNITY_EDITOR
using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
{
    public class BuildingPartRegistryPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            BuildingPartRegistry registry = BuildingPartRegistry.Instance;
            if (registry == null)
            {
                return;
            }

            bool registryChanged = false;

            for (int i = 0; i < importedAssets.Length; i++)
            {
                string assetPath = importedAssets[i];
                if (!assetPath.EndsWith(".prefab", StringComparison.Ordinal))
                {
                    continue;
                }

                if (registry.ContainsAssetPath(assetPath))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (!prefab)
                {
                    continue;
                }

                if (!prefab.TryGetComponent(out BuildingPart part))
                {
                    continue;
                }

                registryChanged |= registry.RegisterPart(part, assetPath);
            }

            for (int i = 0; i < deletedAssets.Length; i++)
            {
                string assetPath = deletedAssets[i];
                if (!assetPath.EndsWith(".prefab", StringComparison.Ordinal))
                {
                    continue;
                }

                registryChanged |= registry.UnregisterPartByAssetPath(assetPath);
            }

            if (registryChanged)
            {
                registry.ClearCache();
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif