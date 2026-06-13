/// <summary>
/// Project : Mind Code Interactive
/// Class : PackageManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager
{
    public enum DependencyStatus { Valid, Missing, Version_Mismatch }

    public static class PackageManager
    {
        private const string DATA_FOLDER = "Data";

        public static List<PackageData> LoadPackages()
        {
            List<PackageData> loadedPackagesList = new List<PackageData>();

            string[] manifestFilePathsArray = Directory.GetFiles("Assets", "manifest.json", SearchOption.AllDirectories);

            foreach (string manifestFilePath in manifestFilePathsArray)
            {
                string packageFolderPath = Path.GetDirectoryName(manifestFilePath);

                string dataFolderPath = Path.Combine(packageFolderPath, DATA_FOLDER);

                string[] unityPackageFiles = Directory.Exists(dataFolderPath)
                    ? Directory.GetFiles(dataFolderPath, "*.unitypackage", SearchOption.TopDirectoryOnly)
                    : new string[0];

                string packageUnityPackagePath = unityPackageFiles.Length > 0 ? unityPackageFiles[0] : null;

                PackageManifest loadedManifest = JsonUtility.FromJson<PackageManifest>(File.ReadAllText(manifestFilePath));

                if (!string.IsNullOrWhiteSpace(loadedManifest.Type))
                {
                    loadedManifest.Type = loadedManifest.Type.Trim().ToLowerInvariant();
                }

                loadedPackagesList.Add(new PackageData
                {
                    Manifest = loadedManifest,
                    FolderPath = packageFolderPath,
                    UnityPackagePath = packageUnityPackagePath,
                    IsInstalled = loadedManifest.Installed
                });
            }

            return loadedPackagesList;
        }

        public static bool IsUnityVersionCompatible(PackageData packageData)
        {
            if (packageData.Manifest == null || string.IsNullOrWhiteSpace(packageData.Manifest.UnityVersion))
            {
                return true;
            }

            string cleanedCurrent = GetCleanUnityVersion(Application.unityVersion);
            string cleanedRequired = GetCleanUnityVersion(packageData.Manifest.UnityVersion);

            return CompareUnityVersions(cleanedCurrent, cleanedRequired) >= 0;
        }

        public static string GetCleanUnityVersion(string versionString)
        {
            int suffixIndex = versionString.IndexOfAny(new char[] { 'a', 'b', 'f', 'p' });
            return suffixIndex >= 0 ? versionString.Substring(0, suffixIndex) : versionString;
        }

        public static void Install(PackageData packageToInstall)
        {
            string installedFolderPath = Path.Combine(packageToInstall.FolderPath, "Installed");

            if (Directory.Exists(installedFolderPath))
            {
                EditorUtility.DisplayDialog("Installation", "Already installed.", "OK");
                return;
            }

            EditorPrefs.SetString("NGPM_PendingInstallFolder", installedFolderPath);
            EditorPrefs.SetString("NGPM_PendingPackageName", packageToInstall.Manifest.Name);

            if (packageToInstall.Manifest.Symbols != null && packageToInstall.Manifest.Symbols.Length > 0)
            {
                EditorPrefs.SetString("NGPM_PendingSymbols", string.Join(";", packageToInstall.Manifest.Symbols));
                AddDefineSymbols(packageToInstall.Manifest.Symbols);
            }
            SaveManifest(packageToInstall, true);
            AssetDatabase.ImportPackage(packageToInstall.UnityPackagePath, false);
        }

        public static void Uninstall(PackageData packageToUninstall)
        {
            string installedFolderPath = Path.Combine(packageToUninstall.FolderPath, "Installed");

            bool folderExists = Directory.Exists(installedFolderPath);
            bool symbolsDefined = packageToUninstall.Manifest.Symbols != null &&
                                  packageToUninstall.Manifest.Symbols.Length > 0 &&
                                  AreSymbolsDefined(packageToUninstall.Manifest.Symbols);

            if (!folderExists && !symbolsDefined)
            {
                EditorUtility.DisplayDialog("Uninstall", "Package is not installed.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Uninstall Confirmation", "All changes will be lost. Continue?", "Yes", "No"))
            {
                return;
            }

            if (folderExists)
            {
                Directory.Delete(installedFolderPath, true);

                string installedMeta = installedFolderPath + ".meta";
                if (File.Exists(installedMeta))
                {
                    File.Delete(installedMeta);
                }
            }

            foreach (string filePath in Directory.GetFiles(packageToUninstall.FolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName == "manifest.json")
                {
                    continue;
                }

                File.Delete(filePath);

                string fileMeta = filePath + ".meta";
                if (File.Exists(fileMeta))
                {
                    File.Delete(fileMeta);
                }
            }

            foreach (string dirPath in Directory.GetDirectories(packageToUninstall.FolderPath, "*", SearchOption.TopDirectoryOnly))
            {
                string dirName = Path.GetFileName(dirPath);
                if (dirName == DATA_FOLDER)
                {
                    continue;
                }

                Directory.Delete(dirPath, true);

                string dirMeta = dirPath + ".meta";
                if (File.Exists(dirMeta))
                {
                    File.Delete(dirMeta);
                }
            }

            packageToUninstall.IsInstalled = false;
            RemoveDefineSymbols(packageToUninstall.Manifest.Symbols);
            SaveManifest(packageToUninstall, false);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Uninstall", "Uninstalled successfully.", "OK");
        }

        public static void Update(PackageData packageToUpdate)
        {
            if (!EditorUtility.DisplayDialog("Update Confirmation", "This will override the existing package with current folder content. Continue?", "Yes", "No"))
            {
                return;
            }

            string[] allFiles = Directory.GetFiles(packageToUpdate.FolderPath, "*", SearchOption.AllDirectories);
            List<string> assetPaths = new List<string>();

            for (int i = 0; i < allFiles.Length; i++)
            {
                string filePath = allFiles[i].Replace("\\", "/");
                string fileName = Path.GetFileName(filePath);

                if (fileName == "manifest.json" || filePath.EndsWith(".meta") || filePath.EndsWith(".unitypackage"))
                {
                    continue;
                }

                int assetsIndex = filePath.IndexOf("Assets/");
                if (assetsIndex < 0)
                {
                    continue;
                }

                assetPaths.Add(filePath.Substring(assetsIndex));
            }

            if (assetPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Update", "No assets found to export.", "OK");
                return;
            }

            AssetDatabase.ExportPackage(assetPaths.ToArray(), packageToUpdate.UnityPackagePath, ExportPackageOptions.Recurse);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Update", "Package updated successfully.", "OK");
        }

        private static void SaveManifest(PackageData package, bool installedState)
        {
            package.Manifest.Installed = installedState;

            string manifestPath = Path.Combine(package.FolderPath, "manifest.json");

            string json = JsonUtility.ToJson(package.Manifest, true);

            File.WriteAllText(manifestPath, json);

            package.IsInstalled = installedState;

            AssetDatabase.Refresh();
        }

        public static DependencyStatus GetDependencyStatus(string dependencyString, List<PackageData> packagesList)
        {
            ParseDependency(dependencyString, out string parsedName, out string parsedVersion);

            foreach (PackageData packageData in packagesList)
            {
                if (packageData.Manifest.Name.Trim() == parsedName && packageData.IsInstalled)
                {
                    return !string.IsNullOrEmpty(parsedVersion) && packageData.Manifest.Version.Trim() != parsedVersion.Trim()
                        ? DependencyStatus.Version_Mismatch
                        : DependencyStatus.Valid;
                }
            }

            return DependencyStatus.Missing;
        }

        public static PackageData GetDependencyPackage(string dependencyString, List<PackageData> packagesList)
        {
            ParseDependency(dependencyString, out string parsedName, out _);
            return packagesList.Find(p => p.Manifest.Name.Trim() == parsedName);
        }

        public static bool AreDependenciesSatisfied(PackageData packageData, List<PackageData> packagesList)
        {
            if (!IsUnityVersionCompatible(packageData))
            {
                return false;
            }

            if (packageData.Manifest.Dependencies == null || packageData.Manifest.Dependencies.Length == 0)
            {
                return true;
            }

            foreach (string dependencyString in packageData.Manifest.Dependencies)
            {
                if (GetDependencyStatus(dependencyString, packagesList) != DependencyStatus.Valid)
                {
                    return false;
                }
            }

            return true;
        }

        public static void AddDefineSymbols(string[] symbolsToAdd)
        {
            if (symbolsToAdd == null || symbolsToAdd.Length == 0)
            {
                return;
            }

            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2021_2_OR_NEWER
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
#else
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#endif

            List<string> symbolsList = BuildSymbolsList(currentDefines);
            bool anyAdded = false;

            for (int i = 0; i < symbolsToAdd.Length; i++)
            {
                string symbol = symbolsToAdd[i];
                if (!string.IsNullOrEmpty(symbol) && !symbolsList.Contains(symbol))
                {
                    symbolsList.Add(symbol);
                    anyAdded = true;
                }
            }

            if (!anyAdded)
            {
                return;
            }

            string newDefines = string.Join(";", symbolsList.ToArray());

#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup), newDefines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines);
#endif
        }

        public static void RemoveDefineSymbols(string[] symbolsToRemove)
        {
            if (symbolsToRemove == null || symbolsToRemove.Length == 0)
            {
                return;
            }

            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2021_2_OR_NEWER
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
#else
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#endif

            List<string> symbolsList = BuildSymbolsList(currentDefines);
            bool anyRemoved = false;

            for (int i = 0; i < symbolsToRemove.Length; i++)
            {
                string symbol = symbolsToRemove[i];
                if (string.IsNullOrEmpty(symbol))
                {
                    continue;
                }

                int index = symbolsList.IndexOf(symbol);
                if (index >= 0)
                {
                    symbolsList.RemoveAt(index);
                    anyRemoved = true;
                }
            }

            if (!anyRemoved)
            {
                return;
            }

            string newDefines = string.Join(";", symbolsList.ToArray());

#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup), newDefines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines);
#endif
        }

        public static int CompareUnityVersions(string currentVersion, string requiredVersion)
        {
            string[] currentParts = currentVersion.Split('.');
            string[] requiredParts = requiredVersion.Split('.');

            int maxLength = Mathf.Max(currentParts.Length, requiredParts.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int current = (i < currentParts.Length && int.TryParse(currentParts[i], out int c)) ? c : 0;
                int required = (i < requiredParts.Length && int.TryParse(requiredParts[i], out int r)) ? r : 0;

                if (current != required)
                {
                    return current.CompareTo(required);
                }
            }

            return 0;
        }

        private static bool AreSymbolsDefined(string[] symbols)
        {
            BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2021_2_OR_NEWER
            string defines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));
#else
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
#endif

            for (int i = 0; i < symbols.Length; i++)
            {
                if (!defines.Contains(symbols[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<string> BuildSymbolsList(string defines)
        {
            string[] tokens = defines.Split(';');
            List<string> list = new List<string>();

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!string.IsNullOrEmpty(tokens[i]) && !list.Contains(tokens[i]))
                {
                    list.Add(tokens[i]);
                }
            }

            return list;
        }

        private static void ParseDependency(string dependencyString, out string name, out string version)
        {
            int separatorIndex = dependencyString.IndexOf(':');
            if (separatorIndex >= 0)
            {
                name = dependencyString.Substring(0, separatorIndex).Trim();
                version = dependencyString.Substring(separatorIndex + 1).Trim();
            }
            else
            {
                name = dependencyString.Trim();
                version = null;
            }
        }
    }
}