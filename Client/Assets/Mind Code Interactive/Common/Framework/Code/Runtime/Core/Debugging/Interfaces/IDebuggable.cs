/// <summary>
/// Project : Mind Code Interactive
/// Class : IDebuggable.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces
{
    public interface IDebuggable
    {
        bool DebugEnabled { get; }
        DebugRenderer.ViewFlags DebugFlags { get; set; }
        bool RequireSelection { get; }
        void OnDebugRender();
    }
}