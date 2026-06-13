/// <summary>
/// Project : Easy Build System
/// Class : ISaveable.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Interfaces
{
    public interface ISaveable
    {
        string GetSaveKey();

        object GetSaveData();

        void LoadSaveData(object data);
    }
}