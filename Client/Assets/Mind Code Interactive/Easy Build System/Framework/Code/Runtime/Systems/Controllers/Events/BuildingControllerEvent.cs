/// <summary>
/// Project : Easy Build System
/// Class : BuildingControllerEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Events
{
    public class BuildingControllerEvent
    {
        public class BuildViewChangedEventArgs : BuildingEventArgs
        {
            public BuildingViewType View { get; }

            public BuildViewChangedEventArgs(BuildingViewType view) => View = view;
        }

        public class BuildModeChangedEventArgs : BuildingEventArgs
        {
            public BuildingMode Mode { get; }

            public BuildModeChangedEventArgs(BuildingMode mode) => Mode = mode;
        }

        public class BuildSelectionChangedEventArgs : BuildingEventArgs
        {
            public BuildingPart SelectedPart { get; }

            public BuildSelectionChangedEventArgs(BuildingPart part) => SelectedPart = part;
        }
    }
}