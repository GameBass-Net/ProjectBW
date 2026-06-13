/// <summary>
/// Project : Easy Build System
/// Class : BuildingDebrisBehavior.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

using Random = UnityEngine.Random;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations
{
    [BuildingBehavior("Building Debris Behavior",
        "Spawns physics debris when the part is destroyed, loses stability or hits the ground.")]
    public class BuildingDebrisBehavior : BuildingBehavior
    {
        public enum DestructionTrigger { OnDestroyed, OnFallingImpact, OnStabilityLost, OnDestroyedOrStabilityLost }

        [Serializable]
        public class DebrisSpawnData
        {
            [SerializeField] private GameObject m_prefab;
            [SerializeField] private AudioClipPlayer m_sound;
            [SerializeField] private Vector3 m_positionOffset = Vector3.zero;
            [SerializeField] private Vector3 m_rotationOffset = Vector3.zero;

            public GameObject Prefab => m_prefab;
            public AudioClipPlayer Sound => m_sound;
            public Vector3 PositionOffset => m_positionOffset;
            public Vector3 RotationOffset => m_rotationOffset;
        }

        [SerializeField] private DestructionTrigger m_trigger = DestructionTrigger.OnDestroyed;
        [SerializeField] private float m_impactSpeedThreshold = 3f;
        [SerializeField] private DebrisSpawnData[] m_variantSpawns = Array.Empty<DebrisSpawnData>();
        [SerializeField] private float m_upwardForce = 2f;
        [SerializeField] private float m_minForce = 1.5f;
        [SerializeField] private float m_maxForce = 4f;
        [SerializeField] private float m_randomTorque = 2f;
        [SerializeField] private float m_maxDepenetrationVelocity = 2f;
        [SerializeField] private float m_lifetime = 10f;
        [SerializeField] private LayerMask m_ignoreLayer;

        private const float DEBRIS_SPAWN_RATIO = 1.2f;

        private bool m_spawned;
        private BuildingCollapseCondition m_collapseCondition;

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);

            if (!Application.isPlaying || IsDisabled)
            {
                return;
            }

            EventPublisher.Subscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);

            m_collapseCondition = part.ConditionSystem.GetCondition(typeof(BuildingCollapseCondition)) as BuildingCollapseCondition;

            if (m_collapseCondition != null)
            {
                m_collapseCondition.StabilityLost.AddListener(OnStabilityLost);
            }
        }

        public override void Shutdown()
        {
            if (!Application.isPlaying || IsDisabled)
            {
                return;
            }

            EventPublisher.Unsubscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);

            if (m_collapseCondition != null)
            {
                m_collapseCondition.StabilityLost.RemoveListener(OnStabilityLost);
            }

            base.Shutdown();
        }

        private void OnPartDestroyed(BuildingStateEvent.DestroyedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            if (m_trigger == DestructionTrigger.OnDestroyed || m_trigger == DestructionTrigger.OnDestroyedOrStabilityLost)
            {
                TrySpawn();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_trigger != DestructionTrigger.OnFallingImpact || m_spawned)
            {
                return;
            }

            if (m_collapseCondition == null || !m_collapseCondition.IsFalling)
            {
                return;
            }

            if (collision.relativeVelocity.sqrMagnitude >= m_impactSpeedThreshold * m_impactSpeedThreshold)
            {
                TrySpawn();
            }
        }

        private void OnStabilityLost()
        {
            if (m_trigger == DestructionTrigger.OnStabilityLost || m_trigger == DestructionTrigger.OnDestroyedOrStabilityLost)
            {
                TrySpawn();
            }
        }

        private void TrySpawn()
        {
            if (m_spawned || Part == null)
            {
                return;
            }

            Collider[] colliders = Part.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i])
                {
                    colliders[i].enabled = false;
                }
            }

            Rigidbody[] rigidbodies = Part.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                if (rigidbodies[i])
                {
                    rigidbodies[i].isKinematic = true;
                    rigidbodies[i].detectCollisions = false;
                }
            }

            RendererVariantData activeVariant = Part.RendererSystem?.Active;
            if (activeVariant?.Root == null)
            {
                return;
            }

            int variantIndex = Part.RendererSystem.ActiveIndex;
            if (variantIndex < 0 || variantIndex >= m_variantSpawns.Length)
            {
                return;
            }

            DebrisSpawnData spawnData = m_variantSpawns[variantIndex];
            if (spawnData?.Prefab == null)
            {
                return;
            }

            Part.RendererSystem.SetVariantVisibility(false);

            Vector3 worldPos = activeVariant.Root.TransformPoint(spawnData.PositionOffset);
            Quaternion worldRot = activeVariant.Root.rotation * Quaternion.Euler(spawnData.RotationOffset);

            SpawnDebris(spawnData.Prefab, worldPos, worldRot);
            spawnData.Sound?.PlayAtPosition(worldPos);

            m_spawned = true;
        }

        private void SpawnDebris(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject debrisInstance = Instantiate(prefab, position, rotation);

            if (m_ignoreLayer != 0)
            {
                int layerIndex = (int)Mathf.Log(m_ignoreLayer.value, 2);
                debrisInstance.SetLayersInChildren(layerIndex);
                Physics.IgnoreLayerCollision(layerIndex, layerIndex, true);
            }

            Collider[] colliders = debrisInstance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i])
                {
                    Physics.IgnoreCollision(colliders[i], colliders[i], true);
                }
            }

            Rigidbody[] rigidbodies = debrisInstance.GetComponentsInChildren<Rigidbody>(true);

            int dividerInt = Mathf.Max(1, Mathf.RoundToInt(DEBRIS_SPAWN_RATIO));

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                if (!rigidbodies[i])
                {
                    continue;
                }

                if (dividerInt > 1 && i % dividerInt != 0)
                {
                    Destroy(rigidbodies[i].gameObject);
                    continue;
                }

                rigidbodies[i].maxDepenetrationVelocity = m_maxDepenetrationVelocity;
                rigidbodies[i].interpolation = RigidbodyInterpolation.Interpolate;
                rigidbodies[i].collisionDetectionMode = CollisionDetectionMode.Discrete;

#if UNITY_6000_0_OR_NEWER
                rigidbodies[i].angularDamping = 3.5f;
#else
                rigidbodies[i].angularDrag = 3.5f;
#endif
                rigidbodies[i].maxAngularVelocity = 6f;
                rigidbodies[i].inertiaTensorRotation = Quaternion.identity;
                rigidbodies[i].inertiaTensor = Vector3.one;

                Vector3 randomDir = Random.onUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);

                Vector3 force = randomDir * Random.Range(m_minForce, m_maxForce) + Vector3.up * m_upwardForce;

                rigidbodies[i].AddForce(force, ForceMode.Impulse);

                Vector3 torque = Random.insideUnitSphere * m_randomTorque;
                torque.y *= 0.3f;

                rigidbodies[i].AddTorque(torque, ForceMode.Impulse);
            }

            if (m_lifetime > 0f)
            {
                Destroy(debrisInstance, m_lifetime);
            }
        }
    }
}