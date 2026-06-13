/// <summary>
/// Project : Easy Build System
/// Class : PlacementBuildingState.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Helpers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations
{
    public class PlacementBuildingState : BuildingState
    {
        [SerializeField] private bool m_snapOnlyIfValid = true;
        [SerializeField] private bool m_avoidOccupiedCells = true;
        [SerializeField] private bool m_lockRotation;
        [SerializeField] private bool m_invertPreviewRotation;
        [SerializeField] private bool m_preservePreviewRotation = true;

        protected BuildingPart m_previewPart;
        protected Quaternion m_currentRotation;
        protected Vector3 m_currentScale;
        protected BuildingSocket m_closestSocket;
        protected bool m_hasSnappedRotation;

        private readonly BuildingSocketSearcher m_socketSearcher = new BuildingSocketSearcher();
        private readonly PreviewMovementSolver m_movementSolver = new PreviewMovementSolver();

        private Quaternion m_lastRotation = Quaternion.identity;
        private Vector3 m_cachedTargetPosition;
        private Vector3 m_cachedTargetNormal = Vector3.up;
        private bool m_isMoving;

        public bool SnapOnlyIfValid { get => m_snapOnlyIfValid; set => m_snapOnlyIfValid = value; }
        public bool AvoidOccupiedCells { get => m_avoidOccupiedCells; set => m_avoidOccupiedCells = value; }
        public bool LockRotation { get => m_lockRotation; set => m_lockRotation = value; }
        public bool InvertPreviewRotation { get => m_invertPreviewRotation; set => m_invertPreviewRotation = value; }
        public bool PreservePreviewRotation { get => m_preservePreviewRotation; set => m_preservePreviewRotation = value; }

        public BuildingPart PreviewPart { get => m_previewPart; set => m_previewPart = value; }
        public Quaternion CurrentPreviewRotation { get => m_currentRotation; set => m_currentRotation = value; }
        public Vector3 CurrentPreviewScale { get => m_currentScale; set => m_currentScale = value; }
        public BuildingSocket ClosestSocket { get => m_closestSocket; set => m_closestSocket = value; }
        public bool HasSnappedRotation { get => m_hasSnappedRotation; set => m_hasSnappedRotation = value; }

        public bool HasSocket => m_closestSocket != null;
        public bool HasPreview => m_previewPart != null;

        public override BuildingMode Mode => BuildingMode.Placement;

        protected Vector3 TargetPosition => m_cachedTargetPosition;
        protected Vector3 TargetNormal => m_cachedTargetNormal;

        public override void EnterState()
        {
            base.EnterState();
            m_movementSolver.AvoidOccupiedCells = m_avoidOccupiedCells;
            CreatePreview();
        }

        public override void UpdateState()
        {
            base.UpdateState();
            RefreshTargetCache();
            m_movementSolver.AvoidOccupiedCells = m_avoidOccupiedCells;
            UpdatePreview();
        }

        public override void ExitState()
        {
            DestroyPreview();
            base.ExitState();
        }

        public override void OnValidateAction()
        {
            ConditionResult result = new ConditionResult(false);
            bool isValid = HasPreview && CheckValidity(m_previewPart, Mode, out result);

            PublishValidateAttempt(result);

            if (!isValid)
            {
                return;
            }

            PlacePreview();

            if (BuildingController.ActiveMode != BuildingMode.Placement)
            {
                return;
            }

            if (!CancelStateAfterValidation)
            {
                CreatePreview();
            }
            else
            {
                base.OnValidateAction();
            }
        }

        public override void OnCancelAction(bool cancelMode = true)
        {
            DestroyPreview();
            base.OnCancelAction(cancelMode);
        }

        public override void OnRotateAction(int direction) => RotatePreview(direction);

        protected virtual void CreatePreview()
        {
            if (HasPreview)
            {
                return;
            }

            BuildingPart selected = BuildingController?.SelectedPart;
            if (selected == null)
            {
                Debug.LogWarning("No Building Part selected.", this);
                return;
            }

            m_previewPart = BuildingManager.Instance.CreatePreview(selected);
            if (m_previewPart == null)
            {
                return;
            }

            m_currentRotation = m_preservePreviewRotation ? m_lastRotation : m_previewPart.transform.rotation;

            if (m_invertPreviewRotation && (!m_preservePreviewRotation || m_lastRotation == Quaternion.identity))
            {
                m_currentRotation *= Quaternion.Euler(0f, 180f, 0f);
            }

            m_currentScale = m_previewPart.transform.localScale;

            Vector3 spawnPosition = ResolveInitialSpawnPosition();
            m_previewPart.Move(spawnPosition, m_currentRotation, m_currentScale);
        }

        protected virtual void UpdatePreview()
        {
            if (!HasPreview)
            {
                return;
            }

            BuildingSocket validSocket = ResolveSocketSnap(out Vector3 position, out Quaternion rotation, out Vector3 scale);

            if (validSocket == null)
            {
                position = TargetPosition;

                if (m_hasSnappedRotation)
                {
                    m_hasSnappedRotation = false;
                    m_currentRotation = Quaternion.identity;
                }

                rotation = ResolveFreeRotation();
                scale = m_currentScale;
            }

            UpdateSocketState(validSocket);
            ApplyPreviewTransform(position, rotation, scale, validSocket != null);
        }

        protected virtual void PlacePreview()
        {
            if (!CanPlace())
            {
                return;
            }

            BuildingManager.Instance.PlacePart(
                m_previewPart,
                m_previewPart.transform.position,
                m_previewPart.transform.rotation,
                m_previewPart.transform.localScale,
                m_closestSocket);

            m_closestSocket = null;
            m_hasSnappedRotation = false;
        }

        protected void RotatePreview(float direction)
        {
            if (Mathf.Abs(direction) < Mathf.Epsilon || !HasPreview)
            {
                return;
            }

            bool snapped = HasSocket;

            if (m_lockRotation && !snapped)
            {
                return;
            }

            BuildingPlacementSettings settings = m_previewPart.PlacementSystem.Settings;

            if (snapped && !settings.PreviewAllowSnappedRotation)
            {
                return;
            }

            Vector3 step = snapped ? settings.PreviewSnappedRotationStep : settings.PreviewRotationStep;
            float sign = Mathf.Sign(direction);

            if (snapped)
            {
                float yaw = Mathf.Round(m_currentRotation.eulerAngles.y / step.y) * step.y + step.y * sign;
                m_currentRotation = Quaternion.Euler(0f, Mathf.Repeat(yaw, 360f), 0f);
            }
            else
            {
                m_currentRotation = Quaternion.Euler(step * sign) * m_currentRotation;
            }

            if (m_preservePreviewRotation)
            {
                m_lastRotation = m_currentRotation;
            }
        }

        protected void DestroyPreview()
        {
            if (!HasPreview)
            {
                return;
            }

            BuildingManager.Instance.DestroyPreview(m_previewPart);
            m_previewPart = null;
        }

        protected bool MovePreview(Vector3 position, Quaternion rotation, Vector3 scale, bool isSnapped)
        {
            return m_movementSolver.Move(m_previewPart, position, rotation, scale, TargetNormal, View, isSnapped);
        }

        private void RefreshTargetCache()
        {
            BuildingView view = BuildingController?.ActiveView;

            if (view != null && view.Raycast(out RaycastHit hit, QueryTriggerInteraction.Ignore) && hit.collider != null)
            {
                m_cachedTargetPosition = hit.point;
                m_cachedTargetNormal = hit.normal;
            }
            else
            {
                m_cachedTargetPosition = Vector3.zero;
                m_cachedTargetNormal = Vector3.up;
            }
        }

        private Vector3 ResolveInitialSpawnPosition()
        {
            Vector3 spawnPosition = TargetPosition;

            if (spawnPosition == Vector3.zero)
            {
                Transform camera = View.RaycastCamera.transform;
                spawnPosition = View.GetOriginTransform().position + camera.forward.normalized * (View.MaxValidDistance - 0.1f);
            }

            return m_movementSolver.ApplyInitialGrounding(m_previewPart, spawnPosition, View);
        }

        private BuildingSocket ResolveSocketSnap(out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = TargetPosition;
            rotation = m_currentRotation;
            scale = m_currentScale;

            BuildingSocket candidate = m_socketSearcher.FindBest(m_previewPart, TargetPosition, BuildingController.ActiveView, BuildingManager.Instance.SocketLayer);
            if (candidate == null)
            {
                return null;
            }

            SocketSnapResult snap = candidate.GetSnappingPoint(m_previewPart);
            bool socketChanged = m_closestSocket != candidate;

            if (socketChanged && !m_hasSnappedRotation)
            {
                m_currentRotation = Quaternion.identity;
                m_hasSnappedRotation = true;
            }

            Vector3 predictedPosition = snap.Position;
            Quaternion predictedRotation = snap.Rotation * m_currentRotation;
            Vector3 predictedScale = Vector3.Scale(m_currentScale, snap.Scale);

            bool isPredictionValid = !m_snapOnlyIfValid || !socketChanged
                || ValidateSnapPrediction(predictedPosition, predictedRotation, predictedScale);

            if (!isPredictionValid)
            {
                return null;
            }

            position = predictedPosition;
            rotation = predictedRotation;
            scale = predictedScale;
            return candidate;
        }

        private Quaternion ResolveFreeRotation()
        {
            if (!m_lockRotation || m_previewPart.PlacementSystem.Settings.PreviewSurfaceAlignment)
            {
                return m_currentRotation;
            }

            Vector3 forward = BuildingController.ActiveView.RaycastCamera.transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.01f ? Quaternion.LookRotation(forward.normalized) : m_currentRotation;
        }

        private bool ValidateSnapPrediction(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            BuildingCollisionCondition collision = m_previewPart.ConditionSystem.GetCondition(typeof(BuildingCollisionCondition)) as BuildingCollisionCondition;
            return collision == null || collision.PredictCollision(position, rotation, scale, true).IsValid;
        }

        private void UpdateSocketState(BuildingSocket socket)
        {
            if (socket != null)
            {
                if (m_previewPart.AttachedSocket != socket)
                {
                    m_previewPart.SetSocket(socket);
                }
                m_closestSocket = socket;
            }
            else
            {
                m_previewPart.ClearSocket();
                m_closestSocket = null;
            }
        }

        private void ApplyPreviewTransform(Vector3 position, Quaternion rotation, Vector3 scale, bool isSnapped)
        {
            m_isMoving = MovePreview(position, rotation, scale, isSnapped);
            bool isValid = !m_isMoving && CheckValidity(m_previewPart, Mode, out _);
            m_previewPart.PlacementSystem.UpdatePreview(isValid, BuildingPart.BuildingState.Placement);
        }

        private bool CanPlace() => HasPreview && !m_isMoving && m_previewPart.gameObject.activeSelf && CheckValidity(m_previewPart, Mode, out _);
    }
}
