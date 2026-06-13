/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIStyles.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIStyles
    {
        public static Color s_headerTextColor => EditorGUIUtility.isProSkin ? Color.white : Color.black;

        public static Color SeparatorColor => EditorGUIUtility.isProSkin
            ? new Color(0.5f, 0.5f, 0.5f, 0.35f)
            : new Color(0.3f, 0.3f, 0.3f, 0.5f);

        public static Color HeaderBackgroundColor => EditorGUIUtility.isProSkin
            ? new Color(0.1f, 0.1f, 0.1f, 0.3f)
            : new Color(0.4f, 0.4f, 0.4f, 0.3f);

        public static Color TabSeparatorColor => EditorGUIUtility.isProSkin ? Color.black / 4f : Color.gray / 2f;

        public static GUIStyle BorderBoxHeaderStyle => new GUIStyle
        {
            padding = new RectOffset(6, 6, 3, 6)
        };

        public static GUIStyle BorderBoxWithPaddingStyle => new GUIStyle
        {
            padding = new RectOffset(6, 6, 6, 6),
            border = new RectOffset(1, 1, 1, 1),
            normal = { background = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "border_dark" : "border_light") }
        };

        public static GUIStyle BorderBoxStyle => new GUIStyle
        {
            border = new RectOffset(1, 1, 1, 1),
            normal = { background = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "border_dark" : "border_light") }
        };

        public static GUIStyle MiniBoldLabelCenter => new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };
    }
}