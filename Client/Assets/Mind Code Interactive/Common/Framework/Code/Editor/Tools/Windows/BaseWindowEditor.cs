/// <summary>
/// Project : Mind Code Interactive
/// Class : BaseWindowEditor.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows
{
    public abstract class BaseWindowEditor : EditorWindow
    {
        protected virtual void OnEnable() => OnWindowEnable();

        protected virtual void OnDisable() => OnWindowDisable();

        protected virtual void OnWindowEnable() { }

        protected virtual void OnWindowDisable() { }

        public static T OpenWindow<T>(GUIContent windowTitle, Vector2 windowSize, bool isResizable = true, bool isUtilityWindow = false) where T : EditorWindow
        {
            T createdWindow = GetWindow<T>(isUtilityWindow, windowTitle.text, true);
            float editorPixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
            float screenDisplayWidth = Screen.currentResolution.width / editorPixelsPerPoint;
            float screenDisplayHeight = Screen.currentResolution.height / editorPixelsPerPoint;
            float centeredPositionX = (screenDisplayWidth - windowSize.x) * 0.5f;
            float centeredPositionY = (screenDisplayHeight - windowSize.y) * 0.5f;
            createdWindow.position = new Rect(Mathf.Round(centeredPositionX), Mathf.Round(centeredPositionY), windowSize.x, windowSize.y);
            createdWindow.minSize = windowSize;
            if (!isResizable)
            {
                createdWindow.maxSize = windowSize;
            }

            createdWindow.titleContent = windowTitle;
            createdWindow.Show();
            createdWindow.Focus();
            return createdWindow;
        }
    }
}