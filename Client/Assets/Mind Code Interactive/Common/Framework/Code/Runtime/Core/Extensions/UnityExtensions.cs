/// <summary>
/// Project : Mind Code Interactive
/// Class : UnityExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class UnityExtensions
    {
        public static Transform RecursiveFindChild(this Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform foundChild = RecursiveFindChild(child, childName);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }

        public static void SetLayersInChildren(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayersInChildren(layer);
            }
        }

        public static bool IsValid(this Vector3 value)
        {
            return !(float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z) ||
                     float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z));
        }
    }
}