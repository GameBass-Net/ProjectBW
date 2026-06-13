/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveMetaData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
{
    [Serializable]
    public class BuildingSaveMetaData
    {
        [SerializeField] protected bool m_hasData;
        [SerializeField] protected string m_sceneName;
        [SerializeField] protected string m_saveTime;
        [SerializeField] protected string m_saverVersion;
        [SerializeField] protected int m_buildingCount;
        [SerializeField] protected List<string> m_usedPrefabIds = new List<string>();

        public bool HasData { get => m_hasData; set => m_hasData = value; }

        public string SceneName { get => m_sceneName; set => m_sceneName = value; }

        public DateTime SaveTime
        {
            get => DateTime.TryParse(m_saveTime, out DateTime result) ? result : default;
            set => m_saveTime = value.ToString("o");
        }

        public string SaverVersion { get => m_saverVersion; set => m_saverVersion = value; }

        public int BuildingCount { get => m_buildingCount; set => m_buildingCount = value; }

        public List<string> UsedPrefabIds
        {
            get => m_usedPrefabIds;
            set => m_usedPrefabIds = value ?? new List<string>();
        }

        public BuildingSaveMetaData()
        {
            m_usedPrefabIds = new List<string>();
        }

        public BuildingSaveMetaData(
            bool hasData,
            string sceneName,
            DateTime saveTime,
            string saverVersion,
            int buildingCount,
            List<string> usedPrefabIds)
        {
            m_hasData = hasData;
            m_sceneName = sceneName;
            SaveTime = saveTime;
            m_saverVersion = saverVersion;
            m_buildingCount = buildingCount;
            m_usedPrefabIds = usedPrefabIds ?? new List<string>();
        }
    }
}