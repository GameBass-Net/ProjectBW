/// <summary>
/// Project : Easy Build System
/// Class : ExtensionPageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.extension", "Easy Build System", "Extensions", "Editor/Icons/addons", 3)]
    public class ExtensionPageLayout : PageLayout
    {
        private List<PackageData> m_allPackages = new List<PackageData>();
        private string m_searchText = string.Empty;

        public override void OnEnable()
        {
            RefreshPackages();
        }

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Extensions",
                "Browse available extensions and manage their installation status."
            );

            DrawSearchBar();
            DrawAvailableAddons();

            EditorGUIExtended.InspectorBottom();
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search :", GUILayout.Width(55));
            m_searchText = GUILayout.TextField(m_searchText, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);
        }

        private void DrawAvailableAddons()
        {
            List<PackageData> addonPackagesFiltered = m_allPackages
                .Where(packageToFilter => !string.IsNullOrWhiteSpace(packageToFilter.Manifest.Type) &&
                            packageToFilter.Manifest.Type.Trim().ToLowerInvariant() == "extension")
                .ToList();

            if (!string.IsNullOrEmpty(m_searchText))
            {
                string searchQueryLowercase = m_searchText.ToLowerInvariant();

                addonPackagesFiltered = addonPackagesFiltered
                    .Where(packageToSearch =>
                        packageToSearch.Manifest.Name.ToLowerInvariant().Contains(searchQueryLowercase) ||
                        (!string.IsNullOrEmpty(packageToSearch.Manifest.Description) &&
                         packageToSearch.Manifest.Description.ToLowerInvariant().Contains(searchQueryLowercase)))
                    .ToList();
            }

            if (addonPackagesFiltered.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No add-on packages found.\nEnsure packages are present and manifests are configured.",
                    MessageType.Info
                );

                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            foreach (PackageData packageDataToDisplay in addonPackagesFiltered)
            {
                DrawPackageBox(packageDataToDisplay);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawPackageBox(PackageData packageDataToDraw)
        {
            EditorGUIExtended.BeginVertical();

            using (EditorGUIExtended.MarginScope())
            {
                PackageManifest packageManifestData = packageDataToDraw.Manifest;

                GUILayout.Space(-5f);

                EditorGUILayout.BeginHorizontal();

                EditorGUIExtended.ColoredLabel(packageManifestData.Name + " | v" + packageManifestData.Version, Color.white, EditorGUILabels.LabelType.Bold);

                GUILayout.FlexibleSpace();

                //bool canInstallOrUninstall = packageDataToDraw.IsInstalled || PackageManager.AreDependenciesSatisfied(packageDataToDraw, m_allPackages);

                EditorGUI.BeginDisabledGroup(true);
                string installButtonLabel = !packageDataToDraw.IsInstalled ? "Uninstall Extension..." : "Install Extension...";
                bool wasButtonClicked = packageDataToDraw.IsInstalled
                    ? EditorGUIExtended.WarningButton(installButtonLabel, GUILayout.Width(150f))
                    : EditorGUIExtended.Button(installButtonLabel, GUILayout.Width(150f));

                if (wasButtonClicked)
                {
                    if (packageDataToDraw.IsInstalled)
                    {
                        PackageManager.Uninstall(packageDataToDraw);
                    }
                    else
                    {
                        PackageManager.Install(packageDataToDraw);
                    }

                    RefreshPackages();
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(-5f);

                EditorGUIExtended.LinkLabel("Author : " + packageManifestData.Author, packageManifestData.Link);

                if (!string.IsNullOrEmpty(packageManifestData.Description))
                {
                    GUILayout.Label("Description : " + packageManifestData.Description);
                }

                if (!string.IsNullOrEmpty(packageManifestData.UnityVersion) ||
                    (packageManifestData.Dependencies != null && packageManifestData.Dependencies.Length > 0))
                {
                    DrawRequirements(packageManifestData);
                }
            }

            EditorGUIExtended.EndVertical();
        }

        private void DrawRequirements(PackageManifest packageManifestToDisplay)
        {
            bool hasUnityVersionRequirement = !string.IsNullOrEmpty(packageManifestToDisplay.UnityVersion);
            bool hasDependenciesRequirement = packageManifestToDisplay.Dependencies != null && packageManifestToDisplay.Dependencies.Length > 0;

            EditorGUIExtended.Label("Requirements :", EditorGUILabels.LabelType.Bold);

            if (hasUnityVersionRequirement)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Unity Version :", GUILayout.Width(90));

                string currentUnityVersionRunning = Application.unityVersion;
                string requiredUnityVersion = packageManifestToDisplay.UnityVersion;

                string cleanedCurrentVersion = PackageManager.GetCleanUnityVersion(currentUnityVersionRunning);
                string cleanedRequiredVersion = PackageManager.GetCleanUnityVersion(requiredUnityVersion);

                int versionComparisonResult = PackageManager.CompareUnityVersions(cleanedCurrentVersion, cleanedRequiredVersion);

                Color versionCompatibilityColor = (versionComparisonResult >= 0) ? Color.green : Color.yellow;

                string versionRequirementDisplayText = (versionComparisonResult >= 0)
                    ? requiredUnityVersion + " or higher"
                    : requiredUnityVersion;

                EditorGUIExtended.ColoredLabel(versionRequirementDisplayText, versionCompatibilityColor);

                GUILayout.EndHorizontal();
            }

            if (hasDependenciesRequirement)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Dependencies :", GUILayout.Width(100));

                for (int i = 0; i < packageManifestToDisplay.Dependencies.Length; i++)
                {
                    string dependencyNameToCheck = packageManifestToDisplay.Dependencies[i];
                    DependencyStatus dependencyStatusToDisplay = PackageManager.GetDependencyStatus(dependencyNameToCheck, m_allPackages);

                    Color dependencyStatusColor = Color.white;

                    if (dependencyStatusToDisplay == DependencyStatus.Valid)
                    {
                        dependencyStatusColor = Color.green;
                    }
                    else if (dependencyStatusToDisplay == DependencyStatus.Missing)
                    {
                        dependencyStatusColor = Color.red;
                    }
                    else if (dependencyStatusToDisplay == DependencyStatus.Version_Mismatch)
                    {
                        dependencyStatusColor = Color.yellow;
                    }

                    EditorGUIExtended.ColoredLabel(dependencyNameToCheck, dependencyStatusColor);

                    if (i < packageManifestToDisplay.Dependencies.Length - 1)
                    {
                        GUILayout.Space(6f);
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private void RefreshPackages()
        {
            m_allPackages = PackageManager.LoadPackages();
        }
    }
}