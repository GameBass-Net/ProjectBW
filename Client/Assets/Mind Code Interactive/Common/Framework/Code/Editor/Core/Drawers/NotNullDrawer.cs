/// <summary>
/// Project : Mind Code Interactive
/// Class : NotNullDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
{
    [CustomPropertyDrawer(typeof(NotNullAttribute))]
    public class NotNullDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isNull = property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null;
            GUI.color = isNull ? Color.red : Color.white;
            EditorGUI.PropertyField(position, property, label);
            GUI.color = Color.white;
        }
    }
}