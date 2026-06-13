/// <summary>
/// Project : Easy Build System
/// Class : BuildingAnimationBehavior.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations
{
    [BuildingBehavior("Building Animation Behavior",
        "Plays animations during building events such as placement, destruction, adjustment or upgrading.")]
    public class BuildingAnimationBehavior : BuildingBehavior
    {
        public enum AnimationType { Animator, Procedural }

        [Serializable]
        public class AnimationEventMappingData
        {
            public enum TriggerType { OnPreviewCreated, OnPlaced, OnAdjusted, OnUpgraded, OnDestroyed }

            [SerializeField] private Animator m_animator;
            [SerializeField, Range(0, 10)] private float m_animatorSpeed = 1f;
            [SerializeField] private int m_animatorLayerIndex = 0;
            [SerializeField, AnimatorState("m_animator", "m_animatorSpeed", "m_animatorLayerIndex")] private string m_animatorStateName;
            [SerializeField] private float m_duration = 0.5f;
            [SerializeField] private AnimationCurve m_scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
            [SerializeField] private AnimationCurve m_rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);
            [SerializeField] private AnimationCurve m_positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

            public TriggerType EventType;
            public AnimationType Type = AnimationType.Animator;

            public Animator Animator { get => m_animator; set => m_animator = value; }

            public float AnimatorSpeed => m_animatorSpeed;

            public int AnimatorLayerIndex => m_animatorLayerIndex;

            public string AnimatorStateName => m_animatorStateName;

            public float Duration => m_duration;

            public AnimationCurve ScaleCurve => m_scaleCurve;

            public AnimationCurve RotationCurve => m_rotationCurve;

            public AnimationCurve PositionCurve => m_positionCurve;
        }

        [SerializeField] private AnimationEventMappingData[] m_eventMappings = Array.Empty<AnimationEventMappingData>();

        private Vector3[] m_baseScales;
        private Vector3[] m_basePositions;
        private Quaternion[] m_baseRotations;
        private Renderer[] m_baseRenderers;

        public AnimationEventMappingData[] EventMappings => m_eventMappings;

        public float AnimationEndTime { get; set; }

        public bool HasActiveAnimation => Application.isPlaying && Time.time < AnimationEndTime;

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);

            if (IsDisabled)
            {
                return;
            }

            EventPublisher.Subscribe<BuildingPartEvent.PreviewCreatedEventArgs>(OnPreviewCreated);
            EventPublisher.Subscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
            EventPublisher.Subscribe<BuildingStateEvent.AdjustedEventArgs>(OnPartAdjusted);
            EventPublisher.Subscribe<BuildingStateEvent.UpgradedEventArgs>(OnPartUpgraded);
            EventPublisher.Subscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);
        }

        public override void Shutdown()
        {
            if (IsDisabled)
            {
                return;
            }

            RestoreBaseTransforms();
            AnimationEndTime = 0f;

            EventPublisher.Unsubscribe<BuildingPartEvent.PreviewCreatedEventArgs>(OnPreviewCreated);
            EventPublisher.Unsubscribe<BuildingStateEvent.PlacedEventArgs>(OnPartPlaced);
            EventPublisher.Unsubscribe<BuildingStateEvent.AdjustedEventArgs>(OnPartAdjusted);
            EventPublisher.Unsubscribe<BuildingStateEvent.UpgradedEventArgs>(OnPartUpgraded);
            EventPublisher.Unsubscribe<BuildingStateEvent.DestroyedEventArgs>(OnPartDestroyed);

            base.Shutdown();
        }

        private void RestoreBaseTransforms()
        {
            if (m_baseRenderers == null || m_baseScales == null)
            {
                return;
            }

            for (int i = 0; i < m_baseRenderers.Length; i++)
            {
                Renderer r = m_baseRenderers[i];
                if (r == null)
                {
                    continue;
                }

                Transform t = r.transform;
                t.localScale = m_baseScales[i];
                t.localPosition = m_basePositions[i];
                t.localRotation = m_baseRotations[i];
            }
        }

        private void OnPreviewCreated(BuildingPartEvent.PreviewCreatedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            HandleAnimationEvent(AnimationEventMappingData.TriggerType.OnPreviewCreated);
        }

        private void OnPartPlaced(BuildingStateEvent.PlacedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            HandleAnimationEvent(AnimationEventMappingData.TriggerType.OnPlaced);
        }

        private void OnPartAdjusted(BuildingStateEvent.AdjustedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            HandleAnimationEvent(AnimationEventMappingData.TriggerType.OnAdjusted);
        }

        private void OnPartUpgraded(BuildingStateEvent.UpgradedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            HandleAnimationEvent(AnimationEventMappingData.TriggerType.OnUpgraded);
        }

        private void OnPartDestroyed(BuildingStateEvent.DestroyedEventArgs args)
        {
            if (args.Part != Part)
            {
                return;
            }

            HandleAnimationEvent(AnimationEventMappingData.TriggerType.OnDestroyed);
        }

        private void HandleAnimationEvent(AnimationEventMappingData.TriggerType triggerType)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (BuildingManager.Instance != null &&
                BuildingManager.Instance.SaveSystem != null &&
                BuildingManager.Instance.SaveSystem.IsLoading)
            {
                return;
            }

            if (IsDisabled)
            {
                return;
            }

            if (HasActiveAnimation)
            {
                return;
            }

            for (int i = 0; i < m_eventMappings.Length; i++)
            {
                AnimationEventMappingData mapping = m_eventMappings[i];
                if (mapping == null || mapping.EventType != triggerType)
                {
                    continue;
                }

                if (mapping.Type == AnimationType.Animator)
                {
                    StartCoroutine(PlayAnimatorAnimation(mapping));
                }
                else
                {
                    StartCoroutine(PlayProceduralAnimation(mapping));
                }
            }
        }

        private IEnumerator PlayAnimatorAnimation(AnimationEventMappingData mapping)
        {
            if (mapping.Animator == null || string.IsNullOrEmpty(mapping.AnimatorStateName))
            {
                yield break;
            }

            mapping.Animator.speed = mapping.AnimatorSpeed;
            mapping.Animator.Play(mapping.AnimatorStateName, mapping.AnimatorLayerIndex, 0f);

            yield return null;

            AnimatorStateInfo stateInfo = mapping.Animator.GetCurrentAnimatorStateInfo(mapping.AnimatorLayerIndex);
            float duration = stateInfo.length / Mathf.Max(0.0001f, mapping.AnimatorSpeed);

            AnimationEndTime = Mathf.Max(AnimationEndTime, Time.time + duration);

            yield return new WaitForSeconds(duration);

            AnimationEndTime = 0f;
        }

        private IEnumerator PlayProceduralAnimation(AnimationEventMappingData mapping)
        {
            RendererVariantData activeVariant = Part.RendererSystem?.Active;
            if (activeVariant?.Renderers == null || activeVariant.Renderers.Length == 0)
            {
                yield break;
            }

            Renderer[] renderers = activeVariant.Renderers;
            float duration = Mathf.Max(0f, mapping.Duration);
            float startTime = Time.time;

            AnimationEndTime = Time.time + duration;

            CaptureBaseTransformsIfNeeded(renderers);

            while (Time.time - startTime < duration)
            {
                float progress = Mathf.Clamp01((Time.time - startTime) / duration);
                float scaleValue = mapping.ScaleCurve.Evaluate(progress);
                float rotationValue = mapping.RotationCurve.Evaluate(progress);
                float positionValue = mapping.PositionCurve.Evaluate(progress);

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (!renderers[i])
                    {
                        continue;
                    }

                    Transform t = renderers[i].transform;
                    t.localScale = m_baseScales[i] * scaleValue;
                    t.localPosition = m_basePositions[i] + Vector3.up * positionValue;
                    t.localRotation = m_baseRotations[i] * Quaternion.Euler(0f, rotationValue, 0f);
                }

                yield return null;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i])
                {
                    continue;
                }

                Transform t = renderers[i].transform;
                t.localScale = m_baseScales[i];
                t.localPosition = m_basePositions[i];
                t.localRotation = m_baseRotations[i];
            }

            AnimationEndTime = 0f;
        }

        private void CaptureBaseTransformsIfNeeded(Renderer[] renderers)
        {
            if (m_baseRenderers != null && m_baseRenderers.Length == renderers.Length)
            {
                bool sameSet = true;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (m_baseRenderers[i] != renderers[i])
                    {
                        sameSet = false;
                        break;
                    }
                }

                if (sameSet)
                {
                    return;
                }
            }

            m_baseRenderers = new Renderer[renderers.Length];
            m_baseScales = new Vector3[renderers.Length];
            m_basePositions = new Vector3[renderers.Length];
            m_baseRotations = new Quaternion[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                m_baseRenderers[i] = renderers[i];
                if (!renderers[i])
                {
                    continue;
                }

                Transform t = renderers[i].transform;
                m_baseScales[i] = t.localScale;
                m_basePositions[i] = t.localPosition;
                m_baseRotations[i] = t.localRotation;
            }
        }
    }
}