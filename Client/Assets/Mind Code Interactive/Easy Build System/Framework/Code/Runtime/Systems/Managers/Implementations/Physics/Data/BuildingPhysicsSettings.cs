/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsSettings.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics.Data
{
    [Serializable]
    public class BuildingPhysicsSettings
    {
        [SerializeField] private bool m_enablePhysics = true;
        [SerializeField, Range(0.05f, 1f)] private float m_checkInterval = 0.1f;
        [SerializeField, Min(1)] private int m_maxChecksPerFrame = 50;

        public bool EnablePhysics { get => m_enablePhysics; set => m_enablePhysics = value; }
        public float CheckInterval { get => m_checkInterval; set => m_checkInterval = Mathf.Clamp(value, 0.05f, 1f); }
        public int MaxChecksPerFrame { get => m_maxChecksPerFrame; set => m_maxChecksPerFrame = Mathf.Max(1, value); }
    }
}