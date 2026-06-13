/// <summary>
/// Project : Mind Code Interactive
/// Class : PropertyCollection.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
{
    public class PropertyCollection : Dictionary<string, SerializedProperty>
    {
        public SerializedProperty Get(string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            string[] pathSegments = propertyPath.Split('.');

            if (!TryGetValue(pathSegments[0], out SerializedProperty currentProperty))
            {
                Debug.LogWarning("SerializedProperty not found: '" + propertyPath + "'");
                return null;
            }

            for (int i = 1; i < pathSegments.Length; i++)
            {
                if (currentProperty == null)
                {
                    Debug.LogWarning("SerializedProperty not found: '" + propertyPath + "'");
                    return null;
                }

                string pathSegment = pathSegments[i];

                if (int.TryParse(pathSegment, out int arrayIndex))
                {
                    currentProperty = currentProperty.GetArrayElementAtIndex(arrayIndex);
                }
                else
                {
                    currentProperty = currentProperty.FindPropertyRelative(pathSegment);
                }
            }

            if (currentProperty == null && Selection.objects.Length < 1)
            {
                Debug.LogWarning("SerializedProperty not found: '" + propertyPath + "'");
            }

            return currentProperty;
        }

        public void Draw(string propertyPath, GUIContent guiLabel = null, bool includeChildProperties = true)
        {
            SerializedProperty property = Get(propertyPath);

            if (property == null)
            {
                if (Selection.objects == null || Selection.objects.Length < 1)
                {
                    Debug.LogWarning("SerializedProperty not found: '" + propertyPath + "'");
                }

                return;
            }

            SerializedObject propertySerializedObject = property.serializedObject;
            if (propertySerializedObject == null || propertySerializedObject.targetObject == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(property, guiLabel ?? new GUIContent(property.displayName), includeChildProperties);
        }

        public void DrawArray(string propertyPath, GUIContent guiLabel = null)
        {
            SerializedProperty arrayProperty = Get(propertyPath);
            if (arrayProperty == null || !arrayProperty.isArray)
            {
                Debug.LogWarning("Property '" + propertyPath + "' is not a valid array.");
                return;
            }

            EditorGUILayout.PropertyField(arrayProperty, guiLabel ?? new GUIContent(arrayProperty.displayName), true);
        }

        public static PropertyCollection CreateFrom(SerializedObject sourceSerializedObject)
        {
            PropertyCollection createdCollection = new PropertyCollection();
            if (sourceSerializedObject == null)
            {
                return createdCollection;
            }

            SerializedProperty iteratorProperty = sourceSerializedObject.GetIterator();
            if (iteratorProperty?.NextVisible(true) ?? false)
            {
                do
                {
                    createdCollection[iteratorProperty.name] = iteratorProperty.Copy();
                }
                while (iteratorProperty.NextVisible(false));
            }

            return createdCollection;
        }
    }
}