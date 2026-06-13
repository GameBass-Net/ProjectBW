/// <summary>
/// Project : Mind Code Interactive
/// Class : HierarchyStyleConfig.cs
/// Namespace : MindCodeInteractive.HierarchyDecorator.Code.Editor.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.HierarchyDecorator.Code.Editor.Data
{
    public class HierarchyStyleConfig : ScriptableObject
    {
        [Serializable]
        public class StyleRule
        {
            [SerializeField] private string m_symbol;
            [SerializeField] private Color m_backgroundDarkSkinColor = Color.gray;
            [SerializeField] private Color m_backgroundLightSkinColor = Color.gray;
            [SerializeField] private FontStyle m_fontStyle = FontStyle.Bold;
            [SerializeField] private TextAnchor m_alignment = TextAnchor.MiddleCenter;
            [SerializeField] private Color m_fontDarkSkinColor = Color.white;
            [SerializeField] private Color m_fontLightSkinColor = Color.black;
            [SerializeField] private int m_fontSize = 12;
            [SerializeField] private string m_iconName;
            [SerializeField] private Vector2 m_iconOffset = Vector2.zero;
            [SerializeField] private Vector2 m_textOffset = Vector2.zero;
            [SerializeField] private bool m_toUpperCase = true;
            [SerializeField] private bool m_showLayer;

            public string Symbol => m_symbol;
            public Color BackgroundDarkSkinColor => m_backgroundDarkSkinColor;
            public Color BackgroundLightSkinColor => m_backgroundLightSkinColor;
            public Color BackgroundColor => EditorGUIUtility.isProSkin ? m_backgroundDarkSkinColor : m_backgroundLightSkinColor;
            public FontStyle FontStyle => m_fontStyle;
            public TextAnchor Alignment => m_alignment;
            public Color FontDarkSkinColor => m_fontDarkSkinColor;
            public Color FontLightSkinColor => m_fontLightSkinColor;
            public Color FontColor => EditorGUIUtility.isProSkin ? m_fontDarkSkinColor : m_fontLightSkinColor;
            public int FontSize => m_fontSize;
            public string IconName => m_iconName;
            public Vector2 IconOffset => m_iconOffset;
            public Vector2 TextOffset => m_textOffset;
            public bool ToUpperCase => m_toUpperCase;
            public bool ShowLayer => m_showLayer;
        }

        [SerializeField] private StyleRule[] m_styleRules;
        [SerializeField] private SceneAsset[] m_includedScenes;

        public StyleRule[] StyleRules => m_styleRules;
        public SceneAsset[] IncludedScenes => m_includedScenes;

        public bool IsActiveSceneIncluded()
        {
            if (m_includedScenes == null || m_includedScenes.Length == 0)
            {
                return true;
            }

            string activeScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

            for (int i = 0; i < m_includedScenes.Length; i++)
            {
                if (m_includedScenes[i] == null)
                {
                    continue;
                }

                if (AssetDatabase.GetAssetPath(m_includedScenes[i]) == activeScenePath)
                {
                    return true;
                }
            }

            return false;
        }
    }
}