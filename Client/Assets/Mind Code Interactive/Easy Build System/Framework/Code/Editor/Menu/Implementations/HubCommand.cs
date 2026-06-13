/// <summary>
/// Project : Easy Build System
/// Class : HubCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class HubCommand : IMenuCommand
    {
        public const string MENU_PATH = "Tools/Mind Code Interactive/Easy Build System/Hub...";
        public const int PRIORITY = -100;

        public string MenuPath => MENU_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute() => HubManagerWindow.OpenWindow();
    }
}