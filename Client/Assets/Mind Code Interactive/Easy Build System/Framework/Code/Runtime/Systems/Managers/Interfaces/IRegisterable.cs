/// <summary>
/// Project : Easy Build System
/// Class : IRegisterable.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Interfaces
{
    public interface IRegisterable : IUniqueObject
    {
        bool IsRegistered { get; set; }
    }
}