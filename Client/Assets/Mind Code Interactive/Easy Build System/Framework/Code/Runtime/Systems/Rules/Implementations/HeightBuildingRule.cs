/// <summary>
/// Project : Easy Build System
/// Class : HeightBuildingRule.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Implementations
{
    [BuildingRule("Height Restriction Rule", "Limits building actions to a defined height range on the Y axis.")]
    public class HeightBuildingRule : BuildingRule
    {
        [SerializeField] private Vector2 m_placementHeightRange = new Vector2(0f, 50f);
        [SerializeField] private Vector2 m_destructionHeightRange = new Vector2(0f, 50f);
        [SerializeField] private Vector2 m_adjustmentHeightRange = new Vector2(0f, 50f);

        public Vector2 PlacementHeightRange { get => m_placementHeightRange; set => m_placementHeightRange = value; }
        public Vector2 DestructionHeightRange { get => m_destructionHeightRange; set => m_destructionHeightRange = value; }
        public Vector2 AdjustmentHeightRange { get => m_adjustmentHeightRange; set => m_adjustmentHeightRange = value; }

        public override ConditionResult Validate(BuildingPart part, BuildingMode mode)
        {
            float objHeight = part.transform.position.y;

            return mode switch
            {
                BuildingMode.Placement => ValidatePlacement(objHeight),
                BuildingMode.Destruction => ValidateDestruction(objHeight),
                BuildingMode.Adjustment => ValidateAdjustment(objHeight),
                _ => new ConditionResult(true)
            };
        }

        private ConditionResult ValidatePlacement(float height)
        {
            return height < m_placementHeightRange.x || height > m_placementHeightRange.y
                ? new ConditionResult(false,
                    "Placement height " + height.ToString("F2") + "m is outside the allowed range of " + m_placementHeightRange.x.ToString("F2") + "m–" + m_placementHeightRange.y.ToString("F2") + "m.")
                : new ConditionResult(true);
        }

        private ConditionResult ValidateDestruction(float height)
        {
            return height < m_destructionHeightRange.x || height > m_destructionHeightRange.y
                ? new ConditionResult(false,
                    "Destruction height " + height.ToString("F2") + "m is outside the allowed range of " + m_destructionHeightRange.x.ToString("F2") + "m–" + m_destructionHeightRange.y.ToString("F2") + "m.")
                : new ConditionResult(true);
        }

        private ConditionResult ValidateAdjustment(float height)
        {
            return height < m_adjustmentHeightRange.x || height > m_adjustmentHeightRange.y
                ? new ConditionResult(false,
                    "Adjustment height " + height.ToString("F2") + "m is outside the allowed range of " + m_adjustmentHeightRange.x.ToString("F2") + "m–" + m_adjustmentHeightRange.y.ToString("F2") + "m.")
                : new ConditionResult(true);
        }
    }
}