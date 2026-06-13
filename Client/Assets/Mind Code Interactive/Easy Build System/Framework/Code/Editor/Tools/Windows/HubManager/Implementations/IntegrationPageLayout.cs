/// <summary>
/// Project : Easy Build System
/// Class : IntegrationPageLayout.cs
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
    [EditorPage("ebs.integration", "Easy Build System", "Integrations", "Editor/Icons/pack", 2)]
    public class IntegrationPageLayout : PageLayout
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
                "Integrations",
                "Browse available integration packages and manage their installation status."
            );

            DrawSearchBar();
            DrawIntegrationPackages();

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

        private void DrawIntegrationPackages()
        {
            List<PackageData> integrationPackagesFiltered = m_allPackages
                .Where(packageToFilter => !string.IsNullOrWhiteSpace(packageToFilter.Manifest.Type) &&
                            packageToFilter.Manifest.Type.Trim().ToLowerInvariant() == "integration")
                .ToList();

            if (!string.IsNullOrEmpty(m_searchText))
            {
                string searchQueryLowercase = m_searchText.ToLowerInvariant();

                integrationPackagesFiltered = integrationPackagesFiltered
                    .Where(packageToSearch =>
                        packageToSearch.Manifest.Name.ToLowerInvariant().Contains(searchQueryLowercase) ||
                        (!string.IsNullOrEmpty(packageToSearch.Manifest.Description) &&
                         packageToSearch.Manifest.Description.ToLowerInvariant().Contains(searchQueryLowercase)))
                    .ToList();
            }

            if (integrationPackagesFiltered.Count == 0)
            {
                EditorGUIExtended.HelpBox(
                    "No integration packages found.\nPlease ensure integration packages are present and manifests are configured.",
                    EditorGUIElements.MessageType.Info
                );

                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            foreach (PackageData packageDataToDisplay in integrationPackagesFiltered)
            {
                DrawPackageBox(packageDataToDisplay);
            }

            GUILayout.EndVertical();
            GUILayout.Space(5f);
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

                bool canInstallOrUninstall = packageDataToDraw.IsInstalled || PackageManager.AreDependenciesSatisfied(packageDataToDraw, m_allPackages);

                EditorGUI.BeginDisabledGroup(!canInstallOrUninstall);

                if (packageDataToDraw.IsInstalled)
                {
                    //if (EditorGUIExtended.Button("Update Integration...", GUILayout.Width(150f), GUILayout.Height(18)))
                    //{
                    //    PackageManager.Update(packageDataToDraw);
                    //    RefreshPackages();
                    //}

                    //GUILayout.Space(4f);

                    if (EditorGUIExtended.WarningButton("Uninstall Integration...", GUILayout.Width(150f), GUILayout.Height(18)))
                    {
                        PackageManager.Uninstall(packageDataToDraw);
                        RefreshPackages();
                    }
                }
                else
                {
                    if (EditorGUIExtended.Button("Install Integration...", GUILayout.Width(150f), GUILayout.Height(18)))
                    {
                        PackageManager.Install(packageDataToDraw);
                        RefreshPackages();
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(-5f);

                EditorGUIExtended.LinkLabel("Author : " + packageManifestData.Author, packageManifestData.Link);

                if (!string.IsNullOrEmpty(packageManifestData.Description))
                {
                    EditorGUIExtended.Label("Description : " + packageManifestData.Description);
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
                GUILayout.Label("Dependencies :", GUILayout.ExpandWidth(false));

                for (int i = 0; i < packageManifestToDisplay.Dependencies.Length; i++)
                {
                    string dependencyNameToCheck = packageManifestToDisplay.Dependencies[i];
                    DependencyStatus dependencyStatusToDisplay = PackageManager.GetDependencyStatus(dependencyNameToCheck, m_allPackages);

                    Color dependencyStatusColor = Color.white;

                    if (dependencyStatusToDisplay == DependencyStatus.Valid)
                    {
                        dependencyStatusColor = Color.green;
                    }
                    else if (dependencyStatusToDisplay == DependencyStatus.Missing ||
                        dependencyStatusToDisplay == DependencyStatus.Version_Mismatch)
                    {
                        dependencyStatusColor = Color.red;
                    }

                    EditorGUIExtended.ColoredLabel(dependencyNameToCheck, dependencyStatusColor);

                    if (i < packageManifestToDisplay.Dependencies.Length - 1)
                    {
                        GUILayout.Label("-", GUILayout.Width(8));
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