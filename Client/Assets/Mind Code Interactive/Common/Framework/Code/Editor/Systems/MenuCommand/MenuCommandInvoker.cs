/// <summary>
/// Project : Mind Code Interactive
/// Class : MenuCommandInvoker.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand
{
    public static class MenuCommandInvoker
    {
        public static void Run<T>() where T : IMenuCommand, new()
        {
            T command = new T();
            if (command.Validate())
            {
                command.Execute();
            }
        }
    }
}