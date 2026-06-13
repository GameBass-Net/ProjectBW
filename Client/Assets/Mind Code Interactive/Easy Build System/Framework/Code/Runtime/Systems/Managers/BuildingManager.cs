/// <summary>
/// Project : Easy Build System
/// Class : BuildingManager.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Constants;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Terrain;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Interfaces;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Events;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers
{
    [ExecuteAlways]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_HIGH_PRIORITY)]
    public class BuildingManager : MonoBehaviour
    {
        public const string SOCKET_LAYER_NAME = "Socket";

        public enum DetectionType { Physics_Based }

        private static BuildingManager s_instance;
        private static INetworkBuildingControllerAdapter s_networkAdapter;

        [SerializeField] private HashSet<BuildingArea> m_registeredAreas = new HashSet<BuildingArea>();
        [SerializeField] private HashSet<BuildingGroup> m_registeredGroups = new HashSet<BuildingGroup>();
        [SerializeField] private HashSet<BuildingPart> m_registeredParts = new HashSet<BuildingPart>();

        private readonly Dictionary<string, BuildingPart> m_partsByUniqueId = new Dictionary<string, BuildingPart>();
        [SerializeField] private HashSet<BuildingSocket> m_registeredSockets = new HashSet<BuildingSocket>();
        [SerializeField] private DetectionType m_socketDetectionType = DetectionType.Physics_Based;
        [SerializeField] private LayerMask m_socketLayer;
        [SerializeField] private BuildingGroupingSettings m_groupingSettings = new BuildingGroupingSettings();
        [SerializeField] private BuildingBatchingSettings m_batchingSettings = new BuildingBatchingSettings();
        [SerializeField] private BuildingSaveSettings m_saveSettings = new BuildingSaveSettings();
        [SerializeField] private BuildingGridSettings m_gridSettings = new BuildingGridSettings();
        [SerializeField] private BuildingPhysicsSettings m_physicsSettings = new BuildingPhysicsSettings();
        [SerializeField] private List<BuildingRule> m_globalRules = new List<BuildingRule>();

        private BuildingGroupingSystem m_groupingSystem;
        private BuildingBatchingSystem m_batchingSystem;
        private BuildingTerrainSystem m_terrainSystem;
        private BuildingSaveSystem m_saveSystem;
        private BuildingGridSystem m_gridSystem;
        private BuildingPhysicsSystem m_physicsSystem;

        public static BuildingManager Instance { get => s_instance; private set => s_instance = value; }
        public HashSet<BuildingArea> GetRegisteredAreas => GetRegistered<BuildingArea>();
        public HashSet<BuildingGroup> GetRegisteredGroups => GetRegistered<BuildingGroup>();
        public HashSet<BuildingPart> GetRegisteredParts => GetRegistered<BuildingPart>();
        public HashSet<BuildingSocket> GetRegisteredSockets => GetRegistered<BuildingSocket>();
        public DetectionType SocketDetectionType { get => m_socketDetectionType; set => m_socketDetectionType = value; }
        public LayerMask SocketLayer { get => m_socketLayer; set => m_socketLayer = value; }
        public BuildingGroupingSettings GroupingSettings { get => m_groupingSettings; set => m_groupingSettings = value; }
        public BuildingBatchingSettings BatchingSettings { get => m_batchingSettings; set => m_batchingSettings = value; }
        public BuildingSaveSettings SaveSettings { get => m_saveSettings; set => m_saveSettings = value; }
        public BuildingGridSettings GridSettings { get => m_gridSettings; set => m_gridSettings = value; }
        public BuildingPhysicsSettings PhysicsSettings { get => m_physicsSettings; set => m_physicsSettings = value; }
        public List<BuildingRule> GlobalRules { get => m_globalRules; set => m_globalRules = value; }
        public BuildingGroupingSystem GroupingSystem => m_groupingSystem;
        public BuildingBatchingSystem BatchingSystem => m_batchingSystem;
        public BuildingTerrainSystem TerrainSystem => m_terrainSystem;
        public BuildingSaveSystem SaveSystem => m_saveSystem;
        public BuildingGridSystem GridSystem => m_gridSystem;
        public BuildingPhysicsSystem PhysicsSystem => m_physicsSystem;

        #region Unity Callbacks

        protected virtual void Awake()
        {
            Instance = this;
        }

        protected virtual void OnEnable()
        {
            Instance = this;

            if (m_socketLayer == 0 || m_socketLayer == 264)
            {
                m_socketLayer = LayerMask.GetMask(SOCKET_LAYER_NAME);
            }

            m_groupingSystem = CreateGroupingSystem();
            m_batchingSystem = CreateBatchingSystem();
            m_terrainSystem = new BuildingTerrainSystem();
            m_saveSystem = CreateSaveSystem();
            m_gridSystem = CreateGridSystem();
            m_physicsSystem = CreatePhysicsSystem();

            m_groupingSystem?.Initialize();
            m_batchingSystem?.Initialize();
            m_terrainSystem?.Initialize();
            m_gridSystem?.Initialize();
            m_physicsSystem?.Initialize();
        }

        protected virtual BuildingGridSystem CreateGridSystem()
            => new BuildingGridSystem(this, m_gridSettings);

        protected virtual BuildingPhysicsSystem CreatePhysicsSystem()
            => new BuildingPhysicsSystem(this, m_physicsSettings);

        protected virtual BuildingSaveSystem CreateSaveSystem()
            => new BuildingSaveSystem(this, m_saveSettings);

        protected virtual BuildingGroupingSystem CreateGroupingSystem()
            => new BuildingGroupingSystem(this, m_groupingSettings);

        protected virtual BuildingBatchingSystem CreateBatchingSystem()
            => new BuildingBatchingSystem(this, m_batchingSettings);

        protected virtual void OnDisable()
        {
            m_groupingSystem?.Shutdown();
            m_batchingSystem?.Shutdown();
            m_terrainSystem?.Shutdown();
            m_gridSystem?.Shutdown();
            m_physicsSystem?.Shutdown();
        }

        private void Start()
        {
            m_saveSystem?.Initialize();
        }

        private void Reset()
        {
            m_socketLayer = LayerMask.GetMask(SOCKET_LAYER_NAME);
        }

        protected virtual void Update()
        {
            m_groupingSystem?.Update();
            m_batchingSystem?.Update();
            m_terrainSystem?.Update();
            m_saveSystem?.Update();
            m_gridSystem?.Update();
            m_physicsSystem?.Update();
        }

        private void OnRenderObject()
        {
            m_groupingSystem?.OnRenderObject();
            m_batchingSystem?.OnRenderObject();
            m_terrainSystem?.OnRenderObject();
            m_saveSystem?.OnRenderObject();
            m_gridSystem?.OnRenderObject();
        }

        private void OnApplicationQuit()
        {
            if (m_saveSettings.SaveMode == SaveModeType.Automatic)
            {
                m_saveSystem?.SaveBuildings();
            }
        }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && m_saveSettings.SaveMode == SaveModeType.Automatic)
                m_saveSystem?.SaveBuildings();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && m_saveSettings.SaveMode == SaveModeType.Automatic)
                m_saveSystem?.SaveBuildings();
        }
#endif

        #endregion

        #region Area Management

        public virtual BuildingArea GetAreaForPart(BuildingPart part)
        {
            HashSet<BuildingArea> areas = GetRegisteredAreas;
            if (part == null || areas == null || areas.Count == 0)
            {
                return null;
            }

            BuildingArea bestArea = null;
            int highestPriority = int.MinValue;

            foreach (BuildingArea area in areas)
            {
                if (area != null && area.AreaPriority > highestPriority && area.ContainsPart(part))
                {
                    bestArea = area;
                    highestPriority = area.AreaPriority;
                }
            }

            return bestArea;
        }

        #endregion

        #region Global Rules

        public virtual ConditionResult ValidateRules(BuildingPart part, BuildingMode mode)
        {
            for (int i = 0; i < m_globalRules.Count; i++)
            {
                BuildingRule rule = m_globalRules[i];
                if (rule == null || !rule.Enabled)
                {
                    continue;
                }

                ConditionResult result = rule.Validate(part, mode);

                if (!result.IsValid)
                {
                    return result;
                }
            }

            return new ConditionResult(true);
        }

        #endregion

        #region Part State & Preview

        public HashSet<BuildingPart> GetPartsByState(BuildingPart.BuildingState state)
        {
            HashSet<BuildingPart> parts = new HashSet<BuildingPart>();
            foreach (BuildingPart part in GetRegistered<BuildingPart>())
            {
                if (part != null && part.State == state)
                {
                    parts.Add(part);
                }
            }
            return parts;
        }

        public BuildingPart CreatePreview(BuildingPart part)
        {
            if (part == null)
            {
                return null;
            }

            BuildingPart preview = Instantiate(part);
            preview.name = "(Preview) " + preview.name;
            preview.SetState(BuildingPart.BuildingState.Placement);
            EventPublisher.Publish(new BuildingPartEvent.PreviewCreatedEventArgs(preview));
            return preview;
        }

        public void DestroyPreview(BuildingPart previewPart)
        {
            if (previewPart == null)
            {
                return;
            }

            EventPublisher.Publish(new BuildingPartEvent.PreviewDestroyedEventArgs(previewPart));
            if (Application.isPlaying)
            {
                Destroy(previewPart.gameObject);
            }
            else
            {
                DestroyImmediate(previewPart.gameObject);
            }
        }

        #endregion

        #region Building Commands

        public virtual BuildingPart PlacePart(BuildingPart part, Vector3 position, Quaternion rotation, Vector3 scale, BuildingSocket socket = null)
        {
            if (part == null)
            {
                return null;
            }

            if (s_networkAdapter != null && s_networkAdapter.IsConnected)
            {
                s_networkAdapter.ExecutePlaceCommand(part, position, rotation, scale, socket);
                return null;
            }

            PlaceCommand command = new PlaceCommand(part, position, rotation, scale, socket);
            BuildingCommandManager.ExecuteCommand(command);
            return command.TargetPart;
        }

        public virtual void AdjustPart(BuildingPart part, Vector3 newPosition, Quaternion newRotation)
        {
            if (part == null)
            {
                return;
            }

            if (s_networkAdapter != null && s_networkAdapter.IsConnected)
            {
                s_networkAdapter.ExecuteAdjustCommand(part, newPosition, newRotation);
                return;
            }

            AdjustCommand command = new AdjustCommand(part, newPosition, newRotation);
            BuildingCommandManager.ExecuteCommand(command);
        }

        public virtual void DestroyPart(BuildingPart part)
        {
            if (part == null)
            {
                return;
            }

            if (s_networkAdapter != null && s_networkAdapter.IsConnected)
            {
                s_networkAdapter.ExecuteDestroyCommand(part);
                return;
            }

            DestroyCommand command = new DestroyCommand(part);
            BuildingCommandManager.ExecuteCommand(command);
        }

        public virtual void DestroyAllPlacedParts(bool includePreplaced = true)
        {
            HashSet<BuildingPart> partsToDestroy = new HashSet<BuildingPart>(GetRegistered<BuildingPart>());
            foreach (BuildingPart part in partsToDestroy)
            {
                if (part == null)
                {
                    continue;
                }

                if (!part.IsRuntimeInstantiated && !includePreplaced)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(part.gameObject);
                }
                else
                {
                    DestroyImmediate(part.gameObject);
                }
            }
            m_registeredParts.Clear();
            m_partsByUniqueId.Clear();
        }

        public virtual void UpgradePart(BuildingPart part, int upgradeIndex)
        {
            if (part == null)
            {
                return;
            }

            if (s_networkAdapter != null && s_networkAdapter.IsConnected)
            {
                s_networkAdapter.ExecuteUpgradeCommand(part, upgradeIndex);
                return;
            }

            UpgradeCommand command = new UpgradeCommand(part, upgradeIndex);
            BuildingCommandManager.ExecuteCommand(command);
        }

        #endregion

        #region Socket Detection

        public virtual HashSet<BuildingSocket> DetectSockets(Vector3 position, float radius)
        {
            HashSet<BuildingSocket> sockets = new HashSet<BuildingSocket>();

            if (m_socketDetectionType == DetectionType.Physics_Based)
            {
                int hitCount = PhysicsExtensions.OverlapSphereNonAlloc(
                    position,
                    radius,
                    out Collider[] colliders,
                    m_socketLayer,
                    null,
                    QueryTriggerInteraction.Collide);

                for (int i = 0; i < hitCount; i++)
                {
                    BuildingSocket socket = colliders[i] != null
                        ? colliders[i].GetComponent<BuildingSocket>()
                        : null;

                    if (socket != null)
                    {
                        sockets.Add(socket);
                    }
                }
            }

            return sockets;
        }

        public virtual BuildingSocket GetNearestSocket(Vector3 position, float radius)
        {
            HashSet<BuildingSocket> sockets = DetectSockets(position, radius);
            if (sockets.Count == 0)
            {
                return null;
            }

            BuildingSocket nearest = null;
            float closestDistance = float.MaxValue;

            foreach (BuildingSocket socket in sockets)
            {
                if (socket == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(position, socket.transform.position);
                if (distance < closestDistance)
                {
                    nearest = socket;
                    closestDistance = distance;
                }
            }

            return nearest;
        }

        #endregion

        #region Part Registry Access

        public List<BuildingPartReference> GetPartReferences()
        {
            return BuildingPartRegistry.Instance != null
                ? BuildingPartRegistry.Instance.PartReferences
                : new List<BuildingPartReference>();
        }

        public BuildingPart GetPartByPrefabId(string prefabId)
        {
            if (string.IsNullOrEmpty(prefabId))
            {
                return null;
            }

            BuildingPart part = BuildingPartRegistry.Instance != null
                ? BuildingPartRegistry.Instance.GetPartByPrefabId(prefabId)
                : null;

            if (part == null)
            {
                Debug.LogWarning("Building Part not found for PrefabId '" + prefabId + "'.", this);
            }

            return part;
        }

        public virtual BuildingPart GetPartByUniqueId(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                return null;
            }

            if (m_partsByUniqueId.TryGetValue(uniqueId, out BuildingPart cached) && cached != null)
            {
                return cached;
            }

            foreach (BuildingPart part in m_registeredParts)
            {
                if (part != null && part.UniqueId == uniqueId)
                {
                    m_partsByUniqueId[uniqueId] = part;
                    return part;
                }
            }

            return null;
        }

        public BuildingPart GetPartByIndex(int index)
        {
            List<BuildingPartReference> references = GetPartReferences();
            int currentIndex = 0;

            for (int r = 0; r < references.Count; r++)
            {
                BuildingPartReference reference = references[r];
                BuildingPart[] parts = reference != null ? reference.BuildingParts : null;
                if (parts == null)
                {
                    continue;
                }

                for (int i = 0; i < parts.Length; i++)
                {
                    BuildingPart part = parts[i];
                    if (part == null)
                    {
                        continue;
                    }

                    if (currentIndex == index)
                    {
                        return part;
                    }

                    currentIndex++;
                }
            }

            return null;
        }

        public BuildingPart[] GetPartsByCategory(string category)
        {
            return BuildingPartRegistry.Instance != null
                ? BuildingPartRegistry.Instance.GetPartsByCategory(category)
                : null;
        }

        public int GetPartCount()
        {
            List<BuildingPartReference> references = GetPartReferences();
            int count = 0;

            for (int r = 0; r < references.Count; r++)
            {
                BuildingPartReference reference = references[r];
                BuildingPart[] parts = reference != null ? reference.BuildingParts : null;
                if (parts == null)
                {
                    continue;
                }

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] != null)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        #endregion

        #region Registration System

        public bool Register<T>(T target) where T : IRegisterable
        {
            bool wasAdded = false;

            switch (target)
            {
                case BuildingArea area:
                    wasAdded = m_registeredAreas.Add(area);
                    if (wasAdded)
                    {
                        OnAreaRegistered(area);
                    }

                    break;
                case BuildingGroup group:
                    wasAdded = m_registeredGroups.Add(group);
                    if (wasAdded)
                    {
                        OnGroupRegistered(group);
                    }

                    break;
                case BuildingPart part:
                    wasAdded = m_registeredParts.Add(part);
                    if (wasAdded)
                    {
                        OnPartRegistered(part);
                    }

                    break;
                case BuildingSocket socket:
                    wasAdded = m_registeredSockets.Add(socket);
                    if (wasAdded)
                    {
                        OnSocketRegistered(socket);
                    }

                    break;
            }

            if (wasAdded)
            {
                target.IsRegistered = true;
                if (target is RegisterableUniqueObject registerableObject)
                {
                    registerableObject.OnRegistered();
                }

                PublishRegisteredEvent(target);
            }

            return wasAdded;
        }

        public bool Unregister<T>(T target) where T : IRegisterable
        {
            bool wasRemoved = false;

            switch (target)
            {
                case BuildingArea area:
                    wasRemoved = m_registeredAreas.Remove(area);
                    if (wasRemoved)
                    {
                        OnAreaUnregistered(area);
                    }

                    break;
                case BuildingGroup group:
                    wasRemoved = m_registeredGroups.Remove(group);
                    if (wasRemoved)
                    {
                        OnGroupUnregistered(group);
                    }

                    break;
                case BuildingPart part:
                    wasRemoved = m_registeredParts.Remove(part);
                    if (wasRemoved)
                    {
                        OnPartUnregistered(part);
                    }

                    break;
                case BuildingSocket socket:
                    wasRemoved = m_registeredSockets.Remove(socket);
                    if (wasRemoved)
                    {
                        OnSocketUnregistered(socket);
                    }

                    break;
            }

            if (wasRemoved)
            {
                target.IsRegistered = false;
                if (target is RegisterableUniqueObject registerableObject)
                {
                    registerableObject.OnUnregistered();
                }

                PublishUnregisteredEvent(target);
            }

            return wasRemoved;
        }

        protected virtual void OnAreaRegistered(BuildingArea area) { }
        protected virtual void OnAreaUnregistered(BuildingArea area) { }
        protected virtual void OnGroupRegistered(BuildingGroup group) { }
        protected virtual void OnGroupUnregistered(BuildingGroup group) { }
        protected virtual void OnPartRegistered(BuildingPart part)
        {
            if (part != null && !string.IsNullOrEmpty(part.UniqueId))
            {
                m_partsByUniqueId[part.UniqueId] = part;
            }
        }

        protected virtual void OnPartUnregistered(BuildingPart part)
        {
            if (part != null && !string.IsNullOrEmpty(part.UniqueId))
            {
                m_partsByUniqueId.Remove(part.UniqueId);
            }
        }
        protected virtual void OnSocketRegistered(BuildingSocket socket) { }
        protected virtual void OnSocketUnregistered(BuildingSocket socket) { }

        public HashSet<T> GetRegistered<T>() where T : IRegisterable
        {
            if (typeof(T) == typeof(BuildingArea))
            {
                return (HashSet<T>)(object)m_registeredAreas;
            }

            if (typeof(T) == typeof(BuildingGroup))
            {
                return (HashSet<T>)(object)m_registeredGroups;
            }

            if (typeof(T) == typeof(BuildingPart))
            {
                return (HashSet<T>)(object)m_registeredParts;
            }

            if (typeof(T) == typeof(BuildingSocket))
            {
                return (HashSet<T>)(object)m_registeredSockets;
            }

            return new HashSet<T>();
        }

        private void PublishRegisteredEvent<T>(T target) where T : IRegisterable
        {
            switch (target)
            {
                case BuildingArea area:
                    EventPublisher.Publish(new BuildingAreaEvent.RegisteredEventArgs(area));
                    break;
                case BuildingGroup group:
                    EventPublisher.Publish(new BuildingGroupEvent.RegisteredEventArgs(group));
                    break;
                case BuildingPart part:
                    EventPublisher.Publish(new BuildingPartEvent.RegisteredEventArgs(part));
                    break;
                case BuildingSocket socket:
                    EventPublisher.Publish(new BuildingSocketEvent.RegisteredEventArgs(socket));
                    break;
            }
        }

        private void PublishUnregisteredEvent<T>(T target) where T : IRegisterable
        {
            switch (target)
            {
                case BuildingArea area:
                    EventPublisher.Publish(new BuildingAreaEvent.UnregisteredEventArgs(area));
                    break;
                case BuildingGroup group:
                    EventPublisher.Publish(new BuildingGroupEvent.UnregisteredEventArgs(group));
                    break;
                case BuildingPart part:
                    EventPublisher.Publish(new BuildingPartEvent.UnregisteredEventArgs(part));
                    break;
                case BuildingSocket socket:
                    EventPublisher.Publish(new BuildingSocketEvent.UnregisteredEventArgs(socket));
                    break;
            }
        }

        #endregion

        #region Network Adapter

        public static void SetNetworkAdapter(INetworkBuildingControllerAdapter adapter)
        {
            s_networkAdapter = adapter;
        }

        public static void SetNetworkSaveAdapter(INetworkBuildingSaveAdapter adapter)
        {
            BuildingSaveSystem.SetNetworkSaveAdapter(adapter);
        }

        #endregion
    }
}