/// <summary>
/// Project : Easy Build System
/// Class : TopDownBuildingView.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
{
    public class TopDownBuildingView : BuildingView
    {
        public override string Name => "Top Down";

        [SerializeField, NotNull] private Transform m_originTransform;

        public override BuildingViewType ViewType => BuildingViewType.TopDown;

        public Transform OriginTransform { get => m_originTransform; set => m_originTransform = value; }

        public override Vector3 GetTargetPoint()
        {
            Ray ray = GetRay();

            if (PhysicsExtensions.RaycastNonAlloc(ray, m_raycastDistance, out RaycastHit hit, m_raycastLayer))
            {
                return hit.point;
            }

            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return Vector3.zero;
        }

        public override Transform GetOriginTransform()
        {
            return m_originTransform != null ? m_originTransform : base.GetOriginTransform();
        }

        public override Ray GetRay()
        {
            if (RaycastCamera == null)
            {
                return new Ray(Vector3.zero, Vector3.down);
            }

            Vector3 mousePosition = GetMousePosition();
            return RaycastCamera.ScreenPointToRay(mousePosition);
        }

        public override float GetDistance(Vector3 originPosition, Vector3 targetPosition)
        {
            originPosition.y = 0f;
            targetPosition.y = 0f;
            return Vector3.Distance(originPosition, targetPosition);
        }

        public override Vector3 GetDirection(Vector3 originPosition, Vector3 targetRotation)
        {
            originPosition.y = 0f;
            targetRotation.y = 0f;
            return targetRotation - originPosition;
        }

        private void Reset()
        {
            m_raycastCamera = GetComponent<Camera>();
            if (m_raycastCamera == null)
            {
                m_raycastCamera = Camera.main;
            }

            m_constrainValidDistance = false;
            m_minValidDistance = 1f;
            m_maxValidDistance = 50f;

            m_snapRadius = 0.5f;
            m_snapMaxAngle = 35f;
            m_snapObstructionCheck = false;
            m_snapObstructionLayers = ~0;
        }
    }
}