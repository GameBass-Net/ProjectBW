/// <summary>
/// Project : Easy Build System
/// Class : PreviewStateMaterialData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
{
    [Serializable]
    public class PreviewStateMaterialData
    {
        [SerializeField] private BuildingPart.BuildingState m_state;
        [SerializeField] private MaterialMode m_materialMode;
        [SerializeField, ShowIf("m_materialMode", MaterialMode.ReplaceMaterial)] private Material m_customMaterial;
        [SerializeField] private Color m_color = Color.white;
        [SerializeField] private string m_colorPropertyName = "_Color";

        public BuildingPart.BuildingState State { get => m_state; set => m_state = value; }

        public MaterialMode MaterialMode { get => m_materialMode; set => m_materialMode = value; }

        public Material CustomMaterial { get => m_customMaterial; set => m_customMaterial = value; }

        public Color Color { get => m_color; set => m_color = value; }

        public string ColorPropertyName { get => m_colorPropertyName; set => m_colorPropertyName = value; }
    }
}