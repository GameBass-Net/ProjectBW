/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUILayouts.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUILayouts
    {
        public static void BeginBorderLayoutVertical(bool shouldDrawBorder = true, bool shouldIncludePadding = false, params GUILayoutOption[] layoutOptions)
        {
            if (shouldDrawBorder)
            {
                Color borderHighlightColor = Color.white / 1.15f;
                GUI.color = borderHighlightColor;
                GUIStyle borderStyle = shouldIncludePadding ? EditorGUIStyles.BorderBoxWithPaddingStyle : EditorGUIStyles.BorderBoxStyle;
                Rect verticalLayoutRect = EditorGUILayout.BeginVertical(borderStyle, layoutOptions);
                GUI.Box(verticalLayoutRect, GUIContent.none, borderStyle);
            }
            else
            {
                EditorGUILayout.BeginVertical(layoutOptions);
            }

            GUI.color = Color.white;
        }

        public static void BeginBorderLayoutHorizontal(bool shouldDrawBorder = true, float backgroundAlphaValue = 0.6f, params GUILayoutOption[] layoutOptions)
        {
            if (shouldDrawBorder)
            {
                Color borderHighlightColor = Color.white / 1.15f;
                borderHighlightColor.a = backgroundAlphaValue;
                GUI.color = borderHighlightColor;
                Rect horizontalLayoutRect = EditorGUILayout.BeginHorizontal(EditorGUIStyles.BorderBoxWithPaddingStyle, layoutOptions);
                GUI.Box(horizontalLayoutRect, GUIContent.none, EditorGUIStyles.BorderBoxWithPaddingStyle);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(layoutOptions);
            }

            GUI.color = Color.white;
        }

        public static void EndBorderHeaderLayoutHorizontal() => EditorGUILayout.EndHorizontal();

        public static void EndBorderHeaderLayoutVertical() => EditorGUILayout.EndVertical();

        public static void BeginHorizontal(params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginHorizontal(EditorGUIStyles.BorderBoxStyle, layoutOptions);
            GUILayout.Space(5f);
        }

        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }

        public static void BeginVertical(params GUILayoutOption[] layoutOptions)
        {
            GUILayout.BeginVertical(EditorGUIStyles.BorderBoxStyle, layoutOptions);
            GUILayout.Space(5f);
        }

        public static void EndVertical()
        {
            GUILayout.EndVertical();
            GUILayout.Space(3f);
        }
    }
}