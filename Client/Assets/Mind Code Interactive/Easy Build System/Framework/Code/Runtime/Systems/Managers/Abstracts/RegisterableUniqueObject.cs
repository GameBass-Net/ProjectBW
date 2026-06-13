/// <summary>
/// Project : Easy Build System
/// Class : RegisterableUniqueObject.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Abstracts;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts
{
    public abstract class RegisterableUniqueObject : BaseUniqueObject, IRegisterable
    {
        public bool IsRegistered { get; set; }

        public override void OnEnable()
        {
            base.OnEnable();

            if (!IsRegistered)
            {
                BuildingManager.Instance?.Register(this);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (IsRegistered)
            {
                BuildingManager.Instance?.Unregister(this);
            }
        }

        public virtual void OnRegistered() { }

        public virtual void OnUnregistered() { }
    }
}