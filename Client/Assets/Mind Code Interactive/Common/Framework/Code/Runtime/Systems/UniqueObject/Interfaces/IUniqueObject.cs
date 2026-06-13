/// <summary>
/// Project : Mind Code Interactive
/// Class : IUniqueObject.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Interfaces
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Interfaces
{
    public interface IUniqueObject
    {
        string PrefabId { get; }
        string UniqueId { get; }
    }
}