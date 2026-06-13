/// <summary>
/// Project : Easy Build System
/// Class : BuildingCommonCondition.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations
{
    [BuildingCondition("Building Common Condition",
        "Controls which building actions are allowed on this Building Part and validates socket attachment and area rules.", true)]
    public class BuildingCommonCondition : BuildingCondition
    {
        [SerializeField] private bool m_enablePlacement = true;
        [SerializeField] private bool m_enableDestruction = true;
        [SerializeField] private bool m_enableAdjustment = true;
        [SerializeField] private bool m_enableUpgrading = true;
        [SerializeField] private bool m_requireSocket;
        [SerializeField] private bool m_ignoreAreas;

        protected BuildingArea m_cachedArea;
        protected int m_lastAreaCheckFrame = -1;

        public override int EvaluationOrder => 4;

        public bool EnablePlacement { get => m_enablePlacement; set => m_enablePlacement = value; }

        public bool EnableDestruction { get => m_enableDestruction; set => m_enableDestruction = value; }

        public bool EnableAdjustment { get => m_enableAdjustment; set => m_enableAdjustment = value; }

        public bool EnableUpgrading { get => m_enableUpgrading; set => m_enableUpgrading = value; }

        public bool RequireSocket { get => m_requireSocket; set => m_requireSocket = value; }

        public bool IgnoreAreas { get => m_ignoreAreas; set => m_ignoreAreas = value; }

        protected override ConditionResult EvaluateInternal(BuildingMode buildMode)
        {
            bool isModeEnabled = false;
            bool shouldCheckSocket = false;

            if (buildMode == BuildingMode.Placement)
            {
                isModeEnabled = m_enablePlacement;
                shouldCheckSocket = true;
            }
            else if (buildMode == BuildingMode.Destruction)
            {
                isModeEnabled = m_enableDestruction;
            }
            else if (buildMode == BuildingMode.Adjustment)
            {
                isModeEnabled = m_enableAdjustment;
                shouldCheckSocket = true;
            }
            else if (buildMode == BuildingMode.Upgrade)
            {
                isModeEnabled = m_enableUpgrading;
            }

            if (!isModeEnabled)
            {
                return new ConditionResult(false, buildMode + " is disabled for this building part.");
            }

            if (shouldCheckSocket && m_requireSocket && Part.AttachedSocket == null)
            {
                return new ConditionResult(false, "Socket connection required for this building part.");
            }

            if (m_ignoreAreas)
            {
                return new ConditionResult(true);
            }

            return CheckAreaRules(buildMode);
        }

        protected virtual ConditionResult CheckAreaRules(BuildingMode buildMode)
        {
            if (m_cachedArea == null || Time.frameCount - m_lastAreaCheckFrame > 30)
            {
                m_cachedArea = BuildingManager.Instance?.GetAreaForPart(Part);
                m_lastAreaCheckFrame = Time.frameCount;
            }

            return m_cachedArea == null ? new ConditionResult(true) : m_cachedArea.ValidateRules(Part, buildMode);
        }
    }
}