/// <summary>
/// Project : Easy Build System
/// Class : BuildingController.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers
{
    [RequireComponent(typeof(BuildingInput))]
    public class BuildingController : MonoBehaviour
    {
        private static BuildingController s_instance;

        [SerializeField] private BuildingView[] m_views;
        [SerializeField] private BuildingView m_activeView;
        [SerializeField] private BuildingState[] m_states;
        [SerializeField] private BuildingAudioData m_audioData = new BuildingAudioData();

        protected BuildingState m_activeState;
        protected BuildingInput m_buildingInput;
        protected BuildingMode m_activeMode = BuildingMode.None;
        protected BuildingPart m_selectedPart;

        public static BuildingController Instance { get => s_instance; private set => s_instance = value; }
        public BuildingView[] Views { get => m_views; set => m_views = value; }
        public BuildingView ActiveView => m_activeView;
        public BuildingState[] States { get => m_states; set => m_states = value; }
        public BuildingState ActiveState => m_activeState;
        public BuildingAudioData AudioData => m_audioData;
        public BuildingMode ActiveMode => m_activeMode;
        public BuildingPart SelectedPart => m_selectedPart;

        public BuildingInput BuildingInput
        {
            get
            {
                if (m_buildingInput == null)
                {
                    m_buildingInput = GetComponent<BuildingInput>();
                }
                return m_buildingInput;
            }
        }

        protected virtual void Awake()
        {
            Instance = this;
            m_selectedPart = null;
        }

        protected virtual void Start()
        {
            if (m_views == null || m_views.Length == 0)
            {
                Debug.LogWarning("No BuildingView assigned in the inspector.", this);
            }

            if (m_states == null || m_states.Length == 0)
            {
                Debug.LogWarning("No BuildingState assigned in the inspector.", this);
            }

            if (m_activeView == null && m_views != null && m_views.Length > 0)
            {
                m_activeView = m_views[0];
            }

            SetMode(BuildingMode.None);
        }

        protected virtual void OnEnable()
        {
            Instance = this;
            m_audioData.SubscribeEvents();
        }

        protected virtual void OnDisable()
        {
            ExitState();
            m_audioData.UnsubscribeEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        protected virtual void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_activeView == null)
            {
                return;
            }

            m_activeState?.UpdateState();
        }

        protected virtual void Reset()
        {
            m_views = new BuildingView[1];
            FirstPersonBuildingView fpView = GetComponent<FirstPersonBuildingView>();
            if (fpView == null)
            {
                fpView = gameObject.AddComponent<FirstPersonBuildingView>();
                fpView.hideFlags = HideFlags.HideInInspector;
            }
            m_views[0] = fpView;

            m_states = new BuildingState[5];

            PlacementBuildingState placement = GetComponent<PlacementBuildingState>();
            if (placement == null)
            {
                placement = gameObject.AddComponent<PlacementBuildingState>();
                placement.hideFlags = HideFlags.HideInInspector;
            }
            m_states[(int)BuildingMode.Placement] = placement;

            AdjustmentBuildingState adjustment = GetComponent<AdjustmentBuildingState>();
            if (adjustment == null)
            {
                adjustment = gameObject.AddComponent<AdjustmentBuildingState>();
                adjustment.hideFlags = HideFlags.HideInInspector;
            }
            m_states[(int)BuildingMode.Adjustment] = adjustment;

            DestructionBuildingState destruction = GetComponent<DestructionBuildingState>();
            if (destruction == null)
            {
                destruction = gameObject.AddComponent<DestructionBuildingState>();
                destruction.hideFlags = HideFlags.HideInInspector;
            }
            m_states[(int)BuildingMode.Destruction] = destruction;

            UpgradeBuildingState upgrade = GetComponent<UpgradeBuildingState>();
            if (upgrade == null)
            {
                upgrade = gameObject.AddComponent<UpgradeBuildingState>();
                upgrade.hideFlags = HideFlags.HideInInspector;
            }
            m_states[(int)BuildingMode.Upgrade] = upgrade;

            m_activeView = fpView;
            m_activeState = null;
        }

        public virtual void SetMode(BuildingMode mode)
        {
            m_activeMode = mode;

            BuildingState nextState = GetState(mode);

            if (nextState != null)
            {
                ChangeState(nextState);
            }
            else
            {
                ExitState();
            }

            EventPublisher.Publish(new BuildingControllerEvent.BuildModeChangedEventArgs(mode));
        }

        public virtual void SetView(BuildingViewType viewType)
        {
            if (m_views == null || m_views.Length == 0)
            {
                return;
            }

            for (int i = 0; i < m_views.Length; i++)
            {
                if (m_views[i] != null && m_views[i].ViewType == viewType)
                {
                    m_activeView = m_views[i];
                    EventPublisher.Publish(new BuildingControllerEvent.BuildViewChangedEventArgs(viewType));
                    return;
                }
            }

            Debug.LogWarning("No view found with type " + viewType + ".", this);
        }

        public virtual void ValidAction()
        {
            m_activeState?.OnValidateAction();
        }

        public virtual void CancelAction()
        {
            m_activeState?.OnCancelAction();
        }

        public virtual void RotateAction(int direction)
        {
            if (direction == 0)
            {
                return;
            }

            m_activeState?.OnRotateAction(direction);
        }

        public virtual void SelectPart(BuildingPart part)
        {
            if (!part)
            {
                return;
            }

            BuildingManager manager = BuildingManager.Instance;
            if (manager && !string.IsNullOrEmpty(part.PrefabId) && !manager.GetPartByPrefabId(part.PrefabId))
            {
                return;
            }

            m_selectedPart = part;
            EventPublisher.Publish(new BuildingControllerEvent.BuildSelectionChangedEventArgs(part));
        }

        public virtual BuildingState GetState(BuildingMode mode)
        {
            if (m_states == null)
            {
                return null;
            }

            for (int i = 0; i < m_states.Length; i++)
            {
                if (m_states[i] != null && m_states[i].Mode == mode)
                {
                    return m_states[i];
                }
            }

            return null;
        }

        protected virtual void ChangeState(BuildingState state)
        {
            m_activeState?.ExitState();
            m_activeState = state;
            m_activeState?.EnterState();
        }

        protected virtual void ExitState()
        {
            m_activeState?.ExitState();
            m_activeState = null;
        }

        public void OnValidateButton() => ValidAction();
        public void OnCancelButton() => CancelAction();
        public void OnRotateButton(int direction) => RotateAction(direction);
        public void OnEnterPlacementMode() => SetMode(BuildingMode.Placement);
        public void OnEnterDestructionMode() => SetMode(BuildingMode.Destruction);
        public void OnEnterAdjustmentMode() => SetMode(BuildingMode.Adjustment);
        public void OnExitMode() => SetMode(BuildingMode.None);
    }
}