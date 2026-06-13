/// <summary>
/// Project : Easy Build System
/// Class : BuildingRadialMenuUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
{
    public class BuildingRadialMenuUI : BuildingMenuUI
    {
        private static BuildingRadialMenuUI s_instance;

#if ENABLE_INPUT_SYSTEM
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_validateAction;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_selectAction;
#endif

        [SerializeField, NotNull] private Transform m_slotContainer;
        [SerializeField, NotNull] private BuildingSlotUI m_slotPrefab;
        [SerializeField] private Animator m_menuAnimator;
        [SerializeField] private string m_openMenuState = "Open";
        [SerializeField] private string m_closeMenuState = "Close";
        [SerializeField] private GameObject[] m_hideWhenMenuOpen;
        [SerializeField, NotNull] private RectTransform m_content;
        [SerializeField] private AudioClipPlayer m_slotClickSound;
        [SerializeField] private AudioClipPlayer m_slotHighlightSound;
        [SerializeField] private float m_slotSpacing = 160f;
        [SerializeField, NotNull] private Image m_selectionFillImage;
        [SerializeField] private Image m_selectionHighlightImage;
        [SerializeField] private Image m_selectionIndicatorImage;
        [SerializeField, NotNull] private RawImage m_selectionIcon;
        [SerializeField, NotNull] private Text m_selectionText;
        [SerializeField] private Text m_selectionDescription;
        [SerializeField] private int m_defaultSelectedSlotIndex;

        private readonly List<Transform> m_categoryRoots = new List<Transform>();
        private int m_hoveredSlotIndex = -1;
        private int m_lastPlayedSoundSlotIndex = -1;
        private bool m_touchActive = false;
        private Vector2 m_touchStartPosition = Vector2.zero;

        public static new BuildingRadialMenuUI Instance { get => s_instance; private set => s_instance = value; }

        public Transform SlotContainer => m_slotContainer;

        public BuildingSlotUI SlotPrefab => m_slotPrefab;

        public Animator MenuAnimator => m_menuAnimator;

        public string OpenMenuState { get => m_openMenuState; set => m_openMenuState = value; }

        public string CloseMenuState { get => m_closeMenuState; set => m_closeMenuState = value; }

        public GameObject[] HideWhenMenuOpen { get => m_hideWhenMenuOpen; set => m_hideWhenMenuOpen = value; }

        public RectTransform Content => m_content;

        public float SlotSpacing => m_slotSpacing;

        public Image SelectionFillImage => m_selectionFillImage;

        public Image SelectionHighlightImage => m_selectionHighlightImage;

        public Image SelectionIndicatorImage => m_selectionIndicatorImage;

        public RawImage SelectionIcon => m_selectionIcon;

        public Text SelectionText => m_selectionText;

        public Text SelectionDescription => m_selectionDescription;

        public int DefaultSelectedSlotIndex => m_defaultSelectedSlotIndex;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
#if ENABLE_INPUT_SYSTEM
            m_validateAction?.action.Enable();
            m_selectAction?.action.Enable();
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if ENABLE_INPUT_SYSTEM
            m_validateAction?.action.Disable();
            m_selectAction?.action.Disable();
#endif
        }

        protected override void Update()
        {
            base.Update();

            if (SlotUIs.Count > 0 && m_hoveredSlotIndex >= 0 && m_hoveredSlotIndex < SlotUIs.Count)
            {
                RefreshSelection(m_hoveredSlotIndex, false);
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

                int categoryIndex = Mathf.Clamp(
                    DefaultCategoryIndex,
                    0,
                    Mathf.Max(0, Categories.Count - 1));

                if (Categories.Count > 0)
                {
                    SelectCategory(categoryIndex);
                    ClearSelectionUI();
                    InitializeSelection();
                }
            };
        }
#endif

        public override void SelectCategory(int categoryIndex)
        {
            base.SelectCategory(categoryIndex);

            if (categoryIndex < 0 || categoryIndex >= Categories.Count)
            {
                return;
            }

            EnsureCategoryContainers();

            for (int i = 0; i < m_categoryRoots.Count; i++)
            {
                if (m_categoryRoots[i] != null)
                {
                    m_categoryRoots[i].gameObject.SetActive(i == categoryIndex);
                }
            }

            RebuildSlotsForCategory(categoryIndex, Application.isPlaying);

            if (SlotUIs.Count > 0)
            {
                m_hoveredSlotIndex = Mathf.Clamp(m_hoveredSlotIndex, 0, SlotUIs.Count - 1);
                RefreshSelection(m_hoveredSlotIndex, true);
            }
            else
            {
                ClearSelectionUI();
            }
        }

        public void RefreshSelection(int slotIndex, bool instant)
        {
            if (SlotUIs.Count == 0 || slotIndex < 0 || slotIndex >= SlotUIs.Count)
            {
                return;
            }

            PositionSlotsRadially();

            for (int i = 0; i < SlotUIs.Count; i++)
            {
                SlotUIs[i].SetHighlight(i == slotIndex);
            }

            UpdateSelectionUI(slotIndex, instant);
        }

        public void PositionSlotsRadially()
        {
            if (SlotUIs.Count == 0)
            {
                return;
            }

            float buttonFillAmount = 1f / SlotUIs.Count;
            float fillRadius = buttonFillAmount * 360f;

            for (int i = 0; i < SlotUIs.Count; i++)
            {
                float slotRot = i * fillRadius + fillRadius * 0.5f;
                float radians = (slotRot - 90) * Mathf.Deg2Rad;
                Vector2 slotPos = new Vector2(m_slotSpacing * Mathf.Cos(radians), -m_slotSpacing * Mathf.Sin(radians));
                SlotUIs[i].transform.localPosition = slotPos;
            }
        }

        public void ClearSelectionUI()
        {
            if (m_selectionFillImage != null)
            {
                m_selectionFillImage.fillAmount = 0f;
                m_selectionFillImage.transform.localRotation = Quaternion.identity;
            }

            if (m_selectionHighlightImage != null)
            {
                m_selectionHighlightImage.fillAmount = 0f;
                m_selectionHighlightImage.transform.localRotation = Quaternion.identity;
            }

            if (m_selectionIcon != null)
            {
                m_selectionIcon.texture = null;
                m_selectionIcon.enabled = false;
            }

            if (m_selectionText != null)
            {
                m_selectionText.text = "";
            }

            if (m_selectionDescription != null)
            {
                m_selectionDescription.gameObject.SetActive(false);
                m_selectionDescription.text = "";
            }
        }

        public void ForceRebuildAllSlots()
        {
            if (m_categoryRoots != null)
            {
                for (int i = 0; i < m_categoryRoots.Count; i++)
                {
                    if (m_categoryRoots[i] != null)
                    {
                        ClearContainer(m_categoryRoots[i]);
                    }
                }
            }

            EnsureCategoryContainers();
            SelectCategory(DefaultCategoryIndex);
        }

        protected override void UpdateInput()
        {
            if (!IsOpen || SlotUIs.Count == 0)
            {
                return;
            }

            ProcessTouchInput();

            if (InputMode == MenuInputMode.Gamepad)
            {
                Vector2 inputDir = GetInputDirection();
                if (inputDir.magnitude >= 0.4f)
                {
                    UpdateHoveredSlotFromDirection(inputDir);
                }

                return;
            }

            if (InputMode == MenuInputMode.Standalone)
            {
#if ENABLE_INPUT_SYSTEM
                if (m_validateAction != null && m_validateAction.action.WasPressedThisFrame())
                {
                    m_validateAction.action.Reset();
                    if (m_hoveredSlotIndex >= 0 && m_hoveredSlotIndex < SlotUIs.Count)
                    {
                        m_slotClickSound.Play();
                        OnSlotSelected(m_hoveredSlotIndex);
                    }
                    return;
                }
#else
                if (Input.GetMouseButtonDown(0))
                {
                    if (m_hoveredSlotIndex >= 0 && m_hoveredSlotIndex < SlotUIs.Count)
                    {
                        m_slotClickSound.Play();
                        OnSlotSelected(m_hoveredSlotIndex);
                    }
                    return;
                }
#endif
                Vector2 mouseDir = GetMouseLocalPosition();
                if (mouseDir.magnitude >= 0.4f)
                {
                    UpdateHoveredSlotFromDirection(mouseDir);
                }
            }
        }

        private const float TOUCH_DRAG_THRESHOLD = 20f;

        private void ProcessTouchInput()
        {
#if ENABLE_INPUT_SYSTEM
            try
            {
                UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
                var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

                if (touches.Count > 0)
                {
                    var touch = touches[0];

                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        m_touchActive = true;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.screenPosition, null, out m_touchStartPosition);
                    }
                    else if (m_touchActive && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.screenPosition, null, out Vector2 localPos);
                        if (localPos.magnitude >= TOUCH_DRAG_THRESHOLD)
                        {
                            UpdateHoveredSlotFromDirection(localPos);
                        }
                    }
                    else if (m_touchActive && touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        m_touchActive = false;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.screenPosition, null, out Vector2 endPos);
                        Vector2 totalDelta = endPos - m_touchStartPosition;

                        if (totalDelta.magnitude < TOUCH_DRAG_THRESHOLD)
                        {
                            int tapped = GetNearestSlotIndex(endPos);
                            if (tapped >= 0)
                            {
                                m_hoveredSlotIndex = tapped;
                                RefreshSelection(m_hoveredSlotIndex, true);
                            }
                        }

                        if (m_hoveredSlotIndex >= 0 && m_hoveredSlotIndex < SlotUIs.Count)
                        {
                            m_slotClickSound.Play();
                            OnSlotSelected(m_hoveredSlotIndex);
                        }
                    }
                }
                else
                {
                    m_touchActive = false;
                }
            }
            catch { }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        Canvas canvas = m_content.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (touch.phase == TouchPhase.Began)
        {
            m_touchActive = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.position, uiCamera, out m_touchStartPosition);
        }
        else if (m_touchActive && touch.phase == TouchPhase.Moved)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.position, uiCamera, out Vector2 localPos);
            if (localPos.magnitude >= TOUCH_DRAG_THRESHOLD)
            {
                UpdateHoveredSlotFromDirection(localPos);
            }
        }
        else if (m_touchActive && touch.phase == TouchPhase.Ended)
        {
            m_touchActive = false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, touch.position, uiCamera, out Vector2 endPos);
            Vector2 totalDelta = endPos - m_touchStartPosition;

            if (totalDelta.magnitude < TOUCH_DRAG_THRESHOLD)
            {
                int tapped = GetNearestSlotIndex(endPos);
                if (tapped >= 0)
                {
                    m_hoveredSlotIndex = tapped;
                    RefreshSelection(m_hoveredSlotIndex, true);
                }
            }

            if (m_hoveredSlotIndex >= 0 && m_hoveredSlotIndex < SlotUIs.Count)
            {
                m_slotClickSound.Play();
                OnSlotSelected(m_hoveredSlotIndex);
            }
        }
    }
    else
    {
        m_touchActive = false;
    }
#endif
        }

        private int GetNearestSlotIndex(Vector2 localPos)
        {
            float best = float.MaxValue;
            int index = -1;

            for (int i = 0; i < SlotUIs.Count; i++)
            {
                float dist = Vector2.Distance(localPos, SlotUIs[i].transform.localPosition);
                if (dist < best)
                {
                    best = dist;
                    index = i;
                }
            }

            return best <= m_slotSpacing ? index : -1;
        }

        private void UpdateHoveredSlotFromDirection(Vector2 direction)
        {
            float inputAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            if (inputAngle < 0f)
            {
                inputAngle += 360f;
            }

            float segmentAngle = 360f / SlotUIs.Count;
            float offsetAngle = segmentAngle * 0.5f;

            int nearestSlotIndex = Mathf.RoundToInt((inputAngle - offsetAngle) / segmentAngle);
            nearestSlotIndex = ((nearestSlotIndex % SlotUIs.Count) + SlotUIs.Count) % SlotUIs.Count;

            if (nearestSlotIndex != m_hoveredSlotIndex)
            {
                m_hoveredSlotIndex = nearestSlotIndex;
                OnSlotHovered(m_hoveredSlotIndex);

                if (m_slotHighlightSound != null && m_hoveredSlotIndex != m_lastPlayedSoundSlotIndex)
                {
                    m_lastPlayedSoundSlotIndex = m_hoveredSlotIndex;
                    m_slotHighlightSound.Play();
                }
            }
        }

        private Vector2 GetMouseLocalPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Mouse.current != null)
            {
                Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, mouseScreenPos, null, out Vector2 localPos);
                return localPos;
            }
            return Vector2.zero;
#else
            if (m_content != null)
            {
                Canvas canvas = m_content.GetComponentInParent<Canvas>();
                Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_content, Input.mousePosition, uiCamera, out Vector2 localPos);
                return localPos;
            }
            return Vector2.zero;
#endif
        }

        protected override void OnSlotsPopulated()
        {
            if (SlotUIs.Count == 0)
            {
                ClearSelectionUI();
                return;
            }

            PositionSlotsRadially();
            InitializeSelection();
        }

        protected override void OnSlotsSelected(int slotIndex)
        {
            SelectedSlotIndex = slotIndex;
            if (slotIndex >= 0 && slotIndex < SlotUIs.Count)
            {
                RefreshSelection(slotIndex, true);
            }
        }

        protected override void OnMenuOpened()
        {
            BuildingController.Instance?.SetMode(BuildingMode.None);

            for (int i = 0; i < m_hideWhenMenuOpen.Length; i++)
            {
                if (m_hideWhenMenuOpen[i] != null)
                {
                    m_hideWhenMenuOpen[i].SetActive(false);
                }
            }

            if (m_menuAnimator != null && !string.IsNullOrEmpty(m_openMenuState))
            {
                m_menuAnimator.Play(m_openMenuState);
            }

            InitializeSelection();
        }

        protected override void OnMenuClosed()
        {
            for (int i = 0; i < m_hideWhenMenuOpen.Length; i++)
            {
                if (m_hideWhenMenuOpen[i] != null)
                {
                    m_hideWhenMenuOpen[i].SetActive(true);
                }
            }

            if (Application.isPlaying && m_menuAnimator != null && !string.IsNullOrEmpty(m_closeMenuState))
            {
                m_menuAnimator.Play(m_closeMenuState);
            }
        }

        private void EnsureCategoryContainers()
        {
            if (SlotContainer == null)
            {
                return;
            }

            m_categoryRoots.Clear();

            int wantedCount = Categories.Count;
            int existingCount = SlotContainer.childCount;
            int activeIndex = CurrentCategoryIndex >= 0
                ? CurrentCategoryIndex
                : Mathf.Clamp(DefaultCategoryIndex, 0, Mathf.Max(0, wantedCount - 1));

            for (int i = 0; i < wantedCount; i++)
            {
                Transform childTransform = i < existingCount ? SlotContainer.GetChild(i) : null;

                if (childTransform == null)
                {
                    string categoryName = Categories[i]?.Name ?? "Unnamed";
                    GameObject categoryGO = new GameObject("Category_" + i + "_" + categoryName, typeof(RectTransform));
                    categoryGO.transform.SetParent(SlotContainer, false);
                    childTransform = categoryGO.transform;
                }
                else
                {
                    childTransform.name = "Category_" + i + "_" + (Categories[i]?.Name ?? "Unnamed");
                }

                childTransform.gameObject.SetActive(i == activeIndex);
                m_categoryRoots.Add(childTransform);
            }
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

        private void RebuildSlotsForCategory(int categoryIndex, bool initializeCallbacks)
        {
            SlotUIs.Clear();

            if (categoryIndex < 0 || categoryIndex >= m_categoryRoots.Count)
            {
                return;
            }

            Transform categoryRoot = m_categoryRoots[categoryIndex];
            BuildingCategoryData category = categoryIndex < Categories.Count ? Categories[categoryIndex] : null;

            if (categoryRoot == null || category == null)
            {
                return;
            }

            int targetSlotCount = category.Slots?.Count ?? 0;

            while (categoryRoot.childCount < targetSlotCount)
            {
                Instantiate(SlotPrefab.gameObject, categoryRoot, false);
            }

            for (int i = 0; i < categoryRoot.childCount; i++)
            {
                Transform slotTransform = categoryRoot.GetChild(i);
                BuildingSlotUI slotUI = slotTransform.GetComponent<BuildingSlotUI>();

                if (i >= targetSlotCount || slotUI == null)
                {
                    slotTransform.gameObject.SetActive(false);
                    continue;
                }

                BuildingSlotData slotData = category.Slots[i];
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
                slotTransform.name = "RadialSlot_" + i + "_" + displayName;

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

            UpdateHighlightedSlot(category);
        }

        private void UpdateHighlightedSlot(BuildingCategoryData category)
        {
            for (int i = 0; i < SlotUIs.Count; i++)
            {
                SlotUIs[i].SetHighlight(false);
            }

            if (category.Slots == null || SelectedSlotIndex < 0 || SelectedSlotIndex >= category.Slots.Count)
            {
                return;
            }

            int visibleIndex = -1;
            int visibleCount = 0;

            for (int i = 0; i < category.Slots.Count; i++)
            {
                BuildingSlotData slotData = category.Slots[i];
                bool hasContent = slotData != null && (!string.IsNullOrEmpty(slotData.Name) || slotData.GetIcon() != null || slotData.Action != null);

                if (!hasContent)
                {
                    continue;
                }

                if (i == SelectedSlotIndex)
                {
                    visibleIndex = visibleCount;
                    break;
                }

                visibleCount++;
            }

            if (visibleIndex >= 0 && visibleIndex < SlotUIs.Count)
            {
                SlotUIs[visibleIndex].SetHighlight(true);
            }
        }

        private Vector2 GetInputDirection()
        {
#if ENABLE_INPUT_SYSTEM
            if (m_selectAction != null && m_selectAction.action != null)
            {
                Vector2 axis = m_selectAction.action.ReadValue<Vector2>();
                if (axis.sqrMagnitude > 0.01f)
                {
                    return axis;
                }
            }

            return Vector2.zero;
#else
            if (Input.GetJoystickNames().Length > 0)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                Vector2 axis = new Vector2(h, v);
                if (axis.sqrMagnitude > 0.01f)
                {
                    return axis;
                }
            }

            return Vector2.zero;
#endif
        }



        private void UpdateSelectionUI(int slotIndex, bool instant)
        {
            float buttonFillAmount = 1f / SlotUIs.Count;
            float fillRadius = buttonFillAmount * 360f;
            float slotRot = slotIndex * fillRadius + fillRadius * 0.5f;
            Quaternion targetRot = Quaternion.Euler(0, 0, -(slotRot - buttonFillAmount * 180f));

            if (m_selectionFillImage != null)
            {
                m_selectionFillImage.transform.localRotation = instant ? targetRot : Quaternion.Slerp(m_selectionFillImage.transform.localRotation, targetRot, 10f * Time.deltaTime);
                m_selectionFillImage.fillAmount = buttonFillAmount;
            }

            if (m_selectionHighlightImage != null)
            {
                m_selectionHighlightImage.transform.localRotation = instant ? targetRot : Quaternion.Slerp(m_selectionHighlightImage.transform.localRotation, targetRot, 10f * Time.deltaTime);
                m_selectionHighlightImage.fillAmount = buttonFillAmount;
            }

            if (m_selectionIndicatorImage != null)
            {
                Quaternion indicatorRot = targetRot * Quaternion.Euler(0, 0, -fillRadius * 0.5f);
                m_selectionIndicatorImage.rectTransform.localRotation = instant ? indicatorRot : Quaternion.Slerp(m_selectionIndicatorImage.rectTransform.localRotation, indicatorRot, 10f * Time.deltaTime);
            }

            BuildingSlotData slotData = CurrentCategory?.Slots != null && slotIndex < CurrentCategory.Slots.Count
                ? CurrentCategory.Slots[slotIndex]
                : null;

            Texture2D slotIcon = slotData?.GetIcon();
            if (m_selectionIcon != null)
            {
                m_selectionIcon.texture = slotIcon;
                m_selectionIcon.enabled = slotIcon != null;
            }

            if (m_selectionText != null)
            {
                m_selectionText.text = slotData?.Name ?? "";
            }

            if (m_selectionDescription != null)
            {
                m_selectionDescription.text = slotData?.Description ?? "";
                m_selectionDescription.gameObject.SetActive(!string.IsNullOrEmpty(slotData?.Description));
            }
        }

        private void InitializeSelection()
        {
            if (SlotUIs.Count > 0)
            {
                m_hoveredSlotIndex = Mathf.Clamp(m_defaultSelectedSlotIndex, 0, SlotUIs.Count - 1);
                RefreshSelection(m_hoveredSlotIndex, true);
            }
        }
    }
}