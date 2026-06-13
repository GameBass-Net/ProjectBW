/// <summary>
/// Project : Easy Build System
/// Class : IntegrityChecksPageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.integrityCheck", "Easy Build System", "Integrity Checks", "Editor/Icons/gear", 3)]
    public class IntegrityChecksPageLayout : PageLayout
    {
        private Vector2 m_integrityScrollPosition;
        private float m_integrityProgress;
        private string m_integrityProgressText = string.Empty;
        private int m_currentCheckIndex;
        private int m_totalChecks;

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Integrity Checks",
                "Configure render pipeline settings and validate project integrity."
            );

            DrawRenderPipelineManagement();
            EditorGUIExtended.Separator();
            DrawProjectIntegrityCheck();
            EditorGUIExtended.InspectorBottom();
        }

        private void DrawRenderPipelineManagement()
        {
            EditorGUIExtended.BeginVertical();

            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);
                EditorGUIExtended.ColoredLabel("Render Pipeline Management", Color.white, EditorGUILabels.LabelType.Bold);

                RenderPipelineAsset currentRenderPipeline = GraphicsSettings.currentRenderPipeline;
                string pipelineDisplayNameToShow = "Built-In Render Pipeline";

                if (currentRenderPipeline != null)
                {
                    string pipelineTypeNameToCheck = currentRenderPipeline.GetType().ToString();

                    if (pipelineTypeNameToCheck.Contains("Universal"))
                    {
                        pipelineDisplayNameToShow = "URP";
                    }
                    else if (pipelineTypeNameToCheck.Contains("HighDefinition"))
                    {
                        pipelineDisplayNameToShow = "HDRP";
                    }
                }

                EditorGUIExtended.Label("Current Pipeline : " + pipelineDisplayNameToShow);
                GUILayout.Space(3f);

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !RepositoryManager.IsRunning;

                if (EditorGUIExtended.Button("Upgrade Package Assets to URP"))
                {
                    SwitchRenderPipeline(toHDRP: false);
                }

                if (EditorGUIExtended.Button("Upgrade Package Assets to HDRP"))
                {
                    SwitchRenderPipeline(toHDRP: true);
                }

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5f);
            EditorGUIExtended.EndVertical();
        }

        private void DrawProjectIntegrityCheck()
        {
            EditorGUIExtended.BeginVertical();

            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);
                EditorGUIExtended.ColoredLabel("Project Integrity Check", Color.white, EditorGUILabels.LabelType.Bold);

                if (RepositoryManager.IsRunning)
                {
                    DrawProgressBar();
                }
                else
                {
                    DrawIntegrityResults();
                }

                using (EditorGUIExtended.DisabledScope(RepositoryManager.IsRunning))
                {
                    if (EditorGUIExtended.Button("Check Project Integrity..."))
                    {
                        RunIntegrityCheckWithFixes();
                    }

                    if (GUILayout.Button("Reset Project Initialization...", EditorStyles.miniButton))
                    {
                        if (EditorUtility.DisplayDialog(
                                "Reset Initialization",
                                "This will reset the project initialization state and run the initialization again.\n\nAre you sure?",
                                "Reset",
                                "Cancel"))
                        {
                            RepositoryManager.ResetValidationState();
                            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                        }
                    }
                }
            }

            GUILayout.Space(5f);
            EditorGUIExtended.EndVertical();
        }

        private void DrawProgressBar()
        {
            GUILayout.Label("Running Integrity Check...");
            GUILayout.Space(3f);

            Rect progressBarRectangle = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(progressBarRectangle, m_integrityProgress, m_integrityProgressText);

            if (!string.IsNullOrEmpty(m_integrityProgressText))
            {
                GUILayout.Space(3f);
                GUILayout.Label("Step " + m_currentCheckIndex + "/" + m_totalChecks + ": " + m_integrityProgressText, EditorStyles.miniLabel);
                GUILayout.Space(5f);
            }
        }

        private void DrawIntegrityResults()
        {
            GUILayout.Label("System Status :");
            GUILayout.Space(5f);

            m_integrityScrollPosition = EditorGUILayout.BeginScrollView(
                m_integrityScrollPosition,
                GUILayout.ExpandHeight(false));

            List<IntegrityResult> integrityResultsList = RepositoryManager.GetLastResults();

            if (integrityResultsList.Count == 0)
            {
                DrawStatusCard("System Integrity", "Ready for integrity check", true);
            }
            else
            {
                foreach (IntegrityResult integrityResultData in integrityResultsList)
                {
                    if (integrityResultData.IsValid)
                    {
                        DrawStatusCard(
                            integrityResultData.Id,
                            integrityResultData.Id + " validation completed successfully.",
                            true);
                    }
                    else
                    {
                        string failureReasonLine = string.IsNullOrEmpty(integrityResultData.Reason) ? string.Empty : ("\n" + integrityResultData.Reason);

                        DrawStatusCard(
                            integrityResultData.Id,
                            integrityResultData.Id + " validation failed." + failureReasonLine,
                            false);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatusCard(string statusCardTitle, string statusCardDescription, bool isStatusValid)
        {
            Color statusIconColorToUse = isStatusValid ? EditorGUIExtended.ColorPalette.Success : EditorGUIExtended.ColorPalette.Error;

            EditorGUIExtended.BeginBorderLayoutVertical();

            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.BeginHorizontal();
                EditorGUIExtended.ColoredLabel(isStatusValid ? "✓" : "✗", statusIconColorToUse, EditorGUILabels.LabelType.Bold);
                EditorGUIExtended.ColoredLabel(statusCardTitle, statusIconColorToUse, EditorGUILabels.LabelType.Bold);
                GUILayout.EndHorizontal();
                EditorGUIExtended.ColoredLabel(statusCardDescription, EditorStyles.label.normal.textColor, EditorGUILabels.LabelType.Normal);
            }

            EditorGUIExtended.EndBorderLayoutVertical();
            GUILayout.Space(5f);
        }

        private void SwitchRenderPipeline(bool toHDRP)
        {
            string[] snapshotGuids = AssetDatabase.FindAssets("t:RenderPipelineUpgradeSnapshot");
            string snapshotPath = snapshotGuids != null && snapshotGuids.Length > 0 ? AssetDatabase.GUIDToAssetPath(snapshotGuids[0]) : null;
            RenderPipelineUpgradeSnapshot snapshot = string.IsNullOrEmpty(snapshotPath) ? null : AssetDatabase.LoadAssetAtPath<RenderPipelineUpgradeSnapshot>(snapshotPath);

            if (!snapshot)
            {
                Debug.LogError("RenderPipelineUpgradeSnapshot not found.");
                return;
            }

            if (toHDRP)
            {
                RenderPipelineUpgrader.ConvertToHDRP(snapshot);
            }
            else
            {
                RenderPipelineUpgrader.ConvertToURP(snapshot);
            }

            EditorApplication.delayCall += RunIntegrityCheckWithFixes;
        }

        private void RunIntegrityCheckWithFixes()
        {
            RepositoryManifest proManifest = Resources.Load<RepositoryManifest>("EbsRepositoryManifest");
            RepositoryManager.RunChecksWithFixes(proManifest, OnProgressUpdate, OnCheckComplete);
        }

        private void OnProgressUpdate(float progressValue, string progressMessage, int currentStepIndex, int totalStepsCount)
        {
            m_integrityProgress = progressValue;
            m_integrityProgressText = progressMessage;
            m_currentCheckIndex = currentStepIndex;
            m_totalChecks = totalStepsCount;
        }

        private void OnCheckComplete(string checkIdentifier, string checkMessage, bool isCheckValid)
        {
            if (checkIdentifier == "System")
            {
                m_integrityProgress = 0f;
                m_integrityProgressText = string.Empty;
            }
        }
    }
}