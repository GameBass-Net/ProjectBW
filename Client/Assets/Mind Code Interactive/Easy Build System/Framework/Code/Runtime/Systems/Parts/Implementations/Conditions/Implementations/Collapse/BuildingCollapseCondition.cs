/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollapseCondition.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections;

using UnityEngine;
using UnityEngine.Events;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse
{
    [BuildingCondition("Building Collapse Condition",
        "Evaluates structural support and triggers a physics-based fall when the Building Part becomes unstable.")]
    public class BuildingCollapseCondition : BuildingCondition
    {
        protected const float STABILITY_CHECK_INTERVAL = 0.1f;

        [SerializeField] private bool m_requireStablePlacement = true;
        [SerializeField] private Vector3 m_boundsPosition = Vector3.zero;
        [SerializeField] private Vector3 m_boundsSize = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private LayerMask m_supportLayerMask = ~0;
        [SerializeField] private bool m_requireBuildingPart = true;
        [Category("PartCategory"), SerializeField] private string[] m_requireTypes;
        [SerializeField] private bool m_requireAnyColliderSupport;
        [SerializeField] private float m_fallPhysicsTime = 5f;
        [SerializeField] private float m_fallPhysicsMass = 50f;
        [SerializeField] private float m_fallPhysicsDrag;

#if UNITY_6000_0_OR_NEWER
        [SerializeField] private PhysicsMaterial m_fallPhysicMaterial;
#else
        [SerializeField] private PhysicMaterial m_fallPhysicMaterial;
#endif

        protected bool m_isFalling;
        protected bool m_canRunPhysicsChecks = true;
        protected bool m_hasSupport;
        protected float m_nextCheckTime;
        protected Rigidbody m_rigidbodyComponent;
        protected BuildingAnimationBehavior m_animationBehavior;

        public UnityEvent StabilityLost;

        public override int EvaluationOrder => 1;

        public bool RequireStablePlacement { get => m_requireStablePlacement; set => m_requireStablePlacement = value; }
        public Vector3 BoundsPosition { get => m_boundsPosition; set => m_boundsPosition = value; }
        public Vector3 BoundsSize { get => m_boundsSize; set => m_boundsSize = value; }
        public LayerMask SupportLayerMask { get => m_supportLayerMask; set => m_supportLayerMask = value; }
        public bool RequireBuildingPart { get => m_requireBuildingPart; set => m_requireBuildingPart = value; }
        public string[] RequireTypes { get => m_requireTypes; set => m_requireTypes = value; }
        public bool RequireAnyColliderSupport { get => m_requireAnyColliderSupport; set => m_requireAnyColliderSupport = value; }
        public float FallPhysicsTime { get => m_fallPhysicsTime; set => m_fallPhysicsTime = value; }
        public float FallPhysicsMass { get => m_fallPhysicsMass; set => m_fallPhysicsMass = value; }
        public float FallPhysicsDrag { get => m_fallPhysicsDrag; set => m_fallPhysicsDrag = value; }

#if UNITY_6000_0_OR_NEWER
        public PhysicsMaterial FallPhysicMaterial => m_fallPhysicMaterial;
#else
        public PhysicMaterial FallPhysicMaterial => m_fallPhysicMaterial;
#endif

        public virtual bool IsFalling { get => m_isFalling; set => m_isFalling = value; }
        public virtual bool HasSupport { get => m_hasSupport; protected set => m_hasSupport = value; }

        protected bool IsAnimating => m_animationBehavior != null && m_animationBehavior.HasActiveAnimation;

        protected Rigidbody RigidbodyComponent
        {
            get
            {
                if (m_rigidbodyComponent == null)
                {
                    m_rigidbodyComponent = Part.GetComponent<Rigidbody>();
                    if (m_rigidbodyComponent == null)
                    {
                        m_rigidbodyComponent = Part.gameObject.AddComponent<Rigidbody>();
                        m_rigidbodyComponent.isKinematic = true;
                    }
                }

                return m_rigidbodyComponent;
            }
        }

        protected virtual void OnEnable()
        {
            EventPublisher.Subscribe<BuildingSaveEvent.LoadStartedEventArgs>(OnBuildingsLoadStarted);
            EventPublisher.Subscribe<BuildingSaveEvent.LoadCompletedEventArgs>(OnBuildingsLoaded);

            BuildingManager manager = BuildingManager.Instance;
            if (manager != null && manager.PhysicsSystem != null)
            {
                manager.PhysicsSystem.Register(this);
            }
        }

        protected virtual void OnDisable()
        {
            EventPublisher.Unsubscribe<BuildingSaveEvent.LoadStartedEventArgs>(OnBuildingsLoadStarted);
            EventPublisher.Unsubscribe<BuildingSaveEvent.LoadCompletedEventArgs>(OnBuildingsLoaded);

            BuildingManager manager = BuildingManager.Instance;
            if (manager != null && manager.PhysicsSystem != null)
            {
                manager.PhysicsSystem.Unregister(this);
            }
        }

        protected virtual void Update()
        {
            BuildingManager manager = BuildingManager.Instance;
            if (manager == null || manager.PhysicsSystem == null || !manager.PhysicsSystem.Settings.EnablePhysics)
            {
                return;
            }

            RunSelfCheck();
        }

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);
            m_animationBehavior = part.BehaviorSystem.GetBehavior(typeof(BuildingAnimationBehavior)) as BuildingAnimationBehavior;
        }

        public override void Reset(BuildingPart part)
        {
            base.Reset(part);
            m_isFalling = false;
            m_nextCheckTime = 0f;
        }

        protected override ConditionResult EvaluateInternal(BuildingMode mode)
        {
            if (mode != BuildingMode.Placement)
            {
                return new ConditionResult(true);
            }

            if (!m_canRunPhysicsChecks || !m_requireStablePlacement)
            {
                return new ConditionResult(true);
            }

            if (IsAnimating)
            {
                return new ConditionResult(m_hasSupport);
            }

            return EvaluateStability()
                ? new ConditionResult(true)
                : new ConditionResult(false, "Support is unstable here.");
        }

        public virtual bool CheckStability()
        {
            if (!Application.isPlaying)
            {
                return true;
            }

            if (IsDisabled || !m_canRunPhysicsChecks)
            {
                return false;
            }

            if (IsAnimating)
            {
                return m_hasSupport;
            }

            bool shouldFall = !EvaluateStability();

            if (shouldFall)
            {
                ApplyPhysics();
            }

            return shouldFall;
        }

        public virtual void RunSelfCheck()
        {
            if (!Application.isPlaying || m_isFalling || IsDisabled || !m_canRunPhysicsChecks)
            {
                return;
            }

            if (Part == null || Part.State != BuildingPart.BuildingState.Placed)
            {
                return;
            }

            if (IsAnimating)
            {
                return;
            }

            if (Time.time < m_nextCheckTime)
            {
                return;
            }

            m_nextCheckTime = Time.time + Mathf.Max(0.1f, STABILITY_CHECK_INTERVAL);

            if (ShouldFall())
            {
                ApplyPhysics();
            }
        }

        protected virtual bool ShouldFall()
        {
            return !EvaluateStability();
        }

        protected virtual bool EvaluateStability()
        {
            Quaternion partRot = Part.transform.rotation;
            Vector3 boundsCenter = Part.transform.TransformPoint(m_boundsPosition);
            Vector3 boundsHalfExtent = m_boundsSize * 0.5f;

            int colliderCount = PhysicsExtensions.OverlapBoxNonAlloc(
                boundsCenter,
                boundsHalfExtent,
                partRot,
                out Collider[] colliders,
                m_supportLayerMask);

            for (int i = 0; i < colliderCount; i++)
            {
                Collider collider = colliders[i];
                if (!collider)
                {
                    continue;
                }

                if ((m_supportLayerMask.value & (1 << collider.gameObject.layer)) == 0)
                {
                    continue;
                }

                BuildingPart otherPart = collider.GetComponentInParent<BuildingPart>();

                if (otherPart == Part)
                {
                    continue;
                }

                if (m_requireAnyColliderSupport)
                {
                    m_hasSupport = true;
                    return true;
                }

                if (otherPart == null)
                {
                    if (!m_requireBuildingPart)
                    {
                        m_hasSupport = true;
                        return true;
                    }

                    continue;
                }

                if (!otherPart.enabled)
                {
                    continue;
                }

                if (m_requireTypes != null && m_requireTypes.Length > 0)
                {
                    bool typeMatches = false;
                    for (int j = 0; j < m_requireTypes.Length; j++)
                    {
                        if (otherPart.Category == m_requireTypes[j])
                        {
                            typeMatches = true;
                            break;
                        }
                    }

                    if (!typeMatches)
                    {
                        continue;
                    }
                }

                BuildingCollapseCondition targetCollapseCondition =
                    otherPart.ConditionSystem.GetCondition(typeof(BuildingCollapseCondition)) as BuildingCollapseCondition;

                if (targetCollapseCondition != null && targetCollapseCondition.IsFalling)
                {
                    continue;
                }

                m_hasSupport = true;
                return true;
            }

            m_hasSupport = false;
            return false;
        }

        protected virtual void ApplyPhysics()
        {
            if (!Application.isPlaying || m_isFalling)
            {
                return;
            }

            m_isFalling = true;
            Part.IsSaveable = false;

            StabilityLost?.Invoke();

            Part.CacheSystem.SetSocketsEnabled(false);
            Part.RendererSystem.SetCollidersConvex(true);

            ApplyFallPhysicMaterial();

            ConfigureFallingRigidbody(RigidbodyComponent);

            Part.gameObject.SetLayersInChildren(LayerMask.NameToLayer("Ignore Raycast"));
            Part.enabled = false;

            StartCoroutine(DestroyAfterFalling());
        }

        protected virtual void ConfigureFallingRigidbody(Rigidbody rb)
        {
            rb.mass = m_fallPhysicsMass;

#if UNITY_6000_0_OR_NEWER
            rb.linearDamping = m_fallPhysicsDrag;
#else
            rb.drag = m_fallPhysicsDrag;
#endif

            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        protected virtual void ApplyFallPhysicMaterial()
        {
            if (m_fallPhysicMaterial == null)
            {
                return;
            }

            Collider[] colliders = Part.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i])
                {
                    colliders[i].sharedMaterial = m_fallPhysicMaterial;
                }
            }
        }

        protected virtual IEnumerator DestroyAfterFalling()
        {
            yield return new WaitForSeconds(m_fallPhysicsTime);
            Destroy(Part.gameObject);
        }

        protected virtual void OnBuildingsLoadStarted(BuildingSaveEvent.LoadStartedEventArgs args)
        {
            m_canRunPhysicsChecks = false;
        }

        protected virtual void OnBuildingsLoaded(BuildingSaveEvent.LoadCompletedEventArgs args)
        {
            m_canRunPhysicsChecks = true;
        }

#if UNITY_EDITOR
        public override void OnDebugRender()
        {
            if (!ShowGizmos || IsDisabled || Part == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                EvaluateStability();
            }

            Vector3 worldPos = Part.transform.TransformPoint(m_boundsPosition);
            Quaternion rot = Part.transform.rotation;

            Color gizmoColor = m_hasSupport
                ? new Color(0f, 1f, 0f, 0.5f)
                : new Color(1f, 0f, 0f, 0.5f);

            DebugRenderer.DrawWireCube(worldPos, m_boundsSize, rot, gizmoColor, 0f, 1f, false);
        }
#endif
    }
}