/// <summary>
/// Project : Easy Build System
/// Class : BuildingManagerEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events
{
    public enum BuildingEventType
    {
        PartRegistered,
        PartUnregistered,
        PartPlaced,
        PartDestroyed,
        PartStateChanged,
        AreaRegistered,
        AreaUnregistered,
        GroupRegistered,
        GroupUpdated,
        GroupUnregistered,
        SocketRegistered,
        SocketUnregistered
    }

    public class BuildingEventArgs : IBaseEvent { }
}