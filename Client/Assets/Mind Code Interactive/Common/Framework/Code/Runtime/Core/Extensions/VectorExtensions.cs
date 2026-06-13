/// <summary>
/// Project : Mind Code Interactive
/// Class : VectorExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 Round(this Vector3 value, int decimals = 3)
        {
            float multiplier = Mathf.Pow(10f, decimals);
            return new Vector3(
                Mathf.Round(value.x * multiplier) / multiplier,
                Mathf.Round(value.y * multiplier) / multiplier,
                Mathf.Round(value.z * multiplier) / multiplier);
        }

        public static Bounds RoundBounds(this Bounds bounds, int decimals = 3)
        {
            return new Bounds(bounds.center.Round(decimals), bounds.size.Round(decimals));
        }

        public static Bounds ToWorldBounds(this Bounds localBounds, Transform transform)
        {
            Vector3 center = localBounds.center;
            Vector3 extents = localBounds.extents;

            Bounds bounds = new Bounds(transform.TransformPoint(center + new Vector3(extents.x, extents.y, extents.z)), Vector3.zero);
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(extents.x, extents.y, -extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(extents.x, -extents.y, extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(extents.x, -extents.y, -extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(-extents.x, extents.y, extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(-extents.x, extents.y, -extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(-extents.x, -extents.y, extents.z)));
            bounds.Encapsulate(transform.TransformPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)));
            return bounds;
        }
    }
}