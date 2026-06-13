/// <summary>
/// Project : Mind Code Interactive
/// Class : CategorySettings.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Linq;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data
{
#if MCI
    [CreateAssetMenu(fileName = "New Category Settings", menuName = "Mind Code Interactive/Common/Category/Category Settings")]
#endif
    public class CategorySettings : Settings<CategorySettings>
    {
        public static CategorySettings Instance => SettingsLoader.GetSettings<CategorySettings>();

        [Header("Category Configuration")]
        [SerializeField] private CategoryType[] m_categoryTypes = Array.Empty<CategoryType>();
        public CategoryType[] CategoryTypes => m_categoryTypes;

        public override Type GetManagerType() => typeof(CategoryManager);

        protected override void OnValidate()
        {
            base.OnValidate();
            ValidateCategoryTypes();
        }

        private void ValidateCategoryTypes()
        {
            if (m_categoryTypes == null)
            {
                return;
            }

            for (int i = 0; i < m_categoryTypes.Length; i++)
            {
                CategoryType categoryType = m_categoryTypes[i];

                if (!string.IsNullOrEmpty(categoryType.Name))
                {
                    categoryType.Name = categoryType.Name.Trim();
                }

                if (categoryType.Categories == null)
                {
                    categoryType.Categories = Array.Empty<string>();
                }
                else
                {
                    categoryType.Categories = categoryType.Categories
                        .Select(category => category?.Trim() ?? string.Empty)
                        .Where(category => !string.IsNullOrEmpty(category))
                        .ToArray();

                    string[] duplicateCategories = categoryType.Categories
                        .GroupBy(category => category)
                        .Where(grouping => grouping.Count() > 1)
                        .Select(grouping => grouping.Key)
                        .ToArray();

                    if (duplicateCategories.Length > 0)
                    {
                        Debug.LogWarning($"Duplicate categories in '{categoryType.Name}': {string.Join(", ", duplicateCategories)}");
                    }
                }

                m_categoryTypes[i] = categoryType;
            }

            string[] duplicateNames = m_categoryTypes
                .Where(categoryType => !string.IsNullOrEmpty(categoryType.Name))
                .GroupBy(categoryType => categoryType.Name)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Key)
                .ToArray();

            if (duplicateNames.Length > 0)
            {
                Debug.LogWarning("Duplicate category type names found: " + string.Join(", ", duplicateNames));
            }
        }
    }
}