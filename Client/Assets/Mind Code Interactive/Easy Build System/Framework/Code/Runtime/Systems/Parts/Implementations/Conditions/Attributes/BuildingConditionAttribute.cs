/// <summary>
/// Project : Easy Build System
/// Class : BuildingConditionAttribute.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BuildingConditionAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public bool IsRequired { get; }

        public BuildingConditionAttribute(string name, string description, bool isRequired = false)
        {
            Name = name;
            Description = description;
            IsRequired = isRequired;
        }
    }
}