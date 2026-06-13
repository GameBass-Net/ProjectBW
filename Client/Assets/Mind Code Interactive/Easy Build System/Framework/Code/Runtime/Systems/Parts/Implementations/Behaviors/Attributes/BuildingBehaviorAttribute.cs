/// <summary>
/// Project : Easy Build System
/// Class : BuildingBehaviorAttribute.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BuildingBehaviorAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public BuildingBehaviorAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}