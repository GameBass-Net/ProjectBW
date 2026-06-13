/// <summary>
/// Project : Easy Build System
/// Class : DestroyCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
{
    public class DestroyCommand : BuildingCommand
    {
        private readonly BuildingPart m_targetPart;

        public DestroyCommand(BuildingPart part)
        {
            m_targetPart = part;
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
                UnityEditor.Undo.RecordObject(m_targetPart, "Destroy Building Part");
                m_targetPart.SetState(BuildingPart.BuildingState.Placed);
            }
#endif

            EventPublisher.Publish(new BuildingStateEvent.DestroyedEventArgs(m_targetPart));

            if (Application.isPlaying)
            {
                float delay = 0f;
                BuildingAnimationBehavior animBehavior = m_targetPart.BehaviorSystem?.GetBehavior(typeof(BuildingAnimationBehavior)) as BuildingAnimationBehavior;

                if (animBehavior != null)
                {
                    delay = Mathf.Max(0f, animBehavior.AnimationEndTime - Time.time);
                }

                Object.Destroy(m_targetPart.gameObject, delay);
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(m_targetPart.gameObject);
#else
                Object.DestroyImmediate(m_targetPart.gameObject, true);
#endif
            }
        }
    }
}