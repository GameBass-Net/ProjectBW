using System;
using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Terrain.Data
{
    [Serializable]
    public class BuildingTerrainBackupData
    {
        [SerializeField] private float[,] m_heights;
        [SerializeField] private float[,,] m_alphaMaps;
        [SerializeField] private Dictionary<int, int[,]> m_detailLayers = new Dictionary<int, int[,]>();
        [SerializeField] private TreeInstance[] m_treeInstances;
        [SerializeField] private TreePrototype[] m_treePrototypes;
        [SerializeField] private DetailPrototype[] m_detailPrototypes;

        public float[,] Heights { get => m_heights; set => m_heights = value; }

        public float[,,] AlphaMaps { get => m_alphaMaps; set => m_alphaMaps = value; }

        public Dictionary<int, int[,]> DetailLayers { get => m_detailLayers; set => m_detailLayers = value; }

        public TreeInstance[] TreeInstances { get => m_treeInstances; set => m_treeInstances = value; }

        public TreePrototype[] TreePrototypes { get => m_treePrototypes; set => m_treePrototypes = value; }

        public DetailPrototype[] DetailPrototypes { get => m_detailPrototypes; set => m_detailPrototypes = value; }

        public BuildingTerrainBackupData(UnityEngine.Terrain terrain)
        {
            TerrainData td = terrain.terrainData;
            m_heights = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);
            m_alphaMaps = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
            for (int i = 0; i < td.detailPrototypes.Length; i++)
            {
                m_detailLayers.Add(i, td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, i));
            }

            m_treeInstances = td.treeInstances;
            m_treePrototypes = td.treePrototypes;
            m_detailPrototypes = td.detailPrototypes;
        }
    }
}