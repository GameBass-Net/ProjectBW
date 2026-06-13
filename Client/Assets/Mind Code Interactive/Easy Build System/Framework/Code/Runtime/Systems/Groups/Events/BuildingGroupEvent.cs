/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups.Events
{
    public static class BuildingGroupEvent
    {
        public abstract class GroupEventArgs : BuildingEventArgs
        {
            public BuildingGroup Group { get; }

            protected GroupEventArgs(BuildingGroup group) => Group = group;
        }

        public class RegisteredEventArgs : GroupEventArgs
        {
            public RegisteredEventArgs(BuildingGroup group) : base(group) { }
        }

        public class UnregisteredEventArgs : GroupEventArgs
        {
            public UnregisteredEventArgs(BuildingGroup group) : base(group) { }
        }

        public class UpdatedEventArgs : GroupEventArgs
        {
            public BuildingPart Part { get; }

            public UpdatedEventArgs(BuildingGroup group, BuildingPart part) : base(group) => Part = part;
        }
    }
}