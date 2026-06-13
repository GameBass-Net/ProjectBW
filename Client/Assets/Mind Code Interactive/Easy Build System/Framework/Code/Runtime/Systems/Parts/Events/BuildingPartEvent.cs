/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events
{
    public static class BuildingPartEvent
    {
        public class PartEventArgs : BuildingEventArgs
        {
            public BuildingPart Part { get; }

            public PartEventArgs(BuildingPart part) => Part = part;
        }

        public class RegisteredEventArgs : PartEventArgs
        {
            public RegisteredEventArgs(BuildingPart part) : base(part) { }
        }

        public class UnregisteredEventArgs : PartEventArgs
        {
            public UnregisteredEventArgs(BuildingPart part) : base(part) { }
        }

        public class StateChangedEventArgs : PartEventArgs
        {
            public BuildingPart.BuildingState LastState { get; }

            public BuildingPart.BuildingState NewState { get; }

            public StateChangedEventArgs(
                BuildingPart part,
                BuildingPart.BuildingState oldState,
                BuildingPart.BuildingState newState) : base(part)
            {
                LastState = oldState;
                NewState = newState;
            }
        }

        public class UpgradeChangedEventArgs : PartEventArgs
        {
            public int OldLevel { get; }

            public int NewLevel { get; }

            public UpgradeChangedEventArgs(
                BuildingPart part,
                int oldLevel,
                int newLevel) : base(part)
            {
                OldLevel = oldLevel;
                NewLevel = newLevel;
            }
        }

        public class PreviewCreatedEventArgs : PartEventArgs
        {
            public PreviewCreatedEventArgs(BuildingPart part) : base(part) { }
        }

        public class PreviewDestroyedEventArgs : PartEventArgs
        {
            public PreviewDestroyedEventArgs(BuildingPart part) : base(part) { }
        }

        public class ConditionValidatedEventArgs : PartEventArgs
        {
            public BuildingCondition Condition { get; }

            public BuildingMode Mode { get; }

            public bool IsValid { get; }

            public string Reason { get; }

            public ConditionValidatedEventArgs(
                BuildingPart part,
                BuildingCondition condition,
                BuildingMode mode,
                bool isValid,
                string reason = "") : base(part)
            {
                Condition = condition;
                Mode = mode;
                IsValid = isValid;
                Reason = reason;
            }
        }

        public class ConditionFailedEventArgs : PartEventArgs
        {
            public BuildingCondition Condition { get; }

            public BuildingMode Mode { get; }

            public string Reason { get; }

            public ConditionFailedEventArgs(
                BuildingPart part,
                BuildingCondition condition,
                BuildingMode mode,
                string reason) : base(part)
            {
                Condition = condition;
                Mode = mode;
                Reason = reason;
            }
        }
    }
}