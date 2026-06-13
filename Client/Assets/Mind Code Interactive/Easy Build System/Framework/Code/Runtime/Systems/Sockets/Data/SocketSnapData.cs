/// <summary>
/// Project : Easy Build System
/// Class : SocketSnapData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data
{
    [Serializable]
    public class SocketSnapData
    {
        [SerializeField] private BuildingSocket.MatchType m_matchBy;
        [SerializeField, Category("PartCategory")] private string m_category;
        [SerializeField, BuildingPartReference] private string m_partReference;
        [SerializeField] private Vector3 m_positionOffset;
        [SerializeField] private Vector3 m_rotationOffset;
        [SerializeField] private Vector3 m_scaleOffset = Vector3.one;

        public BuildingSocket.MatchType MatchBy { get => m_matchBy; set => m_matchBy = value; }

        public string Category { get => m_category; set => m_category = value; }

        public string PartReference { get => m_partReference; set => m_partReference = value; }

        public Vector3 PositionOffset { get => m_positionOffset; set => m_positionOffset = value; }

        public Vector3 RotationOffset { get => m_rotationOffset; set => m_rotationOffset = value; }

        public Vector3 ScaleOffset { get => m_scaleOffset; set => m_scaleOffset = value; }
    }
}