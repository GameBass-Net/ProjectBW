/// <summary>
/// Project : Easy Build System
/// Class : RenderPipelineUpgrader.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines
{
    public static class RenderPipelineUpgrader
    {
        public static void ConvertToURP(RenderPipelineUpgradeSnapshot snapshot)
        {
            if (!ValidateSnapshot(snapshot))
            {
                return;
            }

            try
            {
                ImportPackage(snapshot.URPPackage);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"URP conversion failed: {ex.Message}");
            }
        }

        public static void ConvertToHDRP(RenderPipelineUpgradeSnapshot snapshot)
        {
            if (!ValidateSnapshot(snapshot))
            {
                return;
            }

            try
            {
                ImportPackage(snapshot.HDRPPackage);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"HDRP conversion failed: {ex.Message}");
            }
        }

        private static bool ValidateSnapshot(RenderPipelineUpgradeSnapshot snapshot)
        {
            if (!snapshot)
            {
                Debug.LogError("Snapshot is null");
                return false;
            }

            return true;
        }

        private static void ImportPackage(DefaultAsset packageAsset)
        {
            if (!packageAsset)
            {
                return;
            }

            string packagePath = AssetDatabase.GetAssetPath(packageAsset);
            if (string.IsNullOrEmpty(packagePath) || !packagePath.EndsWith(".unitypackage"))
            {
                return;
            }

            AssetDatabase.ImportPackage(packagePath, false);
        }
    }
}