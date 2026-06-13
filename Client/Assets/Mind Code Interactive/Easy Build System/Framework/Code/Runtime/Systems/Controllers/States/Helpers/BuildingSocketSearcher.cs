/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocketSearcher.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Helpers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Helpers
{
    public sealed class BuildingSocketSearcher
    {
        private static readonly Vector3[] OBSTRUCTION_OFFSETS =
        {
            Vector3.zero,
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back
        };

        private readonly HashSet<BuildingSocket> m_cache = new HashSet<BuildingSocket>();

        public BuildingSocket FindBest(BuildingPart preview, Vector3 targetPosition, BuildingView view, int socketLayer)
        {
            if (preview == null || view == null)
            {
                return null;
            }

            SnapQuery query = view.BuildSnapQuery(targetPosition);
            SnapSettings settings = view.GetSnapSettings();

            if (!CollectCandidates(query, socketLayer))
            {
                return null;
            }

            BuildingSocket best = SelectBest(preview, query, settings);

            return best != null && IsVisible(best, view.GetRay(), settings) ? best : null;
        }

        private bool CollectCandidates(SnapQuery query, int socketLayer)
        {
            m_cache.Clear();

            int hitCount = PhysicsExtensions.OverlapSphereNonAlloc(
                query.Origin,
                query.MaxDistance,
                out Collider[] colliders,
                socketLayer,
                null,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                BuildingSocket socket = colliders[i].GetComponent<BuildingSocket>();
                if (socket != null)
                {
                    m_cache.Add(socket);
                }
            }

            return m_cache.Count > 0;
        }

        private BuildingSocket SelectBest(BuildingPart preview, SnapQuery query, SnapSettings settings)
        {
            BuildingSocket best = null;
            int bestPriority = int.MinValue;
            float bestScore = float.MaxValue;

            foreach (BuildingSocket socket in m_cache)
            {
                if (!socket.IsEnabled() || !socket.IsFitting(preview))
                {
                    continue;
                }

                float score = ScoreCandidate(socket, query, settings);
                if (score < 0f)
                {
                    continue;
                }

                int priority = socket.SocketProperty;
                if (priority > bestPriority || (priority == bestPriority && score < bestScore))
                {
                    best = socket;
                    bestPriority = priority;
                    bestScore = score;
                }
            }

            return best;
        }

        private static float ScoreCandidate(BuildingSocket socket, SnapQuery query, SnapSettings settings)
        {
            Vector3 socketPosition = socket.transform.position;
            Vector3 toSocket = socketPosition - query.Origin;
            float projection = Vector3.Dot(toSocket, query.Direction);

            if (projection < 0f || projection > query.MaxDistance)
            {
                return -1f;
            }

            Vector3 closestPoint = query.Origin + query.Direction * projection;
            float perpendicular = Vector3.Distance(socketPosition, closestPoint);

            if (perpendicular > query.Radius)
            {
                return -1f;
            }

            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(query.Direction, toSocket.normalized), -1f, 1f)) * Mathf.Rad2Deg;
            if (angle > settings.MaxAngle)
            {
                return -1f;
            }

            return projection + perpendicular * 0.25f + angle * 0.01f;
        }

        private static bool IsVisible(BuildingSocket socket, Ray ray, in SnapSettings settings)
        {
            if (!settings.ObstructionCheck)
            {
                return true;
            }

            Vector3 socketPosition = socket.transform.position;
            int mask = settings.ObstructionLayers & ~(1 << socket.gameObject.layer);
            float radius = socket.SocketRadius;

            for (int i = 0; i < OBSTRUCTION_OFFSETS.Length; i++)
            {
                Vector3 testPoint = socketPosition + OBSTRUCTION_OFFSETS[i] * radius;
                if (!PhysicsExtensions.LinecastNonAlloc(ray.origin, testPoint, out _, mask))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
