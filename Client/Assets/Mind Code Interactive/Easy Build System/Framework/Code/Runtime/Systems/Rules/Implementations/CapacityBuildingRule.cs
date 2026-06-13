/// <summary>
/// Project : Easy Build System
/// Class : CapacityBuildingRule.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
{
    [BuildingRule("Capacity Restriction Rule",
        "Caps how many parts can be placed in this area, with optional per-category quotas.")]
    public class CapacityBuildingRule : BuildingRule
    {
        [Serializable]
        public struct CategoryQuota
        {
            [Category("PartCategory")] public string Category;
            [Min(0)] public int MaxCount;
        }

        [SerializeField, Min(0)] private int m_maxTotalParts;
        [SerializeField] private CategoryQuota[] m_categoryQuotas;

        public int MaxTotalParts { get => m_maxTotalParts; set => m_maxTotalParts = value; }

        public CategoryQuota[] CategoryQuotas { get => m_categoryQuotas; set => m_categoryQuotas = value; }

        public override ConditionResult Validate(BuildingPart part, BuildingMode mode)
        {
            if (mode != BuildingMode.Placement || part == null)
            {
                return new ConditionResult(true);
            }

            BuildingArea area = GetComponentInParent<BuildingArea>();
            if (area == null)
            {
                return new ConditionResult(true);
            }

            List<BuildingPart> registered = area.RegisteredParts;
            int currentTotal = registered != null ? registered.Count : 0;

            if (m_maxTotalParts > 0 && currentTotal >= m_maxTotalParts)
            {
                return new ConditionResult(false,
                    "This area is full (" + currentTotal + " / " + m_maxTotalParts + " parts).");
            }

            if (m_categoryQuotas == null || m_categoryQuotas.Length == 0)
            {
                return new ConditionResult(true);
            }

            for (int i = 0; i < m_categoryQuotas.Length; i++)
            {
                CategoryQuota quota = m_categoryQuotas[i];
                if (string.IsNullOrEmpty(quota.Category) || quota.MaxCount <= 0)
                {
                    continue;
                }

                if (!string.Equals(quota.Category, part.Category))
                {
                    continue;
                }

                int currentCategoryCount = CountByCategory(registered, quota.Category);
                if (currentCategoryCount >= quota.MaxCount)
                {
                    return new ConditionResult(false,
                        "Category '" + quota.Category + "' is full in this area (" +
                        currentCategoryCount + " / " + quota.MaxCount + ").");
                }

                break;
            }

            return new ConditionResult(true);
        }

        private static int CountByCategory(List<BuildingPart> parts, string category)
        {
            if (parts == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                BuildingPart part = parts[i];
                if (part == null)
                {
                    continue;
                }

                if (string.Equals(part.Category, category))
                {
                    count++;
                }
            }

            return count;
        }
    }
}