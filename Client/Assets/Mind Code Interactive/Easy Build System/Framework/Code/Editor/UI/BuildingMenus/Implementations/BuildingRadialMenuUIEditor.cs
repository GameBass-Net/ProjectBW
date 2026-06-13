/// <summary>
/// Project : Easy Build System
/// Class : BuildingRadialMenuUIEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

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
    [CustomEditor(typeof(BuildingRadialMenuUI), true)]
    public class BuildingRadialMenuUIEditor : BaseInspectorEditor<BuildingRadialMenuUI>
    {
        private ReorderableList.Code.Editor.ReorderableList m_categoryList;
        private int m_selectedCategoryIndex = -1;

        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable();
            m_categoryList = new ReorderableList.Code.Editor.ReorderableList(Properties.Get("m_categories"), false);
            m_categoryList.Native.onSelectCallback = reorderableListValue =>
            {
                m_selectedCategoryIndex = reorderableListValue.index;
                Repaint();
            };
            m_selectedCategoryIndex = m_categoryList.Native.index;
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "UI radial menu with customizable categories and slot actions arranged radially around a center point.\n" +
                "Each slot executes a configurable action on confirmation such as part selection or building mode switching.\n" +
                "See the documentation for more information about this component.");

            if (EditorUtility.IsPersistent(Target))
            {
                EditorGUIExtended.HelpBox("Place this prefab directly in the scene to see changes in real-time.", EditorGUIElements.MessageType.Info);
                EditorGUIExtended.Separator();
            }

            DrawGeneralSection();
            DrawInputSection();
            DrawAnimationSection();
            DrawAudioSection();
            DrawUIReferencesSection();

            DrawPreviewControls();
            EditorGUIExtended.InspectorBottom();
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure menu categories, assign Building Parts to slots and set the default selection.",
                () =>
                {
                    SerializedProperty categoriesPropertyToUse = Properties.Get("m_categories");
                    SerializedProperty defaultCategoryPropertyToUse = Properties.Get("m_defaultCategoryIndex");
                    SerializedProperty defaultSlotPropertyToUse = Properties.Get("m_defaultSelectedSlotIndex");

                    DrawCategorySelector(categoriesPropertyToUse, defaultCategoryPropertyToUse);
                    DrawSlotSelector(categoriesPropertyToUse, defaultCategoryPropertyToUse, defaultSlotPropertyToUse);

                    EditorGUILayout.Space();
                    m_categoryList?.Layout();

                    using (new EditorGUI.DisabledScope(m_selectedCategoryIndex < 0))
                    {
                        if (GUILayout.Button(GetAutoPopulateLabel(categoriesPropertyToUse), EditorStyles.miniButtonRight))
                        {
                            ShowAutoPopulateMenu();
                        }
                    }
                },
                false, true
            );
        }

        private void DrawInputSection()
        {
            EditorGUIExtended.DrawExpandableSection("Input Settings", "gamepad",
                "Configure how the menu is opened and navigated across keyboard, gamepad and touch input.",
                () =>
                {
                    BuildingMenuUI.MenuInputMode inputModeSelected = (BuildingMenuUI.MenuInputMode)Properties.Get("m_inputMode").enumValueIndex;

#if !ENABLE_INPUT_SYSTEM
                    if (inputModeSelected == BuildingMenuUI.MenuInputMode.Gamepad)
                    {
                        EditorGUIExtended.HelpBox(
                            "Gamepad input requires the New Input System.\n" +
                            "Enable it in Project Settings > Player > Input System.",
                            EditorGUIElements.MessageType.Warning);
                    }
#endif

                    Properties.Draw("m_inputMode", new GUIContent("Input Mode", "Input method used to open, navigate and confirm selections in the menu."));

                    if (inputModeSelected != BuildingMenuUI.MenuInputMode.Mobile)
                    {
                        using (EditorGUIExtended.IndentScope())
                        {
#if !ENABLE_INPUT_SYSTEM
                            if (inputModeSelected == BuildingMenuUI.MenuInputMode.Standalone)
                            {
                                Properties.Draw("m_toggleKey", new GUIContent("Toggle Key", "Keyboard key used to open and close the menu."));
                            }
#else
                            Properties.Draw("m_toggleAction", new GUIContent("Toggle Action", "Input action used to open and close the menu."));
                            Properties.Draw("m_validateAction", new GUIContent("Validate Action", "Input action used to confirm the currently highlighted slot."));
                            Properties.Draw("m_selectAction", new GUIContent("Select Action", "Input action used to navigate between slots."));
#endif
                            if (inputModeSelected != BuildingMenuUI.MenuInputMode.Gamepad)
                            {
                                Properties.Draw("m_lockCursor", new GUIContent("Lock Cursor", "Locks and hides the cursor while the menu is open."));
                            }
                        }
                    }
                },
                false, true);
        }

        private void DrawAnimationSection()
        {
            EditorGUIExtended.DrawExpandableSection("Animation Settings", "animation",
                "Configure the animator and state names used for menu open and close transitions.",
                () =>
                {
                    Properties.Draw("m_menuAnimator", new GUIContent("Menu Animator", "Animator component driving the menu open and close animations."));
                    Properties.Draw("m_openMenuState", new GUIContent("Open State", "Animator state triggered when the menu opens."));
                    Properties.Draw("m_closeMenuState", new GUIContent("Close State", "Animator state triggered when the menu closes."));
                    using (EditorGUIExtended.IndentScope())
                    {
                        Properties.Draw("m_hideWhenMenuOpen", new GUIContent("Hide When Menu Open", "UI elements hidden while the menu is open, such as the HUD or crosshair."));
                    }
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
                        Properties.Draw("m_slotClickSound", new GUIContent("Slot Click Sound", "Played when a slot is confirmed."));
                        Properties.Draw("m_slotHighlightSound", new GUIContent("Slot Highlight Sound", "Played when the highlighted slot changes during navigation."));
                    }
                },
                false, true);
        }

        private void DrawUIReferencesSection()
        {
            EditorGUIExtended.DrawExpandableSection("UI References", "gui",
                "Assign the UI components that make up the radial menu layout.",
                () =>
                {
                    Properties.Draw("m_canvasGroup", new GUIContent("Canvas Group", "Controls the overall visibility and interactivity of the menu."));
                    Properties.Draw("m_content", new GUIContent("Content Container", "RectTransform used as the reference center for input position calculations."));

                    EditorGUIExtended.Separator("Slots Settings");
                    Properties.Draw("m_slotPrefab", new GUIContent("Slot Prefab", "Prefab instantiated for each slot in the menu."));
                    Properties.Draw("m_slotSpacing", new GUIContent("Slot Spacing", "Radial distance from the center to each slot position."));

                    EditorGUIExtended.Separator("Selection Indicators Settings");
                    Properties.Draw("m_selectionFillImage", new GUIContent("Selection Fill", "Image filling the pie-slice area of the selected slot."));
                    Properties.Draw("m_selectionHighlightImage", new GUIContent("Selection Highlight", "Image drawn along the boundary of the selected slot."));
                    Properties.Draw("m_selectionIndicatorImage", new GUIContent("Selection Indicator", "Arrow or pointer image pointing toward the selected slot."));

                    EditorGUIExtended.Separator("Center Selection Settings");
                    Properties.Draw("m_selectionIcon", new GUIContent("Selection Icon", "Displays the thumbnail of the currently selected Building Part."));
                    Properties.Draw("m_selectionText", new GUIContent("Selection Text", "Displays the name of the currently selected Building Part."));
                    Properties.Draw("m_selectionDescription", new GUIContent("Selection Description", "Displays the description of the currently selected Building Part."));
                },
                false, true);
        }

        private void DrawPreviewControls()
        {
            using (EditorGUIExtended.DisabledScope(EditorUtility.IsPersistent(Target)))
            {
                bool isMenuCurrentlyOpen = Target.IsOpen;
                string previewButtonText = isMenuCurrentlyOpen ? "Close Radial Menu" : "Open Radial Menu";

                if (EditorGUIExtended.StateButton(previewButtonText, isMenuCurrentlyOpen))
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

                BuildingRadialMenuUI menuToRefresh = Target;

                if (!IsValidForRebuild(menuToRefresh))
                {
                    return;
                }

                menuToRefresh.SelectCategory(menuToRefresh.DefaultCategoryIndex);
                menuToRefresh.PositionSlotsRadially();

                int selectedSlotIndexToSelect = Mathf.Clamp(menuToRefresh.DefaultSelectedSlotIndex, 0, menuToRefresh.SlotUIs.Count - 1);
                menuToRefresh.RefreshSelection(selectedSlotIndexToSelect, true);

                SyncSerializedObject();
                EditorUtility.SetDirty(menuToRefresh);
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

                BuildingRadialMenuUI menuToRebuild = Target;

                if (!IsValidForRebuild(menuToRebuild))
                {
                    return;
                }

                GameObject rootGameObjectOfMenu = menuToRebuild.gameObject;
                bool isPrefabInstanceToHandle = !Application.isPlaying &&
                    PrefabUtility.IsPartOfPrefabInstance(rootGameObjectOfMenu);

                string prefabPathForSaving = isPrefabInstanceToHandle
                    ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rootGameObjectOfMenu)
                    : null;

                if (isPrefabInstanceToHandle)
                {
                    PrefabUtility.UnpackPrefabInstance(
                        rootGameObjectOfMenu,
                        PrefabUnpackMode.OutermostRoot,
                        InteractionMode.AutomatedAction
                    );
                }

                menuToRebuild.ForceRebuildAllSlots();
                menuToRebuild.SelectCategory(menuToRebuild.DefaultCategoryIndex);
                menuToRebuild.PositionSlotsRadially();

                int slotIndexToSelectAfterRebuild = Mathf.Clamp(menuToRebuild.DefaultSelectedSlotIndex, 0, menuToRebuild.SlotUIs.Count - 1);
                menuToRebuild.RefreshSelection(slotIndexToSelectAfterRebuild, true);

                if (isPrefabInstanceToHandle && !string.IsNullOrEmpty(prefabPathForSaving))
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(
                        rootGameObjectOfMenu,
                        prefabPathForSaving,
                        InteractionMode.AutomatedAction
                    );
                }

                SyncSerializedObject();
                EditorUtility.SetDirty(menuToRebuild);
                Repaint();
            };
        }

        private bool IsValidForRebuild(BuildingRadialMenuUI menuToValidate)
        {
            return menuToValidate != null
                && !PrefabUtility.IsPartOfPrefabAsset(menuToValidate)
                && menuToValidate.gameObject.scene.IsValid()
                && menuToValidate.SlotContainer != null
                && menuToValidate.SlotPrefab != null;
        }

        private void SyncSerializedObject()
        {
            serializedObject.UpdateIfRequiredOrScript();
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Population

        private void ShowAutoPopulateMenu()
        {
            BuildingPartRegistry registryInstance = BuildingPartRegistry.Instance;
            if (registryInstance == null)
            {
                EditorUtility.DisplayDialog("Error", "BuildingPartRegistry not found.", "OK");
                return;
            }

            GenericMenu contextMenuToShow = new GenericMenu();
            string[] collectionGuidArray = AssetDatabase.FindAssets("t:BuildingCollection");

            foreach (string collectionGuidToLoad in collectionGuidArray)
            {
                string assetPathFromGuid = AssetDatabase.GUIDToAssetPath(collectionGuidToLoad);
                BuildingCollection buildingCollectionLoaded = AssetDatabase.LoadAssetAtPath<BuildingCollection>(assetPathFromGuid);

                if (buildingCollectionLoaded == null)
                {
                    continue;
                }

                string fullMenuLabelForCollection = $"By Collection/{buildingCollectionLoaded.Name} (Full)";
                contextMenuToShow.AddItem(new GUIContent(fullMenuLabelForCollection), false, () => PopulateCategoryFromCollection(buildingCollectionLoaded));

                if (buildingCollectionLoaded.PartReferences == null)
                {
                    continue;
                }

                foreach (string partPrefabIdInCollection in buildingCollectionLoaded.PartReferences)
                {
                    if (string.IsNullOrEmpty(partPrefabIdInCollection))
                    {
                        continue;
                    }

                    BuildingPart part = registryInstance.GetPartByPrefabId(partPrefabIdInCollection);
                    if (part == null)
                    {
                        continue;
                    }

                    Texture partThumbnailTextureToDisplay = part.Thumbnail != null
                        ? part.Thumbnail
                        : AssetPreview.GetMiniThumbnail(part.gameObject);

                    string itemMenuLabelForPart = $"By Collection/{buildingCollectionLoaded.Name}/{part.Name}";
                    contextMenuToShow.AddItem(new GUIContent(itemMenuLabelForPart, partThumbnailTextureToDisplay), false, () => AddPartToSelectedCategory(part));
                }
            }

            contextMenuToShow.ShowAsContext();
        }

        private void AddPartToSelectedCategory(BuildingPart part)
        {
            SerializedProperty categoriesPropertyToCheck = Properties.Get("m_categories");

            if (!IsValidCategorySelection(categoriesPropertyToCheck))
            {
                EditorUtility.DisplayDialog("Error", "No valid category selected.", "OK");
                return;
            }

            SerializedProperty categorySlotPropertyToFill = GetSlotsProperty(categoriesPropertyToCheck, m_selectedCategoryIndex);
            if (categorySlotPropertyToFill == null)
            {
                return;
            }

            int emptySlotIndexFound = FindFirstEmptySlot(categorySlotPropertyToFill);

            if (emptySlotIndexFound >= categorySlotPropertyToFill.arraySize)
            {
                categorySlotPropertyToFill.arraySize++;
            }

            FillSlotWithPart(categorySlotPropertyToFill.GetArrayElementAtIndex(emptySlotIndexFound), part);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(Target);
            TriggerRefresh();
        }

        private void PopulateCategoryFromCollection(BuildingCollection buildingCollectionToPopulateFrom)
        {
            try
            {
                if (buildingCollectionToPopulateFrom?.PartReferences == null || buildingCollectionToPopulateFrom.PartReferences.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Building Collection is empty.", "OK");
                    return;
                }

                SerializedProperty categoriesPropertyForPopulation = Properties.Get("m_categories");
                if (!IsValidCategorySelection(categoriesPropertyForPopulation))
                {
                    EditorUtility.DisplayDialog("Error", "No valid category selected.", "OK");
                    return;
                }

                BuildingPartRegistry registryForPopulation = BuildingPartRegistry.Instance;
                if (registryForPopulation == null)
                {
                    EditorUtility.DisplayDialog("Error", "BuildingPartRegistry not found.", "OK");
                    return;
                }

                SerializedProperty slotsPropertyToPopulate = GetSlotsProperty(categoriesPropertyForPopulation, m_selectedCategoryIndex);
                if (slotsPropertyToPopulate == null)
                {
                    return;
                }

                int nextEmptySlotIndexToFill = FindFirstEmptySlot(slotsPropertyToPopulate);

                foreach (string partPrefabIdFromCollection in buildingCollectionToPopulateFrom.PartReferences)
                {
                    if (string.IsNullOrEmpty(partPrefabIdFromCollection))
                    {
                        continue;
                    }

                    BuildingPart part = registryForPopulation.GetPartByPrefabId(partPrefabIdFromCollection);
                    if (part == null)
                    {
                        continue;
                    }

                    if (nextEmptySlotIndexToFill >= slotsPropertyToPopulate.arraySize)
                    {
                        slotsPropertyToPopulate.arraySize = nextEmptySlotIndexToFill + 1;
                    }

                    FillSlotWithPart(slotsPropertyToPopulate.GetArrayElementAtIndex(nextEmptySlotIndexToFill), part);
                    nextEmptySlotIndexToFill = FindFirstEmptySlot(slotsPropertyToPopulate, nextEmptySlotIndexToFill + 1);
                }

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(Target);
                TriggerRefresh();
            }
            catch (System.Exception exceptionCaught)
            {
                Debug.LogException(exceptionCaught);
                EditorUtility.DisplayDialog(
                    "Exception",
                    $"PopulateFromCollection failed:\n{exceptionCaught.Message}",
                    "OK"
                );
            }
        }

        #endregion

        #region Utilities

        private void DrawCategorySelector(SerializedProperty categoriesPropertyToDraw, SerializedProperty defaultCategoryPropertyToDraw)
        {
            if (categoriesPropertyToDraw == null || categoriesPropertyToDraw.arraySize == 0)
            {
                EditorGUILayout.LabelField("Default Category", "No categories defined");
                return;
            }

            string[] categoryNameArrayBuilt = BuildCategoryNames(categoriesPropertyToDraw);
            int currentCategoryIndexSelected = Mathf.Clamp(defaultCategoryPropertyToDraw.intValue, 0, categoriesPropertyToDraw.arraySize - 1);
            int nextCategoryIndexSelected = EditorGUILayout.Popup(new GUIContent("Default Category", "Select the default category to display when menu opens."), currentCategoryIndexSelected, categoryNameArrayBuilt);

            if (nextCategoryIndexSelected != currentCategoryIndexSelected)
            {
                defaultCategoryPropertyToDraw.intValue = nextCategoryIndexSelected;
                serializedObject.ApplyModifiedProperties();
                Target.SelectCategory(nextCategoryIndexSelected);
                TriggerRefresh();
            }
        }

        private void DrawSlotSelector(SerializedProperty categoriesPropertyForSlots, SerializedProperty defaultCategoryPropertyForSlots, SerializedProperty defaultSlotPropertyToSelect)
        {
            if (categoriesPropertyForSlots == null || categoriesPropertyForSlots.arraySize == 0)
            {
                return;
            }

            int categoryIndexToUseForSlots = Mathf.Clamp(defaultCategoryPropertyForSlots.intValue, 0, categoriesPropertyForSlots.arraySize - 1);
            SerializedProperty categoryPropertyForSlots = categoriesPropertyForSlots.GetArrayElementAtIndex(categoryIndexToUseForSlots);
            SerializedProperty slotsPropertyOfCategory = categoryPropertyForSlots.FindPropertyRelative("m_slots");

            if (slotsPropertyOfCategory == null || slotsPropertyOfCategory.arraySize == 0)
            {
                EditorGUILayout.LabelField("Default Selected Slot", "No slots in category");
                return;
            }

            List<int> slotIndicesList = new List<int>();
            List<string> slotLabelsList = new List<string>();

            for (int slotIterationIndex = 0; slotIterationIndex < slotsPropertyOfCategory.arraySize; slotIterationIndex++)
            {
                slotIndicesList.Add(slotIterationIndex);
                slotLabelsList.Add(FormatSlotLabel(slotsPropertyOfCategory.GetArrayElementAtIndex(slotIterationIndex), slotIterationIndex));
            }

            if (slotLabelsList.Count == 0)
            {
                EditorGUILayout.LabelField("Default Selected Slot", "No slots");
                return;
            }

            int currentSelectedSlotIndex = defaultSlotPropertyToSelect.intValue;
            if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= slotIndicesList.Count)
            {
                currentSelectedSlotIndex = 0;
            }

            int nextSelectedSlotIndex = EditorGUILayout.Popup(new GUIContent("Default Selected Slot", "The slot highlighted when menu opens or category is selected."), currentSelectedSlotIndex, slotLabelsList.ToArray());

            if (nextSelectedSlotIndex != currentSelectedSlotIndex)
            {
                int slotIndexToApply = slotIndicesList[nextSelectedSlotIndex];
                defaultSlotPropertyToSelect.intValue = slotIndexToApply;
                serializedObject.ApplyModifiedProperties();
                if (Target.SlotUIs.Count > 0)
                {
                    Target.RefreshSelection(Mathf.Clamp(slotIndexToApply, 0, Target.SlotUIs.Count - 1), true);
                }

                TriggerRefresh();
            }
        }

        private string GetAutoPopulateLabel(SerializedProperty categoriesPropertyForLabel)
        {
            if (categoriesPropertyForLabel == null ||
                m_selectedCategoryIndex < 0 ||
                m_selectedCategoryIndex >= categoriesPropertyForLabel.arraySize)
            {
                return "Auto-populate from Building Collection...";
            }

            SerializedProperty categoryPropertyForLabel = categoriesPropertyForLabel.GetArrayElementAtIndex(m_selectedCategoryIndex);
            string selectedCategoryName = categoryPropertyForLabel.FindPropertyRelative("m_name")?.stringValue;

            if (string.IsNullOrEmpty(selectedCategoryName))
            {
                selectedCategoryName = $"Category {m_selectedCategoryIndex + 1}";
            }

            return $"Auto-populate \"{selectedCategoryName}\" from Building Collection...";
        }

        private bool IsValidCategorySelection(SerializedProperty categoriesPropertyToValidate)
        {
            return categoriesPropertyToValidate != null
                && categoriesPropertyToValidate.arraySize > 0
                && m_selectedCategoryIndex >= 0
                && m_selectedCategoryIndex < categoriesPropertyToValidate.arraySize;
        }

        private SerializedProperty GetSlotsProperty(SerializedProperty categoriesPropertyToUse, int categoryIndexToGet)
        {
            return categoriesPropertyToUse
                .GetArrayElementAtIndex(categoryIndexToGet)
                .FindPropertyRelative("m_slots");
        }

        private int FindFirstEmptySlot(SerializedProperty slotsPropertyToSearch, int startIndexForSearch = 0)
        {
            for (int slotSearchIndex = startIndexForSearch; slotSearchIndex < slotsPropertyToSearch.arraySize; slotSearchIndex++)
            {
                SerializedProperty currentSlotBeingChecked = slotsPropertyToSearch.GetArrayElementAtIndex(slotSearchIndex);

                bool hasNameValue = !string.IsNullOrEmpty(currentSlotBeingChecked.FindPropertyRelative("m_name")?.stringValue);
                bool hasIconValue = currentSlotBeingChecked.FindPropertyRelative("m_icon")?.objectReferenceValue != null;
                bool hasActionValue = !string.IsNullOrEmpty(currentSlotBeingChecked.FindPropertyRelative("m_action")?.managedReferenceFullTypename);

                if (!hasNameValue && !hasIconValue && !hasActionValue)
                {
                    return slotSearchIndex;
                }
            }

            return slotsPropertyToSearch.arraySize;
        }

        private void FillSlotWithPart(SerializedProperty slotPropertyToFill, BuildingPart part)
        {
            slotPropertyToFill.FindPropertyRelative("m_name").stringValue = part.Name;
            slotPropertyToFill.FindPropertyRelative("m_description").stringValue = part.Description;
            slotPropertyToFill.FindPropertyRelative("m_icon").objectReferenceValue = null;

            SerializedProperty actionPropertyToSetup = slotPropertyToFill.FindPropertyRelative("m_action");
            if (actionPropertyToSetup != null && actionPropertyToSetup.propertyType == SerializedPropertyType.ManagedReference)
            {
                actionPropertyToSetup.managedReferenceValue = System.Activator.CreateInstance(typeof(BuildingSelectionAction));
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                actionPropertyToSetup = slotPropertyToFill.FindPropertyRelative("m_action");
                if (actionPropertyToSetup != null)
                {
                    actionPropertyToSetup.FindPropertyRelative("m_partReference").stringValue = part.PrefabId;
                    actionPropertyToSetup.FindPropertyRelative("m_closeMenu").boolValue = true;
                }
            }
        }

        private string[] BuildCategoryNames(SerializedProperty categoriesPropertyToBuild)
        {
            string[] categoryNamesArray = new string[categoriesPropertyToBuild.arraySize];
            for (int categoryIndexToBuild = 0; categoryIndexToBuild < categoriesPropertyToBuild.arraySize; categoryIndexToBuild++)
            {
                string categoryNameValue = categoriesPropertyToBuild.GetArrayElementAtIndex(categoryIndexToBuild).FindPropertyRelative("m_name")?.stringValue ?? "Unnamed";
                categoryNamesArray[categoryIndexToBuild] = $"Category {categoryIndexToBuild + 1}: {categoryNameValue}";
            }
            return categoryNamesArray;
        }

        private string FormatSlotLabel(SerializedProperty slotPropertyToFormat, int slotIndexForLabel)
        {
            string slotNameValue = slotPropertyToFormat.FindPropertyRelative("m_name")?.stringValue;
            if (!string.IsNullOrEmpty(slotNameValue))
            {
                return $"Slot {slotIndexForLabel + 1}: {slotNameValue}";
            }

            SerializedProperty actionPropertyToCheck = slotPropertyToFormat.FindPropertyRelative("m_action");
            if (actionPropertyToCheck != null && !string.IsNullOrEmpty(actionPropertyToCheck.managedReferenceFullTypename))
            {
                return $"Slot {slotIndexForLabel + 1}: {ExtractTypeName(actionPropertyToCheck.managedReferenceFullTypename)}";
            }

            return $"Slot {slotIndexForLabel + 1}: Custom";
        }

        private string ExtractTypeName(string fullTypeNameToExtract)
        {
            if (string.IsNullOrEmpty(fullTypeNameToExtract))
            {
                return "Custom";
            }

            int colonIndexInTypeName = fullTypeNameToExtract.IndexOf(':');
            string typeNameAfterColon = colonIndexInTypeName >= 0 ? fullTypeNameToExtract.Substring(colonIndexInTypeName + 1) : fullTypeNameToExtract;

            int spaceIndexInTypeName = typeNameAfterColon.IndexOf(' ');
            if (spaceIndexInTypeName >= 0)
            {
                typeNameAfterColon = typeNameAfterColon.Substring(spaceIndexInTypeName + 1);
            }

            int lastDotIndexInTypeName = typeNameAfterColon.LastIndexOf('.');
            return lastDotIndexInTypeName >= 0 ? typeNameAfterColon.Substring(lastDotIndexInTypeName + 1) : typeNameAfterColon;
        }

        #endregion
    }
}