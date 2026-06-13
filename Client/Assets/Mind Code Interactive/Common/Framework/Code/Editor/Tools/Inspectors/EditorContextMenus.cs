/// <summary>
/// Project: Mind Code Interactive
/// Class: EditorContextMenus.cs
/// Namespace: MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
/// Copyright: © 2015–2026 Mind Code Interactive. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
{
    public static class EditorContextMenus
    {
        private const string k_ClipboardPrefix = "MCI_CB|";

        public static void Separator(GenericMenu menu)
        {
            menu.AddSeparator(string.Empty);
        }

        public static void AddItem(GenericMenu menu, string label, Action onClick)
        {
            menu.AddItem(new GUIContent(label), false, () =>
            {
                if (onClick == null)
                {
                    return;
                }

                EditorApplication.delayCall += () => onClick.Invoke();
            });
        }

        public static void AddDisabledItem(GenericMenu menu, string label)
        {
            menu.AddDisabledItem(new GUIContent(label));
        }

        public static void AddConfirmItem(GenericMenu menu, string label, string title, string message, Action onConfirm)
        {
            AddItem(menu, label, () =>
            {
                if (EditorUtility.DisplayDialog(title, message, "Yes", "Cancel"))
                {
                    onConfirm?.Invoke();
                }
            });
        }

        public static void AddRemoveComponentItem(GenericMenu menu, MonoBehaviour component, Action onRemove)
        {
            AddConfirmItem(menu, "Remove", "Confirm Removal", $"Remove '{component.GetType().Name}'?", onRemove);
        }

        public static void AddDuplicateItem(GenericMenu menu, Action onDuplicate)
        {
            AddItem(menu, "Duplicate", onDuplicate);
        }

        public static void AddResetItem(GenericMenu menu, Action onReset)
        {
            AddConfirmItem(menu, "Reset", "Confirm Reset", "Reset to default values?", onReset);
        }

        public static void AddEditScriptItem(GenericMenu menu, MonoBehaviour component)
        {
            AddItem(menu, "Edit Script", () =>
            {
                MonoScript script = MonoScript.FromMonoBehaviour(component);
                AssetDatabase.OpenAsset(script);
            });
        }

        public static void AddMoveUpItem(GenericMenu menu, bool enabled, Action onMove)
        {
            if (enabled)
            {
                AddItem(menu, "Move Up", onMove);
                return;
            }

            AddDisabledItem(menu, "Move Up");
        }

        public static void AddMoveDownItem(GenericMenu menu, bool enabled, Action onMove)
        {
            if (enabled)
            {
                AddItem(menu, "Move Down", onMove);
                return;
            }

            AddDisabledItem(menu, "Move Down");
        }

        public static void AddToggleItem(GenericMenu menu, string label, bool currentState, Action<bool> onToggle)
        {
            AddItem(menu, label, () => onToggle?.Invoke(!currentState));
        }

        public static void AddCopyPasteForType(GenericMenu menu, Type dataType, Action onCopy, Action<string> onPaste)
        {
            AddItem(menu, "Copy", onCopy);

            if (TryGetJsonClipboard(dataType, out string json))
            {
                AddItem(menu, "Paste", () => onPaste?.Invoke(json));
                return;
            }

            AddDisabledItem(menu, "Paste");
        }

        public static void SetJsonClipboard(Type dataType, string json)
        {
            EditorGUIUtility.systemCopyBuffer = $"{k_ClipboardPrefix}{dataType.AssemblyQualifiedName}|{json}";
        }

        public static bool TryGetJsonClipboard(Type dataType, out string json)
        {
            json = null;

            string buffer = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrEmpty(buffer) || !buffer.StartsWith(k_ClipboardPrefix))
            {
                return false;
            }

            int separatorIndex = buffer.IndexOf('|', k_ClipboardPrefix.Length);
            if (separatorIndex < 0)
            {
                return false;
            }

            string storedTypeName = buffer.Substring(k_ClipboardPrefix.Length, separatorIndex - k_ClipboardPrefix.Length);
            if (storedTypeName != dataType.AssemblyQualifiedName)
            {
                return false;
            }

            json = buffer.Substring(separatorIndex + 1);
            return !string.IsNullOrEmpty(json);
        }

        public static void RunOnTargets<T>(T[] targets, string undoLabel, Action<T> action) where T : Object
        {
            if (targets == null || action == null)
            {
                return;
            }

            foreach (T target in targets)
            {
                if (!target)
                {
                    continue;
                }

                Undo.RecordObject(target, undoLabel);
                action.Invoke(target);
                EditorUtility.SetDirty(target);
            }

            SceneView.RepaintAll();
        }

        public static (bool canMoveUp, bool canMoveDown) GetMoveCapabilities<TTarget, TItem>(
            TTarget[] targets,
            Type itemType,
            Func<TTarget, List<TItem>> getList)
            where TTarget : Object
        {
            bool canMoveUp = false;
            bool canMoveDown = false;

            foreach (TTarget target in targets)
            {
                if (!target)
                {
                    continue;
                }

                List<TItem> list = getList(target);
                int index = list.FindIndex(item => item != null && item.GetType() == itemType);

                if (index > 0)
                {
                    canMoveUp = true;
                }

                if (index >= 0 && index < list.Count - 1)
                {
                    canMoveDown = true;
                }

                if (canMoveUp && canMoveDown)
                {
                    break;
                }
            }

            return (canMoveUp, canMoveDown);
        }

        public static void MoveItemInList<TItem>(List<TItem> list, Type itemType, int direction)
        {
            int index = list.FindIndex(item => item != null && item.GetType() == itemType);
            int targetIndex = index + direction;

            if (index < 0 || targetIndex < 0 || targetIndex >= list.Count)
            {
                return;
            }

            TItem temp = list[index];
            list[index] = list[targetIndex];
            list[targetIndex] = temp;
        }

        public static void AddMoveItemsForType<TTarget, TItem>(
            GenericMenu menu,
            TTarget[] targets,
            Type itemType,
            Func<TTarget, List<TItem>> getList,
            string undoLabel)
            where TTarget : Object
        {
            (bool canMoveUp, bool canMoveDown) = GetMoveCapabilities(targets, itemType, getList);

            AddMoveUpItem(menu, canMoveUp, () =>
            {
                RunOnTargets(targets, $"Move {undoLabel} Up", target =>
                {
                    MoveItemInList(getList(target), itemType, -1);
                });
            });

            AddMoveDownItem(menu, canMoveDown, () =>
            {
                RunOnTargets(targets, $"Move {undoLabel} Down", target =>
                {
                    MoveItemInList(getList(target), itemType, 1);
                });
            });
        }
    }
}
