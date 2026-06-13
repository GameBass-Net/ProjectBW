/// <summary>
/// Project : Easy Build System
/// Class : RenderPipelineUpgradeSnapshot.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines
{
    public sealed class RenderPipelineUpgradeSnapshot : ScriptableObject
    {
        [SerializeField] private DefaultAsset m_urpPackage;
        [SerializeField] private DefaultAsset m_hdrpPackage;

        public DefaultAsset URPPackage => m_urpPackage;
        public DefaultAsset HDRPPackage => m_hdrpPackage;
    }
}