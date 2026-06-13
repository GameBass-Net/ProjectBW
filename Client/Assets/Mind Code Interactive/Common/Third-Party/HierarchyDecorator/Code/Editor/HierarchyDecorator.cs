/// <summary>
/// Project : Mind Code Interactive
/// Class : HierarchyDecorator.cs
/// Namespace : MindCodeInteractive.HierarchyDecorator.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MindCodeInteractive.HierarchyDecorator.Code.Editor.Data;

namespace MindCodeInteractive.HierarchyDecorator.Code.Editor
{
    [InitializeOnLoad]
    public class HierarchyDecorator
    {
        private static HierarchyStyleConfig StyleConfig => Resources.Load<HierarchyStyleConfig>("HierarchyStyleConfig");

        static HierarchyDecorator()
        {
#pragma warning disable CS0618
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
#pragma warning restore CS0618
        }

        private static void OnHierarchyGUI(int instanceId, Rect selectionRect)
        {
#pragma warning disable CS0618
            GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
#pragma warning restore CS0618
            ProcessHierarchyItem(go, selectionRect);
        }

        private static void ProcessHierarchyItem(GameObject go, Rect selectionRect)
        {
            if (StyleConfig == null || StyleConfig.StyleRules == null)
            {
                return;
            }

            if (!StyleConfig.IsActiveSceneIncluded())
            {
                return;
            }

            if (go == null)
            {
                return;
            }

            HierarchyStyleConfig.StyleRule matchedRule = FindMatchingRule(go.name);

            if (matchedRule != null)
            {
                DrawStyledGameObject(go, selectionRect, matchedRule);
                return;
            }

            DrawLayerInfo(go, selectionRect);
        }

        private static HierarchyStyleConfig.StyleRule FindMatchingRule(string name)
        {
            HierarchyStyleConfig.StyleRule[] rules = StyleConfig.StyleRules;

            for (int i = 0; i < rules.Length; i++)
            {
                if (name.StartsWith(rules[i].Symbol))
                {
                    return rules[i];
                }
            }

            return null;
        }

        private static void DrawStyledGameObject(GameObject go, Rect selectionRect, HierarchyStyleConfig.StyleRule rule)
        {
            DrawBackground(selectionRect, rule.BackgroundColor);

            GUIStyle textStyle = CreateTextStyle(rule);
            GUIContent iconContent = GetIconContent(rule.IconName);
            string displayName = GetDisplayName(go.name, rule);

            Rect iconRect = GetIconRect(selectionRect, rule.IconOffset);
            Rect textRect = GetTextRect(selectionRect, rule.TextOffset);

            if (iconContent != null)
            {
                GUI.Label(iconRect, iconContent);
            }

            EditorGUI.LabelField(textRect, displayName, textStyle);
        }

        private static void DrawBackground(Rect selectionRect, Color color)
        {
            Rect backgroundRect = new Rect(selectionRect)
            {
                x = selectionRect.x - 28f,
                width = selectionRect.width + 44f
            };

            EditorGUI.DrawRect(backgroundRect, color);
        }

        private static GUIStyle CreateTextStyle(HierarchyStyleConfig.StyleRule rule)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                fontStyle = rule.FontStyle,
                alignment = rule.Alignment,
                fontSize = rule.FontSize
            };

            style.normal.textColor = rule.FontColor;
            return style;
        }

        private static GUIContent GetIconContent(string iconName)
            => !string.IsNullOrEmpty(iconName) ? EditorGUIUtility.IconContent(iconName) : null;

        private static string GetDisplayName(string name, HierarchyStyleConfig.StyleRule rule)
        {
            string display = name.Replace(rule.Symbol, string.Empty).TrimStart();
            return rule.ToUpperCase ? display.ToUpper() : display;
        }

        private static Rect GetIconRect(Rect selectionRect, Vector2 offset)
            => new Rect(selectionRect.x + offset.x, selectionRect.y + offset.y, 18f, 18f);

        private static Rect GetTextRect(Rect selectionRect, Vector2 offset)
            => new Rect(selectionRect.x + 20f + offset.x, selectionRect.y + offset.y, selectionRect.width - 20f, selectionRect.height);

        private static void DrawLayerInfo(GameObject go, Rect selectionRect)
        {
            HierarchyStyleConfig.StyleRule[] rules = StyleConfig.StyleRules;

            for (int i = 0; i < rules.Length; i++)
            {
                if (!rules[i].ShowLayer)
                {
                    continue;
                }

                string layerName = LayerMask.LayerToName(go.layer);

                if (string.IsNullOrEmpty(layerName))
                {
                    break;
                }

                GUIStyle layerStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight
                };

                layerStyle.normal.textColor = Color.gray;

                Rect layerRect = new Rect(selectionRect.xMax - 60f, selectionRect.y - 1f, 55f, selectionRect.height);
                GUI.Label(layerRect, layerName, layerStyle);
                break;
            }
        }
    }
}