/// <summary>
/// Project : Mind Code Interactive
/// Class : LayerDrawer.cs
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
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] layerNames = InternalEditorUtility.layers;

            if (property.propertyType == SerializedPropertyType.String)
            {
                HandleStringProperty(position, property, label, layerNames);
                return;
            }

            if (property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                HandleArrayProperty(position, property, label, layerNames);
                return;
            }

            EditorGUI.LabelField(position, label.text, "LayerAttribute requires string or string[] property type.");
        }

        private static void HandleStringProperty(Rect position, SerializedProperty property, GUIContent label, string[] layerNames)
        {
            int currentLayerIndex = LayerMask.NameToLayer(property.stringValue);
            if (currentLayerIndex < 0 || string.IsNullOrEmpty(property.stringValue))
            {
                currentLayerIndex = 0;
                property.stringValue = layerNames.Length > 0 ? layerNames[0] : string.Empty;
            }

            EditorGUI.BeginProperty(position, label, property);
            int chosenLayerIndex = EditorGUI.LayerField(position, label, currentLayerIndex);
            property.stringValue = LayerMask.LayerToName(chosenLayerIndex);
            EditorGUI.EndProperty();
        }

        private static void HandleArrayProperty(Rect position, SerializedProperty property, GUIContent label, string[] layerNames)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.arraySize == 0)
            {
                property.arraySize = 1;
            }

            SerializedProperty firstElement = property.GetArrayElementAtIndex(0);
            if (string.IsNullOrEmpty(firstElement.stringValue))
            {
                firstElement.stringValue = layerNames.Length > 0 ? layerNames[0] : string.Empty;
            }

            int currentMask = BuildMaskFromArray(property, layerNames);
            int updatedMask = EditorGUI.MaskField(position, label, currentMask, layerNames);
            if (updatedMask == 0)
            {
                updatedMask = 1;
            }

            if (updatedMask != currentMask)
            {
                List<string> updatedList = new List<string>();
                for (int i = 0; i < layerNames.Length; i++)
                {
                    if ((updatedMask & (1 << i)) != 0)
                    {
                        updatedList.Add(layerNames[i]);
                    }
                }

                property.arraySize = updatedList.Count;
                for (int i = 0; i < updatedList.Count; i++)
                {
                    property.GetArrayElementAtIndex(i).stringValue = updatedList[i];
                }
            }

            EditorGUI.EndProperty();
        }

        private static int BuildMaskFromArray(SerializedProperty arrayProperty, string[] layerNames)
        {
            int mask = 0;
            HashSet<string> selectedSet = new HashSet<string>();

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                selectedSet.Add(arrayProperty.GetArrayElementAtIndex(i).stringValue);
            }

            for (int i = 0; i < layerNames.Length; i++)
            {
                if (selectedSet.Contains(layerNames[i]))
                {
                    mask |= 1 << i;
                }
            }

            return mask;
        }
    }
}