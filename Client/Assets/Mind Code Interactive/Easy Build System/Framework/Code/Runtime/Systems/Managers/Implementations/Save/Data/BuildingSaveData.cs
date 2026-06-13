/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
{
    [Serializable]
    public class BuildingSaveData
    {
        [SerializeField] private string m_saveTime;
        [SerializeField] private string m_sceneName;
        [SerializeField] private int m_buildingCount;
        [SerializeField] private List<BuildingPartData> m_buildingData = new List<BuildingPartData>();

        public DateTime SaveTime
        {
            get => DateTime.TryParse(m_saveTime, out DateTime result) ? result : default;
            set => m_saveTime = value.ToString("o");
        }

        public string SceneName { get => m_sceneName; set => m_sceneName = value; }

        public int BuildingCount { get => m_buildingCount; set => m_buildingCount = value; }

        public List<BuildingPartData> BuildingData { get => m_buildingData; set => m_buildingData = value ?? new List<BuildingPartData>(); }

        public BuildingSaveData()
        {
            m_buildingData = new List<BuildingPartData>();
        }

        public BuildingSaveData(
            DateTime saveTime,
            string sceneName,
            int buildingCount,
            List<BuildingPartData> buildingData)
        {
            SaveTime = saveTime;
            m_sceneName = sceneName;
            m_buildingCount = buildingCount;
            m_buildingData = buildingData ?? new List<BuildingPartData>();
        }
    }
}