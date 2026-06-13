/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIScopes.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIScopes
    {
        public class DisabledScope : GUI.Scope
        {
            private readonly bool m_previousEnabledState;

            public DisabledScope(bool shouldDisable)
            {
                m_previousEnabledState = GUI.enabled;
                if (shouldDisable)
                {
                    GUI.enabled = false;
                }
            }

            protected override void CloseScope() => GUI.enabled = m_previousEnabledState;
        }

        public class IndentScope : GUI.Scope
        {
            private readonly int m_indentIncrement;

            public IndentScope(int indentLevelIncrement = 1)
            {
                m_indentIncrement = indentLevelIncrement;
                EditorGUI.indentLevel += m_indentIncrement;
            }

            protected override void CloseScope() => EditorGUI.indentLevel -= m_indentIncrement;
        }

        public class MarginScope : GUI.Scope
        {
            public MarginScope(float marginSize = 7f)
            {
                RectOffset marginOffset = new RectOffset((int)marginSize, (int)marginSize, (int)marginSize, (int)marginSize);
                GUILayout.BeginVertical(new GUIStyle { padding = marginOffset });
            }

            public MarginScope(RectOffset marginRectOffset)
                => GUILayout.BeginVertical(new GUIStyle { padding = marginRectOffset });

            protected override void CloseScope() => GUILayout.EndVertical();
        }
    }
}