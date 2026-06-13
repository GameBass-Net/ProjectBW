/// <summary>
/// Project : Easy Build System
/// Class : BuildingAreaEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas.Events
{
    public class BuildingAreaEvent
    {
        public abstract class AreaEventArgs : BuildingEventArgs
        {
            public BuildingArea Area { get; }

            protected AreaEventArgs(BuildingArea area) => Area = area;
        }

        public sealed class RegisteredEventArgs : AreaEventArgs
        {
            public RegisteredEventArgs(BuildingArea area) : base(area) { }
        }

        public sealed class UnregisteredEventArgs : AreaEventArgs
        {
            public UnregisteredEventArgs(BuildingArea area) : base(area) { }
        }
    }
}