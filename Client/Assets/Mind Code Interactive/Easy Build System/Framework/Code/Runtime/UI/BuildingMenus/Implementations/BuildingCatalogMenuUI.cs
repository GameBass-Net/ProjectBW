/// <summary>
/// Project : Easy Build System
/// Class : BuildingCatalogMenuUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
{
    public class BuildingCatalogMenuUI : BuildingMenuUI
    {
        private static BuildingCatalogMenuUI s_instance;

        [SerializeField, NotNull] private Transform m_slotContainer;
        [SerializeField, NotNull] private BuildingSlotUI m_slotPrefab;
        [SerializeField] private Transform m_categoryContainer;
        [SerializeField, NotNull] private BuildingCatalogMenuCategorySlotUI m_categorySlotPrefab;
        [SerializeField] private InputField m_searchField;
        [SerializeField] private Text m_itemCountText;

        private List<BuildingCatalogMenuCategorySlotUI> m_categoryButtons = new List<BuildingCatalogMenuCategorySlotUI>();
        private string m_currentSearchKeyword;

        public static new BuildingCatalogMenuUI Instance { get => s_instance; private set => s_instance = value; }

        public Transform SlotContainer => m_slotContainer;

        public BuildingSlotUI SlotPrefab => m_slotPrefab;

        public Transform CategoryContainer => m_categoryContainer;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void Start()
        {
            base.Start();
            PopulateCategoryButtons();

            int categoryIndex = Mathf.Clamp(DefaultCategoryIndex, 0, Mathf.Max(0, Categories.Count - 1));
            if (Categories.Count > 0)
            {
                SelectCategory(categoryIndex);
            }

            if (IsOpen)
            {
                CloseMenu();
            }

            if (m_searchField != null)
            {
                m_searchField.onValueChanged.AddListener(OnSearchChanged);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null || !gameObject.scene.IsValid())
                {
                    return;
                }

                PopulateCategoryButtons();

                int categoryIndex = Mathf.Clamp(
                    DefaultCategoryIndex,
                    0,
                    Mathf.Max(0, Categories.Count - 1));

                if (Categories.Count > 0)
                {
                    SelectCategory(categoryIndex);
                }
            };
        }
#endif

        public void PopulateCategoryButtons()
        {
            if (m_categoryContainer == null || m_categorySlotPrefab == null)
            {
                return;
            }

            m_categoryButtons.Clear();

            int targetCount = Categories.Count;

            EnsureCategoryButtonCount(targetCount);

            for (int i = 0; i < m_categoryContainer.childCount; i++)
            {
                Transform child = m_categoryContainer.GetChild(i);
                BuildingCatalogMenuCategorySlotUI button = child.GetComponent<BuildingCatalogMenuCategorySlotUI>();

                if (i >= targetCount || button == null)
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                BuildingCategoryData category = Categories[i];
                child.gameObject.SetActive(true);
                button.Button.GetComponentInChildren<Text>().text = category.Name;

                button.Button.onClick.RemoveAllListeners();
                int categoryIndex = i;
                button.Button.onClick.AddListener(() => SelectCategory(categoryIndex));
                m_categoryButtons.Add(button);
            }

            if (Categories.Count > 0)
            {
                CurrentCategoryIndex = DefaultCategoryIndex;
                UpdateCategorySelection();
            }
        }

        public override void SelectCategory(int categoryIndex)
        {
            base.SelectCategory(categoryIndex);

            if (categoryIndex < 0 || categoryIndex >= Categories.Count)
            {
                return;
            }

            SelectedSlotIndex = -1;
            UpdateCategorySelection();
            RebuildSlots(Application.isPlaying);
        }

        public void ForceRebuildAllSlots()
        {
#if UNITY_EDITOR
            GameObject root = gameObject;

            bool isPrefabInstance = !Application.isPlaying &&
                UnityEditor.PrefabUtility.IsPartOfPrefabInstance(root);

            string prefabPath = isPrefabInstance
                ? UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root)
                : null;

            if (isPrefabInstance)
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(
                    root,
                    UnityEditor.PrefabUnpackMode.OutermostRoot,
                    UnityEditor.InteractionMode.AutomatedAction);
            }
#endif

            ClearContainer(m_slotContainer);
            ClearContainer(m_categoryContainer);

            PopulateCategoryButtons();
            SelectCategory(DefaultCategoryIndex);

#if UNITY_EDITOR
            if (isPrefabInstance && !string.IsNullOrEmpty(prefabPath))
            {
                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(
                    root,
                    prefabPath,
                    UnityEditor.InteractionMode.AutomatedAction);
            }
#endif
        }

        private void ClearContainer(Transform container)
        {
            if (container == null)
            {
                return;
            }

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                GameObject child = container.GetChild(i).gameObject;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(child))
                    {
                        UnityEditor.Undo.DestroyObjectImmediate(child);
                    }
                    else
                    {
                        DestroyImmediate(child);
                    }

                    continue;
                }
#endif

                Destroy(child);
            }
        }

        private void EnsureCategoryButtonCount(int targetCount)
        {
            if (m_categoryContainer == null)
            {
                return;
            }

            while (m_categoryContainer.childCount < targetCount)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfPrefabInstance(m_categorySlotPrefab.gameObject))
                {
                    GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(m_categorySlotPrefab.gameObject, m_categoryContainer);
                    if (instance == null)
                    {
                        Instantiate(m_categorySlotPrefab.gameObject, m_categoryContainer, false);
                    }
                }
                else
#endif
                {
                    Instantiate(m_categorySlotPrefab.gameObject, m_categoryContainer, false);
                }
            }
        }

        private void UpdateCategorySelection()
        {
            for (int i = 0; i < m_categoryButtons.Count; i++)
            {
                bool isSelected = i == CurrentCategoryIndex;
                m_categoryButtons[i].SetSelected(isSelected);
            }
        }

        private void OnSearchChanged(string keyword)
        {
            m_currentSearchKeyword = keyword;
            RebuildSlots(Application.isPlaying);
        }

        private void RebuildSlots(bool initializeCallbacks)
        {
            SlotUIs.Clear();

            if (CurrentCategoryIndex < 0 || CurrentCategoryIndex >= Categories.Count)
            {
                DeactivateAllSlots();
                UpdateItemCount(0);
                return;
            }

            BuildingCategoryData category = Categories[CurrentCategoryIndex];

            if (category?.Slots == null)
            {
                DeactivateAllSlots();
                UpdateItemCount(0);
                return;
            }

            List<BuildingSlotData> filteredList;

            if (string.IsNullOrEmpty(m_currentSearchKeyword))
            {
                filteredList = category.Slots;
            }
            else
            {
                filteredList = new List<BuildingSlotData>();
                string keyword = m_currentSearchKeyword.ToLower();

                for (int i = 0; i < category.Slots.Count; i++)
                {
                    BuildingSlotData slot = category.Slots[i];
                    if (slot != null && slot.Name != null && slot.Name.ToLower().Contains(keyword))
                    {
                        filteredList.Add(slot);
                    }
                }
            }

            int targetSlotCount = filteredList.Count;

            EnsureSlotCount(targetSlotCount);

            for (int i = 0; i < SlotContainer.childCount; i++)
            {
                Transform slotTransform = SlotContainer.GetChild(i);
                BuildingSlotUI slotUI = slotTransform.GetComponent<BuildingSlotUI>();

                if (i >= targetSlotCount || slotUI == null)
                {
                    slotTransform.gameObject.SetActive(false);
                    continue;
                }

                BuildingSlotData slotData = filteredList[i];
                slotData?.EnsureAction(this);

                bool hasContent = slotData != null && (!string.IsNullOrEmpty(slotData.Name) || slotData.GetIcon() != null || slotData.Action != null);
                slotTransform.gameObject.SetActive(hasContent);

                if (!hasContent)
                {
                    continue;
                }

                string displayName = !string.IsNullOrEmpty(slotData.Name)
                    ? slotData.Name
                    : (slotData.Action != null ? slotData.Action.GetType().Name : "Empty");
                slotTransform.name = "CatalogSlot_" + i + "_" + displayName;

                if (initializeCallbacks && Application.isPlaying)
                {
                    slotUI.Initialize(slotData, i, OnSlotSelected);
                }
                else
                {
                    slotUI.SetData(slotData);
                    slotUI.Refresh();
                }

                SlotUIs.Add(slotUI);
            }

            UpdateItemCount(SlotUIs.Count);
            OnSlotsPopulated();
        }

        private void EnsureSlotCount(int targetCount)
        {
            if (SlotContainer == null)
            {
                return;
            }

            while (SlotContainer.childCount < targetCount)
            {
                Instantiate(SlotPrefab.gameObject, SlotContainer, false);
            }
        }

        private void DeactivateAllSlots()
        {
            if (SlotContainer == null)
            {
                return;
            }

            for (int i = 0; i < SlotContainer.childCount; i++)
            {
                SlotContainer.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void UpdateItemCount(int count)
        {
            if (m_itemCountText != null)
            {
                m_itemCountText.text = $"Total : {count} Item(s)";
            }
        }

        protected override void OnMenuOpened()
        {
            BuildingController.Instance?.SetMode(BuildingMode.None);
        }
    }
}