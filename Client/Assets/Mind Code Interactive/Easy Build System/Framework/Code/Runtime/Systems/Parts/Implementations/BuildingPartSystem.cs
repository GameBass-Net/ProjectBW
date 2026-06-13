/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations
{
    public abstract class BuildingPartSystem
    {
        protected BuildingPart m_part;

        /// <summary>
        /// Gets or sets the building part reference for this system.
        /// </summary>
        public BuildingPart Part { get => m_part; set => m_part = value; }

        /// <summary>
        /// Initializes the part system with a building part reference.
        /// Override to implement custom initialization logic.
        /// </summary>
        public virtual void Initialize(BuildingPart part)
        {
            m_part = part;
        }

        /// <summary>
        /// Shuts down the part system.
        /// Override to implement custom shutdown logic.
        /// </summary>
        public virtual void Shutdown() { }
    }
}