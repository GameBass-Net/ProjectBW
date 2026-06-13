/// <summary>
/// Project : Mind Code Interactive
/// Class : SerializedPropertyExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Extensions
{
    public static class SerializedPropertyExtensions
    {
        public static void DrawRelative(this SerializedProperty property, string propertyName, GUIContent label)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            SerializedProperty relativeProperty = property.FindPropertyRelative(propertyName);
            if (relativeProperty == null)
            {
                Debug.LogWarning("Relative property '" + propertyName + "' was not found.");
                return;
            }

            EditorGUILayout.PropertyField(relativeProperty, label, true);
        }

        public static SerializedProperty GetRelative(this SerializedProperty property, string propertyName)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            return property.FindPropertyRelative(propertyName);
        }
    }
}