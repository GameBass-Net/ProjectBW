/// <summary>
/// Project : Easy Build System
/// Class : BuildingControllerViewsEditor.cs
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
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers
{
    public static class BuildingControllerViewsEditor
    {
        public static void Draw(BuildingController target, Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (target == null)
            {
                return;
            }

            if (target.Views != null && target.Views.Length > 0)
            {
                for (int viewIndex = 0; viewIndex < target.Views.Length; viewIndex++)
                {
                    DrawViewItem(target, target.Views[viewIndex], viewIndex, editorProvider);
                }
            }
            else
            {
                EditorGUIExtended.HelpBox("No building views added.", EditorGUIElements.MessageType.Warning);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Building View...", EditorStyles.miniButton))
            {
                ShowAddViewMenu(target);
            }
        }

        private static void DrawViewItem(
            BuildingController target,
            BuildingView buildingViewToDraw,
            int viewItemIndex,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider)
        {
            if (buildingViewToDraw == null)
            {
                return;
            }

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent($"Building View - {buildingViewToDraw.Name}", "Camera view used to raycast and detect building targets in the scene."),
                string.Empty,
                () =>
                {
                    UnityEditor.Editor viewEditor = editorProvider != null ? editorProvider(buildingViewToDraw) : null;
                    EditorGUILayout.Separator();
                    viewEditor?.OnInspectorGUI();
                },
                contextMenu => DrawViewContextMenu(contextMenu, target, buildingViewToDraw, viewItemIndex));
        }

        private static void DrawViewContextMenu(
            GenericMenu contextMenuToPopulate,
            BuildingController target,
            BuildingView buildingViewTarget,
            int viewTargetIndex)
        {
            bool isViewCurrentlySelected = target.ActiveView != null && buildingViewTarget.ViewType == target.ActiveView.ViewType;

            EditorContextMenus.AddToggleItem(
                contextMenuToPopulate,
                isViewCurrentlySelected ? "Deselect View" : "Select View",
                isViewCurrentlySelected,
                _ =>
                {
                    Undo.RecordObject(target, "Change Build View");
                    target.SetView(buildingViewTarget.ViewType);
                    EditorUtility.SetDirty(target);
                });

            EditorContextMenus.Separator(contextMenuToPopulate);

            List<BuildingView> viewsList = target.Views.ToList();

            EditorContextMenus.AddMoveUpItem(contextMenuToPopulate, viewTargetIndex > 0, () =>
                EditorContextMenus.RunOnTargets(new[] { target }, "Move View Up", _ =>
                    EditorContextMenus.MoveItemInList(viewsList, buildingViewTarget.GetType(), -1)));

            EditorContextMenus.AddMoveDownItem(contextMenuToPopulate, viewTargetIndex < target.Views.Length - 1, () =>
                EditorContextMenus.RunOnTargets(new[] { target }, "Move View Down", _ =>
                    EditorContextMenus.MoveItemInList(viewsList, buildingViewTarget.GetType(), 1)));

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddRemoveComponentItem(contextMenuToPopulate, buildingViewTarget, () =>
                RemoveView(target, buildingViewTarget, viewTargetIndex));

            EditorContextMenus.AddResetItem(contextMenuToPopulate, () =>
                BuildingControllerEditor.ResetComponent(buildingViewTarget));

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddEditScriptItem(contextMenuToPopulate, buildingViewTarget);
        }

        private static void ShowAddViewMenu(BuildingController target)
        {
            GenericMenu viewSelectionMenu = new GenericMenu();

            List<Type> existingViewTypes = (target.Views ?? Array.Empty<BuildingView>())
                .Where(viewToCheck => viewToCheck != null)
                .Select(viewToCheck => viewToCheck.GetType())
                .ToList();

            List<Type> availableViewTypes = TypeCache.GetTypesDerivedFrom<BuildingView>()
                .Where(viewTypeToAdd => !viewTypeToAdd.IsAbstract && !existingViewTypes.Contains(viewTypeToAdd))
                .OrderBy(viewTypeToSort => viewTypeToSort.Name)
                .ToList();

            foreach (Type viewTypeToAdd in availableViewTypes)
            {
                string displayNameForViewType = ObjectNames.NicifyVariableName(viewTypeToAdd.Name);
                viewSelectionMenu.AddItem(new GUIContent(displayNameForViewType), false, () =>
                {
                    Undo.RecordObject(target, "Add Building View");

                    BuildingView newlyCreatedView = target.gameObject.AddComponent(viewTypeToAdd) as BuildingView;
                    if (newlyCreatedView != null)
                    {
                        newlyCreatedView.hideFlags = HideFlags.HideInInspector;
                    }

                    List<BuildingView> viewsListToModify = target.Views?.ToList() ?? new List<BuildingView>();
                    viewsListToModify.Add(newlyCreatedView);
                    target.Views = viewsListToModify.ToArray();

                    target.SetView(newlyCreatedView.ViewType);

                    EditorUtility.SetDirty(target);
                });
            }

            if (availableViewTypes.Count == 0)
            {
                EditorContextMenus.AddDisabledItem(viewSelectionMenu, "All building views added.");
            }

            viewSelectionMenu.ShowAsContext();
        }

        private static void RemoveView(BuildingController target, BuildingView viewToRemove, int removedViewIndex)
        {
            bool wasViewSelected = viewToRemove.ViewType == target.ActiveView.ViewType;
            List<BuildingView> viewsListToModify = target.Views.ToList();
            viewsListToModify.Remove(viewToRemove);

            UnityEngine.Object.DestroyImmediate(viewToRemove, true);
            target.Views = viewsListToModify.ToArray();

            if (wasViewSelected && target.Views.Length > 0)
            {
                int fallbackViewIndex = Mathf.Clamp(removedViewIndex - 1, 0, target.Views.Length - 1);
                if (target.Views[fallbackViewIndex] != null)
                {
                    target.SetView(target.Views[fallbackViewIndex].ViewType);
                }
            }

            EditorUtility.SetDirty(target);
        }
    }
}