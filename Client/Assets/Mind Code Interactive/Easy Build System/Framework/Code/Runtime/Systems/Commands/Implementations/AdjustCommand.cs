/// <summary>
/// Project : Easy Build System
/// Class : AdjustCommand.cs
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
    public class AdjustCommand : BuildingCommand
    {
        private readonly BuildingPart m_targetPart;
        private readonly Vector3 m_newPosition;
        private readonly Quaternion m_newRotation;

        public AdjustCommand(BuildingPart part, Vector3 newPosition, Quaternion newRotation)
        {
            m_targetPart = part;
            m_newPosition = newPosition;
            m_newRotation = newRotation;
        }

        public override void Execute()
        {
            if (m_targetPart == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(m_targetPart.transform, "Adjust Building Part");
            }
#endif

            m_targetPart.transform.position = m_newPosition;
            m_targetPart.transform.rotation = m_newRotation;
            EventPublisher.Publish(new BuildingStateEvent.AdjustedEventArgs(m_targetPart));
        }
    }
}