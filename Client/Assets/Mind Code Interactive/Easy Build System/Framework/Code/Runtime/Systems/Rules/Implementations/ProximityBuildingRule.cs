/// <summary>
/// Project : Easy Build System
/// Class : ProximityBuildingRule.cs
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
    [BuildingRule("Proximity Restriction Rule",
        "Constrains building actions based on distance to nearby parts of allowed categories.")]
    public class ProximityBuildingRule : BuildingRule
    {
        [Serializable]
        public struct ProximitySettings
        {
            [Category("PartCategory")] public string[] AllowedCategories;
            [Min(0f)] public float MinDistance;
            [Min(0f)] public float MaxDistance;
        }

        [SerializeField] private ProximitySettings m_placementSettings = new ProximitySettings { MinDistance = 0f, MaxDistance = 0f };
        [SerializeField] private ProximitySettings m_destructionSettings = new ProximitySettings { MinDistance = 0f, MaxDistance = 0f };
        [SerializeField] private ProximitySettings m_adjustmentSettings = new ProximitySettings { MinDistance = 0f, MaxDistance = 0f };

        public ProximitySettings PlacementSettings { get => m_placementSettings; set => m_placementSettings = value; }

        public ProximitySettings DestructionSettings { get => m_destructionSettings; set => m_destructionSettings = value; }

        public ProximitySettings AdjustmentSettings { get => m_adjustmentSettings; set => m_adjustmentSettings = value; }

        public override ConditionResult Validate(BuildingPart part, BuildingMode mode)
        {
            if (part == null)
            {
                return new ConditionResult(true);
            }

            return mode switch
            {
                BuildingMode.Placement => ValidateProximity(part, m_placementSettings, "placement"),
                BuildingMode.Destruction => ValidateProximity(part, m_destructionSettings, "destruction"),
                BuildingMode.Adjustment => ValidateProximity(part, m_adjustmentSettings, "adjustment"),
                _ => new ConditionResult(true)
            };
        }

        private ConditionResult ValidateProximity(BuildingPart part, ProximitySettings settings, string opLabel)
        {
            if (settings.MinDistance <= 0f && settings.MaxDistance <= 0f)
            {
                return new ConditionResult(true);
            }

            BuildingArea area = GetComponentInParent<BuildingArea>();
            if (area == null)
            {
                return new ConditionResult(true);
            }

            List<BuildingPart> registered = area.RegisteredParts;
            if (registered == null || registered.Count == 0)
            {
                return settings.MaxDistance > 0f
                    ? new ConditionResult(false,
                        "No allowed parts within " + settings.MaxDistance.ToString("F2") + "m for " + opLabel + ".")
                    : new ConditionResult(true);
            }

            Vector3 origin = part.transform.position;
            bool anyWithinMax = false;
            float closestDistance = float.MaxValue;
            string closestCategory = string.Empty;

            for (int i = 0; i < registered.Count; i++)
            {
                BuildingPart other = registered[i];
                if (other == null || other == part)
                {
                    continue;
                }

                if (!IsCategoryAllowed(other.Category, settings.AllowedCategories))
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, other.transform.position);

                if (settings.MinDistance > 0f && distance < settings.MinDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCategory = other.Category;
                }

                if (settings.MaxDistance > 0f && distance <= settings.MaxDistance)
                {
                    anyWithinMax = true;
                }
            }

            if (settings.MinDistance > 0f && closestDistance < settings.MinDistance)
            {
                return new ConditionResult(false,
                    "Too close to '" + closestCategory + "' part (" + closestDistance.ToString("F2") +
                    "m < required " + settings.MinDistance.ToString("F2") + "m) for " + opLabel + ".");
            }

            if (settings.MaxDistance > 0f && !anyWithinMax)
            {
                return new ConditionResult(false,
                    "No allowed part within " + settings.MaxDistance.ToString("F2") + "m for " + opLabel + ".");
            }

            return new ConditionResult(true);
        }

        private static bool IsCategoryAllowed(string category, string[] allowed)
        {
            if (allowed == null || allowed.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < allowed.Length; i++)
            {
                if (string.Equals(category, allowed[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
