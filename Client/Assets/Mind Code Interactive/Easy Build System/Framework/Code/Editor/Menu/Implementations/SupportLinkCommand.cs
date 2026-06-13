/// <summary>
/// Project : Easy Build System
/// Class : SupportLinkCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class SupportLinkCommand : IMenuCommand
    {
        public const string MENU_PATH = "Tools/Mind Code Interactive/Easy Build System/Support...";
        public const int PRIORITY = 500;

        public string MenuPath => MENU_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute() =>
            Application.OpenURL("https://form.jotform.com/202960719544359");
    }
}