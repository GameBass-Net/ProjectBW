/// <summary>
/// Project : Easy Build System
/// Class : BuildingBatchingSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching
{
    public class BuildingBatchingSystem : BuildingManagerSubSystem
    {
        protected BuildingBatchingSettings m_settings;

        public BuildingBatchingSystem(BuildingManager manager, BuildingBatchingSettings settings)
        {
            m_manager = manager;
            m_settings = settings;
        }

        public override void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!m_settings.EnableBatching || !m_settings.AutoBatching || BuildingController.Instance == null)
            {
                return;
            }

            if (m_manager.SaveSystem != null && m_manager.SaveSystem.IsLoading)
            {
                return;
            }

            foreach (BuildingGroup buildingGroup in m_manager.GetRegisteredGroups)
            {
                float distanceToTarget = GetDistanceToTarget(buildingGroup);
                bool isInBuildMode = BuildingController.Instance.ActiveMode != BuildingMode.None;

                if (isInBuildMode && distanceToTarget <= m_settings.BatchingDistance)
                {
                    if (buildingGroup.IsBatched)
                    {
                        buildingGroup.UnbatchGroup();
                    }
                }
                else if (!buildingGroup.IsBatched)
                {
                    buildingGroup.BatchGroup();
                }
            }
        }

        public virtual void BatchAllGroups()
        {
            if (!m_settings.EnableBatching)
            {
                return;
            }

            foreach (BuildingGroup buildingGroup in m_manager.GetRegisteredGroups)
            {
                if (buildingGroup != null && !buildingGroup.IsBatched)
                {
                    buildingGroup.BatchGroup();
                }
            }
        }

        public virtual void UnbatchAllGroups()
        {
            foreach (BuildingGroup buildingGroup in m_manager.GetRegisteredGroups)
            {
                if (buildingGroup != null && buildingGroup.IsBatched)
                {
                    buildingGroup.UnbatchGroup();
                }
            }
        }

        protected virtual float GetDistanceToTarget(BuildingGroup group)
        {
            Vector3 referencePoint = GetReferencePoint();
            Vector3 closestPointOnGroup = group.GetClosestPoint(referencePoint);

            Vector3 referenceFlatPosition = new Vector3(referencePoint.x, 0, referencePoint.z);
            Vector3 closestPointFlat = new Vector3(closestPointOnGroup.x, 0, closestPointOnGroup.z);

            return Vector3.Distance(referenceFlatPosition, closestPointFlat);
        }

        protected virtual Vector3 GetReferencePoint()
        {
            BuildingController controller = BuildingController.Instance;
            if (controller == null)
            {
                return Vector3.zero;
            }

            BuildingView view = controller.ActiveView;
            if (view != null && controller.ActiveMode != BuildingMode.None)
            {
                Vector3 targetPoint = view.GetTargetPoint();
                if (targetPoint != Vector3.zero)
                {
                    return targetPoint;
                }
            }

            return controller.transform.position;
        }
    }
}