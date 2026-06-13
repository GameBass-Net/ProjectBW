/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorGUIElements.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles
{
    public static class EditorGUIElements
    {
        public enum MessageType { None, Info, Success, Warning, Error }

        public delegate bool OnDragDropCallback(Object[] draggedObjects);

        public static void InspectorHeader(Object target, string headerDescription)
            => InspectorHeader(ObjectNames.NicifyVariableName(target.GetType().Name), headerDescription);

        public static void InspectorHeader(string headerTitle, string headerDescription)
        {
            GUILayout.Space(3f);
            GUILayout.BeginVertical();
            if (!string.IsNullOrEmpty(headerTitle))
            {
                GUIStyle titleStyle = EditorGUIUtility.isProSkin
                    ? new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 12,
                        normal = { textColor = EditorGUIStyles.s_headerTextColor },
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    }
                    : new GUIStyle(EditorStyles.largeLabel)
                    {
                        fontSize = 12,
                        normal = { textColor = EditorGUIStyles.s_headerTextColor },
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                EditorGUILayout.LabelField(headerTitle, titleStyle, GUILayout.ExpandWidth(false));
            }
            InspectorDescription(headerDescription, true);
            GUILayout.EndVertical();
        }

        public static void InspectorDescription(string descriptionText, bool shouldDrawSeparator = true)
        {
            if (!string.IsNullOrEmpty(descriptionText))
            {
                using (new EditorGUIScopes.DisabledScope(false))
                {
                    GUIStyle descriptionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel)
                    {
                        normal = { textColor = EditorGUIUtility.isProSkin ? Color.white * 0.7f : Color.black * 0.7f }
                    };

                    bool isFirstLine = true;
                    string[] textLines = descriptionText.Split('\n');

                    for (int i = 0; i < textLines.Length; i++)
                    {
                        if (!isFirstLine)
                        {
                            GUILayout.Space(-3f);
                        }

                        GUILayout.Label(textLines[i], descriptionStyle);
                        isFirstLine = false;
                    }
                }
            }

            if (shouldDrawSeparator)
            {
                Separator();
            }
        }

        public static void InspectorBottom()
        {
            Separator(false);

            Color footerColor = EditorGUIUtility.isProSkin ? Color.grey : Color.black * 0.6f;
            GUI.color = footerColor;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Copyright © 2015-2026 Mind Code Interactive. All rights reserved.", EditorStyles.wordWrappedMiniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.color = Color.white;
            EditorGUILayout.Separator();
        }

        public static void Separator(bool shouldIndent = false)
        {
            EditorGUILayout.Separator();

            Rect separatorRect = EditorGUILayout.GetControlRect(false, 1f);

            if (shouldIndent)
            {
                float indentPixelOffset = EditorGUI.indentLevel * 15f;
                separatorRect.x += indentPixelOffset;
                separatorRect.width -= indentPixelOffset;
            }

            EditorGUI.DrawRect(separatorRect, EditorGUIStyles.SeparatorColor);
            EditorGUILayout.Separator();
        }

        public static void Separator(string separatorLabel, bool shouldDrawHeaderSeparator = true)
        {
            if (shouldDrawHeaderSeparator)
            {
                Separator();
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black }
            };
            GUILayout.Space(-5f);
            EditorGUILayout.LabelField(separatorLabel, labelStyle);
            GUILayout.Space(1f);
        }

        public static void HelpBox(string messageText, MessageType messageType)
        {
            GUILayout.Space(3f);

            Color boxBackgroundColor = GetHelpBoxColor(messageType);
            Color boxBorderColor = GetHelpBoxBorderColor(messageType);

            GUI.color = boxBackgroundColor;

            Rect boxDrawingRect = EditorGUILayout.BeginHorizontal(EditorGUIStyles.BorderBoxWithPaddingStyle);
            GUI.Box(boxDrawingRect, GUIContent.none, EditorGUIStyles.BorderBoxWithPaddingStyle);

            Rect leftBorderRect = new Rect(boxDrawingRect.x + 0.5f, boxDrawingRect.y + 0.5f, 3f, boxDrawingRect.height - 2f);
            EditorGUI.DrawRect(leftBorderRect, boxBorderColor);

            GUI.color = Color.white;

            using (EditorGUIExtended.MarginScope(2))
            {
                GUILayout.BeginVertical(GUILayout.ExpandHeight(false));

                string[] messageLines = messageText.Split('\n');
                for (int i = 0; i < messageLines.Length; i++)
                {
                    EditorGUIExtended.ColoredLabel(messageLines[i], Color.white);
                    if (i < messageLines.Length - 1)
                    {
                        GUILayout.Space(1f);
                    }
                }

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        public static void DragAndDropArea(string dragMessageText, OnDragDropCallback onObjectsDropped, Func<Object, bool> validateObjectFunction = null, bool allowProjectAssets = true, bool allowSceneObjects = true)
        {
            EditorGUILayout.Space(3f);

            Rect dropZoneArea = GUILayoutUtility.GetRect(0, 60f, GUILayout.ExpandWidth(true));

            bool canAcceptDragDrop = allowProjectAssets || allowSceneObjects;
            Color dropZoneBackgroundColor = canAcceptDragDrop ? Color.white : Color.grey * 0.5f;
            GUI.color = dropZoneBackgroundColor;
            GUI.Box(dropZoneArea, string.Empty, EditorGUIExtended.Styles.BorderBoxWithPaddingStyle);
            GUI.color = Color.white;

            GUIStyle dropZoneMessageStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal,
                fontSize = 10,
                normal = { textColor = canAcceptDragDrop ? Color.grey : Color.grey * 0.7f }
            };

            GUI.Box(dropZoneArea, dragMessageText, dropZoneMessageStyle);

            if (canAcceptDragDrop && dropZoneArea.Contains(Event.current.mousePosition))
            {
                HandleDragAndDropFiltered(validateObjectFunction, onObjectsDropped, allowProjectAssets, allowSceneObjects);
            }

            EditorGUILayout.Space(3f);
        }

        private static Color GetHelpBoxColor(MessageType messageType)
        {
            if (!EditorGUIUtility.isProSkin)
            {
                return new Color(0.9f, 0.9f, 0.9f, 0.8f);
            }

            return messageType switch
            {
                MessageType.Info => new Color(0.85f, 0.92f, 1f, 0.8f),
                MessageType.Warning => new Color(1f, 0.95f, 0.8f, 0.8f),
                MessageType.Error => new Color(1f, 0.85f, 0.85f, 0.8f),
                MessageType.Success => new Color(0.85f, 1f, 0.85f, 0.8f),
                _ => new Color(0.9f, 0.9f, 0.9f, 0.8f),
            };
        }

        private static Color GetHelpBoxBorderColor(MessageType messageType) => messageType switch
        {
            MessageType.Info => EditorGUIExtended.ColorPalette.Info,
            MessageType.Warning => EditorGUIExtended.ColorPalette.Warning,
            MessageType.Error => EditorGUIExtended.ColorPalette.Error,
            MessageType.Success => EditorGUIExtended.ColorPalette.Success,
            _ => new Color(0.6f, 0.6f, 0.6f),
        };

        private static void HandleDragAndDropFiltered(Func<Object, bool> validateObjectFunction, OnDragDropCallback onObjectsDropped, bool allowProjectAssets, bool allowSceneObjects)
        {
            Event currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                    {
                        bool isAnyObjectValid = false;

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (!IsObjectAllowed(draggedObject, allowProjectAssets, allowSceneObjects))
                            {
                                continue;
                            }

                            if (validateObjectFunction == null || validateObjectFunction(draggedObject))
                            {
                                isAnyObjectValid = true;
                                break;
                            }
                        }

                        DragAndDrop.visualMode = isAnyObjectValid ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                        currentEvent.Use();
                        break;
                    }

                case EventType.DragPerform:
                    {
                        DragAndDrop.AcceptDrag();
                        currentEvent.Use();

                        Object[] filteredDraggedObjects = DragAndDrop.objectReferences
                            .Where(obj => IsObjectAllowed(obj, allowProjectAssets, allowSceneObjects))
                            .ToArray();

                        if (filteredDraggedObjects.Length > 0)
                        {
                            onObjectsDropped?.Invoke(filteredDraggedObjects);
                        }

                        break;
                    }
            }
        }

        private static bool IsObjectAllowed(Object droppedObject, bool allowProjectAssets, bool allowSceneObjects)
        {
            GameObject droppedGameObject = droppedObject as GameObject;
            if (droppedGameObject == null)
            {
                return false;
            }

            bool isSceneGameObject = droppedGameObject.scene.IsValid();
            bool isProjectAssetGameObject = !isSceneGameObject;

            if (isSceneGameObject && !allowSceneObjects)
            {
                return false;
            }

            if (isProjectAssetGameObject && !allowProjectAssets)
            {
                return false;
            }

            return true;
        }
    }
}