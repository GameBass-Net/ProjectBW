/// <summary>
/// Project : Easy Build System
/// Class : BuildingBehaviorSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors
{
    [Serializable]
    public class BuildingBehaviorSystem : BuildingPartSystem
    {
        [SerializeField] private List<BuildingBehavior> m_behaviors = new List<BuildingBehavior>();

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);
            Refresh();
        }

        public override void Shutdown()
        {
            foreach (BuildingBehavior behavior in m_behaviors)
            {
                if (behavior && !behavior.IsDisabled)
                {
                    behavior.Shutdown();
                }
            }

            m_behaviors.Clear();
            base.Shutdown();
        }

        public BuildingBehavior AddBehavior(Type behaviorType)
        {
            if (!typeof(BuildingBehavior).IsAssignableFrom(behaviorType))
            {
                Debug.LogError("The type " + behaviorType + " does not inherit from BuildingBehavior.");
                return null;
            }

            BuildingBehavior existing = GetBehavior(behaviorType);
            if (existing)
            {
                return existing;
            }

            BuildingBehavior behavior = Part.gameObject.AddComponent(behaviorType) as BuildingBehavior;
            if (behavior)
            {
                behavior.hideFlags = HideFlags.HideInInspector;
                m_behaviors.Add(behavior);

                if (!behavior.IsDisabled)
                {
                    behavior.Initialize(Part);
                }
            }

            return behavior;
        }

        public bool RemoveBehavior(Type behaviorType)
        {
            BuildingBehavior behavior = GetBehavior(behaviorType);
            if (!behavior)
            {
                return false;
            }

            if (!behavior.IsDisabled)
            {
                behavior.Shutdown();
            }

            m_behaviors.Remove(behavior);

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(behavior);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(behavior, true);
            }

            return true;
        }

        public BuildingBehavior GetBehavior(Type behaviorType)
        {
            for (int i = 0; i < m_behaviors.Count; i++)
            {
                BuildingBehavior b = m_behaviors[i];
                if (b != null && b.GetType() == behaviorType)
                {
                    return b;
                }
            }

            return null;
        }

        public List<BuildingBehavior> GetAllBehaviors()
        {
            return m_behaviors;
        }

        public void Refresh()
        {
            if (Part == null)
            {
                return;
            }

            BuildingBehavior[] existing = Part.GetComponents<BuildingBehavior>();
            HashSet<BuildingBehavior> existingSet = new HashSet<BuildingBehavior>(existing);

            for (int i = 0; i < existing.Length; i++)
            {
                BuildingBehavior behavior = existing[i];
                if (!behavior)
                {
                    continue;
                }

                if (!m_behaviors.Contains(behavior))
                {
                    m_behaviors.Add(behavior);
                }

                if (!behavior.IsDisabled)
                {
                    behavior.Initialize(Part);
                }
            }

            for (int i = m_behaviors.Count - 1; i >= 0; i--)
            {
                if (m_behaviors[i] == null || !existingSet.Contains(m_behaviors[i]))
                {
                    m_behaviors.RemoveAt(i);
                }
            }
        }

        public void DestroyAll()
        {
            foreach (BuildingBehavior behavior in m_behaviors)
            {
                if (!behavior)
                {
                    continue;
                }

                if (!behavior.IsDisabled)
                {
                    behavior.Shutdown();
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(behavior);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(behavior, true);
                }
            }

            m_behaviors.Clear();
        }
    }
}