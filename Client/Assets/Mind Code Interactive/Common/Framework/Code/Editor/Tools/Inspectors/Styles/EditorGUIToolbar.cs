/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIToolbar.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIToolbar
    {
        public static int ToolbarMulti(int selectedIndex, GUIContent[] toolbarTabsArray, float tabHeight = 25f, int maxTabsPerRow = 5)
        {
            int totalTabCount = toolbarTabsArray.Length;
            int rowCount = Mathf.CeilToInt((float)totalTabCount / maxTabsPerRow);
            int currentSelectedIndex = selectedIndex;

            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                int rowStartIndex = rowIndex * maxTabsPerRow;
                int tabsInCurrentRow = Mathf.Min(maxTabsPerRow, totalTabCount - rowStartIndex);
                GUIContent[] currentRowTabs = new GUIContent[tabsInCurrentRow];
                Array.Copy(toolbarTabsArray, rowStartIndex, currentRowTabs, 0, tabsInCurrentRow);

                int localSelectedIndex = (currentSelectedIndex >= rowStartIndex && currentSelectedIndex < rowStartIndex + tabsInCurrentRow) ? currentSelectedIndex - rowStartIndex : -1;
                int newLocalSelectedIndex = Toolbar(localSelectedIndex, currentRowTabs, tabHeight);

                GUILayout.Space(-4f);

                if (newLocalSelectedIndex != -1)
                {
                    currentSelectedIndex = rowStartIndex + newLocalSelectedIndex;
                }
            }

            return currentSelectedIndex;
        }

        public static int Toolbar(int selectedTabIndex, GUIContent[] toolbarTabsArray, float tabHeight = 25f)
        {
            Color toolbarBackgroundColor = Color.white / 1.15f;
            GUI.color = toolbarBackgroundColor;
            EditorGUILayouts.BeginHorizontal();

            float calculatedMaxTabWidth = CalculateMaxTabWidth(toolbarTabsArray);

            for (int tabIndex = 0; tabIndex < toolbarTabsArray.Length; tabIndex++)
            {
                GUILayout.FlexibleSpace();

                GUIStyle tabButtonStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                    fixedHeight = tabHeight,
                    imagePosition = ImagePosition.ImageLeft,
                    padding = new RectOffset(4, 4, 4, 4)
                };

                SetTabColors(selectedTabIndex, tabIndex, tabButtonStyle);

                GUILayout.BeginHorizontal(GUILayout.Width(calculatedMaxTabWidth), GUILayout.Height(tabHeight));

                if (GUILayout.Button(toolbarTabsArray[tabIndex], tabButtonStyle, GUILayout.Width(calculatedMaxTabWidth), GUILayout.Height(tabHeight)))
                {
                    selectedTabIndex = tabIndex;
                }

                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();

                if (tabIndex != toolbarTabsArray.Length - 1)
                {
                    DrawTabSeparator(tabHeight);
                }
            }

            EditorGUILayouts.EndHorizontal();
            return selectedTabIndex;
        }

        private static float CalculateMaxTabWidth(GUIContent[] toolbarTabsArray)
        {
            float maxCalculatedWidth = 0f;
            GUIStyle widthCalculationStyle = new GUIStyle(EditorStyles.label) { richText = true };

            for (int i = 0; i < toolbarTabsArray.Length; i++)
            {
                float tabContentWidth = widthCalculationStyle.CalcSize(toolbarTabsArray[i]).x + 32f;
                if (tabContentWidth > maxCalculatedWidth)
                {
                    maxCalculatedWidth = tabContentWidth;
                }
            }

            return maxCalculatedWidth;
        }

        private static void SetTabColors(int selectedTabIndex, int currentTabIndex, GUIStyle tabStyle)
        {
            Color normalTextColor = EditorGUIUtility.isProSkin ? Color.white / 1.5f : Color.black / 1.5f;
            Color hoverTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            Color selectedTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            if (selectedTabIndex == -1)
            {
                tabStyle.hover.textColor = hoverTextColor;
                tabStyle.normal.textColor = normalTextColor;
            }
            else
            {
                GUI.color = selectedTabIndex == currentTabIndex ? selectedTextColor : normalTextColor;
                tabStyle.hover.textColor = hoverTextColor;
            }
        }

        private static void DrawTabSeparator(float separatorHeight)
        {
            Rect separatorDrawRect = EditorGUILayout.GetControlRect(false, 1f, GUILayout.Width(1f));
            separatorDrawRect.width = 1f;
            separatorDrawRect.height = separatorHeight;
            EditorGUI.DrawRect(separatorDrawRect, EditorGUIStyles.TabSeparatorColor);
        }
    }
}