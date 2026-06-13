/// <summary>
/// Project : Mind Code Interactive
/// Class : ScriptingDefineSymbolsIntegrityCheck.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
{
    [IntegrityCheck(priority: 5)]
    public sealed class ScriptingDefineSymbolsIntegrityCheck : IIntegrityCheck
    {
        public string Id => "Scripting Define Symbols";
        public string Description => "Validates scripting define symbols.";
        public int Priority => 5;
        public RepositoryManifest Manifest { get; set; }
        public string FailReason { get; private set; }

        public bool ShouldRun(RepositoryManifest manifest)
            => manifest.RequiredScriptingDefineSymbols != null && manifest.RequiredScriptingDefineSymbols.Length > 0;

        public bool RunCheck()
        {
            List<string> currentSymbols = GetSymbols();
            string[] missingSymbols = Manifest.RequiredScriptingDefineSymbols
                .Where(symbol => !currentSymbols.Contains(symbol))
                .ToArray();

            if (missingSymbols.Length > 0)
            {
                FailReason = "Missing symbols: " + string.Join(", ", missingSymbols);
                return false;
            }

            FailReason = null;
            return true;
        }

        public bool RunFix()
        {
            List<string> currentSymbols = GetSymbols();
            bool anySymbolAdded = false;

            for (int i = 0; i < Manifest.RequiredScriptingDefineSymbols.Length; i++)
            {
                string requiredSymbol = Manifest.RequiredScriptingDefineSymbols[i];

                if (string.IsNullOrEmpty(requiredSymbol))
                {
                    continue;
                }

                if (!currentSymbols.Contains(requiredSymbol))
                {
                    currentSymbols.Add(requiredSymbol);
                    anySymbolAdded = true;
                }
            }

            if (anySymbolAdded)
            {
                string newScriptingDefines = string.Join(";", currentSymbols.ToArray());

#if UNITY_2021_2_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(
                    UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                        EditorUserBuildSettings.selectedBuildTargetGroup),
                    newScriptingDefines);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    newScriptingDefines);
#endif
            }

            FailReason = null;
            return true;
        }

        private static List<string> GetSymbols()
        {
#if UNITY_2021_2_OR_NEWER
            string rawScriptingDefines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup));
#else
            string rawScriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
#endif

            List<string> symbolsList = new List<string>();

            if (string.IsNullOrEmpty(rawScriptingDefines))
            {
                return symbolsList;
            }

            string[] symbolsArray = rawScriptingDefines.Split(';');

            for (int i = 0; i < symbolsArray.Length; i++)
            {
                string symbol = symbolsArray[i];

                if (!string.IsNullOrEmpty(symbol) && !symbolsList.Contains(symbol))
                {
                    symbolsList.Add(symbol);
                }
            }

            return symbolsList;
        }
    }
}