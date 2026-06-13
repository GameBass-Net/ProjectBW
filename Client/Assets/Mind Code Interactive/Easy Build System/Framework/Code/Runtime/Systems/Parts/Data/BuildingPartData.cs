/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
{
    [Serializable]
    public class BuildingPartData
    {
        [SerializeField] private string m_prefabId;
        [SerializeField] private string m_uniqueId;
        [SerializeField] private Vector3 m_position;
        [SerializeField] private Quaternion m_rotation;
        [SerializeField] private Vector3 m_scale;
        [SerializeField] private BuildingPart.BuildingState m_state;
        [SerializeField] private bool m_hasAttachedSocket;
        [SerializeField] private string m_attachedSocketId;
        [SerializeField] private int m_rendererVariantIndex;
        [SerializeField] private BuildingPartStateData m_customProperties = new BuildingPartStateData();

        public string PrefabId { get => m_prefabId; set => m_prefabId = value; }

        public string UniqueId { get => m_uniqueId; set => m_uniqueId = value; }

        public Vector3 Position { get => m_position; set => m_position = value; }

        public Quaternion Rotation { get => m_rotation; set => m_rotation = value; }

        public Vector3 Scale { get => m_scale; set => m_scale = value; }

        public BuildingPart.BuildingState State { get => m_state; set => m_state = value; }

        public bool HasAttachedSocket { get => m_hasAttachedSocket; set => m_hasAttachedSocket = value; }

        public string AttachedSocketId { get => m_attachedSocketId; set => m_attachedSocketId = value; }

        public int RendererVariantIndex { get => m_rendererVariantIndex; set => m_rendererVariantIndex = value; }

        public BuildingPartStateData CustomProperties => m_customProperties;

        public BuildingPartData(BuildingPart part)
            : this(part,
                   part ? part.transform.position : Vector3.zero,
                   part ? part.transform.rotation : Quaternion.identity,
                   part ? part.transform.localScale : Vector3.one)
        {
        }

        public BuildingPartData(BuildingPart part, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!part)
            {
                return;
            }

            m_prefabId = part.PrefabId;
            m_uniqueId = part.UniqueId;
            m_state = part.State;
            m_position = position;
            m_rotation = rotation;
            m_scale = scale;
            m_hasAttachedSocket = part.AttachedSocket != null;
            m_attachedSocketId = part.AttachedSocket?.UniqueId ?? "";
            m_rendererVariantIndex = part.RendererSystem.ActiveIndex;
        }

        public void ApplyTo(BuildingPart part)
        {
            if (!part)
            {
                return;
            }

            part.SetUniqueId(m_uniqueId);
            part.transform.position = m_position;
            part.transform.rotation = m_rotation;
            part.transform.localScale = m_scale;
            part.SetState(m_state);

            if (m_hasAttachedSocket && !string.IsNullOrEmpty(m_attachedSocketId))
            {
                BuildingSocket socket = BuildingSocket.GetSocketById(m_attachedSocketId);
                if (socket != null)
                {
                    part.SetSocket(socket);
                }
            }

            if (m_rendererVariantIndex > 0)
            {
                part.RendererSystem.SetVariant(m_rendererVariantIndex);
            }
        }
    }
}