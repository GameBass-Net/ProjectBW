/// <summary>
/// Project : Easy Build System
/// Class : MaterialSetData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data
{
    [Serializable]
    public class SerializableRendererMaterials
    {
        [SerializeField] private Material[] m_materials = Array.Empty<Material>();

        public Material[] Materials { get => m_materials; set => m_materials = value; }
    }

    [Serializable]
    public sealed class MaterialSetData
    {
        [SerializeField] private SerializableRendererMaterials[] m_rendererMaterials;

        public SerializableRendererMaterials[] RendererMaterials { get => m_rendererMaterials; set => m_rendererMaterials = value; }
    }
}