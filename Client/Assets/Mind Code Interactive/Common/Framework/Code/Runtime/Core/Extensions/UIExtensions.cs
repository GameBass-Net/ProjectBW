/// <summary>
/// Project : Mind Code Interactive
/// Class : UIExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class UIExtensions
    {
        public static bool IsPointerOverUIObject() => EventSystem.current == null ? false : EventSystem.current.IsPointerOverGameObject();

        public static bool IsInputFieldInFocus()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            if (selectedObject == null)
            {
                return false;
            }

            if (selectedObject.TryGetComponent(out InputField inputField))
            {
                return inputField.isFocused;
            }

#if TMP_PRESENT
            if (selectedObject.TryGetComponent(out TMP_InputField textMeshProInputField))
                return textMeshProInputField.isFocused;
#endif

            return false;
        }

        public static bool IsInteractionActive() => IsPointerOverUIObject() || IsInputFieldInFocus();
    }
}