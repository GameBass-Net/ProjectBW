/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainTextureRuleData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain.Data
{
    [Serializable]
    public class BuildingTerrainTextureRuleData
    {
        [SerializeField] private TerrainLayer m_terrainLayer;
        [SerializeField, Range(0f, 1f)] private float m_requiredBlend = 1f;

        public TerrainLayer TerrainLayer { get => m_terrainLayer; set => m_terrainLayer = value; }

        public float RequiredBlend { get => m_requiredBlend; set => m_requiredBlend = value; }

        public int GetTextureIndex(TerrainData terrainData)
        {
            if (m_terrainLayer == null || terrainData?.terrainLayers == null)
            {
                return -1;
            }

            for (int i = 0; i < terrainData.terrainLayers.Length; i++)
            {
                if (terrainData.terrainLayers[i] == m_terrainLayer)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}