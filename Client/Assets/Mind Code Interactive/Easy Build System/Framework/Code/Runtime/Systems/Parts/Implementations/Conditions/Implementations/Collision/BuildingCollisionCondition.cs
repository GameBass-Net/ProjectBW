/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionCondition.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision
{
    [BuildingCondition("Building Collision Condition",
        "Validates placement by detecting collisions, terrain contact and overlapping Building Parts.")]
    public class BuildingCollisionCondition : BuildingCondition
    {
        public enum RequirementEvaluationType { AllMustMatch, AtLeastOneMatches }

        public struct BoundCollisionResult
        {
            public bool HasCollision;
            public bool HasTerrain;
            public int HitCount;
            public Collider CollidingCollider;
        }

        [SerializeField] private RequirementEvaluationType m_requirementEvaluationMode = RequirementEvaluationType.AllMustMatch;
        [SerializeField] private BuildingCollisionBoundsData[] m_collisionBounds;

        protected Collider[] m_overlapBuffer = new Collider[64];

        public override int EvaluationOrder => 0;

        public RequirementEvaluationType RequirementEvaluationMode { get => m_requirementEvaluationMode; set => m_requirementEvaluationMode = value; }

        public BuildingCollisionBoundsData[] CollisionBounds { get => m_collisionBounds; set => m_collisionBounds = value; }

        protected override ConditionResult EvaluateInternal(BuildingMode buildMode)
        {
            if (buildMode != BuildingMode.Placement && buildMode != BuildingMode.Adjustment)
            {
                return new ConditionResult(true);
            }

            return CheckCollision(Part.transform.position, Part.transform.rotation, Part.transform.localScale);
        }

        public ConditionResult PredictCollision(Vector3 position, Quaternion rotation, Vector3 scale, bool forceSnapped)
        {
            return CheckCollision(position, rotation, scale, forceSnapped);
        }

        public virtual ConditionResult CheckCollision(Vector3 position, Quaternion rotation, Vector3 scale, bool forceSnapped = false)
        {
            if (!Part)
            {
                return new ConditionResult(false, "No building part is assigned.");
            }

            bool isSnapped = forceSnapped || Part.AttachedSocket != null;
            BuildingCollisionBoundsData[] list = GetCollisionBounds();
            bool collision = false;
            bool terrain = false;

            for (int i = 0; i < list.Length; i++)
            {
                BuildingCollisionBoundsData data = list[i];
                if (data == null)
                {
                    continue;
                }

                BoundCollisionResult bound = CheckBoundCollision(data, position, rotation, scale, isSnapped);

                if (data.PreventOverlapping && CheckOverlapping(data, position, rotation, scale))
                {
                    return new ConditionResult(false, "Overlapping with another building part.");
                }

                if (bound.HasTerrain)
                {
                    terrain = true;
                }

                if (bound.HasCollision)
                {
                    collision = true;

                    if (!data.RequireCollision)
                    {
                        string collidingObjectName = bound.CollidingCollider != null ? bound.CollidingCollider.name : "Unknown Object";
                        return new ConditionResult(false, $"Part is colliding with '{collidingObjectName}'.");
                    }
                }
            }

            if (list.Length > 0)
            {
                BuildingCollisionBoundsData d = list[0];

                if (m_requirementEvaluationMode == RequirementEvaluationType.AllMustMatch)
                {
                    if (d.RequireCollision && !collision)
                    {
                        return new ConditionResult(false, "Part must touch another object.");
                    }

                    if (d.RequireTerrain && !terrain)
                    {
                        return new ConditionResult(false, "Part must be placed on terrain.");
                    }
                }
                else
                {
                    if ((d.RequireCollision && collision) ||
                        (d.RequireTerrain && terrain))
                    {
                        return new ConditionResult(true);
                    }

                    if (d.RequireCollision || d.RequireTerrain)
                    {
                        return new ConditionResult(false, "At least one requirement must be met.");
                    }
                }
            }

            return new ConditionResult(true);
        }

        protected virtual bool CheckOverlapping(
            BuildingCollisionBoundsData data,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale)
        {
            if (Part == null)
            {
                return false;
            }

            Vector3 localCenter = data.OverlappingCenter != Vector3.zero ? data.OverlappingCenter : data.Center;
            Vector3 localSize = data.OverlappingSize != Vector3.one ? data.OverlappingSize : data.Size;

            float overlapTolerance = data.OverlappingTolerance > 0f ? data.OverlappingTolerance : 1f;

            Vector3 worldCenter = position + rotation * localCenter;
            Vector3 half = Vector3.Scale(localSize * 0.5f, scale * overlapTolerance);

            int hits = PhysicsExtensions.OverlapBoxNonAlloc(
                worldCenter,
                half,
                rotation,
                out m_overlapBuffer,
                data.CollisionLayer,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits; i++)
            {
                Collider collider = m_overlapBuffer[i];
                if (collider == null)
                {
                    continue;
                }

                BuildingPart hitPart = collider.GetComponentInParent<BuildingPart>();
                if (hitPart == null || hitPart == Part)
                {
                    continue;
                }

                if (hitPart == BuildingPart.LastPlacedPart &&
                    BuildingPart.LastPlacedFrame == Time.frameCount)
                {
                    continue;
                }

                if (data.IgnoreOverlappingTypes != null &&
                    data.IgnoreOverlappingTypes.Length > 0 &&
                    Array.IndexOf(data.IgnoreOverlappingTypes, hitPart.Category) >= 0)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        protected virtual BoundCollisionResult CheckBoundCollision(
            BuildingCollisionBoundsData data,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            bool isSnapped)
        {
            BoundCollisionResult result = new BoundCollisionResult();

            float tolerance = isSnapped ? data.SnappingCollisionTolerance : data.CollisionTolerance;

            Vector3 worldCenter = position + rotation * data.Center;
            Vector3 half = Vector3.Scale(data.Size * 0.5f, scale * tolerance);

            int hits = PhysicsExtensions.OverlapBoxNonAlloc(
                worldCenter,
                half,
                rotation,
                out m_overlapBuffer,
                data.CollisionLayer,
                QueryTriggerInteraction.Ignore);

            result.HitCount = hits;

            for (int i = 0; i < hits; i++)
            {
                Collider collider = m_overlapBuffer[i];
                if (!collider)
                {
                    continue;
                }

                if (collider is TerrainCollider)
                {
                    result.HasTerrain = true;
                    continue;
                }

                BuildingPart part = collider.GetComponentInParent<BuildingPart>();

                if (part == Part)
                {
                    continue;
                }

                if (part != null &&
                    part == BuildingPart.LastPlacedPart &&
                    BuildingPart.LastPlacedFrame == Time.frameCount)
                {
                    continue;
                }

                if (data.IgnoreNestedCollision && isSnapped)
                {
                    BuildingPart owner = Part.AttachedSocket?.ParentPart;
                    if (owner)
                    {
                        if (collider.transform.IsChildOf(owner.transform))
                        {
                            continue;
                        }

                        if (part && part.transform.IsChildOf(owner.transform))
                        {
                            continue;
                        }
                    }
                }

                if (data.IgnoreTags != null && Array.IndexOf(data.IgnoreTags, collider.tag) >= 0)
                {
                    continue;
                }

                if (data.IgnoreBuildingTypes != null &&
                    part &&
                    Array.IndexOf(data.IgnoreBuildingTypes, part.Category) >= 0)
                {
                    continue;
                }

                result.HasCollision = true;
                result.CollidingCollider = collider;
            }

            return result;
        }

        protected virtual BuildingCollisionBoundsData[] GetCollisionBounds()
        {
            return m_collisionBounds ?? Array.Empty<BuildingCollisionBoundsData>();
        }

#if UNITY_EDITOR
        public override void OnDebugRender()
        {
            if (!ShowGizmos || Part == null || IsDisabled)
            {
                return;
            }

            BuildingCollisionBoundsData[] boundsList = GetCollisionBounds();
            if (boundsList.Length == 0)
            {
                return;
            }

            bool isSnapped = Part.AttachedSocket != null;

            for (int b = 0; b < boundsList.Length; b++)
            {
                BuildingCollisionBoundsData boundsData = boundsList[b];
                if (boundsData == null)
                {
                    continue;
                }

                BoundCollisionResult result = CheckBoundCollision(
                    boundsData,
                    Part.transform.position,
                    Part.transform.rotation,
                    Part.transform.localScale,
                    isSnapped);

                bool boundIsValid = !result.HasCollision;
                float tolerance = isSnapped ? boundsData.SnappingCollisionTolerance : boundsData.CollisionTolerance;
                Vector3 worldCenter = Part.transform.position + Part.transform.rotation * boundsData.Center;

                Color transparentFill = boundIsValid ? new Color(0f, 1f, 0f, 0.01f) : new Color(1f, 0f, 0f, 0.01f);
                Color wireFill = boundIsValid ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 0f, 0f, 1f);

                DebugRenderer.DrawCube(worldCenter, boundsData.Size * tolerance, Part.transform.rotation, Part.transform.localScale, transparentFill, 0f, false);
                DebugRenderer.DrawWireCube(worldCenter, boundsData.Size * tolerance, Part.transform.rotation, Part.transform.localScale, wireFill, 0f, 1f, false);

                if (boundsData.PreventOverlapping)
                {
                    bool hasOverlap = CheckOverlapping(boundsData, Part.transform.position, Part.transform.rotation, Part.transform.localScale);
                    float overlapTolerance = boundsData.OverlappingTolerance > 0f ? boundsData.OverlappingTolerance : 1f;

                    Vector3 overlapCenter = Part.transform.position + Part.transform.rotation * boundsData.OverlappingCenter;
                    Vector3 overlapSize = Vector3.Scale(boundsData.OverlappingSize, Part.transform.localScale * overlapTolerance);

                    Color overlapFill = hasOverlap ? new Color(1f, 1f, 0f, 0.05f) : new Color(1f, 0.5f, 0f, 0.05f);
                    Color overlapWire = hasOverlap ? new Color(1f, 1f, 0f, 1f) : new Color(1f, 0.5f, 0f, 1f);

                    DebugRenderer.DrawCube(overlapCenter, overlapSize, Part.transform.rotation, Part.transform.localScale, overlapFill, 0f, false);
                    DebugRenderer.DrawWireCube(overlapCenter, overlapSize, Part.transform.rotation, Part.transform.localScale, overlapWire, 0f, 1f, false);
                }
            }
        }
#endif
    }
}