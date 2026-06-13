/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryType.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Linq;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data
{
    [Serializable]
    public struct CategoryType
    {
        public string Name;
        public string[] Categories;

        public int Count => Categories?.Length ?? 0;

        public bool Contains(string category)
        {
            if (string.IsNullOrEmpty(category) || Categories == null)
            {
                return false;
            }

            return Categories.Contains(category);
        }

        public override bool Equals(object obj) => obj is CategoryType other && Name == other.Name;

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public static bool operator ==(CategoryType left, CategoryType right) => left.Equals(right);

        public static bool operator !=(CategoryType left, CategoryType right) => !left.Equals(right);
    }
}