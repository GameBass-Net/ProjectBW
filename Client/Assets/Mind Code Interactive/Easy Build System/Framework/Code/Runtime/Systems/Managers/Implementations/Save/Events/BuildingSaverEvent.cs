/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveEvent.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Events
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Events
{
    public static class BuildingSaveEvent
    {
        public class SaveStartedEventArgs : BuildingEventArgs
        {
            public BuildingSaveData SaveData { get; }

            public SaveStartedEventArgs(BuildingSaveData saveData) => SaveData = saveData;
        }

        public class SaveCompletedEventArgs : BuildingEventArgs
        {
            public HashSet<BuildingPart> SavedParts { get; }

            public SaveCompletedEventArgs(HashSet<BuildingPart> savedParts) => SavedParts = savedParts;
        }

        public class LoadStartedEventArgs : BuildingEventArgs
        {
            public BuildingSaveData SaveData { get; }

            public LoadStartedEventArgs(BuildingSaveData saveData) => SaveData = saveData;
        }

        public class LoadCompletedEventArgs : BuildingEventArgs
        {
            public HashSet<BuildingPart> LoadedParts { get; }

            public float LoadTime { get; }

            public LoadCompletedEventArgs(HashSet<BuildingPart> loadedParts, float loadTime)
            {
                LoadedParts = loadedParts;
                LoadTime = loadTime;
            }
        }
    }
}