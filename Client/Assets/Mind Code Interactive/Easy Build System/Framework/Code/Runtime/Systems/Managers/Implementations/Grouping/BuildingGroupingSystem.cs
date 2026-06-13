/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupingSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping
{
    public class BuildingGroupingSystem : BuildingManagerSubSystem
    {
        protected BuildingGroupingSettings m_settings;

        public BuildingGroupingSettings Settings => m_settings;

        public BuildingGroupingSystem(BuildingManager manager, BuildingGroupingSettings settings)
        {
            m_manager = manager;
            m_settings = settings;
        }

        public override void Initialize()
        {
            EventPublisher.Subscribe<BuildingPartEvent.RegisteredEventArgs>(OnPartRegistered);
            EventPublisher.Subscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
            EventPublisher.Subscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);
            EventPublisher.Subscribe<BuildingStateEvent.AdjustedEventArgs>(OnPartAdjusted);
            EventPublisher.Subscribe<BuildingSaveEvent.LoadCompletedEventArgs>(OnPartsLoaded);
        }

        public override void Shutdown()
        {
            EventPublisher.Unsubscribe<BuildingPartEvent.RegisteredEventArgs>(OnPartRegistered);
            EventPublisher.Unsubscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
            EventPublisher.Unsubscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);
            EventPublisher.Unsubscribe<BuildingStateEvent.AdjustedEventArgs>(OnPartAdjusted);
            EventPublisher.Unsubscribe<BuildingSaveEvent.LoadCompletedEventArgs>(OnPartsLoaded);
        }

        private void OnPartRegistered(BuildingPartEvent.RegisteredEventArgs args)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!m_settings.EnableGrouping || args.Part == null)
            {
                return;
            }

            if (args.Part.State != BuildingPart.BuildingState.Placed)
            {
                return;
            }

            GroupOrAddToExistingGroup(args.Part);
        }

        protected virtual void OnPartPlaced(BuildingStateEvent.PlacedEventArgs args)
        {
            if (!m_settings.EnableGrouping || args.Part == null)
            {
                return;
            }

            GroupOrAddToExistingGroup(args.Part);
        }

        protected virtual void OnPartDestroyed(BuildingStateEvent.DestroyedEventArgs args)
        {
            if (!m_settings.EnableGrouping || args.Part?.AttachedGroup == null)
            {
                return;
            }

            RecenterGroupPivot(args.Part.AttachedGroup);
        }

        protected virtual void OnPartAdjusted(BuildingStateEvent.AdjustedEventArgs args)
        {
            if (!m_settings.EnableGrouping || args.Part?.AttachedGroup == null)
            {
                return;
            }

            BuildingGroup adjustedGroup = args.Part.AttachedGroup;
            adjustedGroup.RecenterPivot(m_settings.DefaultPivotMode);
        }

        private void OnPartsLoaded(BuildingSaveEvent.LoadCompletedEventArgs args)
        {
            if (!m_settings.EnableGrouping || args.LoadedParts == null)
            {
                return;
            }

            HashSet<BuildingPart> loadedPartsSet = new HashSet<BuildingPart>(args.LoadedParts);

            foreach (BuildingPart loadedPart in loadedPartsSet)
            {
                GroupOrAddToExistingGroup(loadedPart);
            }
        }

        protected virtual void GroupOrAddToExistingGroup(BuildingPart part)
        {
            if (part.AttachedGroup != null)
            {
                return;
            }

            BuildingGroup nearestGroup = FindNearestGroup(part.transform.position);

            if (nearestGroup != null)
            {
                nearestGroup.AddPart(part);
                return;
            }

            HashSet<BuildingPart> nearbyPartsSet = FindNearbyParts(part);

            if (nearbyPartsSet.Count > 0)
            {
                nearbyPartsSet.Add(part);
                GroupNearbyParts(nearbyPartsSet);
            }
            else
            {
                CreateGroupWithSinglePart(part);
            }
        }

        protected virtual HashSet<BuildingPart> FindNearbyParts(BuildingPart part)
        {
            HashSet<BuildingPart> nearbyPartsSet = new HashSet<BuildingPart>();
            HashSet<BuildingPart> allPlacedParts = m_manager.GetPartsByState(BuildingPart.BuildingState.Placed);

            foreach (BuildingPart otherPart in allPlacedParts)
            {
                if (otherPart == null || otherPart == part)
                {
                    continue;
                }

                if (Vector3.Distance(otherPart.transform.position, part.transform.position) <= m_settings.GroupPartNeighborDistance)
                {
                    nearbyPartsSet.Add(otherPart);
                }
            }

            return nearbyPartsSet;
        }

        protected virtual BuildingGroup FindNearestGroup(Vector3 position)
        {
            float bestDistance = float.MaxValue;
            BuildingGroup bestGroup = null;
            float attachmentDistance = m_settings.GroupPartNeighborDistance;

            foreach (BuildingGroup group in m_manager.GetRegisteredGroups)
            {
                if (group == null)
                {
                    continue;
                }

                Bounds groupBounds = group.GroupBounds;
                float distance = (groupBounds.size == Vector3.zero)
                    ? Vector3.Distance(position, group.transform.position)
                    : Vector3.Distance(position, groupBounds.ClosestPoint(position));

                if (distance <= attachmentDistance && distance < bestDistance)
                {
                    bestDistance = distance;
                    bestGroup = group;
                }
            }

            return bestGroup;
        }

        public void GroupNearbyParts(HashSet<BuildingPart> parts)
        {
            BuildingGroup newGroup = null;

            foreach (BuildingPart part in parts)
            {
                if (part == null || part.AttachedGroup != null)
                {
                    continue;
                }

                if (newGroup == null)
                {
                    newGroup = CreateGroupWithSinglePart(part);
                }
                else
                {
                    newGroup.AddPart(part);
                }
            }

            if (newGroup != null)
            {
                RecenterGroupPivot(newGroup);
            }
        }

        protected virtual BuildingGroup CreateGroupWithSinglePart(BuildingPart part)
        {
            GameObject groupGameObject = new GameObject("Building Group");
            groupGameObject.transform.position = part.transform.position;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RegisterCreatedObjectUndo(groupGameObject, "Create Building Group");
            }
#endif

            BuildingGroup newBuildingGroup = groupGameObject.AddComponent<BuildingGroup>();
            newBuildingGroup.AddPart(part);

            return newBuildingGroup;
        }

        protected virtual void RecenterGroupPivot(BuildingGroup group)
        {
            if (group == null || group.IsEmpty())
            {
                return;
            }

            group.RecenterPivot(m_settings.DefaultPivotMode);
        }
    }
}