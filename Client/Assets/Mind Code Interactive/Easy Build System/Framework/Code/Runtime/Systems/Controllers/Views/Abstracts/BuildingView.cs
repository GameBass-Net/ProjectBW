/// <summary>
/// Project : Easy Build System
/// Class : BuildingView.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts
{
    public enum BuildingViewType
    {
        FirstPerson,
        ThirdPerson,
        TopDown,
        Orbital,
        Editor
    }

    public struct SnapQuery
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public float MaxDistance;
        public float Radius;
    }

    public struct SnapSettings
    {
        public float MaxAngle;
        public bool ObstructionCheck;
        public LayerMask ObstructionLayers;
    }

    public abstract class BuildingView : MonoBehaviour
    {
        [SerializeField, NotNull] protected Camera m_raycastCamera;
        [SerializeField, Min(0f)] protected float m_raycastDistance = 100f;
        [SerializeField] protected LayerMask m_raycastLayer = ~0;
        [SerializeField] protected Vector3 m_raycastOffset = Vector3.zero;
        [SerializeField] protected bool m_constrainValidDistance = false;
        [SerializeField] protected float m_minValidDistance = 1f;
        [SerializeField] protected float m_maxValidDistance = 50f;
        [SerializeField, Min(0f)] protected float m_snapRadius = 0.5f;
        [SerializeField, Range(0f, 360f)] protected float m_snapMaxAngle = 35f;
        [SerializeField] protected bool m_snapObstructionCheck = false;
        [SerializeField] protected LayerMask m_snapObstructionLayers = ~0;

        public virtual string Name => GetType().Name.Replace("BuildingView", "").Trim();

        public abstract BuildingViewType ViewType { get; }

        public virtual Camera RaycastCamera { get => m_raycastCamera; set => m_raycastCamera = value; }

        public virtual float RaycastDistance { get => m_raycastDistance; set => m_raycastDistance = value; }

        public virtual LayerMask RaycastLayer { get => m_raycastLayer; set => m_raycastLayer = value; }

        public virtual Vector3 RaycastOffset { get => m_raycastOffset; set => m_raycastOffset = value; }

        public bool ConstrainValidDistance => m_constrainValidDistance;

        public float MinValidDistance => m_minValidDistance;

        public float MaxValidDistance => m_maxValidDistance;

        public virtual float SnapRadius { get => m_snapRadius; set => m_snapRadius = value; }

        public virtual float SnapMaxAngle { get => m_snapMaxAngle; set => m_snapMaxAngle = value; }

        public virtual bool SnapObstructionCheck { get => m_snapObstructionCheck; set => m_snapObstructionCheck = value; }

        public virtual LayerMask SnapObstructionLayers { get => m_snapObstructionLayers; set => m_snapObstructionLayers = value; }

        public abstract Ray GetRay();

        public virtual bool Raycast(out RaycastHit hit, QueryTriggerInteraction queryTrigger = QueryTriggerInteraction.Ignore)
        {
            Ray ray = GetRay();
            int mask = RaycastLayer & ~(1 << 2);
            return PhysicsExtensions.RaycastNonAlloc(ray, m_raycastDistance, out hit, mask, null, queryTrigger);
        }

        public virtual Transform GetOriginTransform()
        {
            return m_raycastCamera != null ? m_raycastCamera.transform : null;
        }

        public virtual float GetDistance(Vector3 originPosition, Vector3 targetPosition)
        {
            return Vector3.Distance(originPosition, targetPosition);
        }

        public virtual Vector3 GetDirection(Vector3 originPosition, Vector3 targetPosition)
        {
            return targetPosition - originPosition;
        }

        public virtual bool IsWithinValidDistance(Vector3 position)
        {
            if (!ConstrainValidDistance)
            {
                return true;
            }

            Transform origin = GetOriginTransform();
            if (origin == null)
            {
                return true;
            }

            float distance = GetDistance(origin.position, position);
            return distance >= MinValidDistance && distance <= MaxValidDistance;
        }

        public virtual Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                return UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            }

            return Input.mousePosition;
#else
            return Input.mousePosition;
#endif
        }

        public virtual Vector3 GetTargetPoint()
        {
            return Raycast(out RaycastHit hit) && hit.collider != null ? hit.point : Vector3.zero;
        }

        public virtual BuildingPart GetTargetPart()
        {
            return Raycast(out RaycastHit hit) && hit.collider != null ? hit.collider.GetComponentInParent<BuildingPart>() : null;
        }

        public virtual SnapQuery BuildSnapQuery(Vector3 targetPosition)
        {
            Ray viewRay = GetRay();
            return new SnapQuery
            {
                Origin = viewRay.origin,
                Direction = viewRay.direction.normalized,
                MaxDistance = m_raycastDistance,
                Radius = m_snapRadius
            };
        }

        public virtual SnapSettings GetSnapSettings() => new SnapSettings
        {
            MaxAngle = m_snapMaxAngle,
            ObstructionCheck = m_snapObstructionCheck,
            ObstructionLayers = m_snapObstructionLayers
        };
    }
}