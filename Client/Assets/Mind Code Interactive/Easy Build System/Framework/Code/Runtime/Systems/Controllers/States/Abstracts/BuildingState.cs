/// <summary>
/// Project : Easy Build System
/// Class : BuildingState.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts
{
    public abstract class BuildingState : MonoBehaviour
    {
        [SerializeField] private BuildingController m_buildingController;
        [SerializeField] private bool m_cancelStateAfterValidation;
        [SerializeField] private bool m_showLogs;

        public BuildingController BuildingController
        {
            get => m_buildingController ? m_buildingController : (m_buildingController = GetComponent<BuildingController>());
        }

        public bool CancelStateAfterValidation { get => m_cancelStateAfterValidation; set => m_cancelStateAfterValidation = value; }
        public bool ShowLogs { get => m_showLogs; set => m_showLogs = value; }

        public abstract BuildingMode Mode { get; }

        public virtual string Name => Mode.ToString();

        protected BuildingView View => BuildingController?.ActiveView;

        public virtual void EnterState() { }
        public virtual void ExitState() { }
        public virtual void UpdateState() { }
        public virtual void OnRotateAction(int direction) { }

        public virtual void OnValidateAction()
        {
            if (m_cancelStateAfterValidation)
            {
                BuildingController.SetMode(BuildingMode.None);
            }
        }

        public virtual void OnCancelAction(bool cancelMode = true)
        {
            EventPublisher.Publish(new BuildingStateEvent.CancelAttemptEventArgs(Mode));
            if (cancelMode)
            {
                BuildingController.SetMode(BuildingMode.None);
            }
        }

        protected bool CheckValidity(BuildingPart part, BuildingMode mode, out ConditionResult result)
        {
            result = part.ConditionSystem.EvaluateConditions(mode);

            if (!result.IsValid)
            {
                LogWarning(result.Reason);
                return false;
            }

            if (View != null && !View.IsWithinValidDistance(part.transform.position))
            {
                result = new ConditionResult(false, "Placement distance is out of range.");
                EventPublisher.Publish(new Parts.Events.BuildingPartEvent.ConditionFailedEventArgs(part, null, mode, result.Reason));
                LogWarning(result.Reason);
                return false;
            }

            return true;
        }

        protected void PublishValidateAttempt(ConditionResult result)
        {
            EventPublisher.Publish(new BuildingStateEvent.ValidateAttemptEventArgs(Mode, result));
        }

        protected void LogWarning(string message)
        {
            if (m_showLogs && !string.IsNullOrEmpty(message))
            {
                Debug.LogWarning(message);
            }
        }
    }
}