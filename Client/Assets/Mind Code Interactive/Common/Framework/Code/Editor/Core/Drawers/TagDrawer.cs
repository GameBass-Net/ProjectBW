/// <summary>
/// Project : Mind Code Interactive
/// Class : TagDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] tagNames = InternalEditorUtility.tags;

            if (property.propertyType == SerializedPropertyType.String)
            {
                HandleStringProperty(position, property, label, tagNames);
                return;
            }

            if (property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                HandleArrayProperty(position, property, label, tagNames);
                return;
            }

            EditorGUI.LabelField(position, label.text, "TagAttribute requires string or string[] property type.");
        }

        private static void HandleStringProperty(Rect position, SerializedProperty property, GUIContent label, string[] tagNames)
        {
            EnsureAtLeastOneTag(tagNames);

            if (string.IsNullOrEmpty(property.stringValue))
            {
                property.stringValue = tagNames[0];
            }

            EditorGUI.BeginProperty(position, label, property);
            string chosenTag = EditorGUI.TagField(position, label, property.stringValue);
            if (chosenTag != property.stringValue)
            {
                property.stringValue = chosenTag;
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndProperty();
        }

        private static void HandleArrayProperty(Rect position, SerializedProperty property, GUIContent label, string[] tagNames)
        {
            EnsureAtLeastOneTag(tagNames);

            EditorGUI.BeginProperty(position, label, property);

            if (property.arraySize == 0)
            {
                property.arraySize = 1;
            }

            SerializedProperty firstElement = property.GetArrayElementAtIndex(0);
            if (string.IsNullOrEmpty(firstElement.stringValue))
            {
                firstElement.stringValue = tagNames[0];
            }

            int currentMask = BuildMaskFromArray(property, tagNames);
            int updatedMask = EditorGUI.MaskField(position, label, currentMask, tagNames);

            if (updatedMask == 0 && tagNames.Length > 0)
            {
                updatedMask = 1;
            }

            if (updatedMask != currentMask)
            {
                List<string> updatedTags = BuildArrayFromMask(updatedMask, tagNames);
                property.arraySize = updatedTags.Count;
                for (int i = 0; i < updatedTags.Count; i++)
                {
                    property.GetArrayElementAtIndex(i).stringValue = updatedTags[i];
                }

                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        private static void EnsureAtLeastOneTag(string[] tagNames)
        {
            if (tagNames == null || tagNames.Length == 0)
            {
                Debug.LogWarning("No tags are defined in the project. Define at least one tag to use TagAttribute.");
            }
        }

        private static int BuildMaskFromArray(SerializedProperty arrayProperty, string[] tagNames)
        {
            int mask = 0;
            HashSet<string> selectedSet = new HashSet<string>();

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                selectedSet.Add(arrayProperty.GetArrayElementAtIndex(i).stringValue);
            }

            for (int i = 0; i < tagNames.Length; i++)
            {
                if (selectedSet.Contains(tagNames[i]))
                {
                    mask |= 1 << i;
                }
            }

            return mask;
        }

        private static List<string> BuildArrayFromMask(int mask, string[] tagNames)
        {
            List<string> tags = new List<string>();
            for (int i = 0; i < tagNames.Length; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    tags.Add(tagNames[i]);
                }
            }
            return tags;
        }
    }
}