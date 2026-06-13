/// <summary>
/// Project : Easy Build System
/// Class : UpgradeBuildingState.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
{
    public class UpgradeBuildingState : BuildingState
    {
        protected BuildingPart m_highlightedPart;
        protected int m_originalVariantIndex = -1;
        protected int m_targetVariantIndex = -1;

        public BuildingPart HighlightedPart => m_highlightedPart;

        public override BuildingMode Mode => BuildingMode.Upgrade;

        public override void EnterState()
        {
            base.EnterState();
            m_targetVariantIndex = -1;
        }

        public override void UpdateState()
        {
            UpdateHighlight();
            base.UpdateState();
        }

        public override void ExitState()
        {
            RestoreAndClearHighlight();
            base.ExitState();
        }

        public override void OnValidateAction()
        {
            ConditionResult result = new ConditionResult(false);
            bool isValid = m_highlightedPart != null
                && m_targetVariantIndex >= 0
                && CheckValidity(m_highlightedPart, Mode, out result);

            PublishValidateAttempt(result);

            if (!isValid)
            {
                return;
            }

            BuildingManager.Instance.UpgradePart(m_highlightedPart, m_targetVariantIndex);
            ClearHighlight();
            base.OnValidateAction();
        }

        public override void OnCancelAction(bool cancelMode = true)
        {
            RestoreAndClearHighlight();
            base.OnCancelAction(cancelMode);
        }

        public virtual void SetTargetVariant(int variantIndex)
        {
            m_targetVariantIndex = variantIndex;

            if (m_highlightedPart != null)
            {
                ApplyTargetVariant();
            }
        }

        protected virtual void UpdateHighlight()
        {
            BuildingPart target = ProbeHighlightTarget();

            if (target == m_highlightedPart)
            {
                if (target == null)
                {
                    RestoreAndClearHighlight();
                }
                return;
            }

            RestoreAndClearHighlight();

            if (target != null)
            {
                m_highlightedPart = target;
                m_originalVariantIndex = target.RendererSystem.ActiveIndex;
                ApplyTargetVariant();
            }
        }

        protected virtual void ApplyTargetVariant()
        {
            if (m_highlightedPart?.RendererSystem == null || m_targetVariantIndex < 0)
            {
                return;
            }

            int clampedIndex = Mathf.Clamp(m_targetVariantIndex, 0, m_highlightedPart.RendererSystem.Count - 1);
            m_highlightedPart.RendererSystem.SetVariant(clampedIndex);
            m_highlightedPart.SetState(BuildingPart.BuildingState.Adjusting);
        }

        protected virtual void ClearHighlight()
        {
            if (m_highlightedPart != null)
            {
                m_highlightedPart.SetState(BuildingPart.BuildingState.Placed);
                m_highlightedPart = null;
            }

            m_originalVariantIndex = -1;
        }

        protected virtual void RestoreAndClearHighlight()
        {
            if (m_highlightedPart != null && m_originalVariantIndex >= 0)
            {
                m_highlightedPart.RendererSystem.SetVariant(m_originalVariantIndex);
            }

            ClearHighlight();
        }

        private BuildingPart ProbeHighlightTarget()
        {
            if (View == null || !View.Raycast(out RaycastHit hit))
            {
                return null;
            }

            BuildingPart part = hit.collider?.GetComponentInParent<BuildingPart>();

            if (part == null || !part.enabled || !View.IsWithinValidDistance(part.transform.position))
            {
                return null;
            }

            return part;
        }
    }
}
