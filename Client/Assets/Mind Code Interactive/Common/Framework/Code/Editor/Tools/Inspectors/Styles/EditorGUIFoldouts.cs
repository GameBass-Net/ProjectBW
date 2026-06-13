/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIFoldouts.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIFoldouts
    {
        private static readonly Dictionary<string, bool> s_foldoutStateCache = new Dictionary<string, bool>();
        private static Action<GenericMenu> s_contextMenuCallback;

        public static bool DrawExpandableSection(GUIContent sectionTitle, string sectionDescription, Action sectionDrawContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => DrawExpandableSectionInternal(sectionTitle, null, sectionDescription, sectionDrawContent, null, shouldIndent, isOpenByDefault);

        public static bool DrawExpandableSection(GUIContent sectionTitle, GUIContent rightDisplayText, string sectionDescription, Action sectionDrawContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => DrawExpandableSectionInternal(sectionTitle, rightDisplayText, sectionDescription, sectionDrawContent, null, shouldIndent, isOpenByDefault);

        public static bool ExpandableSectionWithPane(GUIContent sectionTitle, string sectionDescription, Action sectionDrawContent, Action<GenericMenu> onContextMenuRequested, bool shouldIndent = false, bool isOpenByDefault = false)
            => DrawExpandableSectionInternal(sectionTitle, null, sectionDescription, sectionDrawContent, onContextMenuRequested, shouldIndent, isOpenByDefault);

        public static void DrawSection(string sectionTitle, string iconName, string sectionDescription, Action sectionDrawContent, bool shouldIndent = false)
        {
            GUIContent titleLabel = EditorGUILabels.IconLabel(sectionTitle, iconName);
            const float headerHeight = 26f;

            Rect headerRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect boxRect = headerRect;
            Rect drawingRect = EditorGUILayout.BeginVertical(EditorGUIStyles.BorderBoxHeaderStyle);
            boxRect.yMax = drawingRect.yMax;
            GUI.Box(boxRect, GUIContent.none, EditorGUIStyles.BorderBoxWithPaddingStyle);
            EditorGUI.DrawRect(headerRect, EditorGUIStyles.HeaderBackgroundColor);

            float indentPixelOffset = EditorGUI.indentLevel * 15f;
            headerRect.x += indentPixelOffset;

            GUIStyle headerLabelStyle = new GUIStyle(EditorGUIStyles.MiniBoldLabelCenter)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };

            if (titleLabel.image)
            {
                Rect iconRect = new Rect(headerRect.x + 8f, headerRect.y + (headerRect.height - 15f) * 0.5f, 15f, 15f);
                GUI.DrawTexture(iconRect, titleLabel.image, ScaleMode.ScaleToFit);
                Rect titleTextRect = new Rect(iconRect.xMax + 4f, headerRect.y - 2f, headerRect.width - (iconRect.xMax - headerRect.x) - 10f, headerRect.height);
                EditorGUI.LabelField(titleTextRect, new GUIContent(titleLabel.text), headerLabelStyle);
            }
            else
            {
                Rect titleTextRect = new Rect(headerRect.x + 8f, headerRect.y - 2f, headerRect.width - 14f, headerRect.height);
                EditorGUI.LabelField(titleTextRect, titleLabel, headerLabelStyle);
            }

            GUILayout.BeginVertical(EditorGUIStyles.BorderBoxHeaderStyle);
            if (!string.IsNullOrEmpty(sectionDescription))
            {
                EditorGUIElements.InspectorDescription(sectionDescription, true);
            }

            if (shouldIndent)
            {
                EditorGUI.indentLevel++;
            }

            sectionDrawContent?.Invoke();

            if (shouldIndent)
            {
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(1f);
        }

        public static void PaneOptionsButton(Rect buttonClickArea, Action<GenericMenu> onContextMenuSetup, string iconName = "pane options")
        {
            GUIStyle paneButtonStyle = new GUIStyle("label");

            if (buttonClickArea.Contains(Event.current.mousePosition))
            {
                Color hoverHighlightColor = EditorGUIUtility.isProSkin ? Color.white / 1.5f : Color.black / 1.5f;
                GUI.color = hoverHighlightColor;
                EditorGUIUtility.AddCursorRect(buttonClickArea, MouseCursor.Link);
            }

            Texture paneButtonIcon = EditorGUIUtility.IconContent(iconName).image;

            if (GUI.Button(buttonClickArea, new GUIContent(string.Empty, paneButtonIcon), paneButtonStyle))
            {
                GenericMenu contextMenu = new GenericMenu();
                onContextMenuSetup?.Invoke(contextMenu);
                contextMenu.ShowAsContext();
            }

            GUI.color = Color.white;
        }

        private static bool DrawExpandableSectionInternal(GUIContent sectionTitle, GUIContent rightDisplayText, string sectionDescription, Action sectionDrawContent, Action<GenericMenu> onContextMenuRequested, bool shouldIndent, bool isOpenByDefault)
        {
            string foldoutStateKey = GenerateStateKey(sectionTitle);
            bool isSectionOpen = GetCachedState(foldoutStateKey, isOpenByDefault);

            if (onContextMenuRequested != null)
            {
                GUILayout.Label(string.Empty, GUILayout.Height(0.001f));
            }

            Rect lastDrawnRect = GUILayoutUtility.GetLastRect();
            s_contextMenuCallback = onContextMenuRequested;

            isSectionOpen = DrawFoldoutHeader(sectionTitle, rightDisplayText, isSectionOpen, 26f);
            CacheState(foldoutStateKey, isSectionOpen);

            if (onContextMenuRequested != null)
            {
                Rect contextButtonRect = new Rect(lastDrawnRect.xMax - 25f, lastDrawnRect.y + 6f, 20f, 20f);
                PaneOptionsButton(contextButtonRect, onContextMenuRequested);
            }

            if (isSectionOpen)
            {
                if (shouldIndent)
                {
                    EditorGUI.indentLevel++;
                }

                GUILayout.BeginVertical(EditorGUIStyles.BorderBoxHeaderStyle);
                if (!string.IsNullOrEmpty(sectionDescription))
                {
                    EditorGUIElements.InspectorDescription(sectionDescription, true);
                }

                sectionDrawContent?.Invoke();
                GUILayout.EndVertical();

                if (shouldIndent)
                {
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            s_contextMenuCallback = null;
            EditorGUILayout.Space(1f);
            return isSectionOpen;
        }

        private static bool DrawFoldoutHeader(GUIContent headerTitle, GUIContent rightHeaderText, bool isExpanded, float headerHeight)
        {
            Rect headerDrawingRect = EditorGUILayout.GetControlRect(false, headerHeight + 4f);
            Rect headerBackgroundRect = headerDrawingRect;
            if (isExpanded)
            {
                Rect contentDrawingRect = EditorGUILayout.BeginVertical(EditorGUIStyles.BorderBoxHeaderStyle);
                headerBackgroundRect.yMax = contentDrawingRect.yMax;
            }
            GUI.Box(headerBackgroundRect, GUIContent.none, EditorGUIStyles.BorderBoxWithPaddingStyle);
            Event currentEvent = Event.current;
            bool isMouseOverHeader = headerDrawingRect.Contains(currentEvent.mousePosition);
            EditorGUI.DrawRect(headerDrawingRect, EditorGUIStyles.HeaderBackgroundColor);
            float indentPixelOffset = EditorGUI.indentLevel * 15f;
            Rect foldoutToggleRect = new Rect(headerDrawingRect.x + indentPixelOffset + 8f, headerDrawingRect.y, EditorGUIUtility.singleLineHeight, headerDrawingRect.height);
            isExpanded = GUI.Toggle(foldoutToggleRect, isExpanded, GUIContent.none, EditorStyles.foldout);
            headerDrawingRect.x += indentPixelOffset;
            foldoutToggleRect.x += indentPixelOffset;

            GUIStyle headerLabelStyle = new GUIStyle(EditorGUIStyles.MiniBoldLabelCenter)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };

            float titleLeftBound;
            if (headerTitle.image)
            {
                Rect titleIconRect = new Rect(foldoutToggleRect.xMax, headerDrawingRect.y + (headerDrawingRect.height - 15f) * 0.5f, 15f, 15f);
                GUI.DrawTexture(titleIconRect, headerTitle.image, ScaleMode.ScaleToFit);
                titleLeftBound = titleIconRect.xMax + 4f;
            }
            else
            {
                titleLeftBound = foldoutToggleRect.xMax - 2f;
            }

            if (rightHeaderText != null)
            {
                GUIStyle rightTextStyle = new GUIStyle(EditorGUIStyles.MiniBoldLabelCenter)
                {
                    alignment = TextAnchor.MiddleRight,
                    clipping = TextClipping.Clip,
                    wordWrap = false,
                    fontSize = 10,
                    fontStyle = FontStyle.Normal,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white / 1.5f : Color.black / 1.5f }
                };

                float rightTextMaxBound = headerDrawingRect.xMax - 6f;
                Vector2 rightTextSize = rightTextStyle.CalcSize(rightHeaderText);
                float rightTextWidth = Mathf.Min(rightTextMaxBound - titleLeftBound, rightTextSize.x);
                Rect rightTextRect = new Rect(rightTextMaxBound - rightTextWidth, headerDrawingRect.y - 2f, rightTextWidth, headerDrawingRect.height);
                EditorGUI.LabelField(rightTextRect, rightHeaderText, rightTextStyle);
            }

            float titleMaxRightBound = rightHeaderText != null ? headerDrawingRect.xMax - 6f - 4f : headerDrawingRect.xMax - 10f;
            float titleWidth = Mathf.Max(0f, titleMaxRightBound - titleLeftBound);
            Rect titleTextRect = new Rect(titleLeftBound, headerDrawingRect.y - 2f, titleWidth, headerDrawingRect.height);
            EditorGUI.LabelField(titleTextRect, new GUIContent(headerTitle.text), headerLabelStyle);

            Rect clickableHeaderArea = headerDrawingRect;
            clickableHeaderArea.xMax -= EditorGUIUtility.singleLineHeight;
            if (clickableHeaderArea.Contains(currentEvent.mousePosition) && currentEvent.type == EventType.MouseDown)
            {
                if (currentEvent.button == 0)
                {
                    isExpanded = !isExpanded;
                }
                else if (currentEvent.button == 1 && s_contextMenuCallback != null)
                {
                    GenericMenu contextMenu = new GenericMenu();
                    s_contextMenuCallback(contextMenu);
                    contextMenu.ShowAsContext();
                }
            }

            return isExpanded;
        }

        private static string GenerateStateKey(GUIContent headerContent)
            => "MindCodeInteractive_FoldoutState_title:" + (headerContent != null ? (headerContent.text ?? string.Empty) : string.Empty);

        private static bool GetCachedState(string stateKey, bool defaultValue = false)
        {
            if (!s_foldoutStateCache.TryGetValue(stateKey, out bool cachedStateValue))
            {
                cachedStateValue = EditorPrefs.GetBool(stateKey, defaultValue);
                s_foldoutStateCache[stateKey] = cachedStateValue;
            }
            return cachedStateValue;
        }

        private static void CacheState(string stateKey, bool stateValue)
        {
            s_foldoutStateCache[stateKey] = stateValue;
            EditorPrefs.SetBool(stateKey, stateValue);
        }
    }
}