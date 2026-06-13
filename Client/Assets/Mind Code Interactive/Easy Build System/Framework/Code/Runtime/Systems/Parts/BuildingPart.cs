/// <summary>
/// Project : Easy Build System
/// Class : BuildingPart.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Interfaces;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts
{
    [ExecuteAlways]
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system")]
    public class BuildingPart : RegisterableUniqueObject, ISaveable, IDebuggable
    {
        public enum BuildingState { None, Placed, Queue, Placement, Adjusting, Destruction }

        [SerializeField] new string name;
        [SerializeField] private string m_description;
        [Category("PartCategory"), SerializeField] private string m_category;
        [SerializeField] private Texture2D m_thumbnail;
        [SerializeField] private bool m_isSaveable = true;
        [SerializeField] protected BuildingRendererSystem m_rendererSystem = new BuildingRendererSystem();
        [SerializeField] protected BuildingPlacementSystem m_placementSystem = new BuildingPlacementSystem();
        [SerializeField] protected BuildingBehaviorSystem m_behaviorSystem = new BuildingBehaviorSystem();
        [SerializeField] protected BuildingConditionSystem m_conditionSystem = new BuildingConditionSystem();
        [SerializeField] protected BuildingCacheSystem m_cacheSystem = new BuildingCacheSystem();

#if UNITY_EDITOR
        [SerializeField] private ScriptableObject m_linkedPreset;
#endif

        [SerializeField] private BuildingCollection m_attachedCollection;
        [SerializeField] private BuildingGroup m_attachedGroup;
        [SerializeField] private BuildingSocket m_attachedSocket;
        [SerializeField] private BuildingState m_lastState = BuildingState.None;
        [SerializeField] private BuildingState m_state = BuildingState.None;
        [SerializeField] private bool m_isRuntimeInstantiated = false;

        public string Name { get => name; set => name = value; }

        public string Description { get => m_description; set => m_description = value; }

        public string Category { get => m_category; set => m_category = value; }

        public Texture2D Thumbnail => m_thumbnail;

        public bool IsSaveable { get => m_isSaveable; set => m_isSaveable = value; }

        public BuildingRendererSystem RendererSystem { get => m_rendererSystem; protected set => m_rendererSystem = value; }

        public BuildingPlacementSystem PlacementSystem { get => m_placementSystem; protected set => m_placementSystem = value; }

        public BuildingBehaviorSystem BehaviorSystem { get => m_behaviorSystem; protected set => m_behaviorSystem = value; }

        public BuildingConditionSystem ConditionSystem { get => m_conditionSystem; protected set => m_conditionSystem = value; }

        public BuildingCacheSystem CacheSystem { get => m_cacheSystem; protected set => m_cacheSystem = value; }

#if UNITY_EDITOR
        public ScriptableObject LinkedPreset { get => m_linkedPreset; set => m_linkedPreset = value; }
#endif

        public BuildingCollection AttachedCollection
        {
            get => m_attachedCollection;
            set
            {
                if (m_attachedCollection == value)
                {
                    return;
                }

                m_attachedCollection = value;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEditor.EditorUtility.SetDirty(this);
                }
#endif
            }
        }

        public BuildingGroup AttachedGroup => m_attachedGroup;

        public BuildingSocket AttachedSocket => m_attachedSocket;

        public BuildingState LastState { get => m_lastState; set => m_lastState = value; }

        public BuildingState State => m_state;

        public bool IsPreview => m_state == BuildingState.Placement || m_state == BuildingState.Adjusting;

        public bool IsPlaced => m_state == BuildingState.Placed;

        public bool IsRuntimeInstantiated { get => m_isRuntimeInstantiated; set => m_isRuntimeInstantiated = value; }

        public bool IsDynamic => m_cacheSystem.Rigidbodies != null && m_cacheSystem.Rigidbodies.Length > 0;

        public event Action<BuildingPartStateData> SaveCallback;

        public event Action<BuildingPartStateData> LoadCallback;

        public override void OnEnable()
        {
            base.OnEnable();

            m_cacheSystem.Initialize(this);
            m_placementSystem.Initialize(this);
            m_rendererSystem.Initialize(this);
            m_behaviorSystem.Initialize(this);
            m_conditionSystem.Initialize(this);

            if (Application.isPlaying)
            {
                DebugRendererManager.Register(this);
            }

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                return;
            }

            if (!Application.isPlaying && m_attachedGroup != null)
            {
                if (!m_attachedGroup.GroupedParts.Contains(this))
                {
                    m_attachedGroup.AddPart(this);
                }
            }
#endif
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Unregister(this);
            }

            m_placementSystem.Shutdown();
            m_rendererSystem.Shutdown();
            m_conditionSystem.Shutdown();
            m_behaviorSystem.Shutdown();
            m_cacheSystem.Shutdown();
        }

        protected virtual void OnDestroy()
        {
            if (m_attachedGroup != null)
            {
                m_attachedGroup.RemovePart(this);
            }

            m_behaviorSystem.DestroyAll();
            m_conditionSystem.DestroyAll();
        }

        public static BuildingPart LastPlacedPart { get; private set; }
        public static int LastPlacedFrame { get; private set; } = -1;

        public virtual void SetState(BuildingState newState)
        {
            if (m_state == newState)
            {
                return;
            }

            m_lastState = m_state;
            m_state = newState;

            if (newState == BuildingState.Placed && Application.isPlaying)
            {
                LastPlacedPart = this;
                LastPlacedFrame = Time.frameCount;
            }

            PlacementSystem.HandleStateChange(newState);

            EventPublisher.Publish(new BuildingPartEvent.StateChangedEventArgs(this, m_lastState, m_state));
        }

        public virtual void SetSocket(BuildingSocket socket)
        {
            if (m_attachedSocket == socket)
            {
                return;
            }

            if (m_attachedSocket != null && State == BuildingState.Placed)
            {
                m_attachedSocket.ClearAttachedPart();
            }

            m_attachedSocket = socket;

            if (m_attachedSocket != null && State == BuildingState.Placed)
            {
                m_attachedSocket.SetAttachedPart(this);
            }
        }

        public virtual void ClearSocket() => SetSocket(null);

        public virtual void AttachToGroup(BuildingGroup newGroup)
        {
            m_attachedGroup = newGroup;
        }

        public virtual void Move(SocketSnapData snappingPoint, Transform transform)
        {
            if (snappingPoint == null)
            {
                return;
            }

            Vector3 worldPos = transform.TransformPoint(snappingPoint.PositionOffset);
            Quaternion worldRot = transform.rotation * Quaternion.Euler(snappingPoint.RotationOffset);
            Vector3 worldScale = Vector3.Scale(transform.lossyScale, snappingPoint.ScaleOffset);
            Move(worldPos, worldRot, worldScale);
        }

        public virtual void Move(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
                float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
            {
                return;
            }

            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }

        public virtual string GetSaveKey()
        {
            return UniqueId;
        }

        public virtual object GetSaveData()
        {
            if (!IsSaveable || (State != BuildingState.Placed && State != BuildingState.Queue && State != BuildingState.None))
            {
                return null;
            }

            BuildingPartData partData = new BuildingPartData(this);
            SaveCallback?.Invoke(partData.CustomProperties);
            return partData;
        }

        public virtual void LoadSaveData(object data)
        {
            BuildingPartData partData = data as BuildingPartData;
            if (partData == null)
            {
                return;
            }

            partData.ApplyTo(this);
            LoadCallback?.Invoke(partData.CustomProperties);
        }

        #region IDebuggable

        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.SceneView;

        public bool DebugEnabled => isActiveAndEnabled;

        public DebugRenderer.ViewFlags DebugFlags { get => m_debugFlags; set => m_debugFlags = value; }

        public bool RequireSelection => false;

        public virtual void OnDebugRender()
        {
            if (m_rendererSystem.DrawGizmos)
            {
                Vector3 worldCenter = transform.position + transform.rotation * (m_rendererSystem.Active.Root.localPosition + m_rendererSystem.Active.GetLocalBounds().center);
                DebugRenderer.DrawWireCube(
                    worldCenter,
                    m_rendererSystem.Active.GetLocalBounds().size,
                    transform.rotation,
                    transform.localScale,
                    Color.cyan,
                    0f,
                    1f,
                    false);
            }

            if (m_placementSystem.DrawGizmos && m_placementSystem.Settings.PreviewUseGridSnapping)
            {
                int cellCountX = Mathf.Max(1, Mathf.RoundToInt(m_placementSystem.Settings.PreviewCellSizeX));
                int cellCountZ = Mathf.Max(1, Mathf.RoundToInt(m_placementSystem.Settings.PreviewCellSizeZ));
                float cellUnitSize = 1f;
                Vector3 cellSize = new Vector3(cellUnitSize, 0f, cellUnitSize);

                for (int x = 0; x < cellCountX; x++)
                {
                    for (int z = 0; z < cellCountZ; z++)
                    {
                        float offsetX = (x - (cellCountX - 1) * 0.5f) * cellUnitSize;
                        float offsetZ = (z - (cellCountZ - 1) * 0.5f) * cellUnitSize;
                        Vector3 cellCenter = transform.position + transform.rotation * new Vector3(offsetX, 0f, offsetZ);
                        DebugRenderer.DrawWireCube(cellCenter, cellSize, transform.rotation, Color.red, 0f, 1f, false);
                    }
                }
            }
        }

        #endregion
    }
}