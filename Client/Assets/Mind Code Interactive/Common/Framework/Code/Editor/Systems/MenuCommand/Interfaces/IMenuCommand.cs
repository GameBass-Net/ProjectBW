/// <summary>
/// Project : Mind Code Interactive
/// Class : IMenuCommand.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces
{
    public interface IMenuCommand
    {
        string MenuPath { get; }
        int Priority { get; }
        bool Validate();
        void Execute();
    }
}