/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem
{
    [AddComponentMenu("")]
    public class CategoryManager : Manager<CategorySettings>
    {
        public static CategoryManager Instance => ManagerLocator.GetManager<CategoryManager>();

        protected override void OnInitialize()
        {
            if (Settings == null)
            {
                Debug.LogWarning("No settings assigned to CategoryManager");
            }
        }

        public CategoryType GetCategoryType(string name)
        {
            if (!TryGetCategoryType(name, out CategoryType categoryType))
            {
                Debug.LogWarning($"Category type '{name}' not found");
            }

            return categoryType;
        }

        public bool TryGetCategoryType(string name, out CategoryType categoryType)
        {
            categoryType = default;

            if (Settings == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            for (int i = 0; i < Settings.CategoryTypes.Length; i++)
            {
                if (string.Equals(Settings.CategoryTypes[i].Name, name, StringComparison.Ordinal))
                {
                    categoryType = Settings.CategoryTypes[i];
                    return true;
                }
            }

            return false;
        }

        public string[] GetCategories(string typeName)
        {
            if (TryGetCategoryType(typeName, out CategoryType categoryType))
            {
                return categoryType.Categories ?? Array.Empty<string>();
            }

            return Array.Empty<string>();
        }

        public bool HasCategory(string typeName, string category)
        {
            if (TryGetCategoryType(typeName, out CategoryType categoryType))
            {
                return categoryType.Contains(category);
            }

            return false;
        }
    }
}