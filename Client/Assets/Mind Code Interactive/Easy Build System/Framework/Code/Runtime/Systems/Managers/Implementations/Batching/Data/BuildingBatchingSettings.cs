/// <summary>
/// Project : Pro Build System
/// Class : BuildingProBatchingSettings.cs
/// Namespace : MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching.Data
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Batching.Data
{
    [Serializable]
    public class BuildingBatchingSettings
    {
        [SerializeField] private bool m_enableBatching;
        [SerializeField] private bool m_autoBatching = true;
        [SerializeField] private float m_batchingDistance = 20f;

        public bool EnableBatching { get => m_enableBatching; set => m_enableBatching = value; }
        public bool AutoBatching { get => m_autoBatching; set => m_autoBatching = value; }
        public float BatchingDistance { get => m_batchingDistance; set => m_batchingDistance = value; }
    }
}