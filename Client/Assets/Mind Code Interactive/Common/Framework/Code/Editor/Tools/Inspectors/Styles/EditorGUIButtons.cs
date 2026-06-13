/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIButtons.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIButtons
    {
        public static bool Button(string buttonText, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, null, null, FontStyle.Normal, layoutOptions);

        public static bool Button(string buttonText, Color? buttonBackgroundColor = null, Color? buttonTextColor = null, FontStyle fontStyle = FontStyle.Normal, params GUILayoutOption[] layoutOptions)
        {
            Color effectiveBackgroundColor = Color.white;
            Color effectiveTextColor = Color.white;

            if (EditorGUIUtility.isProSkin)
            {
                effectiveBackgroundColor = buttonBackgroundColor ?? Color.white;
                effectiveTextColor = buttonTextColor ?? Color.white;
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = fontStyle };

            if (EditorGUIUtility.isProSkin && buttonTextColor.HasValue)
            {
                buttonStyle.normal.textColor = effectiveTextColor;
            }

            Color savedBackgroundColor = GUI.backgroundColor;

            if (EditorGUIUtility.isProSkin && buttonBackgroundColor.HasValue)
            {
                GUI.backgroundColor = effectiveBackgroundColor;
            }

            bool isClicked = GUILayout.Button(buttonText, buttonStyle, layoutOptions);
            GUI.backgroundColor = savedBackgroundColor;
            return isClicked;
        }

        public static bool StateButton(string buttonText, bool isActiveState, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, isActiveState ? Color.yellow : Color.white, null, FontStyle.Normal, layoutOptions);

        public static bool SuccessButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, new Color(0.5f, 0.8f, 0.5f), Color.white, FontStyle.Normal, layoutOptions);

        public static bool DangerButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, Color.red, Color.white, FontStyle.Normal, layoutOptions);

        public static bool WarningButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, new Color(1f, 0.8f, 0.2f), Color.white, FontStyle.Normal, layoutOptions);

        public static bool InfoButton(string buttonText, params GUILayoutOption[] layoutOptions)
            => Button(buttonText, new Color(0.4f, 0.7f, 0.9f), Color.white, FontStyle.Normal, layoutOptions);
    }
}