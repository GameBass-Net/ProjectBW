/// <summary>
/// Project : Easy Build System
/// Class : GridCellData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
{
    [Serializable]
    public class GridCellData
    {
        [SerializeField] private Vector3 m_cellPosition;
        [SerializeField] private bool m_isOccupied;

        public Vector3 CellPosition { get => m_cellPosition; set => m_cellPosition = value; }

        public bool IsOccupied { get => m_isOccupied; set => m_isOccupied = value; }

        public GridCellData(Vector3 position, bool isOccupied)
        {
            m_cellPosition = position;
            m_isOccupied = isOccupied;
        }
    }
}