/// <summary>
/// Project : Easy Build System
/// Class : AdjustmentBuildingState.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
{
    public class AdjustmentBuildingState : PlacementBuildingState
    {
        protected readonly struct AdjustmentSnapshot
        {
            public readonly Vector3 Position;
            public readonly Quaternion Rotation;
            public readonly BuildingPart.BuildingState State;
            public readonly bool IsValid;

            public AdjustmentSnapshot(BuildingPart part, BuildingPart.BuildingState state) : this()
                => (Position, Rotation, State, IsValid) = (part.transform.position, part.transform.rotation, state, true);
        }

        [SerializeField] private bool m_resetRotationOnAdjust;

        protected BuildingPart m_highlightedPart;
        protected BuildingPart.BuildingState m_highlightedPartOriginalState;
        protected AdjustmentSnapshot m_snapshot;
        protected bool m_isAdjusting;

        protected BuildingPart m_swapTarget;
        protected AdjustmentSnapshot m_swapSnapshot;

        public BuildingPart HighlightedPart => m_highlightedPart;
        public bool IsAdjusting => m_isAdjusting;
        public BuildingPart SwapTarget => m_swapTarget;

        public override BuildingMode Mode => BuildingMode.Adjustment;

        protected virtual float SwapHoldDistance => 1f;

        public override void EnterState()
        {
            m_isAdjusting = false;
            m_snapshot = default;
        }

        public override void UpdateState()
        {
            if (m_isAdjusting)
            {
                base.UpdateState();
                UpdateSwapPreview();
            }
            else
            {
                UpdateHighlight();
            }
        }

        public override void ExitState()
        {
            if (m_isAdjusting)
            {
                CancelAdjustment();
            }
            else
            {
                ClearHighlight();
            }

            base.ExitState();
        }

        public override void OnValidateAction()
        {
            if (m_isAdjusting)
            {
                ConfirmAdjustment();
                return;
            }

            if (m_highlightedPart == null)
            {
                PublishValidateAttempt(new ConditionResult(false, "No object highlighted"));
                return;
            }

            StartAdjustment();
        }

        public override void OnCancelAction(bool cancelMode = true)
        {
            if (m_isAdjusting)
            {
                CancelAdjustment();
                base.OnCancelAction(cancelMode);
                return;
            }

            ClearHighlight();
            base.OnCancelAction(cancelMode);
        }

        protected override void CreatePreview() { }

        protected override void PlacePreview() => ConfirmAdjustment();

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
                target.SetState(BuildingPart.BuildingState.Adjusting);
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

        protected virtual void StartAdjustment()
        {
            m_snapshot = new AdjustmentSnapshot(m_highlightedPart, m_highlightedPartOriginalState);

            m_previewPart = m_highlightedPart;
            m_currentRotation = m_resetRotationOnAdjust ? Quaternion.identity : m_highlightedPart.transform.rotation;
            m_currentScale = m_highlightedPart.transform.localScale;
            m_isAdjusting = true;

            m_highlightedPart.SetState(BuildingPart.BuildingState.Placement);
            m_highlightedPart = null;

            EventPublisher.Publish(new BuildingStateEvent.AdjustmentStartedEventArgs(m_previewPart));
        }

        protected virtual void UpdateSwapPreview()
        {
            if (m_previewPart == null || !m_snapshot.IsValid)
            {
                return;
            }

            if (m_swapTarget != null)
            {
                if (Vector3.Distance(m_previewPart.transform.position, m_swapSnapshot.Position) <= SwapHoldDistance)
                {
                    return;
                }

                RevertSwap();
            }

            if (View == null || !View.Raycast(out RaycastHit hit))
            {
                return;
            }

            BuildingPart candidate = hit.collider != null ? hit.collider.GetComponentInParent<BuildingPart>() : null;

            if (candidate == null || candidate == m_previewPart || !candidate.enabled)
            {
                return;
            }

            if (candidate.State != BuildingPart.BuildingState.Placed)
            {
                return;
            }

            if (!string.Equals(candidate.Category, m_previewPart.Category))
            {
                return;
            }

            m_swapTarget = candidate;
            m_swapSnapshot = new AdjustmentSnapshot(candidate, candidate.State);
            candidate.transform.SetPositionAndRotation(m_snapshot.Position, m_snapshot.Rotation);
            candidate.SetState(BuildingPart.BuildingState.Adjusting);
            candidate.RendererSystem.SetCollidersEnabled(false);
        }

        protected virtual void RevertSwap()
        {
            if (m_swapTarget == null || !m_swapSnapshot.IsValid)
            {
                m_swapTarget = null;
                m_swapSnapshot = default;
                return;
            }

            m_swapTarget.transform.SetPositionAndRotation(m_swapSnapshot.Position, m_swapSnapshot.Rotation);
            m_swapTarget.SetState(m_swapSnapshot.State);
            m_swapTarget = null;
            m_swapSnapshot = default;
        }

        protected virtual void ConfirmAdjustment()
        {
            if (m_swapTarget != null)
            {
                BuildingPart adjusted = m_previewPart;
                BuildingPart swapped = m_swapTarget;
                BuildingPart.BuildingState swappedRestoreState = m_swapSnapshot.IsValid ? m_swapSnapshot.State : BuildingPart.BuildingState.Placed;

                adjusted.ClearSocket();
                adjusted.SetState(m_snapshot.IsValid ? m_snapshot.State : BuildingPart.BuildingState.Placed);
                BuildingManager.Instance.AdjustPart(adjusted, adjusted.transform.position, adjusted.transform.rotation);

                swapped.SetState(swappedRestoreState);
                BuildingManager.Instance.AdjustPart(swapped, swapped.transform.position, swapped.transform.rotation);

                EventPublisher.Publish(new BuildingStateEvent.AdjustmentEndedEventArgs(adjusted));

                ResetState();

                if (CancelStateAfterValidation)
                {
                    BuildingController.SetMode(BuildingMode.None);
                }

                return;
            }

            ConditionResult result = new ConditionResult(false);
            bool isValid = HasPreview && CheckValidity(m_previewPart, Mode, out result);

            PublishValidateAttempt(result);

            if (!isValid)
            {
                m_previewPart?.PlacementSystem.UpdatePreview(false, BuildingPart.BuildingState.Placement);
                return;
            }

            BuildingPart part = m_previewPart;

            part.ClearSocket();
            part.SetState(m_snapshot.IsValid ? m_snapshot.State : BuildingPart.BuildingState.Placed);

            BuildingManager.Instance.AdjustPart(part, part.transform.position, part.transform.rotation);
            EventPublisher.Publish(new BuildingStateEvent.AdjustmentEndedEventArgs(part));

            ResetState();

            if (CancelStateAfterValidation)
            {
                BuildingController.SetMode(BuildingMode.None);
            }
        }

        protected virtual void CancelAdjustment()
        {
            RevertSwap();

            if (m_previewPart != null && m_snapshot.IsValid)
            {
                m_previewPart.transform.SetPositionAndRotation(m_snapshot.Position, m_snapshot.Rotation);
                m_previewPart.ClearSocket();
                m_previewPart.SetState(m_snapshot.State);

                EventPublisher.Publish(new BuildingStateEvent.AdjustmentEndedEventArgs(m_previewPart));
            }

            ResetState();
        }

        protected virtual void ResetState()
        {
            m_previewPart = null;
            m_closestSocket = null;
            m_hasSnappedRotation = false;
            m_isAdjusting = false;
            m_snapshot = default;
            m_swapTarget = null;
            m_swapSnapshot = default;
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