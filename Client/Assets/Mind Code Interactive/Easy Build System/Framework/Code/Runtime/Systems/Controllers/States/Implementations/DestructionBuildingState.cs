/// <summary>
/// Project : Easy Build System
/// Class : DestructionBuildingState.cs
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
    public class DestructionBuildingState : BuildingState
    {
        protected BuildingPart m_highlightedPart;
        protected BuildingPart.BuildingState m_highlightedPartOriginalState;

        public BuildingPart HighlightedPart { get => m_highlightedPart; set => m_highlightedPart = value; }

        public override BuildingMode Mode => BuildingMode.Destruction;

        public override void UpdateState()
        {
            UpdateHighlight();
            base.UpdateState();
        }

        public override void ExitState()
        {
            ClearHighlight();
            base.ExitState();
        }

        public override void OnValidateAction()
        {
            ConditionResult result = new ConditionResult(false);
            bool isValid = m_highlightedPart != null && CheckValidity(m_highlightedPart, Mode, out result);

            PublishValidateAttempt(result);

            if (!isValid)
            {
                return;
            }

            BuildingManager.Instance.DestroyPart(m_highlightedPart);
            m_highlightedPart = null;
            base.OnValidateAction();
        }

        public override void OnCancelAction(bool cancelMode = true)
        {
            ClearHighlight();
            base.OnCancelAction(cancelMode);
        }

        protected virtual void UpdateHighlight()
        {
            BuildingPart target = ProbeHighlightTarget();

            if (target == m_highlightedPart)
            {
                if (target == null)
                {
                    ClearHighlight();
                }
                return;
            }

            ClearHighlight();

            if (target != null)
            {
                m_highlightedPart = target;
                m_highlightedPartOriginalState = target.State;
                target.SetState(BuildingPart.BuildingState.Destruction);
            }
        }

        protected virtual void ClearHighlight()
        {
            if (m_highlightedPart == null)
            {
                return;
            }

            m_highlightedPart.SetState(m_highlightedPartOriginalState);
            m_highlightedPart = null;
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
