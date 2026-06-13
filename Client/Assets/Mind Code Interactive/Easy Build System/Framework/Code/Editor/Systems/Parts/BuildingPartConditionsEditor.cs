/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartConditionsEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartConditionsEditor
    {
        public static void Draw(
            BuildingPart primaryTarget,
            BuildingPart[] targets,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (primaryTarget == null)
            {
                return;
            }

            List<BuildingCondition> allConditions = primaryTarget.ConditionSystem.GetAllConditions();

            if (allConditions == null || allConditions.Count == 0)
            {
                EditorGUIExtended.Label("No Building Condition(s) added yet.",
                    EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
                EditorGUILayout.Separator();
            }
            else
            {
                foreach (BuildingCondition condition in allConditions)
                {
                    DrawCondition(condition, targets, editorProvider);
                }
            }

            EditorGUIExtended.Separator();

            if (GUILayout.Button("Add Building Condition...", EditorStyles.miniButton))
            {
                ShowAddConditionMenu(targets, allConditions ?? new List<BuildingCondition>());
            }
        }

        public static void DisableGizmos(BuildingPart primaryTarget)
        {
            if (primaryTarget == null)
            {
                return;
            }

            List<BuildingCondition> allConditions = primaryTarget.ConditionSystem.GetAllConditions();
            if (allConditions == null)
            {
                return;
            }

            for (int i = 0; i < allConditions.Count; i++)
            {
                BuildingCondition condition = allConditions[i];
                if (condition)
                {
                    condition.ShowGizmos = false;
                }
            }
        }

        private static void DrawCondition(
            BuildingCondition condition,
            BuildingPart[] targets,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (condition == null)
            {
                return;
            }

            Texture2D stateIcon = condition.IsDisabled
                ? Resources.Load<Texture2D>("Editor/Icons/off")
                : Resources.Load<Texture2D>("Editor/Icons/on");

            SerializedObject serializedObjectForCondition = new SerializedObject(condition);
            SerializedProperty iterator = serializedObjectForCondition.GetIterator();
            iterator.NextVisible(true);
            bool isExpanded = iterator.isExpanded;

            bool isOpen = EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent(condition.Name, stateIcon),
                string.Empty,
                () =>
                {
                    EditorGUIExtended.InspectorHeader(condition.Name, condition.Description);
                    EditorGUI.BeginDisabledGroup(condition.IsDisabled);
                    UnityEditor.Editor conditionEditor = editorProvider != null ? editorProvider(condition) : null;
                    if (conditionEditor != null)
                    {
                        conditionEditor.OnInspectorGUI();
                    }

                    EditorGUI.EndDisabledGroup();
                },
                menu => DrawConditionContextMenu(menu, condition, targets),
                false);

            condition.ShowGizmos = isOpen;
            iterator.isExpanded = isExpanded;
        }

        private static void ShowAddConditionMenu(BuildingPart[] targets, List<BuildingCondition> allConditions)
        {
            GenericMenu menu = new GenericMenu();

            List<Type> allTypes = TypeCache.GetTypesDerivedFrom<BuildingCondition>()
                .Where(t => !t.IsAbstract)
                .ToList();

            HashSet<Type> existingTypes = new HashSet<Type>(
                allConditions.Where(c => c != null).Select(c => c.GetType()));

            List<Type> availableTypes = allTypes
                .Where(t => !existingTypes.Contains(t))
                .OrderBy(t =>
                {
                    BuildingConditionAttribute attr =
                        (BuildingConditionAttribute)Attribute.GetCustomAttribute(t, typeof(BuildingConditionAttribute));
                    return !string.IsNullOrEmpty(attr?.Name)
                        ? attr.Name
                        : ObjectNames.NicifyVariableName(t.Name);
                }, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (Type type in availableTypes)
            {
                BuildingConditionAttribute attr =
                    (BuildingConditionAttribute)Attribute.GetCustomAttribute(type, typeof(BuildingConditionAttribute));

                string displayName = !string.IsNullOrEmpty(attr?.Name)
                    ? attr.Name
                    : ObjectNames.NicifyVariableName(type.Name);

                menu.AddItem(new GUIContent(displayName), false, () =>
                {
                    int count = 0;

                    foreach (BuildingPart part in targets)
                    {
                        if (!part)
                        {
                            continue;
                        }

                        if (part.ConditionSystem.GetCondition(type) != null)
                        {
                            continue;
                        }

                        Undo.RecordObject(part, "Add Building Condition");
                        part.ConditionSystem.AddCondition(type);
                        EditorUtility.SetDirty(part);
                        count++;
                    }

                    SceneView.RepaintAll();
                    Debug.Log($"Condition '{displayName}' added to {count} Building Part(s).", Selection.activeObject);
                });
            }

            if (availableTypes.Count == 0)
            {
                EditorContextMenus.AddDisabledItem(menu, "All conditions already added.");
            }

            menu.ShowAsContext();
        }

        private static void DrawConditionContextMenu(GenericMenu menu, BuildingCondition condition, BuildingPart[] targets)
        {
            Type conditionType = condition.GetType();
            string conditionName = condition.Name;

            EditorContextMenus.AddToggleItem(
                menu,
                condition.IsDisabled ? "Enable Condition" : "Disable Condition",
                condition.IsDisabled,
                isDisabled =>
                {
                    EditorContextMenus.RunOnTargets(targets, isDisabled ? "Disable Condition" : "Enable Condition", part =>
                    {
                        BuildingCondition targetCondition = part.ConditionSystem.GetCondition(conditionType);
                        if (!targetCondition)
                        {
                            return;
                        }

                        targetCondition.IsDisabled = isDisabled;
                        EditorUtility.SetDirty(targetCondition);
                    });

                    Debug.Log($"Condition '{conditionName}' {(isDisabled ? "disabled" : "enabled")}.", Selection.activeObject);
                });

            EditorContextMenus.AddRemoveComponentItem(
                menu,
                condition,
                () =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Remove Condition", part =>
                    {
                        part.ConditionSystem.RemoveCondition(conditionType);
                    });

                    Debug.Log($"Condition '{conditionName}' removed.", Selection.activeObject);
                });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddMoveItemsForType(
                menu,
                targets,
                conditionType,
                part => part.ConditionSystem.GetAllConditions(),
                "Condition");

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddCopyPasteForType(
                menu,
                conditionType,
                () =>
                {
                    EditorContextMenus.SetJsonClipboard(conditionType, JsonUtility.ToJson(condition));
                    Debug.Log($"Condition '{conditionName}' copied.", Selection.activeObject);
                },
                jsonData =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Paste Condition", part =>
                    {
                        BuildingCondition targetCondition = part.ConditionSystem.GetCondition(conditionType);
                        if (targetCondition == null)
                        {
                            targetCondition = part.ConditionSystem.AddCondition(conditionType);
                            EditorUtility.SetDirty(part);
                        }

                        Undo.RecordObject(targetCondition, "Paste Condition");
                        JsonUtility.FromJsonOverwrite(jsonData, targetCondition);
                        EditorUtility.SetDirty(targetCondition);
                    });

                    Debug.Log($"Condition '{conditionName}' pasted.", Selection.activeObject);
                });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddResetItem(
                menu,
                () =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Reset Condition", part =>
                    {
                        BuildingCondition targetCondition = part.ConditionSystem.GetCondition(conditionType);
                        if (targetCondition)
                        {
                            ResetCondition(targetCondition);
                            EditorUtility.SetDirty(targetCondition);
                        }
                    });

                    Debug.Log($"Condition '{conditionName}' reset.", Selection.activeObject);
                });

            EditorContextMenus.AddEditScriptItem(menu, condition);
        }

        private static void ResetCondition(BuildingCondition condition)
        {
            GameObject tempGameObject = new GameObject("TempCondition");
            tempGameObject.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                BuildingCondition tempCondition = tempGameObject.AddComponent(condition.GetType()) as BuildingCondition;
                if (tempCondition != null)
                {
                    BuildingPart part = condition.Part;
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(tempCondition), condition);
                    condition.Part = part;

                    MethodInfo onResetMethod = condition.GetType().GetMethod("OnReset",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (onResetMethod != null)
                    {
                        onResetMethod.Invoke(condition, null);
                    }

                    EditorUtility.SetDirty(condition);
                    if (part != null)
                    {
                        EditorUtility.SetDirty(part);
                    }

                    Debug.Log("Condition '" + condition.Name + "' on Building Part '" + (part != null ? part.Name : "?") + "' reset to defaults.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tempGameObject);
            }
        }
    }
}
