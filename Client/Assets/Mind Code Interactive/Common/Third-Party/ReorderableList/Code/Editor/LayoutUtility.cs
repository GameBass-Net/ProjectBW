/// <summary>
/// Project : Mind Code Interactive
/// Class : LayoutUtility.cs
/// Namespace : MindCodeInteractive.ReorderableList.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.ReorderableList.Code.Editor
{
    public static class LayoutUtility
    {
        private const float FOLDOUT_WIDTH = 12f;
        private const float ELEMENT_HEIGHT = 2f;

        private static bool HasFoldout(SerializedProperty propertyToCheck) => propertyToCheck.propertyType switch
        {
            SerializedPropertyType.Generic => true,
            SerializedPropertyType.Vector4 => true,
            SerializedPropertyType.Quaternion => true,
            _ => false,
        };

        private static Rect FoldoutShiftedRect(Rect originalRect)
        {
            originalRect.x += FOLDOUT_WIDTH;
            originalRect.width -= FOLDOUT_WIDTH;
            return originalRect;
        }

        public static Rect AdjustedRect(Rect originalRect, SerializedProperty propertyToAdjust)
        {
            if (HasFoldout(propertyToAdjust))
            {
                originalRect = FoldoutShiftedRect(originalRect);
            }

            originalRect.y += ELEMENT_HEIGHT;
            return originalRect;
        }

        public static float ElementHeight(SerializedProperty propertyElement)
            => EditorGUI.GetPropertyHeight(propertyElement) + ELEMENT_HEIGHT * 2f;
    }
}