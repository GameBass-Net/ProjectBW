/// <summary>
/// Project : Easy Build System
/// Class : BuildingRulesEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Managers
{
    public static class BuildingRulesEditor
    {
        public static void Draw(
            Component host,
            List<BuildingRule> rules,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (host == null || rules == null)
            {
                return;
            }

            List<BuildingRule> filteredRules = rules.Where(rule => rule != null).ToList();

            if (filteredRules.Count == 0)
            {
                EditorGUIExtended.Label("No building rules added yet.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }
            else
            {
                foreach (BuildingRule rule in filteredRules)
                {
                    DrawRule(host, rules, rule, editorProvider);
                }
            }

            EditorGUIExtended.Separator();

            if (GUILayout.Button("Add Building Rule...", EditorStyles.miniButton))
            {
                ShowAddRuleMenu(host, rules);
            }
        }

        public static void HideInInspector(IEnumerable<BuildingRule> rules)
        {
            if (rules == null)
            {
                return;
            }

            foreach (BuildingRule rule in rules)
            {
                if (rule != null)
                {
                    rule.hideFlags = HideFlags.HideInInspector;
                }
            }
        }

        private static void DrawRule(
            Component host,
            List<BuildingRule> rules,
            BuildingRule rule,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            Texture2D statusIcon = rule.Enabled
                ? Resources.Load<Texture2D>("Editor/Icons/on")
                : Resources.Load<Texture2D>("Editor/Icons/off");

            SerializedObject ruleSerializedObject = new SerializedObject(rule);
            SerializedProperty iterator = ruleSerializedObject.GetIterator();
            iterator.NextVisible(true);
            bool wasExpanded = iterator.isExpanded;

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent($"Building Rule - {rule.RuleName}", statusIcon),
                string.Empty,
                () =>
                {
                    EditorGUIExtended.InspectorDescription(rule.RuleDescription);

                    UnityEditor.Editor customEditor = editorProvider != null ? editorProvider(rule) : null;
                    if (customEditor != null)
                    {
                        EditorGUI.BeginDisabledGroup(!rule.Enabled);
                        customEditor.OnInspectorGUI();
                        EditorGUI.EndDisabledGroup();
                    }
                },
                contextMenu => DrawRuleContextMenu(contextMenu, host, rules, rule));

            iterator.isExpanded = wasExpanded;
            ruleSerializedObject.ApplyModifiedProperties();
        }

        private static void DrawRuleContextMenu(
            GenericMenu contextMenu,
            Component host,
            List<BuildingRule> rules,
            BuildingRule rule)
        {
            EditorContextMenus.AddToggleItem(
                contextMenu,
                rule.Enabled ? "Disable" : "Enable",
                rule.Enabled,
                newEnabled =>
                {
                    rule.Enabled = newEnabled;
                    EditorUtility.SetDirty(rule);
                    EditorUtility.SetDirty(host);
                });

            EditorContextMenus.AddRemoveComponentItem(contextMenu, rule, () =>
            {
                rules.Remove(rule);
                EditorUtility.SetDirty(host);
            });

            EditorContextMenus.Separator(contextMenu);

            EditorContextMenus.AddCopyPasteForType(
                contextMenu,
                rule.GetType(),
                () => EditorContextMenus.SetJsonClipboard(rule.GetType(), JsonUtility.ToJson(rule)),
                json =>
                {
                    JsonUtility.FromJsonOverwrite(json, rule);
                    EditorUtility.SetDirty(rule);
                    EditorUtility.SetDirty(host);
                });

            EditorContextMenus.Separator(contextMenu);

            EditorContextMenus.AddResetItem(contextMenu, () =>
            {
                GameObject temp = new GameObject("Temp");
                temp.hideFlags = HideFlags.HideAndDontSave;

                try
                {
                    BuildingRule defaults = temp.AddComponent(rule.GetType()) as BuildingRule;
                    if (defaults != null)
                    {
                        string defaultJson = JsonUtility.ToJson(defaults);
                        JsonUtility.FromJsonOverwrite(defaultJson, rule);
                        EditorUtility.SetDirty(rule);
                        EditorUtility.SetDirty(host);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(temp);
                }
            });

            EditorContextMenus.AddEditScriptItem(contextMenu, rule);
        }

        private static void ShowAddRuleMenu(Component host, List<BuildingRule> rules)
        {
            GenericMenu menu = new GenericMenu();

            List<Type> derivedTypes = TypeCache.GetTypesDerivedFrom<BuildingRule>()
                .Where(type => !type.IsAbstract)
                .ToList();

            HashSet<Type> alreadyAdded = new HashSet<Type>(
                rules.Where(rule => rule != null).Select(rule => rule.GetType()));

            List<Type> remaining = derivedTypes
                .Where(type => !alreadyAdded.Contains(type))
                .OrderBy(type => type.Name)
                .ToList();

            foreach (Type ruleType in remaining)
            {
                string displayName = GetRuleDisplayName(ruleType);

                menu.AddItem(new GUIContent(displayName), false, () =>
                {
                    BuildingRule newRule = host.gameObject.AddComponent(ruleType) as BuildingRule;
                    if (newRule != null)
                    {
                        newRule.hideFlags = HideFlags.HideInInspector;
                        rules.Add(newRule);
                        EditorUtility.SetDirty(host);
                        EditorUtility.SetDirty(newRule);
                    }
                });
            }

            if (remaining.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("All rules added"));
            }

            menu.ShowAsContext();
        }

        private static string GetRuleDisplayName(Type ruleType)
        {
            BuildingRuleAttribute attribute = Attribute.GetCustomAttribute(
                ruleType,
                typeof(BuildingRuleAttribute)) as BuildingRuleAttribute;

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }

            return ObjectNames.NicifyVariableName(ruleType.Name);
        }
    }
}
