/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes
{
    public class CategoryAttribute : PropertyAttribute
    {
        public string CategoryType { get; private set; }
        public bool Optional { get; private set; }

        public CategoryAttribute(string categoryType, bool optional = false)
        {
            CategoryType = categoryType ?? string.Empty;
            Optional = optional;
        }
    }
}