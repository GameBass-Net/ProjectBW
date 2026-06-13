/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorPageProvider.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Providers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Providers
{
    public static class EditorPageProvider
    {
        private static List<TreePageData> s_cachedDiscoveredPages;
        private static Dictionary<string, TreePageData> s_categoryDataLookup;

        public static List<TreePageData> DiscoverPages()
        {
            if (s_cachedDiscoveredPages != null)
            {
                return s_cachedDiscoveredPages;
            }

            s_cachedDiscoveredPages = new List<TreePageData>();
            s_categoryDataLookup = new Dictionary<string, TreePageData>();

            DiscoverCategories();
            DiscoverPageLayouts();

            s_cachedDiscoveredPages = s_cachedDiscoveredPages
                .OrderBy(pageItem => pageItem.IsCategory ? GetCategoryOrder(pageItem.Id) : GetPageOrder(pageItem.PageLayoutType))
                .ToList();

            foreach (TreePageData categoryPageData in s_cachedDiscoveredPages.Where(pageItem => pageItem.IsCategory))
            {
                categoryPageData.Children = categoryPageData.Children.OrderBy(pageItem => GetPageOrder(pageItem.PageLayoutType)).ToList();
            }

            return s_cachedDiscoveredPages;
        }

        public static TreePageData FindDefaultSelectedPage()
        {
            List<TreePageData> discoveredPages = DiscoverPages();

            TreePageData defaultPageToSelect = discoveredPages.FirstOrDefault(pageItem => !pageItem.IsCategory && pageItem.IsDefault);
            if (defaultPageToSelect != null)
            {
                return defaultPageToSelect;
            }

            foreach (TreePageData categoryPageData in discoveredPages.Where(pageItem => pageItem.IsCategory))
            {
                defaultPageToSelect = FindDefaultSelectedPageRecursive(categoryPageData);
                if (defaultPageToSelect != null)
                {
                    return defaultPageToSelect;
                }
            }

            return null;
        }

        private static void DiscoverCategories()
        {
            List<Type> categoryTypesWithAttribute = GetTypesWithAttribute<EditorCategoryAttribute>();

            foreach (Type categoryTypeToProcess in categoryTypesWithAttribute)
            {
                EditorCategoryAttribute categoryAttribute = categoryTypeToProcess.GetCustomAttribute<EditorCategoryAttribute>();

                TreePageData categoryPageData = new TreePageData(
                    categoryAttribute.Id,
                    categoryAttribute.DisplayName,
                    categoryAttribute.IconPath,
                    null,
                    true
                );

                s_cachedDiscoveredPages.Add(categoryPageData);
                s_categoryDataLookup[categoryAttribute.Id] = categoryPageData;
            }
        }

        private static void DiscoverPageLayouts()
        {
            List<Type> pageTypesWithAttribute = GetTypesWithAttribute<EditorPageAttribute>();

            foreach (Type pageTypeToProcess in pageTypesWithAttribute)
            {
                EditorPageAttribute pageAttribute = pageTypeToProcess.GetCustomAttribute<EditorPageAttribute>();

                TreePageData pageData = new TreePageData(
                    pageAttribute.Id,
                    pageAttribute.DisplayName,
                    pageAttribute.IconPath,
                    pageTypeToProcess,
                    false,
                    pageAttribute.IsDefault
                );

                if (string.IsNullOrEmpty(pageAttribute.Category))
                {
                    s_cachedDiscoveredPages.Add(pageData);
                }
                else
                {
                    TreePageData parentCategoryData = FindOrCreateCategory(pageAttribute.Category);
                    parentCategoryData.AddChild(pageData);
                }
            }
        }

        private static TreePageData FindOrCreateCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                categoryId = "general";
            }

            if (s_categoryDataLookup.TryGetValue(categoryId, out TreePageData existingCategoryData))
            {
                return existingCategoryData;
            }

            TreePageData newlyCreatedCategoryData = new TreePageData(
                categoryId,
                categoryId.First().ToString().ToUpper() + categoryId.Substring(1),
                null,
                null,
                true
            );

            s_cachedDiscoveredPages.Add(newlyCreatedCategoryData);
            s_categoryDataLookup[categoryId] = newlyCreatedCategoryData;

            return newlyCreatedCategoryData;
        }

        private static List<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            List<Type> collectedTypes = new List<Type>();

            foreach (Assembly assemblyToScan in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    List<Type> assemblyTypesWithAttribute = assemblyToScan
                        .GetTypes()
                        .Where(typeItem => typeItem.GetCustomAttribute<T>() != null)
                        .ToList();

                    collectedTypes.AddRange(assemblyTypesWithAttribute);
                }
                catch (ReflectionTypeLoadException reflectionException)
                {
                    Debug.LogWarning("Could not load types from assembly " + assemblyToScan.FullName + ": " + reflectionException.Message);
                }
            }

            return collectedTypes;
        }

        private static int GetCategoryOrder(string categoryId)
        {
            List<Type> categoryTypesWithAttribute = GetTypesWithAttribute<EditorCategoryAttribute>();
            Type categoryTypeFound = categoryTypesWithAttribute.FirstOrDefault(typeItem => typeItem.GetCustomAttribute<EditorCategoryAttribute>().Id == categoryId);
            EditorCategoryAttribute categoryAttributeFound = categoryTypeFound != null ? categoryTypeFound.GetCustomAttribute<EditorCategoryAttribute>() : null;
            return categoryAttributeFound != null ? categoryAttributeFound.Order : 999;
        }

        private static int GetPageOrder(Type pageLayoutType)
        {
            if (pageLayoutType == null)
            {
                return 999;
            }

            EditorPageAttribute pageAttributeFound = pageLayoutType.GetCustomAttribute<EditorPageAttribute>();
            return pageAttributeFound != null ? pageAttributeFound.Order : 999;
        }

        private static TreePageData FindDefaultSelectedPageRecursive(TreePageData parentPageData)
        {
            foreach (TreePageData childPageData in parentPageData.Children)
            {
                if (!childPageData.IsCategory && childPageData.IsDefault)
                {
                    return childPageData;
                }

                if (childPageData.IsCategory)
                {
                    TreePageData recursiveSearchResult = FindDefaultSelectedPageRecursive(childPageData);
                    if (recursiveSearchResult != null)
                    {
                        return recursiveSearchResult;
                    }
                }
            }

            return null;
        }
    }
}