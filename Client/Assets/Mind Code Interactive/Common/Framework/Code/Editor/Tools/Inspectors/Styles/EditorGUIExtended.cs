/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIExtended.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIExtended
    {
        public static class Styles
        {
            public static Color HeaderTextColor => EditorGUIStyles.s_headerTextColor;
            public static Color SeparatorColor => EditorGUIStyles.SeparatorColor;
            public static Color HeaderBackgroundColor => EditorGUIStyles.HeaderBackgroundColor;
            public static Color TabSeparatorColor => EditorGUIStyles.TabSeparatorColor;
            public static GUIStyle BorderBoxHeaderStyle => EditorGUIStyles.BorderBoxHeaderStyle;
            public static GUIStyle BorderBoxWithPaddingStyle => EditorGUIStyles.BorderBoxWithPaddingStyle;
            public static GUIStyle BorderBoxStyle => EditorGUIStyles.BorderBoxStyle;
            public static GUIStyle MiniBoldLabelCenter => EditorGUIStyles.MiniBoldLabelCenter;
        }

        public static class ColorPalette
        {
            public static Color Success => EditorGUIUtility.isProSkin ? new Color(0.3f, 1f, 0.3f) : EditorStyles.label.normal.textColor;
            public static Color Error => EditorGUIUtility.isProSkin ? new Color(1f, 0.4f, 0.4f) : EditorStyles.label.normal.textColor;
            public static Color Warning => EditorGUIUtility.isProSkin ? new Color(1f, 0.8f, 0.2f) : EditorStyles.label.normal.textColor;
            public static Color Info => EditorGUIUtility.isProSkin ? new Color(0.4f, 0.8f, 1f) : EditorStyles.label.normal.textColor;

            public static string SuccessHex => ColorUtility.ToHtmlStringRGB(Success);
            public static string ErrorHex => ColorUtility.ToHtmlStringRGB(Error);
            public static string WarningHex => ColorUtility.ToHtmlStringRGB(Warning);
            public static string InfoHex => ColorUtility.ToHtmlStringRGB(Info);
        }

        public static void InspectorHeader(UnityEngine.Object target, string headerDescription = "")
            => EditorGUIElements.InspectorHeader(target, headerDescription);

        public static void InspectorHeader(string headerTitle, string headerDescription = "")
            => EditorGUIElements.InspectorHeader(headerTitle, headerDescription);

        public static void InspectorDescription(string descriptionText, bool shouldDrawSeparator = true)
            => EditorGUIElements.InspectorDescription(descriptionText, shouldDrawSeparator);

        public static void InspectorBottom()
            => EditorGUIElements.InspectorBottom();

        public static void HelpBox(string messageText, EditorGUIElements.MessageType messageType)
            => EditorGUIElements.HelpBox(messageText, messageType);

        public static void BeginBorderLayoutVertical(bool shouldDrawBorder = true, bool shouldIncludePadding = false, params GUILayoutOption[] layoutOptions)
            => EditorGUILayouts.BeginBorderLayoutVertical(shouldDrawBorder, shouldIncludePadding, layoutOptions);

        public static void BeginBorderLayoutHorizontal(bool shouldDrawBorder = true, float backgroundAlphaValue = 0.6f, params GUILayoutOption[] layoutOptions)
            => EditorGUILayouts.BeginBorderLayoutHorizontal(shouldDrawBorder, backgroundAlphaValue, layoutOptions);

        public static void EndBorderLayoutHorizontal()
            => EditorGUILayouts.EndBorderHeaderLayoutHorizontal();

        public static void EndBorderLayoutVertical()
            => EditorGUILayouts.EndBorderHeaderLayoutVertical();

        public static void BeginHorizontal(params GUILayoutOption[] layoutOptions)
            => EditorGUILayouts.BeginHorizontal(layoutOptions);

        public static void EndHorizontal()
            => EditorGUILayouts.EndHorizontal();

        public static void BeginVertical(params GUILayoutOption[] layoutOptions)
            => EditorGUILayouts.BeginVertical(layoutOptions);

        public static void EndVertical()
            => EditorGUILayouts.EndVertical();

        public static void Separator(bool shouldIndent = false)
            => EditorGUIElements.Separator(shouldIndent);

        public static void Separator(string separatorLabel, bool shouldDrawHeaderSeparator = true)
            => EditorGUIElements.Separator(separatorLabel, shouldDrawHeaderSeparator);

        public static void DrawSection(string sectionTitle, string iconName, string sectionDescription, Action sectionContent, bool shouldIndent = false)
            => EditorGUIFoldouts.DrawSection(sectionTitle, iconName, sectionDescription, sectionContent, shouldIndent);

        public static bool DrawExpandableSection(string sectionTitle, string iconName, string sectionDescription, Action sectionContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => EditorGUIFoldouts.DrawExpandableSection(EditorGUILabels.IconLabel(sectionTitle, iconName), sectionDescription, sectionContent, shouldIndent, isOpenByDefault);

        public static bool DrawExpandableSection(string sectionTitle, string rightDisplayText, string iconName, string sectionDescription, Action sectionContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => EditorGUIFoldouts.DrawExpandableSection(EditorGUILabels.IconLabel(sectionTitle, iconName), new GUIContent(rightDisplayText), sectionDescription, sectionContent, shouldIndent, isOpenByDefault);

        public static bool DrawExpandableSection(GUIContent sectionTitle, string sectionDescription, Action sectionContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => EditorGUIFoldouts.DrawExpandableSection(sectionTitle, sectionDescription, sectionContent, shouldIndent, isOpenByDefault);

        public static bool DrawExpandableSection(GUIContent sectionTitle, GUIContent rightDisplayText, string sectionDescription, Action sectionContent, bool shouldIndent = false, bool isOpenByDefault = false)
            => EditorGUIFoldouts.DrawExpandableSection(sectionTitle, rightDisplayText, sectionDescription, sectionContent, shouldIndent, isOpenByDefault);

        public static bool ExpandableSectionWithPane(GUIContent sectionTitle, string sectionDescription, Action sectionContent, Action<GenericMenu> onContextMenuRequested, bool shouldIndent = false, bool isOpenByDefault = false)
            => EditorGUIFoldouts.ExpandableSectionWithPane(sectionTitle, sectionDescription, sectionContent, onContextMenuRequested, shouldIndent, isOpenByDefault);

        public static void DragAndDropArea(string dragMessageText, EditorGUIElements.OnDragDropCallback onObjectsDropped, Func<UnityEngine.Object, bool> validateObjectFunction = null, bool allowProjectAssets = true, bool allowSceneObjects = true)
            => EditorGUIElements.DragAndDropArea(dragMessageText, onObjectsDropped, validateObjectFunction, allowProjectAssets, allowSceneObjects);

        public static int ToolbarMulti(int selectedIndex, GUIContent[] toolbarTabs, float toolbarHeight = 25f, int maxTabsPerRow = 5)
            => EditorGUIToolbar.ToolbarMulti(selectedIndex, toolbarTabs, toolbarHeight, maxTabsPerRow);

        public static int Toolbar(int selectedIndex, GUIContent[] toolbarTabs, float toolbarHeight = 25f)
            => EditorGUIToolbar.Toolbar(selectedIndex, toolbarTabs, toolbarHeight);

        public static EditorGUIScopes.DisabledScope DisabledScope(bool shouldBeDisabled)
            => new EditorGUIScopes.DisabledScope(shouldBeDisabled);

        public static EditorGUIScopes.IndentScope IndentScope(int indentIncrement = 1)
            => new EditorGUIScopes.IndentScope(indentIncrement);

        public static EditorGUIScopes.MarginScope MarginScope(float marginSize = 7f)
            => new EditorGUIScopes.MarginScope(marginSize);

        public static bool Button(string buttonText, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.Button(buttonText, null, null, FontStyle.Normal, layoutOptions);

        public static bool Button(string buttonText, Color? buttonBackgroundColor = null, Color? buttonTextColor = null, FontStyle textFontStyle = FontStyle.Normal, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.Button(buttonText, buttonBackgroundColor, buttonTextColor, textFontStyle, layoutOptions);

        public static bool StateButton(string buttonText, bool isActiveState, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.StateButton(buttonText, isActiveState, layoutOptions);

        public static bool SuccessButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.SuccessButton(buttonText, layoutOptions);

        public static bool DangerButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.DangerButton(buttonText, layoutOptions);

        public static bool WarningButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.WarningButton(buttonText, layoutOptions);

        public static bool InfoButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => EditorGUIButtons.InfoButton(buttonText, layoutOptions);

        public static void Label(string labelText, EditorGUILabels.LabelType labelType = EditorGUILabels.LabelType.Normal, EditorGUILabels.LabelAlignment textAlignment = EditorGUILabels.LabelAlignment.Left, bool shouldExpandWidth = false)
            => EditorGUILabels.Label(labelText, null, labelType, textAlignment, shouldExpandWidth);

        public static void ColoredLabel(string labelText, Color labelColor, EditorGUILabels.LabelType labelType = EditorGUILabels.LabelType.Normal, EditorGUILabels.LabelAlignment textAlignment = EditorGUILabels.LabelAlignment.Left, bool shouldExpandWidth = false)
            => EditorGUILabels.Label(labelText, labelColor, labelType, textAlignment, shouldExpandWidth);

        public static GUIContent IconLabel(string labelText, string iconName)
            => EditorGUILabels.IconLabel(labelText, iconName);

        public static Texture2D Icon(string iconName)
            => EditorGUILabels.Icon(iconName);

        public static void LinkButtons(EditorGUILabels.LinkButtonData[] linkButtonsArray, float buttonHeight = 22f)
            => EditorGUILabels.LinkButtons(linkButtonsArray, buttonHeight);

        public static void LinkLabel(string linkCaption, string linkUrl)
            => EditorGUILabels.LinkLabel(linkCaption, linkUrl);
    }
}