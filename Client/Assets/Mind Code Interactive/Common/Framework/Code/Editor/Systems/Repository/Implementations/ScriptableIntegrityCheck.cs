/// <summary>
/// Project : Mind Code Interactive
/// Class : ScriptableIntegrityCheck.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.IO;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
{
    [IntegrityCheck(priority: 25)]
    public sealed class ScriptableIntegrityCheck : IIntegrityCheck
    {
        public string Id => "Scriptable Object Validation";
        public string Description => "Validates required ScriptableObjects.";
        public int Priority => 25;
        public RepositoryManifest Manifest { get; set; }
        public string FailReason { get; private set; }

        public bool ShouldRun(RepositoryManifest manifest)
            => manifest.RequiredScriptableObjects != null && manifest.RequiredScriptableObjects.Length > 0;

        public bool RunCheck()
        {
            foreach (ScriptableObject requirement in Manifest.RequiredScriptableObjects)
            {
                if (requirement == null)
                {
                    FailReason = "Missing ScriptableObject reference";
                    return false;
                }

                string assetPath = AssetDatabase.GetAssetPath(requirement);
                if (string.IsNullOrEmpty(assetPath))
                {
                    FailReason = $"ScriptableObject {requirement.name} is not in AssetDatabase";
                    return false;
                }
            }

            FailReason = null;
            return true;
        }

        public bool RunFix()
        {
            FailReason = null;
            return true;
        }
    }
}