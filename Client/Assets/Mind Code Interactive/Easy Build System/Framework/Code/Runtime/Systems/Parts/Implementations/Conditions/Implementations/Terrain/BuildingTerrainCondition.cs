/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainCondition.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Terrain
{
    [BuildingCondition("Building Terrain Condition",
        "Blocks placement on restricted terrain textures or near specific tree prototypes.")]
    public class BuildingTerrainCondition : BuildingCondition
    {
        [SerializeField] private bool m_checkTreesProximity;
        [SerializeField] private float m_treesDetectionRadius = 2f;
        [SerializeField] private int[] m_treesDeniedIndex;
        [SerializeField] private bool m_checkTextures;
        [SerializeField] private BuildingTerrainTextureRuleData[] m_deniedTextures;

        public override int EvaluationOrder => 2;

        public bool CheckTreesProximity { get => m_checkTreesProximity; set => m_checkTreesProximity = value; }

        public float TreesDetectionRadius { get => m_treesDetectionRadius; set => m_treesDetectionRadius = value; }

        public int[] TreesDeniedIndex { get => m_treesDeniedIndex; set => m_treesDeniedIndex = value; }

        public bool CheckTextures { get => m_checkTextures; set => m_checkTextures = value; }

        public BuildingTerrainTextureRuleData[] DeniedTextures { get => m_deniedTextures; set => m_deniedTextures = value; }

        protected override ConditionResult EvaluateInternal(BuildingMode mode)
        {
            UnityEngine.Terrain activeTerrain = UnityEngine.Terrain.activeTerrain;

            if (activeTerrain == null)
            {
                return new ConditionResult(true);
            }

            Vector3 partPos = Part.transform.position;
            TerrainData terrainData = activeTerrain.terrainData;

            if (m_checkTreesProximity && m_treesDeniedIndex != null && m_treesDeniedIndex.Length > 0)
            {
                ConditionResult treeCheckResult = CheckTreeProximity(terrainData, activeTerrain, partPos);
                if (!treeCheckResult.IsValid)
                {
                    return treeCheckResult;
                }
            }

            if (m_checkTextures && m_deniedTextures != null && m_deniedTextures.Length > 0)
            {
                ConditionResult textureCheckResult = CheckTextureRules(terrainData, activeTerrain, partPos);
                if (!textureCheckResult.IsValid)
                {
                    return textureCheckResult;
                }
            }

            return new ConditionResult(true);
        }

        protected virtual ConditionResult CheckTreeProximity(TerrainData terrainData, UnityEngine.Terrain activeTerrain, Vector3 partPosition)
        {
            float proximityRadiusSquared = m_treesDetectionRadius * m_treesDetectionRadius;
            Vector3 terrainPos = activeTerrain.transform.position;

            foreach (TreeInstance treeInstance in terrainData.treeInstances)
            {
                if (Array.IndexOf(m_treesDeniedIndex, treeInstance.prototypeIndex) < 0)
                {
                    continue;
                }

                Vector3 treeWorldPos = Vector3.Scale(treeInstance.position, terrainData.size) + terrainPos;
                if ((treeWorldPos - partPosition).sqrMagnitude <= proximityRadiusSquared)
                {
                    return new ConditionResult(false, "Blocked tree nearby (prototype index " + treeInstance.prototypeIndex + ").");
                }
            }

            return new ConditionResult(true);
        }

        protected virtual ConditionResult CheckTextureRules(TerrainData terrainData, UnityEngine.Terrain activeTerrain, Vector3 partPosition)
        {
            if (terrainData == null || activeTerrain == null)
            {
                return new ConditionResult(true);
            }

            int aw = terrainData.alphamapWidth;
            int ah = terrainData.alphamapHeight;

            if (aw <= 0 || ah <= 0 || terrainData.alphamapLayers <= 0)
            {
                return new ConditionResult(true);
            }

            Vector3 tp = activeTerrain.transform.position;
            Vector3 size = terrainData.size;

            float nx = (partPosition.x - tp.x) / size.x;
            float nz = (partPosition.z - tp.z) / size.z;

            if (nx < 0f || nx > 1f || nz < 0f || nz > 1f)
            {
                return new ConditionResult(true);
            }

            int alphamapX = Mathf.FloorToInt(nx * (aw - 1));
            int alphamapZ = Mathf.FloorToInt(nz * (ah - 1));

            if ((uint)alphamapX >= (uint)aw || (uint)alphamapZ >= (uint)ah)
            {
                return new ConditionResult(true);
            }

            float[,,] alphaMap = terrainData.GetAlphamaps(alphamapX, alphamapZ, 1, 1);

            for (int i = 0; i < m_deniedTextures.Length; i++)
            {
                BuildingTerrainTextureRuleData textureRule = m_deniedTextures[i];
                int textureIndex = textureRule.GetTextureIndex(terrainData);

                if ((uint)textureIndex >= (uint)terrainData.alphamapLayers)
                {
                    continue;
                }

                if (alphaMap[0, 0, textureIndex] >= textureRule.RequiredBlend)
                {
                    return new ConditionResult(false, "Terrain texture denied for placement.");
                }
            }

            return new ConditionResult(true);
        }

#if UNITY_EDITOR
        public override void OnDebugRender()
        {
            if (!ShowGizmos || Part == null || IsDisabled || !m_checkTreesProximity)
            {
                return;
            }

            ConditionResult evaluationResult = EvaluateInternal(BuildingMode.Placement);
            Color wireColor = evaluationResult.IsValid ? Color.green : Color.red;
            Color fillColor = new Color(wireColor.r, wireColor.g, wireColor.b, 0.1f);

            DebugRenderer.DrawWireSphere(Part.transform.position, m_treesDetectionRadius, wireColor);
            DebugRenderer.DrawSphere(Part.transform.position, m_treesDetectionRadius, fillColor);
        }
#endif
    }
}