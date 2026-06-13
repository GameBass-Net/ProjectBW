/// <summary>
/// Project : Easy Build System
/// Class : BuildingMenuUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts
{
    public abstract class BuildingMenuUI : MonoBehaviour
    {
        public enum MenuInputMode { Standalone, Mobile, Gamepad }

        [SerializeField] private List<BuildingCategoryData> m_categories = new List<BuildingCategoryData>();
        [SerializeField] private int m_defaultCategoryIndex;
        [SerializeField] private MenuInputMode m_inputMode = MenuInputMode.Standalone;

#if ENABLE_INPUT_SYSTEM
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_toggleAction;
#else
        [SerializeField] private KeyCode m_toggleKey = KeyCode.Tab;
#endif

        [SerializeField] private bool m_lockCursor = true;
        [SerializeField] private AudioClipPlayer m_openAudio;
        [SerializeField] private AudioClipPlayer m_closeAudio;
        [SerializeField] private CanvasGroup m_canvasGroup;

        private readonly List<BuildingSlotUI> m_slotUIs = new List<BuildingSlotUI>();
        private BuildingCategoryData m_currentCategory;
        private int m_currentCategoryIndex = -1;
        private int m_selectedSlotIndex = -1;
        private bool m_wasCursorLocked;
        private bool m_inputEnabled = true;
        private int m_closedFrame = -1;

        public bool WasClosedThisFrame => Time.frameCount == m_closedFrame;

        public static BuildingMenuUI Instance;

        public List<BuildingCategoryData> Categories => m_categories;

        public int DefaultCategoryIndex => m_defaultCategoryIndex;

        public MenuInputMode InputMode => m_inputMode;

        public List<BuildingSlotUI> SlotUIs => m_slotUIs;

        public BuildingCategoryData CurrentCategory { get => m_currentCategory; set => m_currentCategory = value; }

        public int CurrentCategoryIndex { get => m_currentCategoryIndex; set => m_currentCategoryIndex = value; }

        public int SelectedSlotIndex { get => m_selectedSlotIndex; set => m_selectedSlotIndex = value; }

        public bool IsOpen
        {
            get
            {
                if (m_canvasGroup == null)
                {
                    return false;
                }

                return m_canvasGroup.alpha > 0.99f;
            }
        }

        public bool InputEnabled { get => m_inputEnabled; set => m_inputEnabled = value; }

        public event Action<int> OnSlotHoveredEvent;

        protected virtual void Awake()
        {
            Instance = this;
            EventPublisher.Subscribe<BuildingStateEvent.PlacedEventArgs>(OnBuildingPlaced);
        }

        protected virtual void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            m_toggleAction?.action.Enable();
#endif
        }

        protected virtual void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            m_toggleAction?.action.Disable();
#endif
        }

        protected virtual void Start()
        {
            m_wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;

            InitializeMenu();

            if (IsOpen)
            {
                CloseMenu();
            }
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            HandleToggleInput();

            if (!IsOpen)
            {
                return;
            }

            UpdateInput();
        }

        public virtual void SelectCategory(int categoryIndex)
        {
            if (categoryIndex < 0 || categoryIndex >= m_categories.Count)
            {
                m_currentCategory = null;
                m_currentCategoryIndex = -1;
                return;
            }

            m_currentCategoryIndex = categoryIndex;
            m_currentCategory = m_categories[categoryIndex];
        }

        public virtual void OpenMenu()
        {
            if (Application.isPlaying)
            {
                m_wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
                SetCursorState(false);
                m_openAudio.Play();
            }

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.interactable = true;
                m_canvasGroup.blocksRaycasts = true;
            }

            OnMenuOpened();
        }

        public virtual void CloseMenu()
        {
            if (Application.isPlaying)
            {
                SetCursorState(m_wasCursorLocked);
                m_closeAudio.Play();
            }

            m_closedFrame = Time.frameCount;

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.interactable = false;
                m_canvasGroup.blocksRaycasts = false;
            }

            OnMenuClosed();
        }

        private void InitializeMenu()
        {
            int categoryIndex = m_currentCategoryIndex >= 0
                ? m_currentCategoryIndex
                : Mathf.Clamp(m_defaultCategoryIndex, 0, Mathf.Max(0, m_categories.Count - 1));

            if (m_categories.Count > 0)
            {
                SelectCategory(categoryIndex);
            }
            else
            {
                m_currentCategory = null;
                m_currentCategoryIndex = -1;
            }
        }

        private void HandleToggleInput()
        {
            if (!m_inputEnabled)
            {
                return;
            }

            if (m_inputMode == MenuInputMode.Mobile)
            {
                return;
            }

            if (UIExtensions.IsInputFieldInFocus())
            {
                return;
            }

#if !ENABLE_INPUT_SYSTEM
            if (Input.GetKeyDown(m_toggleKey))
            {
                if (IsOpen)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
#else
            if (m_toggleAction?.action.triggered == true)
            {
                if (IsOpen)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
#endif
        }

        private void SetCursorState(bool locked)
        {
            if (m_inputMode == MenuInputMode.Gamepad)
            {
                return;
            }

            if (!m_lockCursor)
            {
                return;
            }

            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        protected virtual void OnSlotHovered(int slotIndex)
        {
            OnSlotHoveredEvent?.Invoke(slotIndex);
        }

        protected virtual void OnSlotSelected(int slotIndex)
        {
            if (m_currentCategory?.Slots == null || slotIndex < 0 || slotIndex >= m_currentCategory.Slots.Count)
            {
                return;
            }

            BuildingSlotData slotData = m_currentCategory.Slots[slotIndex];
            if (slotData == null)
            {
                return;
            }

            m_selectedSlotIndex = slotIndex;
            slotData.EnsureAction(this);
            slotData.Execute();

            OnSlotHoveredEvent?.Invoke(slotIndex);

            OnSlotsSelected(slotIndex);
        }

        protected virtual void OnBuildingPlaced(BuildingStateEvent.PlacedEventArgs args)
        {
            if (args.Part == null || CurrentCategory?.Slots == null)
            {
                return;
            }

            int slotIndex = SelectedSlotIndex;
            if (slotIndex < 0 || slotIndex >= CurrentCategory.Slots.Count)
            {
                return;
            }

            BuildingSlotData slotData = CurrentCategory.Slots[slotIndex];
            if (slotData == null || !slotData.ConsumeOne() || slotIndex >= m_slotUIs.Count)
            {
                return;
            }

            m_slotUIs[slotIndex].Refresh();
        }

        protected virtual void OnSlotsPopulated() { }

        protected virtual void OnSlotsSelected(int slotIndex) { }

        protected virtual void OnMenuOpened() { }

        protected virtual void OnMenuClosed() { }

        protected virtual void UpdateInput() { }
    }
}