/// <summary>
/// Project : Easy Build System
/// Class : GridStageData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
{
    [Serializable]
    public class GridStageData
    {
        [SerializeField] private string m_name = "Default";
        [SerializeField, Min(1)] private int m_rows = 10;
        [SerializeField, Min(1)] private int m_columns = 10;
        [SerializeField] private float m_height = 0.001f;
        [SerializeField, Min(0f)] private float m_cellSize = 1f;
        [NonSerialized] public float[] HeightsCache;
        [NonSerialized] public bool[] OccupiedCache;
        [NonSerialized] public GridCellData[] GridCells;

        public string Name { get => m_name; set => m_name = value; }

        public int Rows { get => m_rows; set => m_rows = value; }

        public int Columns { get => m_columns; set => m_columns = value; }

        public float Height { get => m_height; set => m_height = value; }

        public float CellSize { get => m_cellSize; set => m_cellSize = value; }

        public GridCellData GetCell(int x, int z)
        {
            return GridCells == null ||
                x < 0 || z < 0 ||
                x >= Rows || z >= Columns ||
                GridCells.Length != Rows * Columns
                ? null
                : GridCells[x * Columns + z];
        }

        public void SetCell(int x, int z, GridCellData cell)
        {
            if (Rows <= 0 || Columns <= 0)
            {
                return;
            }

            if (x < 0 || z < 0 || x >= Rows || z >= Columns)
            {
                return;
            }

            if (GridCells == null || GridCells.Length != Rows * Columns)
            {
                GridCells = new GridCellData[Rows * Columns];
            }

            GridCells[x * Columns + z] = cell;
        }
    }
}