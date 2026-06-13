/// <summary>
/// Project : Mind Code Interactive
/// Class : PackageManifest.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data
{
    [Serializable]
    public class PackageManifest
    {
        public string Name;
        public string Author;
        public string Link;
        public string Version;
        public string Description;
        public string[] Dependencies;
        public string[] Symbols;
        public string Type;
        public string UnityVersion;
        public bool Installed;
    }
}