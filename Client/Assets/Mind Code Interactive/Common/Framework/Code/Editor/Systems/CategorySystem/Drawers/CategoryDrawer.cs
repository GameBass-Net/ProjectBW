/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.CategorySystem.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.CategorySystem.Drawers
{
    [CustomPropertyDrawer(typeof(CategoryAttribute))]
    public class CategoryDrawer : PropertyDrawer
    {
        private CategorySettings m_cachedSettings;
        private SerializedObject m_cachedSettingsSO;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CategoryAttribute categoryAttribute = (CategoryAttribute)attribute;
            string categoryTypeName = categoryAttribute.CategoryType;

            CategorySettings settings = EnsureSettingsAsset();
            if (settings == null)
            {
                EditorGUI.HelpBox(position, "CategorySettings not found and could not be created.", MessageType.Error);
                return;
            }

            if (m_cachedSettingsSO == null || m_cachedSettings != settings)
            {
                m_cachedSettings = settings;
                m_cachedSettingsSO = new SerializedObject(settings);
            }

            SerializedProperty categoryTypeProperty = EnsureCategoryType(m_cachedSettingsSO, categoryTypeName);
            SerializedProperty categoriesProperty = FindRelativeProperty(categoryTypeProperty, "m_categories", "Categories");

            string[] categoryOptions = BuildCategoryOptions(categoriesProperty);
            int currentIndex = GetOptionIndex(categoryOptions, property.stringValue);

            if (string.IsNullOrEmpty(property.stringValue) && categoryOptions.Length > 0 && categoryOptions[0] != "Edit Category List...")
            {
                property.stringValue = categoryOptions[0];
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, categoryOptions);

            if (EditorGUI.EndChangeCheck())
            {
                string selectedOption = categoryOptions[Mathf.Clamp(selectedIndex, 0, categoryOptions.Length - 1)];
                if (selectedOption == "Edit Category List...")
                {
                    CategoryWindow.ShowWindow(categoryTypeName);
                }
                else
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Change Category");
                    property.stringValue = selectedOption;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        private static string[] BuildCategoryOptions(SerializedProperty categoriesProperty)
        {
            if (categoriesProperty == null || !categoriesProperty.isArray || categoriesProperty.arraySize == 0)
            {
                return new[] { "Edit Category List..." };
            }

            int categoryCount = categoriesProperty.arraySize;
            List<string> optionsList = new List<string>(categoryCount + 1);

            for (int i = 0; i < categoryCount; i++)
            {
                SerializedProperty elementProperty = categoriesProperty.GetArrayElementAtIndex(i);
                if (elementProperty != null && !string.IsNullOrEmpty(elementProperty.stringValue))
                {
                    optionsList.Add(elementProperty.stringValue);
                }
            }

            optionsList.Sort(StringComparer.Ordinal);
            optionsList.Add("Edit Category List...");
            return optionsList.ToArray();
        }

        private static int GetOptionIndex(string[] options, string selectedValue)
        {
            if (options == null || options.Length == 0)
            {
                return 0;
            }

            int foundIndex = Array.IndexOf(options, selectedValue);
            return foundIndex < 0 ? 0 : foundIndex;
        }

        private static SerializedProperty FindRelativeProperty(SerializedProperty rootProperty, params string[] propertyNames)
        {
            if (rootProperty == null || propertyNames == null)
            {
                return null;
            }

            for (int i = 0; i < propertyNames.Length; i++)
            {
                SerializedProperty foundProperty = rootProperty.FindPropertyRelative(propertyNames[i]);
                if (foundProperty != null)
                {
                    return foundProperty;
                }
            }

            return null;
        }

        private static CategorySettings EnsureSettingsAsset()
        {
            CategorySettings settings = CategorySettings.Instance;
            if (settings != null)
            {
                return settings;
            }

#if UNITY_EDITOR
            const string resourcesFolder = "Assets/Resources";
            const string settingsFolder = "Assets/Resources/Settings";
            const string assetPath = "Assets/Resources/Settings/CategorySettings.asset";

            if (!AssetDatabase.IsValidFolder(resourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(settingsFolder))
            {
                AssetDatabase.CreateFolder(resourcesFolder, "Settings");
            }

            CategorySettings loadedSettings = AssetDatabase.LoadAssetAtPath<CategorySettings>(assetPath);
            if (loadedSettings != null)
            {
                return loadedSettings;
            }

            CategorySettings createdSettings = ScriptableObject.CreateInstance<CategorySettings>();
            AssetDatabase.CreateAsset(createdSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return createdSettings;
#else
            return null;
#endif
        }

        private static SerializedProperty EnsureCategoryType(SerializedObject settingsSO, string categoryTypeName)
        {
            SerializedProperty categoryTypesProperty = settingsSO.FindProperty("m_categoryTypes");
            if (categoryTypesProperty == null || !categoryTypesProperty.isArray)
            {
                settingsSO.Update();
                categoryTypesProperty = settingsSO.FindProperty("m_categoryTypes");
            }

            int typeIndex = FindCategoryTypeIndex(categoryTypesProperty, categoryTypeName);
            if (typeIndex >= 0)
            {
                settingsSO.Update();
                return categoryTypesProperty.GetArrayElementAtIndex(typeIndex);
            }

            Undo.RecordObject(settingsSO.targetObject, "Create Category Type");

            int newTypeIndex = categoryTypesProperty.arraySize;
            categoryTypesProperty.InsertArrayElementAtIndex(newTypeIndex);
            SerializedProperty newTypeProperty = categoryTypesProperty.GetArrayElementAtIndex(newTypeIndex);

            SerializedProperty typeNameProperty = FindRelativeProperty(newTypeProperty, "m_name", "Name");
            if (typeNameProperty != null)
            {
                typeNameProperty.stringValue = categoryTypeName;
            }

            SerializedProperty newCategoriesProperty = FindRelativeProperty(newTypeProperty, "m_categories", "Categories");
            if (newCategoriesProperty != null && newCategoriesProperty.isArray)
            {
                newCategoriesProperty.arraySize = 0;
            }

            settingsSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(settingsSO.targetObject);

            settingsSO.Update();
            return categoryTypesProperty.GetArrayElementAtIndex(newTypeIndex);
        }

        private static int FindCategoryTypeIndex(SerializedProperty categoryTypesProperty, string categoryTypeName)
        {
            if (categoryTypesProperty == null || !categoryTypesProperty.isArray)
            {
                return -1;
            }

            for (int i = 0; i < categoryTypesProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = categoryTypesProperty.GetArrayElementAtIndex(i);
                SerializedProperty typeNameProperty = FindRelativeProperty(elementProperty, "m_name", "Name");
                if (typeNameProperty != null && string.Equals(typeNameProperty.stringValue, categoryTypeName, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}