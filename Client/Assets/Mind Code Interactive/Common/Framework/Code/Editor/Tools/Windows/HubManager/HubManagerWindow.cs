/// <summary>
/// Project : Mind Code Interactive
/// Class : HubManagerWindow.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Implementations;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Providers;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Views;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager
{
#if UNITY_6000_3_OR_NEWER
#pragma warning disable CS0618
#endif
    public class HubManagerWindow : BaseWindowEditor
    {
        private EditorTreeView m_treeView;
        private TreeViewState m_treeViewState;
        private List<TreePageData> m_treeData;
        private PageLayout m_currentPageLayout;
        private float m_treeViewWidth = 200f;

        public static void OpenWindow()
        {
            GUIContent windowTitle = new GUIContent("Mind Code Interactive Hub");
            Vector2 windowSize = new Vector2(860f, 600f);
            OpenWindow<HubManagerWindow>(windowTitle, windowSize);
        }

        public static void OpenWindowToPage(string pageId)
        {
            GUIContent windowTitle = new GUIContent("Mind Code Interactive Hub");
            Vector2 windowSize = new Vector2(860f, 600f);
            HubManagerWindow window = OpenWindow<HubManagerWindow>(windowTitle, windowSize);
            EditorApplication.delayCall += () => window.NavigateToPageById(pageId);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            InitializeTreeData();

            if (m_treeViewState == null)
            {
                m_treeViewState = new TreeViewState();
            }

            m_treeView = new EditorTreeView(m_treeViewState, m_treeData, OnPageSelected);
            SelectDefaultPage();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_currentPageLayout?.OnDisable();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

        public void SelectPage(TreePageData pageToSelect)
        {
            OnPageSelected(pageToSelect);
            ExpandToPage(pageToSelect);

            int itemId = FindTreeViewItemId(pageToSelect);
            if (itemId != -1)
            {
                m_treeView.SetSelection(new[] { itemId });
                m_treeView.FrameItem(itemId);
            }
        }

        public void NavigateToPageById(string pageId)
        {
            if (m_treeData == null || m_treeData.Count == 0)
            {
                Debug.LogError($"Cannot navigate to page '{pageId}': Tree data not initialized!");
                return;
            }

            TreePageData targetPage = FindPageByIdRecursive(m_treeData, pageId);

            if (targetPage != null)
            {
                SelectPage(targetPage);
            }
            else
            {
                Debug.LogError($"Page with ID '{pageId}' not found! Available pages:");
                LogAvailablePages(m_treeData, 0);
            }
        }

        private void LogAvailablePages(List<TreePageData> pages, int depth)
        {
            foreach (TreePageData page in pages)
            {
                if (page.Children != null && page.Children.Count > 0)
                {
                    LogAvailablePages(page.Children, depth + 1);
                }
            }
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(m_treeViewWidth));
            GUILayout.Space(2f);

            using (EditorGUIExtended.MarginScope())
            {
                DrawHubHeader();
                EditorGUILayout.Separator();
            }

            Rect treeViewDrawRect = GUILayoutUtility.GetRect(m_treeViewWidth, position.height);
            treeViewDrawRect.y -= 1f;
            m_treeView?.OnGUI(treeViewDrawRect);

            EditorGUILayout.EndVertical();
            GUILayout.Space(-1f);
            DrawContentArea();
        }

        private void DrawHubHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Texture2D headerLogo = Resources.Load<Texture2D>("Editor/Icons/logo");
            GUILayout.Label(headerLogo, GUILayout.Width(64f), GUILayout.Height(64f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Mind Code Interactive", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version : 1.0.0", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawContentArea()
        {
            EditorGUILayout.BeginVertical();

            if (m_currentPageLayout != null)
            {
                EditorGUIExtended.BeginBorderLayoutVertical(true, false);

                float scrollViewHeight = position.height;
                float availableContentWidth = position.width - m_treeViewWidth;

                m_currentPageLayout.ScrollPosition = EditorGUILayout.BeginScrollView(
                    m_currentPageLayout.ScrollPosition,
                    GUIStyle.none,
                    GUI.skin.verticalScrollbar,
                    GUILayout.Height(scrollViewHeight),
                    GUILayout.Width(availableContentWidth)
                );

                using (EditorGUIExtended.MarginScope())
                {
                    m_currentPageLayout.DrawLayout();
                }

                EditorGUILayout.EndScrollView();
                EditorGUIExtended.EndBorderLayoutVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void InitializeTreeData() => m_treeData = EditorPageProvider.DiscoverPages();

        private TreePageData FindFirstNonCategoryPage(List<TreePageData> pageDataList)
        {
            foreach (TreePageData pageData in pageDataList)
            {
                if (!pageData.IsCategory)
                {
                    return pageData;
                }

                TreePageData foundChildPage = FindFirstNonCategoryPage(pageData.Children);
                if (foundChildPage != null)
                {
                    return foundChildPage;
                }
            }

            return null;
        }

        private TreePageData FindPageByIdRecursive(List<TreePageData> pageDataList, string pageId)
        {
            foreach (TreePageData pageData in pageDataList)
            {
                if (pageData.Id == pageId)
                {
                    return pageData;
                }

                TreePageData foundPageData = FindPageByIdRecursive(pageData.Children, pageId);
                if (foundPageData != null)
                {
                    return foundPageData;
                }
            }

            return null;
        }

        private void ExpandToPage(TreePageData pageToExpand)
        {
            List<TreePageData> ancestorPathToRoot = new List<TreePageData>();
            TreePageData currentPageAncestor = pageToExpand.Parent;

            while (currentPageAncestor != null)
            {
                ancestorPathToRoot.Add(currentPageAncestor);
                currentPageAncestor = currentPageAncestor.Parent;
            }

            foreach (TreePageData ancestorPage in ancestorPathToRoot)
            {
                int ancestorItemId = FindTreeViewItemId(ancestorPage);
                if (ancestorItemId != -1)
                {
                    m_treeView.SetExpanded(ancestorItemId, true);
                }
            }
        }

        private int FindTreeViewItemId(TreePageData targetPageData)
        {
            IList<TreeViewItem> treeViewRows = m_treeView.GetRows();

            foreach (TreeViewItem treeViewRow in treeViewRows)
            {
                if (treeViewRow is EditorTreeViewItem editorTreeItem && editorTreeItem.PageData == targetPageData)
                {
                    return treeViewRow.id;
                }
            }

            return -1;
        }

        private void OnPageSelected(TreePageData selectedPageData)
        {
            m_currentPageLayout?.OnDisable();

            m_currentPageLayout = selectedPageData == null
                ? null
                : (selectedPageData.IsCategory ? new CategoryOverviewLayout(selectedPageData) : selectedPageData.CreatePageLayout());

            m_currentPageLayout?.OnEnable();
            Repaint();
        }

        private void SelectDefaultPage()
        {
            TreePageData defaultPageToSelect = EditorPageProvider.FindDefaultSelectedPage();

            if (defaultPageToSelect != null)
            {
                SelectPage(defaultPageToSelect);
            }
            else if (m_treeData.Count > 0)
            {
                TreePageData firstNonCategoryPage = FindFirstNonCategoryPage(m_treeData);
                if (firstNonCategoryPage != null)
                {
                    OnPageSelected(firstNonCategoryPage);
                }
            }
        }
    }
#if UNITY_6000_3_OR_NEWER
#pragma warning restore CS0618
#endif
}