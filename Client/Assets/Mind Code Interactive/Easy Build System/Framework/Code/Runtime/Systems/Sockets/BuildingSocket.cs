/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocket.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets
{
    [ExecuteAlways]
    public class BuildingSocket : RegisterableUniqueObject, IDebuggable
    {
        public enum MatchType { Reference, Category }

        [SerializeField, Category("SocketType")] private string m_socketType;
        [SerializeField, Range(0, 100)] private int m_socketProperty;
        [SerializeField] private float m_socketRadius = 0.25f;
        [SerializeField] private List<SocketSnapData> m_snappingPoints = new List<SocketSnapData>();
        [SerializeField] protected BuildingPart m_previewPart;
        [SerializeField] protected BuildingPart m_attachedPart;

        private SphereCollider m_socketCollider;
        private BuildingPart m_cachedParentPart;

        public Vector3 Position => transform.position;

        public string SocketType { get => m_socketType; set => m_socketType = value; }

        public int SocketProperty { get => m_socketProperty; set => m_socketProperty = value; }

        public float SocketRadius { get => m_socketRadius; set => m_socketRadius = value; }

        public List<SocketSnapData> SnappingPoints { get => m_snappingPoints; set => m_snappingPoints = value; }

        public BuildingPart PreviewPart { get => m_previewPart; set => m_previewPart = value; }

        public BuildingPart AttachedPart => m_attachedPart;

        public BuildingPart ParentPart
        {
            get
            {
                if (m_cachedParentPart == null)
                {
                    m_cachedParentPart = gameObject.GetComponentInParent<BuildingPart>();
                }

                return m_cachedParentPart;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            BuildingManager manager = BuildingManager.Instance;

            if (manager != null && manager.SocketDetectionType == BuildingManager.DetectionType.Physics_Based)
            {
                m_socketCollider = gameObject.GetComponent<SphereCollider>();

                if (m_socketCollider == null)
                {
                    m_socketCollider = gameObject.AddComponent<SphereCollider>();
                    m_socketCollider.isTrigger = true;
                    m_socketCollider.hideFlags = HideFlags.HideAndDontSave;
                }

                m_socketCollider.radius = m_socketRadius;
                gameObject.layer = manager.SocketLayer.ToLayerIndex();
            }
            else if (m_socketCollider != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(m_socketCollider);
#else
                Destroy(m_socketCollider);
#endif
                m_socketCollider = null;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Register(this);
            }

            if (m_socketCollider != null)
            {
                m_socketCollider.enabled = true;
                m_socketCollider.hideFlags = HideFlags.HideInInspector;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Unregister(this);
            }

            if (m_socketCollider != null)
            {
                m_socketCollider.enabled = false;
            }
        }

        public bool HasPreview() => m_previewPart != null;

        public bool IsEnabled() => enabled;

        public virtual bool IsFitting(BuildingPart part) => GetOffset(part) != null;

        public virtual bool MatchesAnyRequiredType(string[] requiredTypes)
        {
            return requiredTypes == null || requiredTypes.Length == 0 ||
                   Array.Exists(requiredTypes, t => t == m_socketType);
        }

        public virtual SocketSnapResult GetSnappingPoint(BuildingPart part)
        {
            SocketSnapData snappingPoint = GetOffset(part);

            if (snappingPoint == null)
            {
                return SocketSnapResult.Invalid;
            }

            return new SocketSnapResult(
                true,
                transform.TransformPoint(snappingPoint.PositionOffset),
                transform.rotation * Quaternion.Euler(snappingPoint.RotationOffset),
                snappingPoint.ScaleOffset,
                snappingPoint.PositionOffset,
                Quaternion.Euler(snappingPoint.RotationOffset),
                snappingPoint.ScaleOffset);
        }

        public virtual SocketSnapData GetOffset(BuildingPart part)
        {
            if (part == null)
            {
                return null;
            }

            for (int i = 0; i < m_snappingPoints.Count; i++)
            {
                SocketSnapData data = m_snappingPoints[i];

                if (data.MatchBy == MatchType.Reference && part.PrefabId == data.PartReference)
                {
                    return data;
                }

                if (data.MatchBy == MatchType.Category && part.Category == data.Category)
                {
                    return data;
                }
            }

            return null;
        }

        public virtual BuildingPart CreatePreview(BuildingPart part)
        {
            BuildingPart preview = BuildingManager.Instance.CreatePreview(part);

            preview.SetSocket(this);
            preview.Move(GetOffset(part), transform);

            m_previewPart = preview;

            return preview;
        }

        public virtual void ClearPreview()
        {
            if (m_previewPart != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(m_previewPart.gameObject);
#else
                Destroy(m_previewPart.gameObject);
#endif
                m_previewPart = null;
            }
        }

        public static BuildingSocket GetSocketById(string socketId)
        {
            if (string.IsNullOrEmpty(socketId) || BuildingManager.Instance == null)
            {
                return null;
            }

            foreach (BuildingSocket socket in BuildingManager.Instance.GetRegisteredSockets)
            {
                if (socket != null && socket.UniqueId == socketId)
                {
                    return socket;
                }
            }

            return null;
        }

        public virtual void SetAttachedPart(BuildingPart part)
        {
            if (m_attachedPart == part)
            {
                return;
            }

            m_attachedPart = part;
        }

        public virtual void ClearAttachedPart()
        {
            SetAttachedPart(null);
        }

        public bool HasAttachedPart() => m_attachedPart != null;

        #region IDebuggable

        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.None;

        public bool DebugEnabled => isActiveAndEnabled;

        public DebugRenderer.ViewFlags DebugFlags
        {
            get => m_debugFlags;
            set => m_debugFlags = value;
        }

        public bool RequireSelection => true;

        public virtual void OnDebugRender()
        {
            Color wireColor = new Color(0f, 1f, 1f, 1f);
            Color fillColor = new Color(0f, 1f, 1f, 0.025f);
            Vector3 center = transform.position;

            DebugRenderer.DrawWireSphere(center, m_socketRadius, wireColor, 24, 0f, 1f, false);
            DebugRenderer.DrawSphere(center, m_socketRadius, fillColor, 8, 0f, false);
        }

        #endregion
    }
}