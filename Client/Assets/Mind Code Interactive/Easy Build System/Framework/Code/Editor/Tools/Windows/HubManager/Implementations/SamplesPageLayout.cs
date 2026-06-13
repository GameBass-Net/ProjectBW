/// <summary>
/// Project : Easy Build System
/// Class : SamplesPageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.samples", "Easy Build System", "Samples", "Editor/Icons/play", 1)]
    public sealed class SamplesPageLayout : PageLayout
    {
        private sealed class SampleItem
        {
            public string Label { get; set; }
            public string Description { get; set; }
            public string SceneName { get; set; }
            public string Category { get; set; }
            public string CategoryDescription { get; set; }
            public int Order { get; set; }
        }

        private sealed class CategoryView
        {
            public string Name;
            public string Description;
            public readonly List<SampleItem> Samples = new List<SampleItem>();
        }

        private readonly List<CategoryView> m_orderedCategories = new List<CategoryView>();
        private readonly Dictionary<string, CategoryView> m_categories = new Dictionary<string, CategoryView>();

        public override void OnEnable()
        {
            m_categories.Clear();
            m_orderedCategories.Clear();

            TextAsset samplesTextAsset = Resources.Load<TextAsset>("Samples");
            if (samplesTextAsset != null)
            {
                ParseSamples(samplesTextAsset.text);
            }
        }

        public override void DrawLayout()
        {
            EditorGUIExtended.InspectorHeader(
                "Samples",
                "Browse and open the sample scenes included with the building system."
            );

            for (int i = 0; i < m_orderedCategories.Count; i++)
            {
                DrawCategorySection(m_orderedCategories[i]);
            }

            EditorGUIExtended.InspectorBottom();
        }

        private void ParseSamples(string contentToParse)
        {
            string currentLabel = null;
            string currentDescription = null;
            string currentSceneName = null;
            string currentCategory = null;
            string currentCategoryDescription = null;
            int currentOrder = 0;

            foreach (string rawLine in contentToParse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();

                if (line.StartsWith("# "))
                {
                    if (!string.IsNullOrEmpty(currentLabel))
                    {
                        RegisterSample(currentLabel, currentDescription, currentSceneName, currentCategory, currentCategoryDescription, currentOrder);
                    }

                    currentLabel = line.Substring(2).Trim();
                    currentDescription = null;
                    currentSceneName = null;
                    currentCategory = null;
                    currentCategoryDescription = null;
                    currentOrder = 0;
                }
                else if (line.StartsWith("Category: "))
                {
                    currentCategory = line.Substring(10).Trim();
                }
                else if (line.StartsWith("CategoryDescription: "))
                {
                    currentCategoryDescription = line.Substring(21).Trim();
                }
                else if (line.StartsWith("SceneName: "))
                {
                    currentSceneName = line.Substring(11).Trim();
                }
                else if (line.StartsWith("Order: "))
                {
                    int.TryParse(line.Substring(7).Trim(), out currentOrder);
                }
                else if (!string.IsNullOrEmpty(currentLabel) && !line.StartsWith("#"))
                {
                    string descriptionLine = line.Replace("\\n", "\n");
                    currentDescription = string.IsNullOrEmpty(currentDescription) ? descriptionLine : currentDescription + "\n" + descriptionLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLabel))
            {
                RegisterSample(currentLabel, currentDescription, currentSceneName, currentCategory, currentCategoryDescription, currentOrder);
            }

            foreach (KeyValuePair<string, CategoryView> kvp in m_categories)
            {
                kvp.Value.Samples.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
        }

        private void RegisterSample(string label, string description, string sceneName, string category, string categoryDescription, int order)
        {
            string categoryKey = string.IsNullOrWhiteSpace(category) ? "Uncategorized" : category.Trim();

            if (!m_categories.TryGetValue(categoryKey, out CategoryView categoryView))
            {
                categoryView = new CategoryView { Name = categoryKey };
                m_categories[categoryKey] = categoryView;
                m_orderedCategories.Add(categoryView);
            }

            if (!string.IsNullOrWhiteSpace(categoryDescription) && string.IsNullOrWhiteSpace(categoryView.Description))
            {
                categoryView.Description = categoryDescription.Trim();
            }

            categoryView.Samples.Add(new SampleItem
            {
                Label = label,
                Description = description ?? string.Empty,
                SceneName = sceneName ?? string.Empty,
                Category = categoryKey,
                CategoryDescription = categoryDescription ?? string.Empty,
                Order = order
            });
        }

        private static string ResolveScenePath(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            string normalizedSceneName = NormalizeSceneName(sceneName);

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

                if (string.IsNullOrWhiteSpace(scenePath))
                {
                    continue;
                }

                string fileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (string.Equals(NormalizeSceneName(fileName), normalizedSceneName, StringComparison.OrdinalIgnoreCase))
                {
                    return scenePath;
                }
            }

            return null;
        }

        private static string NormalizeSceneName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value
                .Trim()
                .Replace('–', '-')
                .Replace('—', '-')
                .Replace('‒', '-')
                .Replace('−', '-')
                .Replace('\u00A0', ' ');
        }

        private void DrawCategorySection(CategoryView categoryView)
        {
            EditorGUIExtended.DrawExpandableSection(
                categoryView.Name,
                string.Empty,
                string.IsNullOrWhiteSpace(categoryView.Description) ? string.Empty : categoryView.Description,
                () => DrawSampleList(categoryView.Samples),
                false,
                true
            );
        }

        private void DrawSampleList(List<SampleItem> samples)
        {
            if (samples == null || samples.Count == 0)
            {
                GUILayout.Label("No samples defined for this category.", EditorStyles.miniLabel);
                return;
            }

            for (int i = 0; i < samples.Count; i++)
            {
                SampleItem sample = samples[i];
                if (string.IsNullOrEmpty(sample.SceneName))
                {
                    continue;
                }

                EditorGUIExtended.BeginBorderLayoutVertical(true, true);

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(sample.Label, EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                if (EditorGUIExtended.Button("Open Scene", GUILayout.Width(120)))
                {
                    string resolvedPath = ResolveScenePath(sample.SceneName);
                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        EditorSceneManager.OpenScene(resolvedPath, OpenSceneMode.Single);
                    }
                    else
                    {
                        Debug.LogWarning("[Easy Build System] Scene not found in project: " + sample.SceneName);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrWhiteSpace(sample.Description))
                {
                    GUILayout.Label(sample.Description, EditorStyles.wordWrappedMiniLabel);
                }

                EditorGUIExtended.EndBorderLayoutVertical();

                GUILayout.Space(3f);
            }
        }
    }
}