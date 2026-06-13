/// <summary>
/// Project : Easy Build System
/// Class : BuildingBehavior.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts
{
    public abstract class BuildingBehavior : MonoBehaviour
    {
        [SerializeField] private BuildingPart m_part;
        [SerializeField] private bool m_isDisabled = false;

        private BuildingBehaviorAttribute m_behaviorInfo;

        public BuildingPart Part { get => m_part; set => m_part = value; }

        public bool IsDisabled { get => m_isDisabled; set => m_isDisabled = value; }

        public string Name => BehaviorInfo?.Name ?? GetType().Name;

        public string Description => BehaviorInfo?.Description ?? string.Empty;

        private BuildingBehaviorAttribute BehaviorInfo
        {
            get
            {
                if (m_behaviorInfo == null)
                {
                    m_behaviorInfo = (Attribute.GetCustomAttribute(GetType(),
                        typeof(BuildingBehaviorAttribute)) as BuildingBehaviorAttribute);
                }

                return m_behaviorInfo;
            }
        }

        public virtual void Initialize(BuildingPart part)
        {
            Part = part;
        }

        public virtual void Shutdown() { }
    }
}