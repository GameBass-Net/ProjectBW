/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacementSettings.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Helpers;


namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
{
    public enum MaterialMode { OriginalMaterial, ReplaceMaterial }

    public enum MaterialTransitionType { None, Pulse }

    [Serializable]
    public class BuildingPlacementSettings
    {
        [SerializeField] private bool m_enablePreviewMaterial = true;
        [SerializeField]
        private PreviewStateMaterialData[] m_previewStateMaterials = new PreviewStateMaterialData[4]
        {
            new PreviewStateMaterialData { State = BuildingPart.BuildingState.Queue, MaterialMode = MaterialMode.OriginalMaterial, Color = new Color(1, 1, 1, 0.5f), ColorPropertyName = "_Color" },
            new PreviewStateMaterialData { State = BuildingPart.BuildingState.Placement, MaterialMode = MaterialMode.OriginalMaterial, Color = new Color(0, 1, 0, 0.5f), ColorPropertyName = "_Color" },
            new PreviewStateMaterialData { State = BuildingPart.BuildingState.Adjusting, MaterialMode = MaterialMode.OriginalMaterial, Color = new Color(1, 1, 1, 0.5f), ColorPropertyName = "_Color" },
            new PreviewStateMaterialData { State = BuildingPart.BuildingState.Destruction, MaterialMode = MaterialMode.OriginalMaterial, Color = new Color(1, 0, 0, 0.5f), ColorPropertyName = "_Color" }
        };
        [SerializeField] private MaterialTransitionType m_previewMaterialTransition = MaterialTransitionType.None;
        [SerializeField, Range(0f, 1f)] private float m_pulseMinAlpha = 0.2f;
        [SerializeField, Range(0f, 1f)] private float m_pulseMaxAlpha = 0.8f;
        [SerializeField] private float m_pulseFrequency = 1f;
        [SerializeField] private Vector3 m_previewOffsetPosition;
        [SerializeField] private bool m_previewClampPosition;
        [SerializeField] private Vector3 m_previewClampMinPosition;
        [SerializeField] private Vector3 m_previewClampMaxPosition;
        [SerializeField] private bool m_previewSmoothMovement;
        [SerializeField] private float m_previewMovementSmoothSpeed = 10f;
        [SerializeField] private float m_previewSnappingPositionThreshold = 0.1f;
        [SerializeField] private float m_previewSnappingRotationThreshold = 5f;
        [SerializeField] private bool m_previewRoundMovement;
        [SerializeField] private Vector2 m_previewRoundCellSize = Vector2.one;
        [SerializeField] private bool m_previewSurfaceAlignment;
        [SerializeField] private Vector3 m_previewSurfaceAlignmentAxis = Vector3.up;
        [SerializeField] private Vector3 m_previewRotationStep = new Vector3(0, 45, 0);
        [SerializeField] private bool m_previewClampRotation;
        [SerializeField] private Vector3 m_previewClampMinRotation;
        [SerializeField] private Vector3 m_previewClampMaxRotation;
        [SerializeField] private bool m_previewAllowSnappedRotation;
        [SerializeField] private Vector3 m_previewSnappedRotationStep = new Vector3(0, 45, 0);
        [SerializeField] private bool m_previewUseGridSnapping;
        [SerializeField] private Vector2 m_previewCellSize = Vector2.one;
        [SerializeField] private bool m_previewLockToGrid = true;
        [SerializeField] private bool m_previewForceGrounding;
        [SerializeField] private LayerMask m_previewGroundingLayer = 1 << 0;
        [SerializeField] private bool m_previewGroundingElevation;
        [SerializeField, Range(0, 1)] private float m_previewGroundingElevationStartRatio = 0.8f;
        [SerializeField] private float m_previewGroundingElevationMaxHeight = 0.5f;
        [SerializeField] private bool m_previewUseDirectionIndicator;
        [SerializeField] private GameObject m_previewDirectionIndicatorPrefab;
        [SerializeField] private Vector3 m_previewDirectionIndicatorPosition;
        [SerializeField] private Vector3 m_previewDirectionIndicatorRotation;
        [SerializeField] private Vector3 m_previewDirectionIndicatorScale = new Vector3(1, 1, 1);
        [SerializeField] private StateGameObjectData[] m_stateGameObjects;

        public bool EnablePreviewMaterial { get => m_enablePreviewMaterial; set => m_enablePreviewMaterial = value; }

        public PreviewStateMaterialData[] PreviewStateMaterials { get => m_previewStateMaterials; set => m_previewStateMaterials = value; }

        public MaterialTransitionType PreviewMaterialTransition { get => m_previewMaterialTransition; set => m_previewMaterialTransition = value; }

        public float PulseMinAlpha { get => m_pulseMinAlpha; set => m_pulseMinAlpha = value; }

        public float PulseMaxAlpha { get => m_pulseMaxAlpha; set => m_pulseMaxAlpha = value; }

        public float PulseFrequency { get => m_pulseFrequency; set => m_pulseFrequency = value; }

        public Vector3 PreviewOffsetPosition { get => m_previewOffsetPosition; set => m_previewOffsetPosition = value; }

        public bool PreviewClampPosition { get => m_previewClampPosition; set => m_previewClampPosition = value; }

        public Vector3 PreviewClampMinPosition { get => m_previewClampMinPosition; set => m_previewClampMinPosition = value; }

        public Vector3 PreviewClampMaxPosition { get => m_previewClampMaxPosition; set => m_previewClampMaxPosition = value; }

        public bool PreviewSmoothMovement { get => m_previewSmoothMovement; set => m_previewSmoothMovement = value; }

        public float PreviewMovementSmoothSpeed { get => m_previewMovementSmoothSpeed; set => m_previewMovementSmoothSpeed = value; }

        public float PreviewSnappingPositionThreshold { get => m_previewSnappingPositionThreshold; set => m_previewSnappingPositionThreshold = value; }

        public float PreviewSnappingRotationThreshold { get => m_previewSnappingRotationThreshold; set => m_previewSnappingRotationThreshold = value; }

        public bool PreviewRoundMovement { get => m_previewRoundMovement; set => m_previewRoundMovement = value; }

        public float PreviewRoundCellSizeX { get => m_previewRoundCellSize.x; set => m_previewRoundCellSize.x = value; }

        public float PreviewRoundCellSizeZ { get => m_previewRoundCellSize.y; set => m_previewRoundCellSize.y = value; }

        public bool PreviewSurfaceAlignment { get => m_previewSurfaceAlignment; set => m_previewSurfaceAlignment = value; }

        public Vector3 PreviewSurfaceAlignmentAxis { get => m_previewSurfaceAlignmentAxis; set => m_previewSurfaceAlignmentAxis = value; }

        public Vector3 PreviewRotationStep { get => m_previewRotationStep; set => m_previewRotationStep = value; }

        public bool PreviewClampRotation { get => m_previewClampRotation; set => m_previewClampRotation = value; }

        public Vector3 PreviewClampMinRotation { get => m_previewClampMinRotation; set => m_previewClampMinRotation = value; }

        public Vector3 PreviewClampMaxRotation { get => m_previewClampMaxRotation; set => m_previewClampMaxRotation = value; }

        public bool PreviewAllowSnappedRotation { get => m_previewAllowSnappedRotation; set => m_previewAllowSnappedRotation = value; }

        public Vector3 PreviewSnappedRotationStep { get => m_previewSnappedRotationStep; set => m_previewSnappedRotationStep = value; }

        public bool PreviewUseGridSnapping { get => m_previewUseGridSnapping; set => m_previewUseGridSnapping = value; }

        public float PreviewCellSizeX { get => m_previewCellSize.x; set => m_previewCellSize.x = value; }

        public float PreviewCellSizeZ { get => m_previewCellSize.y; set => m_previewCellSize.y = value; }

        public bool PreviewLockToGrid { get => m_previewLockToGrid; set => m_previewLockToGrid = value; }

        public bool PreviewForceGrounding { get => m_previewForceGrounding; set => m_previewForceGrounding = value; }

        public LayerMask PreviewGroundingLayer { get => m_previewGroundingLayer; set => m_previewGroundingLayer = value; }

        public bool PreviewGroundingElevation { get => m_previewGroundingElevation; set => m_previewGroundingElevation = value; }

        public float PreviewGroundingElevationStartRatio { get => m_previewGroundingElevationStartRatio; set => m_previewGroundingElevationStartRatio = value; }

        public float PreviewGroundingElevationMaxHeight { get => m_previewGroundingElevationMaxHeight; set => m_previewGroundingElevationMaxHeight = value; }

        public bool PreviewUseDirectionIndicator { get => m_previewUseDirectionIndicator; set => m_previewUseDirectionIndicator = value; }

        public GameObject PreviewDirectionIndicatorPrefab { get => m_previewDirectionIndicatorPrefab; set => m_previewDirectionIndicatorPrefab = value; }

        public Vector3 PreviewDirectionIndicatorPosition { get => m_previewDirectionIndicatorPosition; set => m_previewDirectionIndicatorPosition = value; }

        public Vector3 PreviewDirectionIndicatorRotation { get => m_previewDirectionIndicatorRotation; set => m_previewDirectionIndicatorRotation = value; }

        public Vector3 PreviewDirectionIndicatorScale { get => m_previewDirectionIndicatorScale; set => m_previewDirectionIndicatorScale = value; }

        public StateGameObjectData[] StateGameObjects { get => m_stateGameObjects; set => m_stateGameObjects = value; }
    }
}