/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUILabels.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUILabels
    {
        public const string ICONS_FOLDER_PATH = "Editor/Icons";

        public enum LabelType { Normal, Mini, Bold }
        public enum LabelAlignment { Left, Center, Right }

        public struct LinkButtonData
        {
            public string Label;
            public string IconName;
            public string Url;

            public LinkButtonData(string buttonLabel, string buttonIconName, string buttonUrl)
            {
                Label = buttonLabel;
                IconName = buttonIconName;
                Url = buttonUrl;
            }
        }

        public static void Label(string labelText, Color? labelColor = null, LabelType labelDisplayType = LabelType.Normal, LabelAlignment textHorizontalAlignment = LabelAlignment.Left, bool shouldExpandWidth = false)
        {
            GUIStyle baseDisplayStyle = labelDisplayType switch
            {
                LabelType.Mini => EditorStyles.miniLabel,
                LabelType.Bold => EditorStyles.boldLabel,
                _ => GUI.skin.label,
            };

            GUIStyle finalDisplayStyle = new GUIStyle(baseDisplayStyle);
            finalDisplayStyle.richText = true;
            finalDisplayStyle.alignment = TextAnchor.MiddleLeft;

            if (labelDisplayType == LabelType.Bold)
            {
                finalDisplayStyle.contentOffset = new Vector2(1f, 0f);
            }

            string displayedText = labelText;

            if (labelColor.HasValue && EditorGUIUtility.isProSkin)
            {
                string colorHexCode = ColorUtility.ToHtmlStringRGB(labelColor.Value);
                displayedText = "<color=#" + colorHexCode + ">" + labelText + "</color>";
            }

            if (textHorizontalAlignment == LabelAlignment.Center)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(displayedText, finalDisplayStyle, shouldExpandWidth ? GUILayout.ExpandWidth(true) : GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (textHorizontalAlignment == LabelAlignment.Right)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(displayedText, finalDisplayStyle, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }
            else
            {
                if (shouldExpandWidth)
                {
                    GUILayout.Label(displayedText, finalDisplayStyle);
                }
                else
                {
                    GUILayout.Label(displayedText, finalDisplayStyle, GUILayout.ExpandWidth(false));
                }
            }
        }

        public static GUIContent IconLabel(string labelText, string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return new GUIContent(labelText);
            }

            return new GUIContent(labelText, Icon(iconName));
        }

        public static Texture2D Icon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            string iconResourcePath = ICONS_FOLDER_PATH + "/" + iconName;
            Texture2D loadedIcon = Resources.Load<Texture2D>(iconResourcePath);

            if (loadedIcon == null)
            {
                Debug.LogWarning("Icon not found at Resources path: " + iconResourcePath);
            }

            return loadedIcon;
        }

        public static void LinkButtons(LinkButtonData[] linkButtonsArray, float buttonHeight = 22f)
        {
            if (linkButtonsArray == null || linkButtonsArray.Length == 0)
            {
                return;
            }

            GUIContent[] buttonContents = new GUIContent[linkButtonsArray.Length];

            for (int i = 0; i < linkButtonsArray.Length; i++)
            {
                Texture buttonIconTexture = null;

                if (!string.IsNullOrEmpty(linkButtonsArray[i].IconName))
                {
                    if (linkButtonsArray[i].IconName.Contains("/"))
                    {
                        buttonIconTexture = Resources.Load<Texture2D>(linkButtonsArray[i].IconName);
                    }
                    else
                    {
                        GUIContent builtInIconContent = EditorGUIUtility.IconContent(linkButtonsArray[i].IconName);
                        buttonIconTexture = builtInIconContent != null ? builtInIconContent.image : null;
                    }
                }

                buttonContents[i] = new GUIContent(" " + linkButtonsArray[i].Label, buttonIconTexture);
            }

            int selectedButtonIndex = EditorGUIToolbar.Toolbar(-1, buttonContents, buttonHeight);
            if (selectedButtonIndex >= 0 && selectedButtonIndex < linkButtonsArray.Length && !string.IsNullOrEmpty(linkButtonsArray[selectedButtonIndex].Url))
            {
                Application.OpenURL(linkButtonsArray[selectedButtonIndex].Url);
            }
        }

        public static void LinkLabel(string linkCaption, string linkUrl)
        {
            GUIStyle linkLabelStyle = new GUIStyle(GUI.skin.label);
            linkLabelStyle.richText = true;

            string linkColorCode = EditorGUIUtility.isProSkin ? "#3386FF" : "#0066CC";
            string coloredLinkCaption = "<color=" + linkColorCode + ">" + linkCaption + "</color>";

            bool wasLinkClicked = GUILayout.Button(coloredLinkCaption, linkLabelStyle);
            Rect lastLinkRect = GUILayoutUtility.GetLastRect();
            lastLinkRect.width = linkLabelStyle.CalcSize(new GUIContent(coloredLinkCaption)).x;
            EditorGUIUtility.AddCursorRect(lastLinkRect, MouseCursor.Link);

            if (wasLinkClicked && !string.IsNullOrEmpty(linkUrl))
            {
                Application.OpenURL(linkUrl);
            }
        }
    }
}