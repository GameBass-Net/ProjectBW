/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryOverviewLayout.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    public class CategoryOverviewLayout : PageLayout
    {
        private TreePageData m_categoryPageData;

        public CategoryOverviewLayout(TreePageData categoryData) => m_categoryPageData = categoryData;

        public override void DrawLayout()
        {
            if (m_categoryPageData == null)
            {
                return;
            }

            EditorGUIExtended.InspectorHeader(
                m_categoryPageData.DisplayName + " Overview",
                "Quick summary of this category and its pages.\nSee details, page count, and open any page."
            );

            GUILayout.BeginVertical();

            EditorGUIExtended.Separator("Category Information", false);
            GUILayout.Label("Pages : " + m_categoryPageData.Children.Count + " page(s)");

            GUILayout.Space(10f);

            if (m_categoryPageData.Children.Count > 0)
            {
                foreach (TreePageData childPageData in m_categoryPageData.Children)
                {
                    GUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();

                    if (childPageData.IconTexture != null)
                    {
                        if (EditorGUIUtility.isProSkin)
                        {
                            GUILayout.Label(childPageData.IconTexture, GUILayout.Width(20f), GUILayout.Height(20f));
                        }
                        else
                        {
                            Color savedGuiColor = GUI.color;
                            GUI.color = EditorStyles.label.normal.textColor;
                            GUILayout.Label(childPageData.IconTexture, GUILayout.Width(20f), GUILayout.Height(20f));
                            GUI.color = savedGuiColor;
                        }
                    }

                    EditorGUILayout.BeginVertical();
                    GUILayout.Label(childPageData.DisplayName, EditorStyles.boldLabel);

                    if (!string.IsNullOrEmpty(childPageData.Id))
                    {
                        GUIStyle childIdStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            normal = { textColor = Color.gray }
                        };
                        GUILayout.Label("ID: " + childPageData.Id, childIdStyle);
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.FlexibleSpace();

                    if (EditorGUIExtended.Button("Open", GUILayout.Width(80f), GUILayout.Height(20f)))
                    {
                        NavigateToPage(childPageData);
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUILayout.Space(5f);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                GUIStyle emptyStateTextStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("No pages in this category...", emptyStateTextStyle);
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5f);
            }

            GUILayout.EndVertical();

            EditorGUIExtended.InspectorBottom();
        }

        private void NavigateToPage(TreePageData targetPageData)
        {
            HubManagerWindow hubWindow = EditorWindow.GetWindow<HubManagerWindow>();
            if (hubWindow != null)
            {
                hubWindow.SelectPage(targetPageData);
            }
        }
    }
}