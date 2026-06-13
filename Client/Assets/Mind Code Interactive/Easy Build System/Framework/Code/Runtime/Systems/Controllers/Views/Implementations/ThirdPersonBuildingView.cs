/// <summary>
/// Project : Easy Build System
/// Class : ThirdPersonBuildingView.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations
{
    public class ThirdPersonBuildingView : BuildingView
    {
        public override string Name => "Third Person";

        [SerializeField, NotNull] private Transform m_originTransform;

        public override BuildingViewType ViewType => BuildingViewType.ThirdPerson;

        public Transform OriginTransform { get => m_originTransform; set => m_originTransform = value; }

        public override Transform GetOriginTransform()
        {
            return m_originTransform != null ? m_originTransform : base.GetOriginTransform();
        }

        public override Ray GetRay()
        {
            if (RaycastCamera == null)
            {
                return new Ray(Vector3.zero, Vector3.forward);
            }

            return new Ray(
                RaycastCamera.transform.position + RaycastOffset,
                RaycastCamera.transform.forward
            );
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
            m_snapRadius = 1f;
            m_snapMaxAngle = 15f;
            m_snapObstructionCheck = false;
            m_snapObstructionLayers = ~0;
        }
    }
}