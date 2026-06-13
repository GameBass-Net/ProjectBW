/// <summary>
/// Project : Easy Build System
/// Class : RoadmapPageLayout.cs
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
    [EditorPage("ebs.roadmap", "Easy Build System", "Roadmap", "Editor/Icons/roadmap", 5)]
    public class RoadmapPageLayout : PageLayout
    {
        private class RoadmapItem
        {
            public string Version { get; set; }
            public string Date { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public EditorGUIElements.MessageType Status { get; set; }
        }

        private List<RoadmapItem> m_roadmapItems = new List<RoadmapItem>();

        public override void OnEnable()
        {
            TextAsset roadmapTextAsset = Resources.Load<TextAsset>("Roadmap");
            if (roadmapTextAsset != null)
            {
                ParseRoadmap(roadmapTextAsset.text);
            }
        }

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Roadmap",
                "Planned features, upcoming improvements and development priorities."
            );

            for (int i = 0; i < m_roadmapItems.Count; i++)
            {
                RoadmapItem roadmapItemToDisplay = m_roadmapItems[i];

                Color statusColorToUse = GetStatusColor(roadmapItemToDisplay.Status);
                string htmlColorCodeForStatus = ColorUtility.ToHtmlStringRGB(statusColorToUse);
                string statusLabelToShow = GetStatusLabel(roadmapItemToDisplay.Status);

                string formattedMessageContent =
                    "<color=#" + htmlColorCodeForStatus + "><b>" + roadmapItemToDisplay.Title + "</b></color>\n" +
                    roadmapItemToDisplay.Description;

                List<string> metaPartsToDisplay = new List<string>();

                if (!string.IsNullOrEmpty(statusLabelToShow))
                {
                    metaPartsToDisplay.Add(statusLabelToShow);
                }

                if (!string.IsNullOrEmpty(roadmapItemToDisplay.Version))
                {
                    metaPartsToDisplay.Add("Version: " + roadmapItemToDisplay.Version);
                }

                if (!string.IsNullOrEmpty(roadmapItemToDisplay.Date))
                {
                    metaPartsToDisplay.Add("Target: " + roadmapItemToDisplay.Date);
                }

                if (metaPartsToDisplay.Count > 0)
                {
                    string metaLineToRender = string.Join(" | ", metaPartsToDisplay);
                    EditorGUIExtended.ColoredLabel(
                        metaLineToRender,
                        Color.gray,
                        EditorGUILabels.LabelType.Mini,
                        EditorGUILabels.LabelAlignment.Left
                    );

                    EditorGUILayout.Separator();
                }

                EditorGUIExtended.HelpBox(formattedMessageContent, roadmapItemToDisplay.Status);

                if (i < m_roadmapItems.Count - 1)
                {
                    EditorGUIExtended.Separator();
                }
            }

            EditorGUIExtended.InspectorBottom();
        }

        private void ParseRoadmap(string contentToParse)
        {
            m_roadmapItems.Clear();

            string currentItemTitle = null;
            string currentItemDescription = null;
            string currentItemVersion = null;
            string currentItemTargetDate = null;
            EditorGUIElements.MessageType currentItemStatus = EditorGUIElements.MessageType.Info;

            foreach (string rawLineContent in contentToParse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string lineContentToProcess = rawLineContent.Trim();

                if (lineContentToProcess.StartsWith("# "))
                {
                    if (!string.IsNullOrEmpty(currentItemTitle))
                    {
                        m_roadmapItems.Add(new RoadmapItem
                        {
                            Version = currentItemVersion,
                            Date = currentItemTargetDate,
                            Title = currentItemTitle,
                            Description = currentItemDescription ?? string.Empty,
                            Status = currentItemStatus
                        });
                    }

                    currentItemTitle = lineContentToProcess.Substring(2).Trim();
                    currentItemDescription = null;
                    currentItemVersion = null;
                    currentItemTargetDate = null;
                    currentItemStatus = EditorGUIElements.MessageType.None;
                }
                else if (lineContentToProcess.StartsWith("Status: "))
                {
                    currentItemStatus = GetStatusType(lineContentToProcess.Substring(8).Trim().ToUpper());
                }
                else if (lineContentToProcess.StartsWith("Version: "))
                {
                    currentItemVersion = lineContentToProcess.Substring(9).Trim();
                }
                else if (lineContentToProcess.StartsWith("Target: "))
                {
                    currentItemTargetDate = lineContentToProcess.Substring(8).Trim();
                }
                else if (!string.IsNullOrEmpty(currentItemTitle) && !lineContentToProcess.StartsWith("#"))
                {
                    string descriptionLineToAdd = lineContentToProcess.Replace("\\n", "\n");
                    currentItemDescription = string.IsNullOrEmpty(currentItemDescription) ? descriptionLineToAdd : currentItemDescription + "\n" + descriptionLineToAdd;
                }
            }

            if (!string.IsNullOrEmpty(currentItemTitle))
            {
                m_roadmapItems.Add(new RoadmapItem
                {
                    Date = currentItemTargetDate,
                    Version = currentItemVersion,
                    Title = currentItemTitle,
                    Description = currentItemDescription ?? string.Empty,
                    Status = currentItemStatus
                });
            }
        }

        private string GetStatusLabel(EditorGUIElements.MessageType messageTypeToConvert)
        {
            switch (messageTypeToConvert)
            {
                case EditorGUIElements.MessageType.Success:
                    return "Done";
                case EditorGUIElements.MessageType.Info:
                    return "In Progress";
                case EditorGUIElements.MessageType.Warning:
                    return "Planned";
                default:
                    return string.Empty;
            }
        }

        private EditorGUIElements.MessageType GetStatusType(string statusTextToConvert)
        {
            switch (statusTextToConvert)
            {
                case "COMPLETED":
                    return EditorGUIElements.MessageType.Success;
                case "IN PROGRESS":
                    return EditorGUIElements.MessageType.Info;
                case "PLANNED":
                    return EditorGUIElements.MessageType.Warning;
                default:
                    return EditorGUIElements.MessageType.None;
            }
        }

        private Color GetStatusColor(EditorGUIElements.MessageType messageTypeToConvert)
        {
            switch (messageTypeToConvert)
            {
                case EditorGUIElements.MessageType.Success:
                    return EditorGUIExtended.ColorPalette.Success;
                case EditorGUIElements.MessageType.Info:
                    return EditorGUIExtended.ColorPalette.Info;
                case EditorGUIElements.MessageType.Warning:
                    return EditorGUIExtended.ColorPalette.Warning;
                default:
                    return Color.gray;
            }
        }
    }
}