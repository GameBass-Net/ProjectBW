/// <summary>
/// Project : Mind Code Interactive
/// Class : ResourcePathDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
{
    [CustomPropertyDrawer(typeof(ResourcePathAttribute))]
    public class ResourcePathDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect textFieldRect = new Rect(position.x, position.y, position.width - 30f, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 30f, position.y, 30f, position.height);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(textFieldRect, property, label);

            if (GUI.Button(buttonRect, "..."))
            {
                BrowseResourceFolder(property);
            }

            EditorGUI.EndProperty();
        }

        private static void BrowseResourceFolder(SerializedProperty property)
        {
            string projectAssetsPath = Application.dataPath;
            string selectedPath = EditorUtility.OpenFolderPanel("Select Resource Folder", projectAssetsPath, "");

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            string normalizedPath = selectedPath.Replace("\\", "/");
            int resourcesIndex = normalizedPath.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);

            if (resourcesIndex < 0)
            {
                Debug.LogError("Folder must be inside a Resources folder.");
                return;
            }

            string resourceRelativePath = normalizedPath.Substring(resourcesIndex + "/Resources/".Length);
            if (resourceRelativePath.EndsWith("/"))
            {
                resourceRelativePath = resourceRelativePath.Substring(0, resourceRelativePath.Length - 1);
            }

            property.stringValue = resourceRelativePath;
        }
    }
}