/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartBehaviorsEditor.cs
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
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartBehaviorsEditor
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

            List<BuildingBehavior> allBehaviors = primaryTarget.BehaviorSystem.GetAllBehaviors();

            if (allBehaviors == null || allBehaviors.Count == 0)
            {
                EditorGUIExtended.Label("No Building Behavior(s) added yet.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }
            else
            {
                foreach (BuildingBehavior behavior in allBehaviors.Where(b => b != null))
                {
                    DrawBehavior(behavior, targets, editorProvider);
                }
            }

            EditorGUIExtended.Separator();

            if (GUILayout.Button("Add Building Behavior...", EditorStyles.miniButton))
            {
                ShowAddBehaviorsMenu(targets, allBehaviors ?? new List<BuildingBehavior>());
            }
        }

        private static void DrawBehavior(
            BuildingBehavior behavior,
            BuildingPart[] targets,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (!behavior)
            {
                return;
            }

            behavior.hideFlags = HideFlags.HideInInspector;

            Texture2D stateIcon = behavior.IsDisabled
                ? Resources.Load<Texture2D>("Editor/Icons/off")
                : Resources.Load<Texture2D>("Editor/Icons/on");

            UnityEditor.Editor behaviorEditor = editorProvider != null ? editorProvider(behavior) : null;
            if (!behaviorEditor)
            {
                return;
            }

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent(behavior.Name, stateIcon),
                string.Empty,
                () =>
                {
                    EditorGUIExtended.InspectorHeader(behavior.Name, behavior.Description);
                    EditorGUI.BeginDisabledGroup(behavior.IsDisabled);
                    behaviorEditor.OnInspectorGUI();
                    EditorGUI.EndDisabledGroup();
                },
                menu => DrawBehaviorContextMenu(menu, behavior, targets),
                false);
        }

        private static void ShowAddBehaviorsMenu(BuildingPart[] targets, List<BuildingBehavior> allBehaviors)
        {
            GenericMenu menu = new GenericMenu();

            List<Type> allBehaviorTypes = TypeCache.GetTypesDerivedFrom<BuildingBehavior>()
                .Where(type => !type.IsAbstract)
                .ToList();

            HashSet<Type> existingBehaviorTypes = new HashSet<Type>(
                allBehaviors.Where(behavior => behavior != null)
                            .Select(behavior => behavior.GetType()));

            List<Type> availableBehaviorTypes = allBehaviorTypes
                .Where(type => !existingBehaviorTypes.Contains(type))
                .ToList();

            foreach (Type behaviorType in availableBehaviorTypes)
            {
                string displayName = GetBehaviorDisplayName(behaviorType);

                EditorContextMenus.AddItem(menu, displayName, () =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Add Building Behavior", part =>
                    {
                        part.BehaviorSystem.AddBehavior(behaviorType);
                    });

                    Debug.Log($"Behavior '{displayName}' added to {targets.Length} Building Part(s).", Selection.activeObject);
                });
            }

            if (availableBehaviorTypes.Count == 0)
            {
                EditorContextMenus.AddDisabledItem(menu, "All behaviors already added.");
            }

            menu.ShowAsContext();
        }

        private static string GetBehaviorDisplayName(Type behaviorType)
        {
            BuildingBehaviorAttribute attribute = Attribute.GetCustomAttribute(behaviorType,
                typeof(BuildingBehaviorAttribute)) as BuildingBehaviorAttribute;

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }

            return behaviorType.Name;
        }

        private static void DrawBehaviorContextMenu(GenericMenu menu, BuildingBehavior behavior, BuildingPart[] targets)
        {
            Type behaviorType = behavior.GetType();
            string behaviorName = behavior.Name;

            EditorContextMenus.AddToggleItem(
                menu,
                behavior.IsDisabled ? "Enable Behavior" : "Disable Behavior",
                behavior.IsDisabled,
                isDisabled =>
                {
                    EditorContextMenus.RunOnTargets(targets, isDisabled ? "Disable Behavior" : "Enable Behavior", part =>
                    {
                        BuildingBehavior targetBehavior = part.BehaviorSystem.GetBehavior(behaviorType);
                        if (!targetBehavior)
                        {
                            return;
                        }

                        targetBehavior.IsDisabled = isDisabled;

                        if (isDisabled)
                        {
                            targetBehavior.Shutdown();
                        }
                        else
                        {
                            targetBehavior.Initialize(part);
                        }
                    });
                });

            EditorContextMenus.AddRemoveComponentItem(
                menu,
                behavior,
                () =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Remove Behavior", part =>
                    {
                        part.BehaviorSystem.RemoveBehavior(behaviorType);
                    });
                });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddMoveItemsForType(
                menu,
                targets,
                behaviorType,
                part => part.BehaviorSystem.GetAllBehaviors(),
                "Behavior");

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddCopyPasteForType(
                menu,
                behaviorType,
                () =>
                {
                    EditorContextMenus.SetJsonClipboard(behaviorType, JsonUtility.ToJson(behavior));
                },
                jsonData =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Paste Behavior", part =>
                    {
                        BuildingBehavior targetBehavior = part.BehaviorSystem.GetBehavior(behaviorType)
                            ?? part.BehaviorSystem.AddBehavior(behaviorType);
                        JsonUtility.FromJsonOverwrite(jsonData, targetBehavior);
                    });
                });

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddResetItem(
                menu,
                () =>
                {
                    EditorContextMenus.RunOnTargets(targets, "Reset Behavior", part =>
                    {
                        BuildingBehavior targetBehavior = part.BehaviorSystem.GetBehavior(behaviorType);
                        if (targetBehavior)
                        {
                            ResetBehavior(targetBehavior);
                        }
                    });
                });

            EditorContextMenus.AddEditScriptItem(menu, behavior);
        }

        private static void ResetBehavior(BuildingBehavior behavior)
        {
            GameObject tempGameObject = new GameObject("TempBehavior");
            tempGameObject.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                BuildingBehavior tempBehavior = tempGameObject.AddComponent(behavior.GetType()) as BuildingBehavior;
                if (tempBehavior != null)
                {
                    BuildingPart part = behavior.Part;
                    bool isDisabled = behavior.IsDisabled;
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(tempBehavior), behavior);
                    behavior.Part = part;
                    behavior.IsDisabled = isDisabled;

                    MethodInfo onResetMethod = behavior.GetType().GetMethod("OnReset",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (onResetMethod != null)
                    {
                        onResetMethod.Invoke(behavior, null);
                    }

                    EditorUtility.SetDirty(behavior);
                    if (part != null)
                    {
                        EditorUtility.SetDirty(part);
                    }

                    Debug.Log("Behavior '" + behavior.Name + "' on Building Part '" + (part != null ? part.Name : "?") + "' reset to defaults.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tempGameObject);
            }
        }
    }
}
