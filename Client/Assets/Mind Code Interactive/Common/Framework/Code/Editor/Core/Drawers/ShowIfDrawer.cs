/// <summary>
/// Project : Mind Code Interactive
/// Class : ShowIfDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = attribute as ShowIfAttribute;
            SerializedProperty comparedProperty = GetSiblingProperty(property, showIfAttribute.ComparedPropertyName);

            if (comparedProperty == null)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }

            if (!ShouldShow(comparedProperty, showIfAttribute) && showIfAttribute.DisablingBehavior == ShowIfAttribute.DisablingType.DontDraw)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            if (property.propertyType == SerializedPropertyType.Generic)
            {
                int childCount = 0;
                float totalHeight = 0f;

                IEnumerator children = property.GetEnumerator();
                while (children.MoveNext())
                {
                    SerializedProperty child = children.Current as SerializedProperty;
                    GUIContent childLabel = new GUIContent(child.displayName);
                    totalHeight += EditorGUI.GetPropertyHeight(child, childLabel) + EditorGUIUtility.standardVerticalSpacing;
                    childCount++;
                }

                return childCount > 0 ? totalHeight - EditorGUIUtility.standardVerticalSpacing : 0f;
            }

            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = attribute as ShowIfAttribute;
            SerializedProperty comparedProperty = GetSiblingProperty(property, showIfAttribute.ComparedPropertyName);

            if (comparedProperty == null)
            {
                Debug.LogError("Cannot find compared property: " + showIfAttribute.ComparedPropertyName);
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (ShouldShow(comparedProperty, showIfAttribute))
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    IEnumerator children = property.GetEnumerator();
                    Rect rowRect = position;

                    while (children.MoveNext())
                    {
                        SerializedProperty child = children.Current as SerializedProperty;
                        GUIContent childLabel = new GUIContent(child.displayName);
                        float childHeight = EditorGUI.GetPropertyHeight(child, childLabel);
                        rowRect.height = childHeight;

                        EditorGUI.PropertyField(rowRect, child, childLabel, true);
                        rowRect.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
            else if (showIfAttribute.DisablingBehavior == ShowIfAttribute.DisablingType.ReadOnly)
            {
                bool previousEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = previousEnabled;
            }
        }

        private static bool ShouldShow(SerializedProperty comparedProperty, ShowIfAttribute showIfAttribute)
        {
            switch (comparedProperty.type)
            {
                case "bool":
                    return comparedProperty.boolValue.Equals(showIfAttribute.ComparedValue);

                case "Enum":
                    return comparedProperty.enumValueIndex.Equals((int)showIfAttribute.ComparedValue);

                default:
                    Debug.LogError("Unsupported compared property type '" + comparedProperty.type + "' on '" + showIfAttribute.ComparedPropertyName + "'.");
                    return true;
            }
        }

        private static SerializedProperty GetSiblingProperty(SerializedProperty property, string siblingName)
        {
            string propertyPath = property.propertyPath;
            int lastDotIndex = propertyPath.LastIndexOf('.');
            string pathPrefix = lastDotIndex >= 0 ? propertyPath.Substring(0, lastDotIndex + 1) : string.Empty;
            string siblingPath = pathPrefix + siblingName;
            return property.serializedObject.FindProperty(siblingPath);
        }
    }
}