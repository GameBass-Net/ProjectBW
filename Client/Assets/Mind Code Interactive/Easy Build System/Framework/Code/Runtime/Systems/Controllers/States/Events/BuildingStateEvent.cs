/// <summary>
/// Project : Easy Build System
/// Class : BuildingStateEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events
{
    public static class BuildingStateEvent
    {
        public abstract class StateEventArgs : IBaseEvent
        {
            public BuildingMode Mode { get; }

            protected StateEventArgs(BuildingMode mode) => Mode = mode;
        }

        public class DestroyedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public DestroyedEventArgs(BuildingPart part) : base(part) { }
        }

        public class PlacedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public PlacedEventArgs(BuildingPart part) : base(part) { }
        }

        public class AdjustedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public AdjustedEventArgs(BuildingPart part) : base(part) { }
        }

        public class AdjustmentStartedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public AdjustmentStartedEventArgs(BuildingPart part) : base(part) { }
        }

        public class AdjustmentEndedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public AdjustmentEndedEventArgs(BuildingPart part) : base(part) { }
        }

        public class UpgradedEventArgs : BuildingPartEvent.PartEventArgs
        {
            public UpgradedEventArgs(BuildingPart part) : base(part) { }
        }

        public class ValidateAttemptEventArgs : StateEventArgs
        {
            public ConditionResult Result { get; }

            public ValidateAttemptEventArgs(BuildingMode mode, ConditionResult result) : base(mode) => Result = result;
        }

        public class CancelAttemptEventArgs : StateEventArgs
        {
            public CancelAttemptEventArgs(BuildingMode mode) : base(mode) { }
        }
    }
}