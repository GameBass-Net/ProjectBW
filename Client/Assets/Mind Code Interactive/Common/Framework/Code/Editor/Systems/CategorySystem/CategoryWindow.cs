/// <summary>
/// Project : Mind Code Interactive
/// Class : CategoryWindow.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.CategorySystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.CategorySystem.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.CategorySystem
{
    public class CategoryWindow : BaseWindowEditor
    {
        private static string s_typeName;
        private SerializedObject m_settingsSO;
        private SerializedProperty m_categoriesProp;
        private ReorderableList.Code.Editor.ReorderableList m_list;
        private Vector2 m_scrollPosition;

        public static void ShowWindow(string typeName)
        {
            s_typeName = typeName;

            CategoryWindow categoryWindow = CreateInstance<CategoryWindow>();
            categoryWindow.titleContent = new GUIContent("Edit Category List...");
            categoryWindow.minSize = new Vector2(450f, 260f);
            categoryWindow.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            CategorySettings settings = EnsureSettingsAsset();
            if (settings == null)
            {
                return;
            }

            m_settingsSO = new SerializedObject(settings);

            SerializedProperty typeProp = EnsureCategoryType(m_settingsSO, s_typeName);
            m_categoriesProp = FindRelativeProperty(typeProp, "m_categories", "Categories");

            if (m_categoriesProp != null && m_categoriesProp.isArray)
            {
                m_list = new ReorderableList.Code.Editor.ReorderableList(m_categoriesProp, false);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Apply();
        }

        private void OnGUI()
        {
            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(5f);
                GUILayout.Label("Existing '" + s_typeName + "' Categories :", EditorStyles.boldLabel);
                GUILayout.Space(5f);

                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.ExpandHeight(true));

                if (m_settingsSO != null)
                {
                    m_settingsSO.Update();
                }

                if (m_list != null && m_list.Native != null && m_list.Native.serializedProperty != null)
                {
                    m_list.Layout();
                }
                else
                {
                    EditorGUILayout.HelpBox("Categories property not found. Check CategoryType field names.", MessageType.Error);
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Close"))
                {
                    Close();
                }

                EditorGUIExtended.InspectorBottom();
            }

            Apply();
            Repaint();
        }

        private void Apply()
        {
            if (m_settingsSO == null || m_settingsSO.targetObject == null)
            {
                return;
            }

            m_settingsSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_settingsSO.targetObject);
        }

        private static CategorySettings EnsureSettingsAsset()
        {
            CategorySettings settings = CategorySettings.Instance;
            if (settings != null)
            {
                return settings;
            }

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

            CategorySettings createdSettings = AssetDatabase.LoadAssetAtPath<CategorySettings>(assetPath);
            if (createdSettings != null)
            {
                return createdSettings;
            }

            createdSettings = CreateInstance<CategorySettings>();
            AssetDatabase.CreateAsset(createdSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return createdSettings;
        }

        private static SerializedProperty EnsureCategoryType(SerializedObject settingsSO, string typeName)
        {
            settingsSO.Update();

            SerializedProperty categoryTypesProperty = settingsSO.FindProperty("m_categoryTypes");
            if (categoryTypesProperty == null || !categoryTypesProperty.isArray)
            {
                return null;
            }

            int foundIndex = -1;
            for (int i = 0; i < categoryTypesProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = categoryTypesProperty.GetArrayElementAtIndex(i);
                SerializedProperty typeNameProperty = FindRelativeProperty(elementProperty, "m_name", "Name");
                if (typeNameProperty != null && string.Equals(typeNameProperty.stringValue, typeName, StringComparison.Ordinal))
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex >= 0)
            {
                return categoryTypesProperty.GetArrayElementAtIndex(foundIndex);
            }

            Undo.RecordObject(settingsSO.targetObject, "Create Category Type");

            int newIndex = categoryTypesProperty.arraySize;
            categoryTypesProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newElementProperty = categoryTypesProperty.GetArrayElementAtIndex(newIndex);
            SerializedProperty newTypeNameProperty = FindRelativeProperty(newElementProperty, "m_name", "Name");
            if (newTypeNameProperty != null)
            {
                newTypeNameProperty.stringValue = typeName;
            }

            SerializedProperty newCategoriesProperty = FindRelativeProperty(newElementProperty, "m_categories", "Categories");
            if (newCategoriesProperty != null && newCategoriesProperty.isArray)
            {
                newCategoriesProperty.arraySize = 0;
            }

            settingsSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(settingsSO.targetObject);

            settingsSO.Update();
            return categoryTypesProperty.GetArrayElementAtIndex(newIndex);
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
    }
}