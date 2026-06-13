/// <summary>
/// Project : Mind Code Interactive
/// Class : ISettings.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces
{
    public interface ISettings
    {
        string ManagerName { get; }
        int Priority { get; }
        bool AutoInitialize { get; }
        bool AutoCreate { get; }
        Type GetManagerType();
    }
}