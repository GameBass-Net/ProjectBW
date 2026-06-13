/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocketEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Events
{
    public static class BuildingSocketEvent
    {
        public abstract class SocketEventArgs : BuildingEventArgs
        {
            public BuildingSocket Socket { get; }

            protected SocketEventArgs(BuildingSocket socket) => Socket = socket;
        }

        public class RegisteredEventArgs : SocketEventArgs
        {
            public RegisteredEventArgs(BuildingSocket socket) : base(socket) { }
        }

        public class UnregisteredEventArgs : SocketEventArgs
        {
            public UnregisteredEventArgs(BuildingSocket socket) : base(socket) { }
        }
    }
}