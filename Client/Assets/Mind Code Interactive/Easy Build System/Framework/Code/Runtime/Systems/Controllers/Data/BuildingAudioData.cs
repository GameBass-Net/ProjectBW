/// <summary>
/// Project : Easy Build System
/// Class : BuildingAudioData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Data
{
    [Serializable]
    public class BuildingAudioData
    {
        [SerializeField] private AudioClipPlayer m_enterPlacementModeClip;
        [SerializeField] private AudioClipPlayer m_placementValidClip;
        [SerializeField] private AudioClipPlayer m_placementFailedClip;
        [SerializeField] private AudioClipPlayer m_cancelPlacementClip;
        [SerializeField] private AudioClipPlayer m_exitPlacementModeClip;

        [SerializeField] private AudioClipPlayer m_enterDestructionModeClip;
        [SerializeField] private AudioClipPlayer m_destructionValidClip;
        [SerializeField] private AudioClipPlayer m_destructionFailedClip;
        [SerializeField] private AudioClipPlayer m_cancelDestructionClip;
        [SerializeField] private AudioClipPlayer m_exitDestructionModeClip;

        [SerializeField] private AudioClipPlayer m_enterAdjustmentModeClip;
        [SerializeField] private AudioClipPlayer m_adjustmentValidClip;
        [SerializeField] private AudioClipPlayer m_adjustmentFailedClip;
        [SerializeField] private AudioClipPlayer m_cancelAdjustmentClip;
        [SerializeField] private AudioClipPlayer m_exitAdjustmentModeClip;

        [SerializeField] private AudioClipPlayer m_enterUpgradeModeClip;
        [SerializeField] private AudioClipPlayer m_upgradeValidClip;
        [SerializeField] private AudioClipPlayer m_upgradeFailedClip;
        [SerializeField] private AudioClipPlayer m_cancelUpgradeClip;
        [SerializeField] private AudioClipPlayer m_exitUpgradeModeClip;

        private BuildingMode m_lastMode = BuildingMode.None;

        public void SubscribeEvents()
        {
            EventPublisher.Subscribe<BuildingControllerEvent.BuildModeChangedEventArgs>(OnModeChanged);
            EventPublisher.Subscribe<BuildingStateEvent.ValidateAttemptEventArgs>(OnValidateAttempt);
            EventPublisher.Subscribe<BuildingStateEvent.CancelAttemptEventArgs>(OnCancelAttempt);
        }

        public void UnsubscribeEvents()
        {
            EventPublisher.Unsubscribe<BuildingControllerEvent.BuildModeChangedEventArgs>(OnModeChanged);
            EventPublisher.Unsubscribe<BuildingStateEvent.ValidateAttemptEventArgs>(OnValidateAttempt);
            EventPublisher.Unsubscribe<BuildingStateEvent.CancelAttemptEventArgs>(OnCancelAttempt);
        }

        public AudioClipPlayer GetEnterClip(BuildingMode mode) => mode switch
        {
            BuildingMode.Placement => m_enterPlacementModeClip,
            BuildingMode.Destruction => m_enterDestructionModeClip,
            BuildingMode.Adjustment => m_enterAdjustmentModeClip,
            BuildingMode.Upgrade => m_enterUpgradeModeClip,
            _ => null
        };

        public AudioClipPlayer GetExitClip(BuildingMode mode) => mode switch
        {
            BuildingMode.Placement => m_exitPlacementModeClip,
            BuildingMode.Destruction => m_exitDestructionModeClip,
            BuildingMode.Adjustment => m_exitAdjustmentModeClip,
            BuildingMode.Upgrade => m_exitUpgradeModeClip,
            _ => null
        };

        public AudioClipPlayer GetValidateClip(BuildingMode mode) => mode switch
        {
            BuildingMode.Placement => m_placementValidClip,
            BuildingMode.Destruction => m_destructionValidClip,
            BuildingMode.Adjustment => m_adjustmentValidClip,
            BuildingMode.Upgrade => m_upgradeValidClip,
            _ => null
        };

        public AudioClipPlayer GetFailedClip(BuildingMode mode) => mode switch
        {
            BuildingMode.Placement => m_placementFailedClip,
            BuildingMode.Destruction => m_destructionFailedClip,
            BuildingMode.Adjustment => m_adjustmentFailedClip,
            BuildingMode.Upgrade => m_upgradeFailedClip,
            _ => null
        };

        public AudioClipPlayer GetCancelClip(BuildingMode mode) => mode switch
        {
            BuildingMode.Placement => m_cancelPlacementClip,
            BuildingMode.Destruction => m_cancelDestructionClip,
            BuildingMode.Adjustment => m_cancelAdjustmentClip,
            BuildingMode.Upgrade => m_cancelUpgradeClip,
            _ => null
        };

        private void Play(AudioClipPlayer player)
        {
            if (player == null)
            {
                return;
            }

            if (BuildingController.Instance != null)
            {
                player.PlayAtPosition(BuildingController.Instance.transform.position);
            }
            else
            {
                player.Play();
            }
        }

        private void OnModeChanged(BuildingControllerEvent.BuildModeChangedEventArgs args)
        {
            Play(GetExitClip(m_lastMode));
            Play(GetEnterClip(args.Mode));
            m_lastMode = args.Mode;
        }

        private void OnValidateAttempt(BuildingStateEvent.ValidateAttemptEventArgs args)
        {
            AudioClipPlayer clip = args.Result.IsValid ? GetValidateClip(args.Mode) : GetFailedClip(args.Mode);
            Play(clip);
        }

        private void OnCancelAttempt(BuildingStateEvent.CancelAttemptEventArgs args)
        {
            Play(GetCancelClip(args.Mode));
        }
    }
}