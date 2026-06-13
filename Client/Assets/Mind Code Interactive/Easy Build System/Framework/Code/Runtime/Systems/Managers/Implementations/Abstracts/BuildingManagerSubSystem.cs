/// <summary>
/// Project : Easy Build System
/// Class : BuildingManagerSubSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts
{
    public abstract class BuildingManagerSubSystem
    {
        protected BuildingManager m_manager;

        public virtual void Initialize() { }

        public virtual void Shutdown() { }

        public virtual void Update() { }

        public virtual void OnRenderObject() { }
    }
}