/// <summary>
/// Project : Easy Build System
/// Class : FirstPersonBuildingView.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
{
    public class FirstPersonBuildingView : BuildingView
    {
        public override string Name => "First Person";

        [SerializeField, NotNull] private Transform m_originTransform;

        public override BuildingViewType ViewType => BuildingViewType.FirstPerson;

        public Transform OriginTransform { get => m_originTransform; set => m_originTransform = value; }

        public override Transform GetOriginTransform()
        {
            return m_originTransform != null ? m_originTransform : base.GetOriginTransform();
        }

        public override Ray GetRay()
        {
            if (m_originTransform != null)
            {
                return new Ray(m_originTransform.position, m_originTransform.forward);
            }

            if (RaycastCamera != null)
            {
                return new Ray(RaycastCamera.transform.TransformPoint(RaycastOffset), RaycastCamera.transform.forward);
            }

            return new Ray(Vector3.zero, Vector3.forward);
        }

        private void Reset()
        {
            m_raycastCamera = GetComponent<Camera>();
            if (m_raycastCamera == null)
            {
                m_raycastCamera = Camera.main;
                m_originTransform = m_raycastCamera.transform;
            }

            m_constrainValidDistance = true;
            m_minValidDistance = 0f;
            m_maxValidDistance = 8f;
            m_snapRadius = 1f;
            m_snapMaxAngle = 15f;
            m_snapObstructionCheck = false;
            m_snapObstructionLayers = ~0;
        }
    }
}