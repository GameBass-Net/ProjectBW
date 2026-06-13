/// <summary>
/// Project : Mind Code Interactive
/// Class : BackgroundUtility.cs
/// Namespace : MindCodeInteractive.ReorderableList.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MindCodeInteractive.ReorderableList.Code.Editor
{
    public static class BackgroundUtility
    {
        private static readonly Color s_activeColorLightSkin = new Color(0.3f, 0.6f, 0.95f, 0.95f);
        private static readonly Color s_activeColorDarkSkin = new Color(0.2f, 0.4f, 0.7f, 0.95f);
        private static readonly Color s_backgroundColorLightSkin = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color s_backgroundColorDarkSkin = new Color(0.25f, 0.25f, 0.25f, 1f);

        private static Texture2D s_singlePixelTexture;

        private static Color ActiveColor
        {
            get
            {
#if UNITY_EDITOR
                if (EditorGUIUtility.isProSkin)
                {
                    return s_activeColorDarkSkin;
                }
#endif
                return s_activeColorLightSkin;
            }
        }

        private static Color DifferentBackgroundColor
        {
            get
            {
#if UNITY_EDITOR
                if (EditorGUIUtility.isProSkin)
                {
                    return s_backgroundColorDarkSkin;
                }
#endif
                return s_backgroundColorLightSkin;
            }
        }

        private static float ElementLeftMargin
        {
            get
            {
#if UNITY_EDITOR
                if (EditorGUIUtility.isProSkin)
                {
                    return 1f;
                }
#endif
                return 2f;
            }
        }

        private static float ElementRightExtrusion => 2f;

        private static float ActiveLeftMargin => 1f;

        private static float ActiveRightExtrusion
        {
            get
            {
#if UNITY_EDITOR
                if (EditorGUIUtility.isProSkin)
                {
                    return 2f;
                }
#endif
                return 1f;
            }
        }

        public static void DrawElementBackgroundColorDifferent(Rect drawRect)
            => DrawElementBackgroundColor(drawRect, DifferentBackgroundColor, ElementLeftMargin, ElementRightExtrusion);

        public static void DrawElementBackgroundColorActive(Rect drawRect)
            => DrawElementBackgroundColor(drawRect, ActiveColor, ActiveLeftMargin, ActiveRightExtrusion);

        private static void DrawElementBackgroundColor(Rect drawRect, Color backgroundColor, float leftMargin, float rightExtrusion)
        {
            Rect adjustedDrawRect = HorizontalAdjusted(drawRect, leftMargin, rightExtrusion);

            EnsurePixelTexture();

            Color previousGuiColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(adjustedDrawRect, s_singlePixelTexture, ScaleMode.StretchToFill);
            GUI.color = previousGuiColor;
        }

        private static Rect HorizontalAdjusted(Rect drawRect, float leftMargin, float rightExtrusion)
        {
            drawRect.x += leftMargin;
            drawRect.width -= leftMargin + rightExtrusion;
            return drawRect;
        }

        private static void EnsurePixelTexture()
        {
            if (s_singlePixelTexture != null)
            {
                return;
            }

            s_singlePixelTexture = new Texture2D(1, 1);
            s_singlePixelTexture.SetPixel(0, 0, Color.white);
            s_singlePixelTexture.Apply();
            s_singlePixelTexture.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}