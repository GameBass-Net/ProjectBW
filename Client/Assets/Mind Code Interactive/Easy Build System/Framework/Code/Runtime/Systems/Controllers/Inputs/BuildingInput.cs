/// <summary>
/// Project : Easy Build System
/// Class : BuildingInput.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs
{
    public class BuildingInput : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_validateActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_cancelActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_rotateActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_selectActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_placementModeActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_destructionModeActionRef;
        [SerializeField] private UnityEngine.InputSystem.InputActionReference m_adjustmentModeActionRef;

        private static int s_enabledInputs;

        private const float AXIS_DEADZONE = 0.0001f;
#else
        [SerializeField] private KeyCode m_validateKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode m_cancelKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode m_placementModeKey = KeyCode.E;
        [SerializeField] private KeyCode m_destructionModeKey = KeyCode.R;
        [SerializeField] private KeyCode m_adjustmentModeKey = KeyCode.T;

        [SerializeField] private bool m_useScrollWheelInput = true;
#endif

        [SerializeField] private bool m_blockWhenPointerOverUI = true;
        [SerializeField] private bool m_enableDirectControls = true;
        [SerializeField] private bool m_useCustomPartsSelection = false;
        [SerializeField, BuildingPartReference] private string[] m_customPartReferences;

        public bool UseCustomPartsSelection { get => m_useCustomPartsSelection; set => m_useCustomPartsSelection = value; }
        public string[] CustomPartReferences { get => m_customPartReferences; set => m_customPartReferences = value; }

        private BuildingController m_controller;
        private int m_selectedPrefabIndex;
        private bool m_pointerOverUI;
        private bool m_isInitialized;

        private Dictionary<string, BuildingPart> m_partCache;
        private bool m_partCacheBuilt;

#if ENABLE_INPUT_SYSTEM
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_enabledInputs = 0;
        }
#endif

        protected virtual void Awake()
        {
            m_controller ??= GetComponent<BuildingController>();
            if (m_controller != null)
            {
                return;
            }
#pragma warning disable CS0618
            m_controller = FindObjectOfType<BuildingController>();
#pragma warning restore CS0618
        }

        protected virtual void OnEnable()
        {
            RegisterInputs();
        }

        protected virtual void OnDisable()
        {
            UnregisterInputs();
        }

        protected virtual void Update()
        {
            UpdateInputs();
        }

        public virtual void Reset()
        {
#if ENABLE_INPUT_SYSTEM
            string resourcePath = "Default Input Actions";
            UnityEngine.InputSystem.InputActionAsset asset = Resources.Load<UnityEngine.InputSystem.InputActionAsset>(resourcePath);
            if (asset == null)
            {
                return;
            }

            m_validateActionRef = CreateActionRef(asset, "Validate");
            m_cancelActionRef = CreateActionRef(asset, "Cancel");
            m_rotateActionRef = CreateActionRef(asset, "Rotate");
            m_selectActionRef = CreateActionRef(asset, "Select");
            m_placementModeActionRef = CreateActionRef(asset, "Placement Mode");
            m_destructionModeActionRef = CreateActionRef(asset, "Destruction Mode");
            m_adjustmentModeActionRef = CreateActionRef(asset, "Adjustment Mode");
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private UnityEngine.InputSystem.InputActionReference CreateActionRef(UnityEngine.InputSystem.InputActionAsset asset, string actionName)
        {
            UnityEngine.InputSystem.InputAction action = asset.FindAction(actionName, throwIfNotFound: false);
            return action != null ? UnityEngine.InputSystem.InputActionReference.Create(action) : null;
        }
#endif

        protected virtual void RegisterInputs()
        {
#if ENABLE_INPUT_SYSTEM
            if (m_validateActionRef != null ||
                m_cancelActionRef != null ||
                m_rotateActionRef != null ||
                m_selectActionRef != null ||
                m_placementModeActionRef != null ||
                m_destructionModeActionRef != null ||
                m_adjustmentModeActionRef != null)
            {
                s_enabledInputs++;

                if (s_enabledInputs == 1)
                {
                    EnableAction(m_validateActionRef);
                    EnableAction(m_cancelActionRef);
                    EnableAction(m_rotateActionRef);
                    EnableAction(m_selectActionRef);
                    EnableAction(m_placementModeActionRef);
                    EnableAction(m_destructionModeActionRef);
                    EnableAction(m_adjustmentModeActionRef);
                }
            }
#endif
        }

        protected virtual void UnregisterInputs()
        {
#if ENABLE_INPUT_SYSTEM
            if (m_validateActionRef != null ||
                m_cancelActionRef != null ||
                m_rotateActionRef != null ||
                m_selectActionRef != null ||
                m_placementModeActionRef != null ||
                m_destructionModeActionRef != null ||
                m_adjustmentModeActionRef != null)
            {
                s_enabledInputs--;

                if (s_enabledInputs <= 0)
                {
                    s_enabledInputs = 0;

                    DisableAction(m_validateActionRef);
                    DisableAction(m_cancelActionRef);
                    DisableAction(m_rotateActionRef);
                    DisableAction(m_selectActionRef);
                    DisableAction(m_placementModeActionRef);
                    DisableAction(m_destructionModeActionRef);
                    DisableAction(m_adjustmentModeActionRef);
                }
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void EnableAction(UnityEngine.InputSystem.InputActionReference actionRef)
        {
            if (actionRef != null)
            {
                actionRef.action.Enable();
            }
        }

        private void DisableAction(UnityEngine.InputSystem.InputActionReference actionRef)
        {
            if (actionRef != null)
            {
                actionRef.action.Disable();
            }
        }
#endif

        protected virtual void UpdateInputs()
        {
            if (!m_isInitialized)
            {
                SelectFirstCustomPart();
                m_isInitialized = true;
            }

            m_pointerOverUI = m_blockWhenPointerOverUI && UIExtensions.IsInteractionActive();

#if !ENABLE_INPUT_SYSTEM
            ProcessLegacyInput();
#else
            ProcessNewInput();
#endif

            HandleAxisInput();
        }

        private void SelectFirstCustomPart()
        {
            if (!m_useCustomPartsSelection || m_customPartReferences == null || m_customPartReferences.Length == 0)
            {
                return;
            }

            EnsurePartCache();

            string partId = m_customPartReferences[0];
            if (string.IsNullOrEmpty(partId))
            {
                return;
            }

            BuildingPart part = FindPartById(partId);
            if (part != null && m_controller)
            {
                m_controller.SelectPart(part);
            }
        }

        private void EnsurePartCache()
        {
            if (m_partCacheBuilt)
            {
                return;
            }

            BuildingManager manager = BuildingManager.Instance;
            if (!manager)
            {
                return;
            }

            m_partCache = new Dictionary<string, BuildingPart>();
            int partCount = manager.GetPartCount();

            for (int i = 0; i < partCount; i++)
            {
                BuildingPart part = manager.GetPartByIndex(i);
                if (part != null && !string.IsNullOrEmpty(part.PrefabId))
                {
                    m_partCache[part.PrefabId] = part;
                }
            }

            m_partCacheBuilt = true;
        }

        private void InvalidatePartCache()
        {
            m_partCache = null;
            m_partCacheBuilt = false;
        }

#if !ENABLE_INPUT_SYSTEM
        protected virtual void ProcessLegacyInput()
        {
            BuildingMenuUI menu = BuildingMenuUI.Instance;
            bool menuJustClosed = menu != null && menu.WasClosedThisFrame;

            if (!menuJustClosed && Input.GetKeyDown(m_validateKey))
            {
                HandleValidate();
            }

            if (Input.GetKeyDown(m_cancelKey))
            {
                HandleCancel();
            }

            if (Input.GetKeyDown(m_placementModeKey))
            {
                HandlePlacementMode();
            }

            if (Input.GetKeyDown(m_destructionModeKey))
            {
                HandleDestructionMode();
            }

            if (Input.GetKeyDown(m_adjustmentModeKey))
            {
                HandleAdjustmentMode();
            }
        }
#else
        protected virtual void ProcessNewInput()
        {
            BuildingMenuUI menu = BuildingMenuUI.Instance;
            bool menuJustClosed = menu != null && menu.WasClosedThisFrame;

            if (!menuJustClosed && m_validateActionRef != null && m_validateActionRef.action.WasPerformedThisFrame())
            {
                HandleValidate();
            }

            if (m_cancelActionRef != null && m_cancelActionRef.action.WasPerformedThisFrame())
            {
                HandleCancel();
            }

            if (m_placementModeActionRef != null && m_placementModeActionRef.action.WasPerformedThisFrame())
            {
                HandlePlacementMode();
            }

            if (m_destructionModeActionRef != null && m_destructionModeActionRef.action.WasPerformedThisFrame())
            {
                HandleDestructionMode();
            }

            if (m_adjustmentModeActionRef != null && m_adjustmentModeActionRef.action.WasPerformedThisFrame())
            {
                HandleAdjustmentMode();
            }
        }
#endif

        protected virtual void HandleAxisInput()
        {
            if (m_pointerOverUI || !m_controller)
            {
                return;
            }

            float value = GetAxisValue();
#if ENABLE_INPUT_SYSTEM
            if (Mathf.Abs(value) <= AXIS_DEADZONE)
            {
                return;
            }
#else
            if (value == 0f)
            {
                return;
            }
#endif

            int direction = value > 0f ? 1 : -1;

            if (m_controller.ActiveMode == BuildingMode.Placement ||
                            m_controller.ActiveMode == BuildingMode.Adjustment)
            {
                m_controller.RotateAction(direction);
                return;
            }

            if (m_enableDirectControls && m_useCustomPartsSelection)
            {
                HandleSelect(direction);
            }
        }

        protected virtual float GetAxisValue()
        {
#if !ENABLE_INPUT_SYSTEM
            if (!m_useScrollWheelInput)
            {
                return 0f;
            }

            return Input.GetAxis("Mouse ScrollWheel");
#else
            float value = 0f;

            if (m_rotateActionRef != null)
            {
                value += m_rotateActionRef.action.ReadValue<float>();
            }

            if (m_selectActionRef != null)
            {
                value += m_selectActionRef.action.ReadValue<float>();
            }

            return value;
#endif
        }

        protected virtual void HandleValidate()
        {
            if (m_pointerOverUI || !m_controller)
            {
                return;
            }

            m_controller.ValidAction();
        }

        protected virtual void HandleCancel()
        {
            if (m_pointerOverUI || !m_controller)
            {
                return;
            }

            m_controller.CancelAction();
        }

        protected virtual void HandleSelect(int direction)
        {
            if (m_pointerOverUI || !m_controller || m_controller.ActiveMode != BuildingMode.None)
            {
                return;
            }

            if (m_useCustomPartsSelection)
            {
                SelectCustomPart(direction);
            }
            else
            {
                SelectStandardPart(direction);
            }
        }

        protected virtual void SelectCustomPart(int direction)
        {
            string[] partIdArray = m_customPartReferences;
            int partCount = partIdArray?.Length ?? 0;

            if (partCount == 0)
            {
                return;
            }

            EnsurePartCache();

            int currentIndex = (m_selectedPrefabIndex + direction + partCount) % partCount;

            for (int i = 0; i < partCount; i++)
            {
                string partId = partIdArray[currentIndex];
                if (!string.IsNullOrEmpty(partId))
                {
                    BuildingPart part = FindPartById(partId);
                    if (part != null)
                    {
                        m_selectedPrefabIndex = currentIndex;
                        m_controller.SelectPart(part);
                        return;
                    }
                }

                currentIndex = (currentIndex + direction + partCount) % partCount;
            }
        }

        protected virtual void SelectStandardPart(int direction)
        {
            BuildingManager manager = BuildingManager.Instance;
            if (!manager)
            {
                return;
            }

            int totalCount = manager.GetPartCount();
            if (totalCount == 0)
            {
                return;
            }

            m_selectedPrefabIndex = (m_selectedPrefabIndex + direction + totalCount) % totalCount;
            BuildingPart part = manager.GetPartByIndex(m_selectedPrefabIndex);

            if (part)
            {
                m_controller.SelectPart(part);
            }
        }

        private BuildingPart FindPartById(string partId)
        {
            if (string.IsNullOrEmpty(partId))
            {
                return null;
            }

            EnsurePartCache();

            if (m_partCache != null && m_partCache.TryGetValue(partId, out BuildingPart part) && part)
            {
                return part;
            }

            InvalidatePartCache();
            EnsurePartCache();

            if (m_partCache != null && m_partCache.TryGetValue(partId, out part) && part)
            {
                return part;
            }

            return null;
        }

        protected virtual void HandlePlacementMode()
        {
            if (m_pointerOverUI || !m_controller || !m_enableDirectControls)
            {
                return;
            }

            m_controller.SetMode(BuildingMode.Placement);
        }

        protected virtual void HandleDestructionMode()
        {
            if (m_pointerOverUI || !m_controller || !m_enableDirectControls)
            {
                return;
            }

            m_controller.SetMode(BuildingMode.Destruction);
        }

        protected virtual void HandleAdjustmentMode()
        {
            if (m_pointerOverUI || !m_controller || !m_enableDirectControls)
            {
                return;
            }

            m_controller.SetMode(BuildingMode.Adjustment);
        }
    }
}