/// <summary>
/// Project : Easy Build System
/// Class : GridModeVisualEntry.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grid.Data
{
    [Serializable]
    public class GridModeVisualEntry
    {
        [SerializeField] private BuildingMode m_mode;
        [SerializeField] private bool m_showGridVisuals = false;

        public BuildingMode Mode { get => m_mode; set => m_mode = value; }

        public bool ShowGridVisuals { get => m_showGridVisuals; set => m_showGridVisuals = value; }
    }
}