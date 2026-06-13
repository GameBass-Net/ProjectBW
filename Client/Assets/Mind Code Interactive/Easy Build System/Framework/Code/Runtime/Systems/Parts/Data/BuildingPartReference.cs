/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartReference.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
{
    [Serializable]
    public class BuildingPartReference
    {
        [SerializeField] private string m_category;
        [SerializeField] private BuildingPart[] m_buildingParts;

        public string Category => m_category;

        public BuildingPart[] BuildingParts => m_buildingParts;

        public BuildingPartReference(string category, List<BuildingPart> parts)
        {
            m_category = category;
            m_buildingParts = parts != null ? parts.ToArray() : Array.Empty<BuildingPart>();
        }
    }
}