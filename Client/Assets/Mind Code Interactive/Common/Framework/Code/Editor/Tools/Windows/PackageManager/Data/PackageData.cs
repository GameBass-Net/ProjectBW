/// <summary>
/// Project : Mind Code Interactive
/// Class : PackageData.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.PackageManager.Data
{
    [Serializable]
    public class PackageData
    {
        public PackageManifest Manifest;
        public string FolderPath;
        public string UnityPackagePath;
        public bool IsInstalled;
    }
}