/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Physics
{
    public class BuildingPhysicsSystem : BuildingManagerSubSystem
    {
        protected BuildingPhysicsSettings m_settings;

        protected readonly List<BuildingCollapseCondition> m_registered = new List<BuildingCollapseCondition>();

        protected float m_nextCheckTime;
        protected int m_currentBatchStart;
        protected bool m_isPaused;

        public BuildingPhysicsSettings Settings => m_settings;
        public int RegisteredCount => m_registered.Count;
        public bool IsPaused { get => m_isPaused; set => m_isPaused = value; }

        public BuildingPhysicsSystem(BuildingManager manager, BuildingPhysicsSettings settings)
        {
            m_manager = manager;
            m_settings = settings;
        }

        public override void Initialize()
        {
            m_registered.Clear();
            m_currentBatchStart = 0;
            m_nextCheckTime = 0f;
        }

        public override void Shutdown()
        {
            m_registered.Clear();
        }

        public override void Update()
        {
            if (!Application.isPlaying || !m_settings.EnablePhysics || m_isPaused)
            {
                return;
            }

            if (m_registered.Count == 0)
            {
                return;
            }

            if (Time.time < m_nextCheckTime)
            {
                return;
            }

            m_nextCheckTime = Time.time + m_settings.CheckInterval;

            ProcessBatch();
        }

        public virtual void Register(BuildingCollapseCondition condition)
        {
            if (condition != null && !m_registered.Contains(condition))
            {
                m_registered.Add(condition);
            }
        }

        public virtual void Unregister(BuildingCollapseCondition condition)
        {
            m_registered.Remove(condition);
        }

        public void Pause() => m_isPaused = true;

        public void Resume() => m_isPaused = false;

        public virtual List<BuildingCollapseCondition> GetFallingParts()
        {
            List<BuildingCollapseCondition> falling = new List<BuildingCollapseCondition>();

            for (int i = 0; i < m_registered.Count; i++)
            {
                BuildingCollapseCondition condition = m_registered[i];
                if (condition != null && condition.IsFalling)
                {
                    falling.Add(condition);
                }
            }

            return falling;
        }

        protected virtual void ProcessBatch()
        {
            CleanupNullEntries();

            int count = m_registered.Count;
            if (count == 0)
            {
                return;
            }

            int batchSize = Mathf.Min(m_settings.MaxChecksPerFrame, count);

            if (m_currentBatchStart >= count)
            {
                m_currentBatchStart = 0;
            }

            int batchEnd = Mathf.Min(m_currentBatchStart + batchSize, count);

            ProcessBatchRange(m_currentBatchStart, batchEnd);

            m_currentBatchStart = batchEnd >= count ? 0 : batchEnd;
        }

        protected virtual void ProcessBatchRange(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                BuildingCollapseCondition condition = m_registered[i];

                if (condition == null || condition.IsDisabled || condition.IsFalling)
                {
                    continue;
                }

                BuildingPart part = condition.Part;

                if (part == null || part.State != BuildingPart.BuildingState.Placed)
                {
                    continue;
                }

                condition.RunSelfCheck();
            }
        }

        protected virtual void CleanupNullEntries()
        {
            for (int i = m_registered.Count - 1; i >= 0; i--)
            {
                if (m_registered[i] == null || m_registered[i].Part == null)
                {
                    m_registered.RemoveAt(i);
                }
            }
        }
    }
}