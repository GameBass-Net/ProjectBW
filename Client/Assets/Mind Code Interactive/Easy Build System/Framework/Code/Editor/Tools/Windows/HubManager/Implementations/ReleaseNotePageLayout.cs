/// <summary>
/// Project : Easy Build System
/// Class : ReleaseNotePageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.releaseNote", "Easy Build System", "Release Notes", "Editor/Icons/changelog", 4)]
    public class ReleaseNotePageLayout : PageLayout
    {
        private Dictionary<string, Dictionary<string, List<string>>> m_versions =
            new Dictionary<string, Dictionary<string, List<string>>>();
        private Dictionary<string, string> m_versionNotices = new Dictionary<string, string>();

        private const string LATEST_RELEASE_DATE = "April 26, 2026";
        private const string LATEST_RELEASE_TYPE = "Experimental";

        private Color m_currentVersionColor = Color.green;
        private string m_updateMessage = string.Empty;

        public override void OnEnable()
        {
            TextAsset changelogTextAsset = Resources.Load<TextAsset>("Changelog");
            if (changelogTextAsset != null)
            {
                ParseChangelog(changelogTextAsset.text);
            }
        }

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Release Notes",
                "Version history with new features, improvements and resolved issues."
            );

            DrawVersionInformation();
            EditorGUIExtended.Separator();
            DrawAllVersions();
            EditorGUIExtended.InspectorBottom();
        }

        private void DrawVersionInformation()
        {
            string currentVersionNumber = RepositoryManifest.Get("EbsRepositoryManifest").Version;

            EditorGUILayout.BeginHorizontal();
            DrawVersionCard(LATEST_RELEASE_DATE, "Release Date", EditorGUIExtended.ColorPalette.Success);
            GUILayout.Space(3f);
            DrawVersionCard(currentVersionNumber, "Current Version", m_currentVersionColor);
            GUILayout.Space(3f);
            DrawVersionCard(LATEST_RELEASE_TYPE, "Release Type", EditorGUIExtended.ColorPalette.Warning);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(m_updateMessage))
            {
                EditorGUILayout.Space(5f);
                EditorGUIExtended.HelpBox(m_updateMessage, EditorGUIElements.MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(5f);
            }

            if (EditorGUIExtended.Button("Check for Updates..."))
            {
                CheckForUpdates(currentVersionNumber);
            }
        }

        private void DrawVersionCard(string cardValueToDisplay, string cardLabelToDisplay, Color valueColorToUse)
        {
            EditorGUIExtended.BeginVertical(GUILayout.MinHeight(40), GUILayout.ExpandWidth(true));
            EditorGUIExtended.ColoredLabel(cardValueToDisplay, valueColorToUse, EditorGUILabels.LabelType.Bold, EditorGUILabels.LabelAlignment.Center);
            EditorGUIExtended.Label(cardLabelToDisplay, EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            EditorGUIExtended.EndVertical();
        }

        private void DrawAllVersions()
        {
            string firstVersionKeyInDictionary = m_versions.Keys.FirstOrDefault();

            foreach (KeyValuePair<string, Dictionary<string, List<string>>> versionEntry in m_versions)
            {
                string versionLabelToDisplay = versionEntry.Key;
                Dictionary<string, List<string>> sectionsInCurrentVersion = versionEntry.Value;

                EditorGUIExtended.DrawExpandableSection(
                    new GUIContent(versionLabelToDisplay),
                    string.Empty,
                    () =>
                    {
                        if (m_versionNotices.TryGetValue(versionEntry.Key, out string notice))
                        {
                            EditorGUIExtended.HelpBox(notice, EditorGUIElements.MessageType.Warning);
                            GUILayout.Space(5);
                        }

                        foreach (KeyValuePair<string, List<string>> sectionEntry in sectionsInCurrentVersion)
                        {
                            DrawSectionHeader(sectionEntry.Key);
                            GUILayout.Space(3);
                            DrawFeatureList(sectionEntry.Value);
                            GUILayout.Space(10);
                        }
                    },
                    false,
                    versionEntry.Key == firstVersionKeyInDictionary
                );

                GUILayout.Space(5);
            }
        }

        private void DrawSectionHeader(string sectionTitleToDisplay)
        {
            string sectionKeyLowercase = sectionTitleToDisplay.ToLowerInvariant();

            if (sectionKeyLowercase == "added" || sectionKeyLowercase == "features")
            {
                EditorGUIExtended.ColoredLabel(sectionTitleToDisplay, Color.green, EditorGUILabels.LabelType.Bold);
            }
            else if (sectionKeyLowercase == "changed" || sectionKeyLowercase == "improvements")
            {
                EditorGUIExtended.ColoredLabel(sectionTitleToDisplay, Color.cyan, EditorGUILabels.LabelType.Bold);
            }
            else if (sectionKeyLowercase == "fixed" || sectionKeyLowercase == "fixes")
            {
                EditorGUIExtended.ColoredLabel(sectionTitleToDisplay, Color.yellow, EditorGUILabels.LabelType.Bold);
            }
            else if (sectionKeyLowercase == "removed" || sectionKeyLowercase == "deprecated")
            {
                EditorGUIExtended.ColoredLabel(sectionTitleToDisplay, Color.white, EditorGUILabels.LabelType.Bold);
            }
        }

        private void DrawFeatureList(List<string> featureItemsToDisplay)
        {
            GUIStyle wrappedLabelStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };

            foreach (string featureItemToRender in featureItemsToDisplay)
            {
                EditorGUILayout.BeginHorizontal();
                //GUILayout.Label("-", GUILayout.Width(10));
                GUILayout.Label(featureItemToRender, wrappedLabelStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ParseChangelog(string contentToParse)
        {
            m_versions.Clear();
            m_versionNotices.Clear();

            string currentVersionToProcess = null;
            string currentSectionToProcess = null;

            foreach (string rawLineContent in contentToParse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string lineContentToProcess = rawLineContent.Trim();

                if (lineContentToProcess.StartsWith("# "))
                {
                    currentVersionToProcess = lineContentToProcess.Substring(2).Trim();

                    Match versionMatchResult = Regex.Match(currentVersionToProcess, @"Version\s+([0-9\.]+)\s*-\s*(\w+\s+\d{1,2},\s+\d{4})(?:\s+\[(.*?)\])?");
                    if (versionMatchResult.Success)
                    {
                        string extractedVersionNumber = versionMatchResult.Groups[1].Value;
                        string extractedReleaseDate = versionMatchResult.Groups[2].Value;
                        string extractedReleaseType = versionMatchResult.Groups[3].Success ? versionMatchResult.Groups[3].Value : "Stable";

                        currentVersionToProcess = "Version " + extractedVersionNumber + " - " + extractedReleaseDate + " [" + extractedReleaseType + "]";
                    }

                    m_versions[currentVersionToProcess] = new Dictionary<string, List<string>>();
                }
                else if (lineContentToProcess.StartsWith("> ") && currentVersionToProcess != null)
                {
                    string notice = lineContentToProcess.Substring(2).Trim();
                    if (!m_versionNotices.ContainsKey(currentVersionToProcess))
                    {
                        m_versionNotices[currentVersionToProcess] = notice;
                    }
                    else
                    {
                        m_versionNotices[currentVersionToProcess] += "\n" + notice;
                    }
                }
                else if (lineContentToProcess.StartsWith("## ") && currentVersionToProcess != null)
                {
                    currentSectionToProcess = lineContentToProcess.Substring(3).Trim();

                    if (!m_versions[currentVersionToProcess].ContainsKey(currentSectionToProcess))
                    {
                        m_versions[currentVersionToProcess][currentSectionToProcess] = new List<string>();
                    }
                }
                else if (lineContentToProcess.StartsWith("- ") && currentVersionToProcess != null && currentSectionToProcess != null)
                {
                    m_versions[currentVersionToProcess][currentSectionToProcess].Add(lineContentToProcess.Substring(2).Trim());
                }
            }
        }

        private void CheckForUpdates(string currentVersionToCheck)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Checking for Updates", "Fetching version info from Asset Store...", 0.5f);

                string assetStoreUrlToCheck = "https://assetstore.unity.com/packages/templates/systems/easy-build-system-modular-building-system-45394";
                using (WebClient httpClientToUse = new WebClient())
                {
                    string pageHtmlContent = httpClientToUse.DownloadString(assetStoreUrlToCheck);

                    Match versionMatchResult = Regex.Match(
                        pageHtmlContent,
                        @"Latest version.*?([0-9]+\.[0-9]+\.[0-9]+)",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    if (versionMatchResult.Success)
                    {
                        string onlineVersionNumberFound = versionMatchResult.Groups[1].Value.Trim();
                        CompareVersions(currentVersionToCheck, onlineVersionNumberFound);
                    }
                    else
                    {
                        m_updateMessage = "Unable to retrieve the latest version from the Asset Store.\nPlease check your internet connection or try again later.";
                        m_currentVersionColor = Color.gray;
                    }
                }
            }
            catch
            {
                m_updateMessage = "An error occurred while checking for updates.";
                m_currentVersionColor = Color.gray;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void CompareVersions(string currentVersionToCompare, string onlineVersionToCompare)
        {
            if (currentVersionToCompare != onlineVersionToCompare)
            {
                m_currentVersionColor = EditorGUIExtended.ColorPalette.Warning;
                m_updateMessage = "A new version (" + onlineVersionToCompare + ") is available on the Asset Store!\n" +
                    "Consider updating from your current version (" + currentVersionToCompare + ") to get the latest features and improvements.";
            }
            else
            {
                m_currentVersionColor = EditorGUIExtended.ColorPalette.Success;
                m_updateMessage = "You're running the latest version (" + currentVersionToCompare + ") of Easy Build System.\n" +
                    "Your system is up to date with all the latest features and improvements.";
            }
        }
    }
}