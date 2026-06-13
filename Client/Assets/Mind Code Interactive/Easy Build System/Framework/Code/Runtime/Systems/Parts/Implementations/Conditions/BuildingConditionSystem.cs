/// <summary>
/// Project : Easy Build System
/// Class : BuildingConditionSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions
{
    [Serializable]
    public class BuildingConditionSystem : BuildingPartSystem
    {
        [SerializeField] private List<BuildingCondition> m_conditions = new List<BuildingCondition>();

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);
            Refresh();
        }

        public override void Shutdown()
        {
            m_conditions.Clear();
            base.Shutdown();
        }

        public BuildingCondition AddCondition(Type conditionType)
        {
            if (!typeof(BuildingCondition).IsAssignableFrom(conditionType))
            {
                Debug.LogError("The type " + conditionType + " does not inherit from BuildingCondition.");
                return null;
            }

            BuildingCondition existing = GetCondition(conditionType);
            if (existing != null)
            {
                return existing;
            }

#if UNITY_EDITOR
            BuildingCondition condition = UnityEditor.Undo.AddComponent(Part.gameObject, conditionType) as BuildingCondition;
#else
            BuildingCondition condition = Part.gameObject.AddComponent(conditionType) as BuildingCondition;
#endif

            if (condition != null)
            {
                condition.hideFlags = HideFlags.HideInInspector;
                if (condition.Part == null)
                {
                    condition.Part = Part;
                }

                m_conditions.Add(condition);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(Part);
#endif
            }

            return condition;
        }

        public bool RemoveCondition(Type conditionType)
        {
            BuildingCondition condition = GetCondition(conditionType);
            if (condition == null)
            {
                return false;
            }

            m_conditions.Remove(condition);

#if UNITY_EDITOR
            UnityEditor.Undo.DestroyObjectImmediate(condition);
            UnityEditor.EditorUtility.SetDirty(Part);
#else
            UnityEngine.Object.DestroyImmediate(condition, true);
#endif

            return true;
        }

        public BuildingCondition GetCondition(Type conditionType)
        {
            for (int i = 0; i < m_conditions.Count; i++)
            {
                BuildingCondition condition = m_conditions[i];
                if (condition != null && condition.GetType() == conditionType)
                {
                    return condition;
                }
            }

            return null;
        }

        public List<BuildingCondition> GetAllConditions()
        {
            return m_conditions;
        }

        public ConditionResult EvaluateConditions(BuildingMode buildMode)
        {
            m_conditions.Sort((a, b) =>
            {
                if (a == null)
                {
                    return 1;
                }

                if (b == null)
                {
                    return -1;
                }

                return a.EvaluationOrder.CompareTo(b.EvaluationOrder);
            });

            for (int i = 0; i < m_conditions.Count; i++)
            {
                BuildingCondition condition = m_conditions[i];
                if (condition == null || condition.IsDisabled)
                {
                    continue;
                }

                ConditionResult result = condition.Evaluate(buildMode);

#if UNITY_EDITOR
                if (condition.ShowLogs && !string.IsNullOrEmpty(result.Reason))
                {
                    if (result.IsValid)
                    {
                        Debug.Log(result.Reason);
                    }
                    else
                    {
                        Debug.LogWarning(result.Reason);
                    }
                }
#endif

                if (!result.IsValid)
                {
                    return result;
                }
            }

            return new ConditionResult(true);
        }

        public void Refresh()
        {
            if (Part == null)
            {
                return;
            }

            BuildingCondition[] existing = Part.GetComponents<BuildingCondition>();
            HashSet<BuildingCondition> existingSet = new HashSet<BuildingCondition>(existing);

            for (int i = 0; i < existing.Length; i++)
            {
                BuildingCondition condition = existing[i];
                if (condition != null && !m_conditions.Contains(condition))
                {
                    if (condition.Part == null)
                    {
                        condition.Part = Part;
                    }

                    m_conditions.Add(condition);
                }
            }

            for (int i = m_conditions.Count - 1; i >= 0; i--)
            {
                if (m_conditions[i] == null || !existingSet.Contains(m_conditions[i]))
                {
                    m_conditions.RemoveAt(i);
                }
            }
        }

        public void DestroyAll()
        {
            foreach (BuildingCondition condition in m_conditions)
            {
                if (!condition)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(condition);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(condition, true);
                }
            }

            m_conditions.Clear();
        }
    }
}