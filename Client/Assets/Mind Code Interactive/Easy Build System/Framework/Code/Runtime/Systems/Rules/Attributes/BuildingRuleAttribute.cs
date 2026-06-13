/// <summary>
/// Project : Easy Build System
/// Class : BuildingRuleAttribute.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Rules.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildingRuleAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public BuildingRuleAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}