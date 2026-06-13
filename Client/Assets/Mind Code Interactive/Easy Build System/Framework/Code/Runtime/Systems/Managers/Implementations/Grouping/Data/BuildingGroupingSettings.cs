/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupingSettings.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Grouping.Data
{
    [Serializable]
    public class BuildingGroupingSettings
    {
        [SerializeField] private bool m_enablePartsGrouping = true;
        [SerializeField] private float m_groupPartNeighborDistance = 5f;

        public bool EnableGrouping { get => m_enablePartsGrouping; set => m_enablePartsGrouping = value; }

        public float GroupPartNeighborDistance { get => m_groupPartNeighborDistance; set => m_groupPartNeighborDistance = value; }

        [SerializeField] private GroupPivotMode m_defaultPivotMode = GroupPivotMode.Center;

        public GroupPivotMode DefaultPivotMode { get => m_defaultPivotMode; set => m_defaultPivotMode = value; }
    }
}