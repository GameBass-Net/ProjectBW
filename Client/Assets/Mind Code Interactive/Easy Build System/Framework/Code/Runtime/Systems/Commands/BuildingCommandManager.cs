/// <summary>
/// Project : Easy Build System
/// Class : BuildingCommandManager.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands
{
    public static class BuildingCommandManager
    {
        public static void ExecuteCommand(BuildingCommand command)
        {
            if (command == null)
            {
                Debug.LogError("Cannot execute a null command.");
                return;
            }

            command.Execute();
        }
    }
}