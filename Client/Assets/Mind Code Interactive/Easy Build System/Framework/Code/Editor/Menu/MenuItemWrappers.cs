/// <summary>
/// Project : Easy Build System
/// Class : MenuItemWrappers.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu
{
    public static class MenuItemWrappers
    {
        [MenuItem(HubCommand.MENU_PATH, false, HubCommand.PRIORITY)]
        public static void ExecuteHubCommand()
        {
            MenuCommandInvoker.Run<HubCommand>();
        }

        [MenuItem(CreateBuildingManagerCommand.GAMEOBJECT_PATH, false, CreateBuildingManagerCommand.PRIORITY)]
        [MenuItem(CreateBuildingManagerCommand.TOOLS_PATH, false, CreateBuildingManagerCommand.PRIORITY)]
        public static void ExecuteCreateBuildingManager()
        {
            MenuCommandInvoker.Run<CreateBuildingManagerCommand>();
        }

        [MenuItem(CreateBuildingAreaCommand.GAMEOBJECT_PATH, false, CreateBuildingAreaCommand.PRIORITY)]
        [MenuItem(CreateBuildingAreaCommand.TOOLS_PATH, false, CreateBuildingAreaCommand.PRIORITY)]
        public static void ExecuteCreateBuildingArea()
        {
            MenuCommandInvoker.Run<CreateBuildingAreaCommand>();
        }

        [MenuItem(CreateBuildingPartCommand.GAMEOBJECT_PATH, false, CreateBuildingPartCommand.PRIORITY)]
        [MenuItem(CreateBuildingPartCommand.TOOLS_PATH, false, CreateBuildingPartCommand.PRIORITY)]
        public static void ExecuteCreateBuildingPart()
        {
            MenuCommandInvoker.Run<CreateBuildingPartCommand>();
        }

        [MenuItem(CreateBuildingSocketCommand.GAMEOBJECT_PATH, false, CreateBuildingSocketCommand.PRIORITY)]
        [MenuItem(CreateBuildingSocketCommand.TOOLS_PATH, false, CreateBuildingSocketCommand.PRIORITY)]
        public static void ExecuteCreateBuildingSocket()
        {
            MenuCommandInvoker.Run<CreateBuildingSocketCommand>();
        }

        [MenuItem(CreateBuildingCollectionCommand.TOOLS_PATH, false, CreateBuildingCollectionCommand.PRIORITY)]
        public static void ExecuteCreateBuildingCollectionAsset()
        {
            MenuCommandInvoker.Run<CreateBuildingCollectionCommand>();
        }

        [MenuItem(CreateBuildingMenuCommand.GAMEOBJECT_PATH, false, CreateBuildingMenuCommand.PRIORITY)]
        [MenuItem(CreateBuildingMenuCommand.TOOLS_PATH, false, CreateBuildingMenuCommand.PRIORITY)]
        public static void ExecuteCreateBuildingCatalogMenu()
        {
            MenuCommandInvoker.Run<CreateBuildingMenuCommand>();
        }

        [MenuItem(CreateBuildingRadialMenuCommand.GAMEOBJECT_PATH, false, CreateBuildingRadialMenuCommand.PRIORITY)]
        [MenuItem(CreateBuildingRadialMenuCommand.TOOLS_PATH, false, CreateBuildingRadialMenuCommand.PRIORITY)]
        public static void ExecuteCreateBuildingRadialMenu()
        {
            MenuCommandInvoker.Run<CreateBuildingRadialMenuCommand>();
        }

        [MenuItem(QuickSetupCommand.MENU_PATH, false, QuickSetupCommand.PRIORITY)]
        public static void ExecuteQuickSetup()
        {
            MenuCommandInvoker.Run<QuickSetupCommand>();
        }

        [MenuItem(SupportLinkCommand.MENU_PATH, false, SupportLinkCommand.PRIORITY)]
        public static void ExecuteSupport()
        {
            MenuCommandInvoker.Run<SupportLinkCommand>();
        }

        [MenuItem(DocumentationLinkCommand.MENU_PATH, false, DocumentationLinkCommand.PRIORITY)]
        public static void ExecuteDocumentation()
        {
            MenuCommandInvoker.Run<DocumentationLinkCommand>();
        }
    }
}