/// <summary>
/// Project : Easy Build System
/// Class : BuildingControllerStatesEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers
{
    public static class BuildingControllerStatesEditor
    {
        public static void Draw(BuildingController target, Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (target == null)
            {
                return;
            }

            if (target.States != null && target.States.Length > 0)
            {
                for (int stateIndex = 0; stateIndex < target.States.Length; stateIndex++)
                {
                    DrawStateItem(target, target.States[stateIndex], stateIndex, editorProvider);
                }
            }
            else
            {
                EditorGUIExtended.HelpBox("No building states added.", EditorGUIElements.MessageType.Warning);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Building State...", EditorStyles.miniButton))
            {
                ShowAddStateMenu(target);
            }
        }

        private static void DrawStateItem(
            BuildingController target,
            BuildingState buildingStateToDraw,
            int stateItemIndex,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (buildingStateToDraw == null)
            {
                return;
            }

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent($"Building State - {buildingStateToDraw.Name}", "Handles placement, validation and cancellation logic for its corresponding building mode."),
                string.Empty,
                () =>
                {
                    UnityEditor.Editor stateEditor = editorProvider != null ? editorProvider(buildingStateToDraw) : null;
                    EditorGUILayout.Separator();
                    stateEditor?.OnInspectorGUI();
                },
                contextMenu => DrawStateContextMenu(contextMenu, target, buildingStateToDraw, stateItemIndex));
        }

        private static void DrawStateContextMenu(
            GenericMenu contextMenuToPopulate,
            BuildingController target,
            BuildingState buildingStateTarget,
            int stateTargetIndex)
        {
            List<BuildingState> statesList = target.States.ToList();

            EditorContextMenus.AddMoveUpItem(contextMenuToPopulate, stateTargetIndex > 0, () =>
                EditorContextMenus.RunOnTargets(new[] { target }, "Move State Up", _ =>
                    EditorContextMenus.MoveItemInList(statesList, buildingStateTarget.GetType(), -1)));

            EditorContextMenus.AddMoveDownItem(contextMenuToPopulate, stateTargetIndex < target.States.Length - 1, () =>
                EditorContextMenus.RunOnTargets(new[] { target }, "Move State Down", _ =>
                    EditorContextMenus.MoveItemInList(statesList, buildingStateTarget.GetType(), 1)));

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddRemoveComponentItem(contextMenuToPopulate, buildingStateTarget, () =>
                RemoveState(target, buildingStateTarget));

            EditorContextMenus.AddResetItem(contextMenuToPopulate, () =>
                BuildingControllerEditor.ResetComponent(buildingStateTarget));

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddEditScriptItem(contextMenuToPopulate, buildingStateTarget);
        }

        private static void ShowAddStateMenu(BuildingController target)
        {
            GenericMenu stateSelectionMenu = new GenericMenu();

            List<Type> existingStateTypes = (target.States ?? Array.Empty<BuildingState>())
                .Where(stateToCheck => stateToCheck != null)
                .Select(stateToCheck => stateToCheck.GetType())
                .ToList();

            List<Type> availableStateTypes = TypeCache.GetTypesDerivedFrom<BuildingState>()
                .Where(stateTypeToAdd => !stateTypeToAdd.IsAbstract && !existingStateTypes.Contains(stateTypeToAdd))
                .OrderBy(stateTypeToSort => stateTypeToSort.Name)
                .ToList();

            foreach (Type stateTypeToAdd in availableStateTypes)
            {
                string displayNameForStateType = ObjectNames.NicifyVariableName(stateTypeToAdd.Name);
                stateSelectionMenu.AddItem(new GUIContent(displayNameForStateType), false, () =>
                {
                    Undo.RecordObject(target, "Add Building State");

                    BuildingState newlyCreatedState = target.gameObject.AddComponent(stateTypeToAdd) as BuildingState;
                    if (newlyCreatedState != null)
                    {
                        newlyCreatedState.hideFlags = HideFlags.HideInInspector;
                    }

                    List<BuildingState> statesListToModify = target.States?.ToList() ?? new List<BuildingState>();
                    statesListToModify.Add(newlyCreatedState);
                    target.States = statesListToModify.ToArray();

                    EditorUtility.SetDirty(target);
                });
            }

            if (availableStateTypes.Count == 0)
            {
                EditorContextMenus.AddDisabledItem(stateSelectionMenu, "All building states added.");
            }

            stateSelectionMenu.ShowAsContext();
        }

        private static void RemoveState(BuildingController target, BuildingState stateToRemove)
        {
            List<BuildingState> statesListToModify = target.States.ToList();
            statesListToModify.Remove(stateToRemove);
            target.States = statesListToModify.ToArray();
            EditorUtility.SetDirty(target);
        }
    }
}