/// <summary>
/// Project : Mind Code Interactive
/// Class : IManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces
{
    public interface IManager
    {
        string Name { get; }
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }
}