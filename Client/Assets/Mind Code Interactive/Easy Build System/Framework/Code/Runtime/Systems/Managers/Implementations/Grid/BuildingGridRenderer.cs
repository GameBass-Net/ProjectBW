/// <summary>
/// Project : Easy Build System
/// Class : BuildingGridRenderer.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid
/// Copyright : © 2015 - 2026 Mind Code Inter active
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid
{
    public class BuildingGridRenderer
    {
        private const int MAX_INSTANCES = 1023;

        private BuildingGridSettings m_settings;

        private Mesh m_instancedMesh;
        private Material m_instancedMaterial;
        private MaterialPropertyBlock m_freePropertyBlock;
        private MaterialPropertyBlock m_occupiedPropertyBlock;
        private MaterialPropertyBlock m_highlightPropertyBlock;
        private Matrix4x4 m_meshLocalMatrix = Matrix4x4.identity;
        private bool m_hasMeshLocalMatrix;
        private bool m_instancingInitialized;
        private readonly Matrix4x4[] m_instancedBatchMatrices = new Matrix4x4[MAX_INSTANCES];

        private List<Matrix4x4> m_freeInstanceMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> m_occupiedInstanceMatrices = new List<Matrix4x4>();
        private List<Matrix4x4> m_highlightInstanceMatrices = new List<Matrix4x4>();
        private HashSet<Vector2Int> m_highlightedCells = new HashSet<Vector2Int>();
        private bool m_instancedCellsDirty = true;
        private bool m_gridVisualsVisible = true;

        public bool IsVisible => m_gridVisualsVisible;

        public BuildingGridRenderer(BuildingGridSettings settings)
        {
            m_settings = settings;
        }

        public void ClearVisuals()
        {
            m_freeInstanceMatrices.Clear();
            m_occupiedInstanceMatrices.Clear();
            m_highlightInstanceMatrices.Clear();
            m_highlightedCells.Clear();
            m_instancedCellsDirty = true;
        }

        public void SetVisible(bool visible)
        {
            m_gridVisualsVisible = visible;
        }

        public void SetHighlightedCells(HashSet<Vector2Int> cells)
        {
            m_highlightedCells.Clear();

            if (cells != null)
            {
                foreach (Vector2Int cell in cells)
                {
                    m_highlightedCells.Add(cell);
                }
            }

            m_instancedCellsDirty = true;
        }

        public void UpdateCellColors(HashSet<Vector2Int> cellsToUpdate, GridStageData stage)
        {
            m_instancedCellsDirty = true;
        }

        public void UpdateAllCellColors(GridStageData stage)
        {
            if (stage == null)
            {
                return;
            }

            m_instancedCellsDirty = true;
        }

        public void UpdateInstancedMode(GridStageData stage)
        {
            if (stage == null || !m_instancedCellsDirty)
            {
                return;
            }

            EnsureInstancingInitialized();

            if (!m_instancingInitialized || m_instancedMesh == null || m_instancedMaterial == null)
            {
                return;
            }

            if (m_freePropertyBlock == null)
            {
                m_freePropertyBlock = new MaterialPropertyBlock();
            }

            if (m_occupiedPropertyBlock == null)
            {
                m_occupiedPropertyBlock = new MaterialPropertyBlock();
            }

            if (m_highlightPropertyBlock == null)
            {
                m_highlightPropertyBlock = new MaterialPropertyBlock();
            }

            m_freePropertyBlock.Clear();
            m_occupiedPropertyBlock.Clear();
            m_highlightPropertyBlock.Clear();

            m_freePropertyBlock.SetColor("_Color", m_settings.CellFreeColor);
            m_freePropertyBlock.SetColor("_EmissionColor", m_settings.CellFreeColor);

            m_occupiedPropertyBlock.SetColor("_Color", m_settings.CellOccupiedColor);
            m_occupiedPropertyBlock.SetColor("_EmissionColor", m_settings.CellOccupiedColor);

            m_highlightPropertyBlock.SetColor("_Color", m_settings.CellHighlightColor);
            m_highlightPropertyBlock.SetColor("_EmissionColor", m_settings.CellHighlightColor);

            m_freeInstanceMatrices.Clear();
            m_occupiedInstanceMatrices.Clear();
            m_highlightInstanceMatrices.Clear();

            GridCellData[] cells = stage.GridCells;
            if (cells == null)
            {
                return;
            }

            for (int x = 0; x < stage.Rows; x++)
            {
                for (int z = 0; z < stage.Columns; z++)
                {
                    GridCellData cell = stage.GetCell(x, z);
                    if (cell == null)
                    {
                        continue;
                    }

                    Matrix4x4 cellMatrix = GetCellMatrix(cell);
                    Vector2Int cellIndex = new Vector2Int(x, z);

                    if (m_highlightedCells.Contains(cellIndex))
                    {
                        m_highlightInstanceMatrices.Add(cellMatrix);
                    }
                    else if (cell.IsOccupied)
                    {
                        m_occupiedInstanceMatrices.Add(cellMatrix);
                    }
                    else
                    {
                        m_freeInstanceMatrices.Add(cellMatrix);
                    }
                }
            }

            m_instancedCellsDirty = false;
        }

        public void RenderInstanced()
        {
            if (!m_gridVisualsVisible || !m_instancingInitialized)
            {
                return;
            }

            DrawBatch(m_freeInstanceMatrices, m_freePropertyBlock);
            DrawBatch(m_occupiedInstanceMatrices, m_occupiedPropertyBlock);
            DrawBatch(m_highlightInstanceMatrices, m_highlightPropertyBlock);
        }

        public void MarkDirty()
        {
            m_instancedCellsDirty = true;
        }

        private void DrawBatch(List<Matrix4x4> matrices, MaterialPropertyBlock propertyBlock)
        {
            int count = matrices.Count;
            int index = 0;

            while (index < count)
            {
                int batchCount = Mathf.Min(MAX_INSTANCES, count - index);

                for (int i = 0; i < batchCount; i++)
                {
                    m_instancedBatchMatrices[i] = matrices[index + i];
                }

                Graphics.DrawMeshInstanced(
                    m_instancedMesh,
                    0,
                    m_instancedMaterial,
                    m_instancedBatchMatrices,
                    batchCount,
                    propertyBlock);

                index += batchCount;
            }
        }

        private void EnsureInstancingInitialized()
        {
            if (m_instancingInitialized || m_settings.CellVisualPrefab == null)
            {
                return;
            }

            MeshRenderer prefabRenderer = m_settings.CellVisualPrefab.GetComponentInChildren<MeshRenderer>();
            MeshFilter meshFilter = m_settings.CellVisualPrefab.GetComponentInChildren<MeshFilter>();

            if (prefabRenderer == null || meshFilter == null)
            {
                return;
            }

            if (prefabRenderer.sharedMaterial == null || meshFilter.sharedMesh == null)
            {
                return;
            }

            m_instancedMesh = meshFilter.sharedMesh;
            m_instancedMaterial = prefabRenderer.sharedMaterial;

            if (!m_instancedMaterial.enableInstancing)
            {
                Debug.LogWarning("[BuildingGridRenderer] GPU Instancing is not enabled on material '" + m_instancedMaterial.name + "'. Enable it in the material inspector or the grid will not render.");
                m_instancedMaterial.enableInstancing = true;
            }

            m_freePropertyBlock = new MaterialPropertyBlock();
            m_occupiedPropertyBlock = new MaterialPropertyBlock();
            m_highlightPropertyBlock = new MaterialPropertyBlock();

            Transform meshTransform = meshFilter.transform;
            m_meshLocalMatrix = Matrix4x4.TRS(
                meshTransform.localPosition,
                meshTransform.localRotation,
                meshTransform.localScale);
            m_hasMeshLocalMatrix = true;

            m_instancingInitialized = true;
        }

        private Matrix4x4 GetCellMatrix(GridCellData cell)
        {
            Matrix4x4 rootMatrix = Matrix4x4.TRS(
                cell.CellPosition,
                Quaternion.identity,
                Vector3.one * m_settings.CellVisualSize);

            if (m_hasMeshLocalMatrix)
            {
                return rootMatrix * m_meshLocalMatrix;
            }

            return rootMatrix;
        }
    }
}