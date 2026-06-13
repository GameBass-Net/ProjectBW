/// <summary>
/// Project : Easy Build System
/// Class : CategoryFilterBuildingRule.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
{
    [BuildingRule("Category Restriction Rule", "Limits building actions to parts whose category matches the allowed list.")]
    public class CategoryFilterBuildingRule : BuildingRule
    {
        [SerializeField, Category("PartCategory")] private string[] m_allowedForPlacement;
        [SerializeField, Category("PartCategory")] private string[] m_allowedForDestruction;
        [SerializeField, Category("PartCategory")] private string[] m_allowedForAdjustment;

        public string[] AllowedForPlacement { get => m_allowedForPlacement; set => m_allowedForPlacement = value; }

        public string[] AllowedForDestruction { get => m_allowedForDestruction; set => m_allowedForDestruction = value; }

        public string[] AllowedForAdjustment { get => m_allowedForAdjustment; set => m_allowedForAdjustment = value; }

        public override ConditionResult Validate(BuildingPart part, BuildingMode mode)
        {
            return mode switch
            {
                BuildingMode.Placement => ValidateAllowList(part, m_allowedForPlacement, "placement"),
                BuildingMode.Destruction => ValidateAllowList(part, m_allowedForDestruction, "destruction"),
                BuildingMode.Adjustment => ValidateAllowList(part, m_allowedForAdjustment, "adjustment"),
                _ => new ConditionResult(true)
            };
        }

        private ConditionResult ValidateAllowList(BuildingPart part, string[] allowedTypes, string opLabel)
        {
            string partType = part.Category;

            for (int i = 0; i < (allowedTypes?.Length ?? 0); i++)
            {
                if (partType == allowedTypes[i])
                {
                    return new ConditionResult(true);
                }
            }

            return new ConditionResult(false, "Building type '" + partType + "' is not allowed for " + opLabel + ".");
        }
    }
}