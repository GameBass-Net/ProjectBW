/// <summary>
/// Project : Easy Build System
/// Class : DocumentationLinkCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class DocumentationLinkCommand : IMenuCommand
    {
        public const string MENU_PATH = "Tools/Mind Code Interactive/Easy Build System/Documentation...";
        public const int PRIORITY = 501;

        public string MenuPath => MENU_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute() => Application.OpenURL("https://mindcodeinteractive.gitbook.io/easy-build-system/");
    }
}