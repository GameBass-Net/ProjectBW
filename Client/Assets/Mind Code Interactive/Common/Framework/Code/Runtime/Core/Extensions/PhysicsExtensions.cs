/// <summary>
/// Project : Mind Code Interactive
/// Class : PhysicsExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class PhysicsExtensions
    {
        public static readonly LayerMask s_allLayers = ~0;
        public static readonly Collider[] s_overlappedColliders = new Collider[512];
        public static readonly RaycastHit[] s_raycastHits = new RaycastHit[128];

        public static int ToLayerIndex(this LayerMask mask)
        {
            int maskValue = mask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((maskValue & (1 << i)) != 0)
                {
                    return i;
                }
            }
            return 0;
        }

        public static int RaycastAllNonAlloc(Ray ray, float distance, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return Physics.RaycastNonAlloc(ray, s_raycastHits, distance, layerMask, triggerInteraction);
        }

        public static bool RaycastNonAlloc(Ray ray, float distance, out RaycastHit hitInfo, int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, s_raycastHits, distance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                int closestHit = -1;
                float closestDistance = Mathf.Infinity;
                bool hasIgnoredRoot = ignoredRoot != null;

                for (int i = 0; i < hitCount; i++)
                {
                    if (hasIgnoredRoot && s_raycastHits[i].transform.IsChildOf(ignoredRoot))
                    {
                        continue;
                    }

                    if (s_raycastHits[i].distance < closestDistance)
                    {
                        closestDistance = s_raycastHits[i].distance;
                        closestHit = i;
                    }
                }

                if (closestHit != -1)
                {
                    hitInfo = s_raycastHits[closestHit];
                    return true;
                }
            }

            hitInfo = default;
            return false;
        }

        public static bool LinecastNonAlloc(Vector3 from, Vector3 to, out RaycastHit hitInfo, int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (distance <= 0f)
            {
                hitInfo = default;
                return false;
            }

            Ray ray = new Ray(from, direction / distance);
            return RaycastNonAlloc(ray, distance, out hitInfo, layerMask, ignoredRoot, triggerInteraction);
        }

        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 extents, Quaternion orientation, out Collider[] colliders, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int size = Physics.OverlapBoxNonAlloc(center, extents, s_overlappedColliders, orientation, layerMask, triggerInteraction);
            colliders = s_overlappedColliders;
            return size;
        }

        public static int OverlapSphereNonAlloc(Vector3 position, float radius, out Collider[] colliders, int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int totalCount = Physics.OverlapSphereNonAlloc(position, radius, s_overlappedColliders, layerMask, triggerInteraction);

            if (ignoredRoot == null)
            {
                colliders = s_overlappedColliders;
                return totalCount;
            }

            int validCount = 0;
            for (int i = 0; i < totalCount; i++)
            {
                if (!s_overlappedColliders[i].transform.IsChildOf(ignoredRoot))
                {
                    s_overlappedColliders[validCount] = s_overlappedColliders[i];
                    validCount++;
                }
            }

            colliders = s_overlappedColliders;
            return validCount;
        }

        public static bool BoxCastNonAlloc(Vector3 center, Vector3 halfExtents, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int count = Physics.BoxCastNonAlloc(
                center,
                halfExtents,
                direction,
                s_raycastHits,
                Quaternion.identity,
                maxDistance,
                layerMask,
                triggerInteraction);

            if (count > 0)
            {
                int closest = -1;
                float closestDistance = Mathf.Infinity;
                bool ignore = ignoredRoot != null;

                for (int i = 0; i < count; i++)
                {
                    if (ignore && s_raycastHits[i].transform.IsChildOf(ignoredRoot))
                    {
                        continue;
                    }

                    float distance = s_raycastHits[i].distance;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = i;
                    }
                }

                if (closest != -1)
                {
                    hitInfo = s_raycastHits[closest];
                    return true;
                }
            }

            hitInfo = default;
            return false;
        }

        public static int SphereCastAllNonAlloc(Vector3 origin, float radius, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            return Physics.SphereCastNonAlloc(origin, radius, direction.normalized, s_raycastHits, maxDistance, layerMask, triggerInteraction);
        }
    }
}