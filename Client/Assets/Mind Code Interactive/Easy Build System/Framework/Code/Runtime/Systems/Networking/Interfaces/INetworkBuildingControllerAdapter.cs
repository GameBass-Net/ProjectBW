/// <summary>
/// Project : Easy Build System
/// Class : INetworkBuildingControllerAdapter.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces
{
    public interface INetworkBuildingControllerAdapter
    {
        bool IsConnected { get; }

        void ExecutePlaceCommand(BuildingPart part, Vector3 position, Quaternion rotation, Vector3 scale, BuildingSocket socket = null);

        void ExecuteAdjustCommand(BuildingPart part, Vector3 newPosition, Quaternion newRotation);

        void ExecuteDestroyCommand(BuildingPart part);

        void ExecuteUpgradeCommand(BuildingPart part, int upgradeIndex);
    }
}