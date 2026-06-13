/// <summary>
/// Project : Easy Build System
/// Class : BuildingRule.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Abstracts
{
    public abstract class BuildingRule : MonoBehaviour
    {
        [SerializeField] protected bool m_enabled = true;

        private BuildingRuleAttribute m_ruleInfo;

        public bool Enabled { get => m_enabled; set => m_enabled = value; }

        public string RuleName => RuleInfo != null ? RuleInfo.Name : GetType().Name;

        public string RuleDescription => RuleInfo != null ? RuleInfo.Description : string.Empty;

        private BuildingRuleAttribute RuleInfo
        {
            get
            {
                if (m_ruleInfo == null)
                {
                    m_ruleInfo = (BuildingRuleAttribute)Attribute.GetCustomAttribute(GetType(), typeof(BuildingRuleAttribute));
                }

                return m_ruleInfo;
            }
        }

        public abstract ConditionResult Validate(BuildingPart part, BuildingMode mode);
    }
}