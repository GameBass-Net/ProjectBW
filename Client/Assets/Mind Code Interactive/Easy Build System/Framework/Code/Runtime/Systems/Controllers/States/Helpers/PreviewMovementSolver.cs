/// <summary>
/// Project : Easy Build System
/// Class : PreviewMovementSolver.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Helpers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Helpers
{
    public sealed class PreviewMovementSolver
    {
        private GridCellData m_lastValidCell;

        public bool AvoidOccupiedCells { get; set; } = true;

        public bool Move(BuildingPart preview, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 normal, BuildingView view, bool isSnapped)
        {
            if (preview == null || view == null)
            {
                return false;
            }

            BuildingPlacementSettings settings = preview.PlacementSystem.Settings;
            Vector3 origin = view.GetOriginTransform().position;
            Transform camera = view.RaycastCamera.transform;
            float maxDistance = view.MaxValidDistance;
            float safeDistance = maxDistance - 0.1f;

            if (!isSnapped)
            {
                position = settings.PreviewForceGrounding
                    ? ApplyGrounding(position, settings, origin, camera, safeDistance, maxDistance, view)
                    : ApplyFreeMovement(position, settings, origin, camera, safeDistance, maxDistance, view);
            }

            if (settings.PreviewSurfaceAlignment)
            {
                rotation = ApplySurfaceAlignment(rotation, normal, settings);
            }

            if (!isSnapped)
            {
                position = ApplyGrid(position, rotation, settings, preview);
            }

            return ApplyMovement(preview, position, rotation, scale, settings, isSnapped);
        }

        public Vector3 ApplyInitialGrounding(BuildingPart preview, Vector3 spawnPosition, BuildingView view)
        {
            BuildingPlacementSettings settings = preview.PlacementSystem.Settings;
            if (!settings.PreviewSmoothMovement || !settings.PreviewForceGrounding)
            {
                return spawnPosition;
            }

            Vector3 origin = view.GetOriginTransform().position;
            Transform camera = view.RaycastCamera.transform;
            float maxDistance = view.MaxValidDistance;
            float safeDistance = maxDistance - 0.1f;

            return ApplyGrounding(spawnPosition, settings, origin, camera, safeDistance, maxDistance, view);
        }

        private Vector3 ApplyGrounding(Vector3 position, BuildingPlacementSettings settings, Vector3 origin, Transform camera, float safeDistance, float maxDistance, BuildingView view)
        {
            float distanceFromOrigin = position != Vector3.zero ? view.GetDistance(origin, position) : safeDistance;

            if (position == Vector3.zero)
            {
                position = origin + camera.forward.normalized * safeDistance;
                position.y = camera.position.y;
            }

            if (view.ConstrainValidDistance && distanceFromOrigin > safeDistance)
            {
                position = ClampToDistance(position, origin, safeDistance, view);
            }

            if (settings.PreviewGroundingElevation)
            {
                float elevationThreshold = maxDistance * settings.PreviewGroundingElevationStartRatio;
                float elevation = distanceFromOrigin > elevationThreshold
                    ? settings.PreviewGroundingElevationMaxHeight * Mathf.InverseLerp(elevationThreshold, maxDistance, distanceFromOrigin)
                    : 0f;

                Vector3 boxOrigin = new Vector3(position.x, position.y + settings.PreviewGroundingElevationMaxHeight + 1f, position.z);

                if (PhysicsExtensions.BoxCastNonAlloc(boxOrigin, Vector3.one * 0.01f, Vector3.down, out RaycastHit hit, Mathf.Infinity, settings.PreviewGroundingLayer))
                {
                    position.y = hit.point.y + elevation;
                }
            }
            else if (PhysicsExtensions.RaycastNonAlloc(new Ray(position + Vector3.up * 0.1f, Vector3.down), Mathf.Infinity, out RaycastHit hit, settings.PreviewGroundingLayer))
            {
                position.y = hit.point.y;
            }

            position += settings.PreviewOffsetPosition;

            if (view.GetDistance(origin, position) > maxDistance)
            {
                position = ClampToDistance(position, origin, safeDistance, view);
            }

            return position;
        }

        private static Vector3 ApplyFreeMovement(Vector3 position, BuildingPlacementSettings settings, Vector3 origin, Transform camera, float safeDistance, float maxDistance, BuildingView view)
        {
            if (position == Vector3.zero)
            {
                position = camera.position + camera.forward.normalized * safeDistance;
            }

            position += settings.PreviewOffsetPosition;

            if (view.ConstrainValidDistance && view.GetDistance(origin, position) > maxDistance)
            {
                position = ClampToDistance(position, origin, safeDistance, view);
            }

            return position;
        }

        private static Vector3 ClampToDistance(Vector3 position, Vector3 origin, float maxDistance, BuildingView view)
        {
            Vector3 direction = view.GetDirection(origin, position);
            return direction.sqrMagnitude > 0f ? origin + direction.normalized * maxDistance : position;
        }

        private static Quaternion ApplySurfaceAlignment(Quaternion rotation, Vector3 normal, BuildingPlacementSettings settings)
        {
            normal = normal.normalized;
            Vector3 axis = settings.PreviewSurfaceAlignmentAxis.normalized;
            if (axis.sqrMagnitude <= Mathf.Epsilon)
            {
                axis = Vector3.forward;
            }

            Vector3 up = Vector3.ProjectOnPlane(Vector3.up, normal);
            if (up.sqrMagnitude < 0.0001f)
            {
                up = Vector3.ProjectOnPlane(Vector3.forward, normal);
            }
            up.Normalize();

            Quaternion surfaceRotation = Quaternion.LookRotation(normal, up);
            Quaternion axisCorrection = Quaternion.FromToRotation(axis, Vector3.forward);
            Quaternion aligned = surfaceRotation * axisCorrection;

            if (settings.PreviewClampRotation)
            {
                Vector3 euler = aligned.eulerAngles;
                euler.x = Mathf.Clamp(euler.x, settings.PreviewClampMinRotation.x, settings.PreviewClampMaxRotation.x);
                euler.y = Mathf.Clamp(euler.y, settings.PreviewClampMinRotation.y, settings.PreviewClampMaxRotation.y);
                euler.z = Mathf.Clamp(euler.z, settings.PreviewClampMinRotation.z, settings.PreviewClampMaxRotation.z);
                aligned = Quaternion.Euler(euler);
            }

            return aligned;
        }

        private Vector3 ApplyGrid(Vector3 position, Quaternion rotation, BuildingPlacementSettings settings, BuildingPart preview)
        {
            if (settings.PreviewUseGridSnapping)
            {
                BuildingGridSystem grid = BuildingManager.Instance?.GridSystem;

                if (grid != null && grid.Settings.EnableGrid && grid.HasGeneratedGrid)
                {
                    GridCellData cell = grid.GetCell(position);
                    bool canSnap = cell != null && cell.CellPosition.IsValid()
                        && (!AvoidOccupiedCells || (!cell.IsOccupied && grid.AreOccupancyCellsWithinGrid(preview, cell.CellPosition, rotation)));

                    if (canSnap)
                    {
                        m_lastValidCell = cell;
                        return ApplyClamping(cell.CellPosition, settings);
                    }

                    if (settings.PreviewLockToGrid && m_lastValidCell?.CellPosition.IsValid() == true)
                    {
                        return ApplyClamping(m_lastValidCell.CellPosition, settings);
                    }
                }
            }

            if (settings.PreviewRoundMovement)
            {
                position.x = Mathf.Round(position.x / settings.PreviewRoundCellSizeX) * settings.PreviewRoundCellSizeX;
                position.z = Mathf.Round(position.z / settings.PreviewRoundCellSizeZ) * settings.PreviewRoundCellSizeZ;
            }

            return ApplyClamping(position, settings);
        }

        private static Vector3 ApplyClamping(Vector3 position, BuildingPlacementSettings settings)
        {
            if (!settings.PreviewClampPosition)
            {
                return position;
            }

            return new Vector3(
                Mathf.Clamp(position.x, settings.PreviewClampMinPosition.x, settings.PreviewClampMaxPosition.x),
                Mathf.Clamp(position.y, settings.PreviewClampMinPosition.y, settings.PreviewClampMaxPosition.y),
                Mathf.Clamp(position.z, settings.PreviewClampMinPosition.z, settings.PreviewClampMaxPosition.z));
        }

        private static bool ApplyMovement(BuildingPart preview, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, BuildingPlacementSettings settings, bool isSnapped)
        {
            if (!settings.PreviewSmoothMovement || isSnapped)
            {
                preview.Move(targetPosition, targetRotation, targetScale);
                return false;
            }

            float smooth = 1f - Mathf.Exp(-settings.PreviewMovementSmoothSpeed * Time.deltaTime);

            Vector3 newPosition = Vector3.Lerp(preview.transform.position, targetPosition, smooth);
            Quaternion newRotation = Quaternion.Slerp(preview.transform.rotation, targetRotation, smooth);
            Vector3 newScale = Vector3.Lerp(preview.transform.localScale, targetScale, smooth);

            if (Vector3.Distance(newPosition, targetPosition) <= settings.PreviewSnappingPositionThreshold)
            {
                newPosition = targetPosition;
            }

            if (Quaternion.Angle(newRotation, targetRotation) <= settings.PreviewSnappingRotationThreshold)
            {
                newRotation = targetRotation;
            }

            preview.Move(newPosition, newRotation, newScale);

            return Vector3.Distance(newPosition, targetPosition) > settings.PreviewSnappingPositionThreshold
                || Quaternion.Angle(newRotation, targetRotation) > settings.PreviewSnappingRotationThreshold;
        }
    }
}
