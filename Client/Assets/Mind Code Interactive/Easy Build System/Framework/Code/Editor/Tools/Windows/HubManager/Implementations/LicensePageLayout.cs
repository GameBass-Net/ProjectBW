/// <summary>
/// Project : Easy Build System
/// Class : LicensePageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.license", "Easy Build System", "Licenses", "Editor/Icons/copyright", 6)]
    public class LicensePageLayout : PageLayout
    {
        private class LicenseItem
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Copyright { get; set; }
            public string License { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
        }

        private List<LicenseItem> m_licenseItems = new List<LicenseItem>();

        public override void OnEnable()
        {
            TextAsset licenseTextAsset = Resources.Load<TextAsset>("Licenses");
            if (licenseTextAsset != null)
            {
                ParseLicenses(licenseTextAsset.text);
            }
        }

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Licenses",
                "View third-party libraries and their licensing terms used in the building system."
            );

            foreach (LicenseItem licenseItemToDisplay in m_licenseItems)
            {
                DrawLicenseBox(licenseItemToDisplay);
            }

            EditorGUIExtended.InspectorBottom();
        }

        private void DrawLicenseBox(LicenseItem licenseItemToDraw)
        {
            EditorGUIExtended.BeginVertical();

            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);

                EditorGUILayout.BeginHorizontal();

                EditorGUIExtended.ColoredLabel(licenseItemToDraw.Name + " | v" + licenseItemToDraw.Version, Color.white, EditorGUILabels.LabelType.Bold);

                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(licenseItemToDraw.Url))
                {
                    if (EditorGUIExtended.Button("View License...", GUILayout.Width(130f)))
                    {
                        Application.OpenURL(licenseItemToDraw.Url);
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(-5f);

                if (!string.IsNullOrEmpty(licenseItemToDraw.Copyright))
                {
                    GUILayout.Label("Copyright: " + licenseItemToDraw.Copyright);
                }

                if (!string.IsNullOrEmpty(licenseItemToDraw.License))
                {
                    GUILayout.Label("License: " + licenseItemToDraw.License);
                }

                if (!string.IsNullOrEmpty(licenseItemToDraw.Description))
                {
                    GUILayout.Label("Description: " + licenseItemToDraw.Description);
                }
            }

            EditorGUIExtended.EndVertical();
        }

        private void ParseLicenses(string contentToParse)
        {
            m_licenseItems.Clear();

            string currentLicenseName = null;
            string currentLicenseVersion = null;
            string currentLicenseCopyright = null;
            string currentLicenseDescription = null;
            string currentLicenseUrl = null;
            string currentLicenseType = null;

            foreach (string rawLineContent in contentToParse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string lineContentToProcess = rawLineContent.Trim();

                if (lineContentToProcess.StartsWith("# "))
                {
                    if (!string.IsNullOrEmpty(currentLicenseName))
                    {
                        m_licenseItems.Add(new LicenseItem
                        {
                            Name = currentLicenseName,
                            Version = currentLicenseVersion,
                            Copyright = currentLicenseCopyright,
                            License = currentLicenseType,
                            Description = currentLicenseDescription ?? string.Empty,
                            Url = currentLicenseUrl
                        });
                    }

                    currentLicenseName = lineContentToProcess.Substring(2).Trim();
                    currentLicenseVersion = null;
                    currentLicenseCopyright = null;
                    currentLicenseType = null;
                    currentLicenseDescription = null;
                    currentLicenseUrl = null;
                }
                else if (lineContentToProcess.StartsWith("Version: "))
                {
                    currentLicenseVersion = lineContentToProcess.Substring(9).Trim();
                }
                else if (lineContentToProcess.StartsWith("Copyright: "))
                {
                    currentLicenseCopyright = lineContentToProcess.Substring(11).Trim();
                }
                else if (lineContentToProcess.StartsWith("License: "))
                {
                    currentLicenseType = lineContentToProcess.Substring(9).Trim();
                }
                else if (lineContentToProcess.StartsWith("Url: "))
                {
                    currentLicenseUrl = lineContentToProcess.Substring(5).Trim();
                }
                else if (!string.IsNullOrEmpty(currentLicenseName) && !lineContentToProcess.StartsWith("#"))
                {
                    if (string.IsNullOrEmpty(currentLicenseDescription))
                    {
                        currentLicenseDescription = lineContentToProcess;
                    }
                    else
                    {
                        currentLicenseDescription += " " + lineContentToProcess;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentLicenseName))
            {
                m_licenseItems.Add(new LicenseItem
                {
                    Name = currentLicenseName,
                    Version = currentLicenseVersion,
                    Copyright = currentLicenseCopyright,
                    License = currentLicenseType,
                    Description = currentLicenseDescription ?? string.Empty,
                    Url = currentLicenseUrl
                });
            }
        }
    }
}