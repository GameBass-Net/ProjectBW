/// <summary>
/// Project : Easy Build System
/// Class : BuildingCondition.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts
{
    public struct ConditionResult
    {
        public bool IsValid;
        public string Reason;

        public ConditionResult(bool isValid, string reason = "")
        {
            IsValid = isValid;
            Reason = reason;
        }
    }

    [ExecuteAlways]
    public abstract class BuildingCondition : MonoBehaviour, IDebuggable
    {
        [field: NonSerialized] private BuildingPart m_part;
        [SerializeField] private bool m_isDisabled;

#if UNITY_EDITOR
        [SerializeField] private bool m_showLogs;
        [SerializeField] private bool m_showGizmos = true;
#endif

        private BuildingConditionAttribute m_conditionInfo;
        private ConditionResult m_cachedResult;
        private BuildingMode m_lastEvaluatedMode = BuildingMode.None;
        private int m_lastEvaluatedFrame = -1;

        public virtual int EvaluationOrder => 0;

        public BuildingPart Part { get => m_part; set => m_part = value; }

        public bool IsDisabled { get => m_isDisabled; set => m_isDisabled = value; }

#if UNITY_EDITOR
        public bool ShowLogs { get => m_showLogs; set => m_showLogs = value; }
        public virtual bool ShowGizmos { get => m_showGizmos; set => m_showGizmos = value; }
#endif

        public string Name => ConditionInfo != null ? ConditionInfo.Name : GetType().Name;

        public string Description => ConditionInfo != null ? ConditionInfo.Description : string.Empty;

        public bool IsRequired => ConditionInfo != null && ConditionInfo.IsRequired;

        private BuildingConditionAttribute ConditionInfo
        {
            get
            {
                if (m_conditionInfo == null)
                {
                    m_conditionInfo = (BuildingConditionAttribute)Attribute.GetCustomAttribute(
                        GetType(),
                        typeof(BuildingConditionAttribute));
                }

                return m_conditionInfo;
            }
        }

        private void Awake()
        {
            m_part = GetComponent<BuildingPart>();
            Initialize(m_part);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                DebugRendererManager.Unregister(this);
            }
        }

        public virtual void Initialize(BuildingPart part)
        {
            if (Application.isPlaying)
            {
                DebugRendererManager.Register(this);
            }
        }

        public virtual void Reset(BuildingPart part) { }

        public ConditionResult Evaluate(BuildingMode buildMode)
        {
            if (m_lastEvaluatedMode == buildMode && Time.frameCount == m_lastEvaluatedFrame)
            {
                return m_cachedResult;
            }

            ConditionResult evaluationResult = EvaluateCondition(buildMode);

            m_cachedResult = evaluationResult;
            m_lastEvaluatedMode = buildMode;
            m_lastEvaluatedFrame = Time.frameCount;

            if (Part != null)
            {
                if (evaluationResult.IsValid)
                {
                    EventPublisher.Publish(
                        new BuildingPartEvent.ConditionValidatedEventArgs(Part, this, buildMode, true, evaluationResult.Reason));
                }
                else
                {
                    EventPublisher.Publish(
                        new BuildingPartEvent.ConditionFailedEventArgs(Part, this, buildMode, evaluationResult.Reason));
                }
            }

            return evaluationResult;
        }

        protected virtual ConditionResult EvaluateCondition(BuildingMode buildMode)
        {
            return EvaluateInternal(buildMode);
        }

        protected abstract ConditionResult EvaluateInternal(BuildingMode buildMode);

        #region IDebuggable

        [SerializeField] private bool m_enableDebug = false;
        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.SceneView;

        public bool EnableDebug
        {
            get => m_enableDebug;
            set => m_enableDebug = value;
        }

        public bool DebugEnabled => isActiveAndEnabled && m_enableDebug;

        public DebugRenderer.ViewFlags DebugFlags
        {
            get => m_debugFlags;
            set => m_debugFlags = value;
        }

        public bool RequireSelection => true;

        public virtual void OnDebugRender() { }

        #endregion
    }
}