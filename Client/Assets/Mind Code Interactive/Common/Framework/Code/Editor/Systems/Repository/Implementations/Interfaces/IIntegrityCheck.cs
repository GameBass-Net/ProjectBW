/// <summary>
/// Project : Mind Code Interactive
/// Class : IIntegrityCheck.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces
{
    public interface IIntegrityCheck
    {
        string Id { get; }
        string Description { get; }
        int Priority { get; }
        string FailReason { get; }
        RepositoryManifest Manifest { get; set; }

        bool ShouldRun(RepositoryManifest manifest);
        bool RunCheck();
        bool RunFix();
    }
}