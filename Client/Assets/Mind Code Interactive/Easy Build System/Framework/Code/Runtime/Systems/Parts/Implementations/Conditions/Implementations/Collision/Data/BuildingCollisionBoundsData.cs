/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollisionBoundsData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collision.Data
{
    [Serializable]
    public class BuildingCollisionBoundsData
    {
        [SerializeField] private Vector3 m_center = Vector3.zero;
        [SerializeField] private Vector3 m_size = Vector3.one;
        [SerializeField, Range(0f, 2f)] private float m_collisionTolerance = 0.95f;
        [SerializeField, Range(0f, 2f)] private float m_snappingCollisionTolerance = 0.95f;
        [SerializeField] private LayerMask m_collisionLayer = 1 << 0;
        [SerializeField] private bool m_preventOverlapping;
        [SerializeField] private Vector3 m_overlappingCenter = Vector3.zero;
        [SerializeField] private Vector3 m_overlappingSize = Vector3.one;
        [SerializeField, Range(0f, 2f)] private float m_overlappingTolerance = 1f;
        [SerializeField, Category("PartCategory")] private string[] m_ignoreOverlappingTypes;
        [SerializeField] private bool m_requireCollision;
        [SerializeField] private bool m_requireTerrain;
        [SerializeField] private bool m_ignoreNestedCollision;
        [SerializeField, Tag] private string[] m_ignoreTags;
        [SerializeField, Category("PartCategory")] private string[] m_ignoreBuildingTypes;

        public Vector3 Center { get => m_center; set => m_center = value; }

        public Vector3 Size { get => m_size; set => m_size = value; }

        public float CollisionTolerance { get => m_collisionTolerance; set => m_collisionTolerance = value; }

        public float SnappingCollisionTolerance { get => m_snappingCollisionTolerance; set => m_snappingCollisionTolerance = value; }

        public LayerMask CollisionLayer { get => m_collisionLayer; set => m_collisionLayer = value; }

        public bool PreventOverlapping { get => m_preventOverlapping; set => m_preventOverlapping = value; }

        public Vector3 OverlappingCenter { get => m_overlappingCenter; set => m_overlappingCenter = value; }

        public Vector3 OverlappingSize { get => m_overlappingSize; set => m_overlappingSize = value; }

        public float OverlappingTolerance { get => m_overlappingTolerance; set => m_overlappingTolerance = value; }

        public string[] IgnoreOverlappingTypes { get => m_ignoreOverlappingTypes; set => m_ignoreOverlappingTypes = value; }

        public bool RequireCollision { get => m_requireCollision; set => m_requireCollision = value; }

        public bool RequireTerrain { get => m_requireTerrain; set => m_requireTerrain = value; }

        public bool IgnoreNestedCollision { get => m_ignoreNestedCollision; set => m_ignoreNestedCollision = value; }

        public string[] IgnoreTags { get => m_ignoreTags; set => m_ignoreTags = value; }

        public string[] IgnoreBuildingTypes { get => m_ignoreBuildingTypes; set => m_ignoreBuildingTypes = value; }
    }
}