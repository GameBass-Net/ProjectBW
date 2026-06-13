/// <summary>
/// Project : Easy Build System
/// Class : BuildingCatalogMenuUIEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.UI.BuildingMenus.Implementations
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingCatalogMenuUI), true)]
    public class BuildingCatalogMenuUIEditor : BaseInspectorEditor<BuildingCatalogMenuUI>
    {
        private ReorderableList.Code.Editor.ReorderableList m_categoryList;
        private int m_selectedCategoryIndex = -1;

        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable();

            m_categoryList = new ReorderableList.Code.Editor.ReorderableList(Properties.Get("m_categories"), false);
            m_categoryList.Native.onSelectCallback = list =>
            {
                m_selectedCategoryIndex = list.index;
                Repaint();
            };

            m_selectedCategoryIndex = m_categoryList.Native.index;
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "UI catalog menu with customizable categories and slot actions organized in a scrollable list.\n" +
                "Each slot executes a configurable action on confirmation such as part selection or building mode switching.\n" +
                "See the documentation for more information about this component.");

            if (EditorUtility.IsPersistent(Target))
            {
                EditorGUIExtended.HelpBox("Place this prefab directly in the scene to see changes in real-time.", EditorGUIElements.MessageType.Info);
                EditorGUIExtended.Separator();
            }

            DrawGeneralSection();
            DrawInputSection();
            DrawAudioSection();
            DrawUIReferencesSection();

            DrawPreviewControls();
            EditorGUIExtended.InspectorBottom();
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure menu categories, assign Building Parts to slots and set the default category.",
                () =>
                {
                    SerializedProperty categoriesProperty = Properties.Get("m_categories");
                    SerializedProperty defaultCategoryProperty = Properties.Get("m_defaultCategoryIndex");

                    DrawCategorySelector(categoriesProperty, defaultCategoryProperty);

                    EditorGUILayout.Space();
                    m_categoryList?.Layout();

                    using (new EditorGUI.DisabledScope(m_selectedCategoryIndex < 0))
                    {
                        if (GUILayout.Button(GetAutoPopulateLabel(categoriesProperty), EditorStyles.miniButtonRight))
                        {
                            ShowAutoPopulateMenu();
                        }
                    }
                },
                false, true);
        }

        private void DrawInputSection()
        {
            EditorGUIExtended.DrawExpandableSection("Input Settings", "keyboard",
                "Configure how the menu is opened and navigated using standalone input.",
                () =>
                {
                    SerializedProperty inputModeProp = Properties.Get("m_inputMode");

                    if (inputModeProp.enumValueIndex != (int)BuildingMenuUI.MenuInputMode.Standalone)
                    {
                        inputModeProp.enumValueIndex = (int)BuildingMenuUI.MenuInputMode.Standalone;
                        inputModeProp.serializedObject.ApplyModifiedProperties();
                    }

#if !ENABLE_INPUT_SYSTEM
                    Properties.Draw("m_toggleKey", new GUIContent("Toggle Key", "Keyboard key used to open and close the menu."));
#else
                    Properties.Draw("m_toggleAction", new GUIContent("Toggle Action", "Input action used to open and close the menu."));
#endif

                    Properties.Draw("m_lockCursor", new GUIContent("Lock Cursor", "Locks and hides the cursor while the menu is open."));
                },
                false, true);
        }

        private void DrawAudioSection()
        {
            EditorGUIExtended.DrawExpandableSection("Audio Settings", "audio",
                "Configure the audio clips played in response to menu interactions.",
                () =>
                {
                    using (EditorGUIExtended.IndentScope())
                    {
                        Properties.Draw("m_openAudio", new GUIContent("Open Sound", "Played when the menu opens."));
                        Properties.Draw("m_closeAudio", new GUIContent("Close Sound", "Played when the menu closes."));
                    }
                },
                false, true);
        }

        private void DrawUIReferencesSection()
        {
            EditorGUIExtended.DrawExpandableSection("UI References", "gui",
                "Assign the UI components that make up the catalog menu layout.",
                () =>
                {
                    Properties.Draw("m_canvasGroup", new GUIContent("Canvas Group", "Controls the overall visibility and interactivity of the menu."));

                    EditorGUIExtended.Separator("Category Navigation");
                    Properties.Draw("m_categoryContainer", new GUIContent("Category Container", "Parent transform where category navigation buttons are instantiated."));
                    Properties.Draw("m_categorySlotPrefab", new GUIContent("Category Slot Prefab", "Prefab instantiated for each category navigation button."));

                    EditorGUIExtended.Separator("Building Parts Slots");
                    Properties.Draw("m_slotContainer", new GUIContent("Slot Container", "Parent transform where Building Part slots are instantiated."));
                    Properties.Draw("m_slotPrefab", new GUIContent("Slot Prefab", "Prefab instantiated for each Building Part slot."));

                    EditorGUIExtended.Separator("Search & Count");
                    Properties.Draw("m_searchField", new GUIContent("Search Field", "Input field used to filter Building Parts by name."));
                    Properties.Draw("m_itemCountText", new GUIContent("Item Count Text", "Displays the total number of Building Parts in the active category."));
                },
                false, true);
        }

        private void DrawPreviewControls()
        {
            using (EditorGUIExtended.DisabledScope(EditorUtility.IsPersistent(Target)))
            {
                bool isOpen = Target.IsOpen;
                string buttonText = isOpen ? "Close Catalog" : "Open Catalog";

                if (EditorGUIExtended.StateButton(buttonText, isOpen))
                {
                    if (Target.IsOpen)
                    {
                        Target.CloseMenu();
                    }
                    else
                    {
                        Target.OpenMenu();
                    }

                    TriggerRefresh();
                }

                if (GUILayout.Button("Force Rebuild UI"))
                {
                    TriggerForceRebuild();
                }
            }
        }

        #region Rebuild & Refresh

        private void TriggerRefresh()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (target == null)
                {
                    return;
                }

                BuildingCatalogMenuUI menu = Target;

                if (!IsValidForRebuild(menu))
                {
                    return;
                }

                menu.PopulateCategoryButtons();
                menu.ForceRebuildAllSlots();
                menu.SelectCategory(menu.DefaultCategoryIndex);

                SyncSerializedObject();
                EditorUtility.SetDirty(menu);
                Repaint();
            };
        }

        private void TriggerForceRebuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (target == null)
                {
                    return;
                }

                BuildingCatalogMenuUI menu = Target;

                if (!IsValidForRebuild(menu))
                {
                    return;
                }

                GameObject root = menu.gameObject;
                bool isPrefabInstance = !Application.isPlaying &&
                    PrefabUtility.IsPartOfPrefabInstance(root);

                string prefabPath = isPrefabInstance
                    ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root)
                    : null;

                if (isPrefabInstance)
                {
                    PrefabUtility.UnpackPrefabInstance(
                        root,
                        PrefabUnpackMode.OutermostRoot,
                        InteractionMode.AutomatedAction
                    );
                }

                menu.PopulateCategoryButtons();
                menu.ForceRebuildAllSlots();
                menu.SelectCategory(menu.DefaultCategoryIndex);

                if (isPrefabInstance && !string.IsNullOrEmpty(prefabPath))
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(
                        root,
                        prefabPath,
                        InteractionMode.AutomatedAction
                    );
                }

                SyncSerializedObject();
                EditorUtility.SetDirty(menu);
                Repaint();
            };
        }

        private bool IsValidForRebuild(BuildingCatalogMenuUI menu)
        {
            return menu != null
                && !PrefabUtility.IsPartOfPrefabAsset(menu)
                && menu.gameObject.scene.IsValid()
                && menu.SlotContainer != null
                && menu.SlotPrefab != null
                && menu.CategoryContainer != null;
        }

        private void SyncSerializedObject()
        {
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Population

        private void ShowAutoPopulateMenu()
        {
            BuildingPartRegistry registry = BuildingPartRegistry.Instance;
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Error", "BuildingPartRegistry not found.", "OK");
                return;
            }

            GenericMenu menu = new GenericMenu();
            string[] collectionGuids = AssetDatabase.FindAssets("t:BuildingCollection");

            foreach (string guid in collectionGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BuildingCollection collection = AssetDatabase.LoadAssetAtPath<BuildingCollection>(path);

                if (collection == null)
                {
                    continue;
                }

                string fullLabel = $"By Collection/{collection.Name} (Full)";
                menu.AddItem(new GUIContent(fullLabel), false, () => PopulateCategoryFromCollection(collection));

                if (collection.PartReferences == null)
                {
                    continue;
                }

                foreach (string partId in collection.PartReferences)
                {
                    if (string.IsNullOrEmpty(partId))
                    {
                        continue;
                    }

                    BuildingPart part = registry.GetPartByPrefabId(partId);
                    if (part == null)
                    {
                        continue;
                    }

                    Texture icon = part.Thumbnail != null
                        ? part.Thumbnail
                        : AssetPreview.GetMiniThumbnail(part.gameObject);

                    string itemLabel = $"By Collection/{collection.Name}/{part.Name}";
                    menu.AddItem(new GUIContent(itemLabel, icon), false, () => AddPartToSelectedCategory(part));
                }
            }

            menu.ShowAsContext();
        }

        private void AddPartToSelectedCategory(BuildingPart part)
        {
            SerializedProperty categoriesProperty = Properties.Get("m_categories");

            if (!IsValidCategorySelection(categoriesProperty))
            {
                EditorUtility.DisplayDialog("Error", "No valid category selected.", "OK");
                return;
            }

            SerializedProperty slotsProperty = GetSlotsProperty(categoriesProperty, m_selectedCategoryIndex);
            if (slotsProperty == null)
            {
                return;
            }

            int slotIndex = FindFirstEmptySlot(slotsProperty);

            if (slotIndex >= slotsProperty.arraySize)
            {
                slotsProperty.arraySize++;
            }

            FillSlotWithPart(slotsProperty.GetArrayElementAtIndex(slotIndex), part);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(Target);
            TriggerRefresh();
        }

        private void PopulateCategoryFromCollection(BuildingCollection buildingCollection)
        {
            try
            {
                if (buildingCollection?.PartReferences == null || buildingCollection.PartReferences.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Building Collection is empty.", "OK");
                    return;
                }

                SerializedProperty categoriesProperty = Properties.Get("m_categories");
                if (!IsValidCategorySelection(categoriesProperty))
                {
                    EditorUtility.DisplayDialog("Error", "No valid category selected.", "OK");
                    return;
                }

                BuildingPartRegistry registry = BuildingPartRegistry.Instance;
                if (registry == null)
                {
                    EditorUtility.DisplayDialog("Error", "BuildingPartRegistry not found.", "OK");
                    return;
                }

                SerializedProperty slotsProperty = GetSlotsProperty(categoriesProperty, m_selectedCategoryIndex);
                if (slotsProperty == null)
                {
                    return;
                }

                int nextEmptySlotIndex = FindFirstEmptySlot(slotsProperty);

                foreach (string partPrefabId in buildingCollection.PartReferences)
                {
                    if (string.IsNullOrEmpty(partPrefabId))
                    {
                        continue;
                    }

                    BuildingPart part = registry.GetPartByPrefabId(partPrefabId);
                    if (part == null)
                    {
                        continue;
                    }

                    if (nextEmptySlotIndex >= slotsProperty.arraySize)
                    {
                        slotsProperty.arraySize = nextEmptySlotIndex + 1;
                    }

                    FillSlotWithPart(slotsProperty.GetArrayElementAtIndex(nextEmptySlotIndex), part);
                    nextEmptySlotIndex = FindFirstEmptySlot(slotsProperty, nextEmptySlotIndex + 1);
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(Target);
                TriggerRefresh();
            }
            catch (Exception caughtException)
            {
                Debug.LogException(caughtException);
                EditorUtility.DisplayDialog(
                    "Exception",
                    $"PopulateFromCollection failed:\n{caughtException.Message}",
                    "OK"
                );
            }
        }

        #endregion

        #region Utilities

        private void DrawCategorySelector(SerializedProperty categoriesProperty, SerializedProperty defaultCategoryProperty)
        {
            if (categoriesProperty == null || categoriesProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("Default Category", "No categories defined");
                return;
            }

            string[] categoryNames = BuildCategoryNames(categoriesProperty);
            int currentIndex = Mathf.Clamp(defaultCategoryProperty.intValue, 0, categoriesProperty.arraySize - 1);
            int nextIndex = EditorGUILayout.Popup(new GUIContent("Default Category", "Select the default category to display when menu opens."), currentIndex, categoryNames);

            if (nextIndex != currentIndex)
            {
                defaultCategoryProperty.intValue = nextIndex;
                serializedObject.ApplyModifiedProperties();
                TriggerRefresh();
            }
        }

        private string GetAutoPopulateLabel(SerializedProperty categoriesProperty)
        {
            if (categoriesProperty == null ||
                m_selectedCategoryIndex < 0 ||
                m_selectedCategoryIndex >= categoriesProperty.arraySize)
            {
                return "Auto-populate from Building Collection...";
            }

            SerializedProperty categoryProperty = categoriesProperty.GetArrayElementAtIndex(m_selectedCategoryIndex);
            string categoryName = categoryProperty.FindPropertyRelative("m_name")?.stringValue;

            if (string.IsNullOrEmpty(categoryName))
            {
                categoryName = $"Category {m_selectedCategoryIndex + 1}";
            }

            return $"Auto-populate \"{categoryName}\" from Building Collection...";
        }

        private bool IsValidCategorySelection(SerializedProperty categoriesProperty)
        {
            return categoriesProperty != null
                && categoriesProperty.arraySize > 0
                && m_selectedCategoryIndex >= 0
                && m_selectedCategoryIndex < categoriesProperty.arraySize;
        }

        private SerializedProperty GetSlotsProperty(SerializedProperty categoriesProperty, int categoryIndex)
        {
            return categoriesProperty
                .GetArrayElementAtIndex(categoryIndex)
                .FindPropertyRelative("m_slots");
        }

        private int FindFirstEmptySlot(SerializedProperty slotsProperty, int startIndex = 0)
        {
            for (int s = startIndex; s < slotsProperty.arraySize; s++)
            {
                SerializedProperty slot = slotsProperty.GetArrayElementAtIndex(s);

                bool hasName = !string.IsNullOrEmpty(slot.FindPropertyRelative("m_name")?.stringValue);
                bool hasIcon = slot.FindPropertyRelative("m_icon")?.objectReferenceValue != null;
                bool hasAction = !string.IsNullOrEmpty(slot.FindPropertyRelative("m_action")?.managedReferenceFullTypename);

                if (!hasName && !hasIcon && !hasAction)
                {
                    return s;
                }
            }

            return slotsProperty.arraySize;
        }

        private void FillSlotWithPart(SerializedProperty slotProperty, BuildingPart part)
        {
            slotProperty.FindPropertyRelative("m_name").stringValue = part.Name;
            slotProperty.FindPropertyRelative("m_description").stringValue = part.Description;
            slotProperty.FindPropertyRelative("m_icon").objectReferenceValue = null;

            SerializedProperty actionProperty = slotProperty.FindPropertyRelative("m_action");
            if (actionProperty != null && actionProperty.propertyType == SerializedPropertyType.ManagedReference)
            {
                actionProperty.managedReferenceValue = Activator.CreateInstance(typeof(BuildingSelectionAction));
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                actionProperty = slotProperty.FindPropertyRelative("m_action");
                if (actionProperty != null)
                {
                    actionProperty.FindPropertyRelative("m_partReference").stringValue = part.PrefabId;
                    actionProperty.FindPropertyRelative("m_closeMenu").boolValue = true;
                }
            }
        }

        private string[] BuildCategoryNames(SerializedProperty categoriesProperty)
        {
            string[] names = new string[categoriesProperty.arraySize];
            for (int i = 0; i < categoriesProperty.arraySize; i++)
            {
                string categoryName = categoriesProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_name")?.stringValue ?? "Unnamed";
                names[i] = $"Category {i + 1}: {categoryName}";
            }
            return names;
        }

        #endregion
    }
}