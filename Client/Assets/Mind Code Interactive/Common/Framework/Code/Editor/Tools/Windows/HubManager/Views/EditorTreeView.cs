/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorTreeView.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Views
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Views
{
#if UNITY_6000_3_OR_NEWER
#pragma warning disable CS0618
#endif
    public class EditorTreeView : TreeView
    {
        private List<TreePageData> m_treePageDataList;
        private Action<TreePageData> m_onSelectionChangedCallback;

        public EditorTreeView(TreeViewState viewState, List<TreePageData> treePageData, Action<TreePageData> selectionChangedCallback) : base(viewState)
        {
            m_treePageDataList = treePageData;
            m_onSelectionChangedCallback = selectionChangedCallback;

            showBorder = true;
            showAlternatingRowBackgrounds = false;
            rowHeight = 22f;
            depthIndentWidth = 15f;
            baseIndent = 5f;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem rootItem = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            int nextItemId = 1;
            foreach (TreePageData pageData in m_treePageDataList)
            {
                TreeViewItem createdTreeItem = CreateTreeViewItem(pageData, ref nextItemId);
                rootItem.AddChild(createdTreeItem);
            }

            return rootItem;
        }

        protected override void SelectionChanged(IList<int> selectedItemIds)
        {
            if (selectedItemIds.Count == 0)
            {
                return;
            }

            EditorTreeViewItem selectedTreeItem = FindItem(selectedItemIds[0], rootItem) as EditorTreeViewItem;
            if (selectedTreeItem != null)
            {
                m_onSelectionChangedCallback?.Invoke(selectedTreeItem.PageData);
            }
        }

        protected override void RowGUI(RowGUIArgs rowGuiArgs)
        {
            Color evenRowColor;
            Color oddRowColor;

            if (EditorGUIUtility.isProSkin)
            {
                evenRowColor = new Color(0.2f, 0.2f, 0.2f);
                oddRowColor = new Color(0f, 0f, 0f, 0f);
            }
            else
            {
                evenRowColor = new Color(0.85f, 0.85f, 0.85f);
                oddRowColor = new Color(0f, 0f, 0f, 0f);
            }

            if (Event.current.type == EventType.Repaint)
            {
                Rect rowRect = rowGuiArgs.rowRect;
                EditorGUI.DrawRect(rowRect, (rowGuiArgs.row % 2 == 0) ? evenRowColor : oddRowColor);

                if (rowGuiArgs.selected)
                {
                    Color rowSelectionColor = EditorGUIUtility.isProSkin
                        ? new Color(0.24f, 0.48f, 0.90f, 0.5f)
                        : new Color(0.24f, 0.48f, 0.90f, 0.8f);

                    EditorGUI.DrawRect(rowRect, rowSelectionColor);
                }
            }

            EditorTreeViewItem treeViewItem = rowGuiArgs.item as EditorTreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            float iconDisplayWidth = treeViewItem.PageData.IconTexture != null ? 18f : 0f;
            float itemContentIndent = GetContentIndent(treeViewItem);

            if (!treeViewItem.PageData.IsCategory && treeViewItem.PageData.Parent == null)
            {
                itemContentIndent -= 13f;
            }
            else if (!treeViewItem.PageData.IsCategory && treeViewItem.PageData.Parent != null)
            {
                itemContentIndent -= 13f;
            }

            Rect iconDrawRect = new Rect(rowGuiArgs.rowRect.x + itemContentIndent, rowGuiArgs.rowRect.y + 3.5f, 14f, 14f);
            Rect labelDrawRect = new Rect(rowGuiArgs.rowRect.x + itemContentIndent + iconDisplayWidth, rowGuiArgs.rowRect.y, rowGuiArgs.rowRect.width - itemContentIndent - iconDisplayWidth, rowGuiArgs.rowRect.height);

            if (treeViewItem.PageData.IconTexture != null)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    GUI.DrawTexture(iconDrawRect, treeViewItem.PageData.IconTexture);
                }
                else
                {
                    Color savedGuiColor = GUI.color;
                    GUI.color = EditorStyles.label.normal.textColor;
                    GUI.DrawTexture(iconDrawRect, treeViewItem.PageData.IconTexture);
                    GUI.color = savedGuiColor;
                }
            }

            GUIStyle labelStyle = treeViewItem.PageData.IsCategory ? EditorStyles.boldLabel : EditorStyles.label;
            GUI.Label(labelDrawRect, treeViewItem.PageData.DisplayName, labelStyle);
        }

        protected override bool CanMultiSelect(TreeViewItem treeItem) => false;

        protected override bool DoesItemMatchSearch(TreeViewItem treeItem, string searchTerm)
        {
            EditorTreeViewItem editorTreeItem = treeItem as EditorTreeViewItem;
            return editorTreeItem == null
                ? false
                : editorTreeItem.PageData.DisplayName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0
                ? true
                : editorTreeItem.PageData.Id.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem rootTreeItem)
        {
            IList<TreeViewItem> allRows = base.BuildRows(rootTreeItem);

            if (string.IsNullOrEmpty(searchString))
            {
                return allRows;
            }

            List<TreeViewItem> filteredSearchRows = new List<TreeViewItem>();

            foreach (TreeViewItem treeItem in allRows)
            {
                if (DoesItemMatchSearch(treeItem, searchString))
                {
                    filteredSearchRows.Add(treeItem);

                    TreeViewItem parentTreeItem = treeItem.parent;
                    while (parentTreeItem != null && parentTreeItem.id != 0)
                    {
                        if (!filteredSearchRows.Contains(parentTreeItem))
                        {
                            filteredSearchRows.Add(parentTreeItem);
                        }

                        parentTreeItem = parentTreeItem.parent;
                    }
                }
            }

            return filteredSearchRows;
        }

        private TreeViewItem CreateTreeViewItem(TreePageData pageData, ref int currentItemId)
        {
            EditorTreeViewItem newTreeItem = new EditorTreeViewItem(currentItemId++, pageData);

            foreach (TreePageData childPageData in pageData.Children)
            {
                TreeViewItem childTreeItem = CreateTreeViewItem(childPageData, ref currentItemId);
                if (newTreeItem.children == null)
                {
                    newTreeItem.children = new List<TreeViewItem>();
                }

                newTreeItem.children.Add(childTreeItem);
                childTreeItem.parent = newTreeItem;
            }

            return newTreeItem;
        }
    }
#if UNITY_6000_3_OR_NEWER
#pragma warning restore CS0618
#endif
}