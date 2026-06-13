/// <summary>
/// Project : Easy Build System
/// Class : INetworkBuildingSaveAdapter.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces
{
    public interface INetworkBuildingSaveAdapter
    {
        bool IsAuthority { get; }

        bool ShouldSaveLocally { get; }

        void OnBeforeLoad();

        void OnAfterLoad();

        void SpawnLoadedBuilding(BuildingPartData data);
    }
}