/// <summary>
/// Project : Easy Build System
/// Class : BuildingArea.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas
{
    [ExecuteAlways]
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system")]
    public class BuildingArea : RegisterableUniqueObject, IDebuggable
    {
        public enum ShapeType { Sphere, Bounds }
        public enum InclusionMode { Partial, Full }

        [SerializeField, Category("AreaType")] private string m_areaType;
        [SerializeField, Range(0, 100)] private int m_areaPriority = 0;
        [SerializeField] private ShapeType m_areaShapeType = ShapeType.Bounds;
        [SerializeField] private float m_areaSphereRadius = 5.0f;
        [SerializeField] private Vector3 m_areaBounds = new Vector3(1f, 1f, 1f);
        [SerializeField] private InclusionMode m_areaInclusionMode = InclusionMode.Partial;
        [SerializeField] private List<BuildingRule> m_buildingRules = new List<BuildingRule>();

        protected readonly List<BuildingPart> m_registeredParts = new List<BuildingPart>();

        public string AreaType { get => m_areaType; set => m_areaType = value; }
        public int AreaPriority { get => m_areaPriority; set => m_areaPriority = value; }
        public ShapeType AreaShapeType { get => m_areaShapeType; set => m_areaShapeType = value; }
        public float AreaSphereRadius { get => m_areaSphereRadius; set => m_areaSphereRadius = value; }
        public Vector3 AreaBounds { get => m_areaBounds; set => m_areaBounds = value; }
        public InclusionMode AreaInclusionMode { get => m_areaInclusionMode; set => m_areaInclusionMode = value; }
        public List<BuildingRule> BuildingRules { get => m_buildingRules; set => m_buildingRules = value; }
        public List<BuildingPart> RegisteredParts { get => m_registeredParts; }

        public override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Register(this);

                EventPublisher.Subscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
                EventPublisher.Subscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);
                EventPublisher.Subscribe<BuildingPartEvent.StateChangedEventArgs>(OnPartStateChanged);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Unregister(this);

                EventPublisher.Unsubscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
                EventPublisher.Unsubscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);
                EventPublisher.Unsubscribe<BuildingPartEvent.StateChangedEventArgs>(OnPartStateChanged);
            }
        }

        public virtual void RegisterPart(BuildingPart part)
        {
            if (!m_registeredParts.Contains(part))
            {
                m_registeredParts.Add(part);
            }
        }

        public virtual void UnregisterPart(BuildingPart part)
        {
            m_registeredParts.Remove(part);
        }

        public virtual ConditionResult ValidateRules(BuildingPart part, BuildingMode mode)
        {
            for (int i = 0; i < m_buildingRules.Count; i++)
            {
                BuildingRule rule = m_buildingRules[i];
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

        public virtual bool ContainsPart(BuildingPart part)
        {
            if (part == null)
            {
                return false;
            }

            Bounds partBounds = part.RendererSystem.Active.GetWorldBounds();

            return m_areaShapeType == ShapeType.Sphere
                ? CheckSphereContains(partBounds)
                : CheckBoundsContains(partBounds);
        }

        protected virtual float GetScaledRadius()
        {
            Vector3 scale = transform.lossyScale;
            return m_areaSphereRadius * Mathf.Max(scale.x, scale.y, scale.z);
        }

        protected virtual Vector3 GetScaledBounds()
        {
            return Vector3.Scale(m_areaBounds, transform.lossyScale);
        }

        protected virtual bool CheckSphereContains(Bounds partBounds)
        {
            float radius = GetScaledRadius();
            float distance = Vector3.Distance(partBounds.center, transform.position);
            float extents = partBounds.extents.magnitude;

            return m_areaInclusionMode == InclusionMode.Partial
                ? distance - extents <= radius
                : distance + extents <= radius;
        }

        protected virtual bool CheckBoundsContains(Bounds partBounds)
        {
            Bounds areaBounds = new Bounds(transform.position, GetScaledBounds());

            if (m_areaInclusionMode == InclusionMode.Partial)
            {
                return areaBounds.Intersects(partBounds);
            }

            return areaBounds.min.x <= partBounds.min.x &&
                   areaBounds.min.y <= partBounds.min.y &&
                   areaBounds.min.z <= partBounds.min.z &&
                   areaBounds.max.x >= partBounds.max.x &&
                   areaBounds.max.y >= partBounds.max.y &&
                   areaBounds.max.z >= partBounds.max.z;
        }

        protected virtual void OnPartPlaced(BuildingStateEvent.PlacedEventArgs args)
        {
            if (args?.Part == null || args.Part.State != BuildingPart.BuildingState.Placed)
            {
                return;
            }

            if (ContainsPart(args.Part))
            {
                RegisterPart(args.Part);
            }
        }

        protected virtual void OnPartDestroyed(BuildingStateEvent.DestroyedEventArgs args)
        {
            if (args?.Part == null || args.Part.State != BuildingPart.BuildingState.Destruction)
            {
                return;
            }

            UnregisterPart(args.Part);
        }

        protected virtual void OnPartStateChanged(BuildingPartEvent.StateChangedEventArgs args)
        {
            if (args?.Part == null)
            {
                return;
            }

            BuildingPart.BuildingState state = args.Part.State;
            if (state != BuildingPart.BuildingState.Placement && state != BuildingPart.BuildingState.Adjusting)
            {
                return;
            }

            if (ContainsPart(args.Part))
            {
                RegisterPart(args.Part);
            }
            else
            {
                UnregisterPart(args.Part);
            }
        }

        #region IDebuggable

        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.SceneView;

        public bool DebugEnabled => isActiveAndEnabled;

        public DebugRenderer.ViewFlags DebugFlags
        {
            get => m_debugFlags;
            set => m_debugFlags = value;
        }

        public bool RequireSelection => false;

        public virtual void OnDebugRender()
        {
            Color wireColor = new Color(1f, 0.85f, 0f);
            Color fillColor = new Color(1f, 0.85f, 0f, 0.02f);
            Vector3 center = transform.position;

            if (m_areaShapeType == ShapeType.Sphere)
            {
                float radius = GetScaledRadius();
                DebugRenderer.DrawWireSphere(center, radius, wireColor, 24, 0.1f, 1f, false);
                DebugRenderer.DrawSphere(center, radius, fillColor, 8, 0.1f, false);
            }
            else
            {
                Vector3 size = GetScaledBounds();
                DebugRenderer.DrawWireCube(center, size, wireColor, 0.1f, 1f, false);
                DebugRenderer.DrawCube(center, size, fillColor, 0.1f, false);
            }
        }

        #endregion
    }
}