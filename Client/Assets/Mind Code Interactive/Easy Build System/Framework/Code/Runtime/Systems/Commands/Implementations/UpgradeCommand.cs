/// <summary>
/// Project : Easy Build System
/// Class : UpgradeCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
{
    public class UpgradeCommand : BuildingCommand
    {
        private readonly BuildingPart m_targetPart;
        private readonly int m_upgradeIndex;

        public UpgradeCommand(BuildingPart part, int upgradeIndex)
        {
            m_targetPart = part;
            m_upgradeIndex = upgradeIndex;
        }

        public override void Execute()
        {
            if (m_targetPart == null || m_targetPart.RendererSystem == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(m_targetPart, "Upgrade Building Part");
            }
#endif

            m_targetPart.RendererSystem.SetVariant(Mathf.Clamp(m_upgradeIndex, 0, m_targetPart.RendererSystem.Count - 1));
            EventPublisher.Publish(new BuildingStateEvent.UpgradedEventArgs(m_targetPart));
        }
    }
}