/// <summary>
/// Project : Mind Code Interactive
/// Class : RenderPipelineContext.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.RenderPipelines
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine.Rendering;

using UnityEditor;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.RenderPipelines
{
    public static class RenderPipelineContext
    {
        public enum RenderPipeline
        {
            BuiltIn,
            URP,
            HDRP
        }

        public static RenderPipeline GetActiveRenderPipeline()
        {
#if UNITY_HDRP
            return RenderPipeline.HDRP;
#elif UNITY_URP
            return RenderPipeline.URP;
#else
            return RenderPipeline.BuiltIn;
#endif
        }

        public static string GetRenderPipelineAsString()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                return GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition") ? "High Definition" : "Universal";
            }

            return "Built-In";
        }
    }

#if UNITY_EDITOR

    [InitializeOnLoad]
    public static class RenderPipelineSymbolInitializer
    {
        static RenderPipelineSymbolInitializer() => SetRenderPipelineSymbols();

        private static void SetRenderPipelineSymbols()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2021_2_OR_NEWER
            string scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
#else
            string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

            string[] rawSymbols = scriptingDefines.Split(';');
            List<string> symbolsList = new List<string>();

            for (int symbolIndex = 0; symbolIndex < rawSymbols.Length; symbolIndex++)
            {
                string symbol = rawSymbols[symbolIndex];
                if (!string.IsNullOrEmpty(symbol) && !symbolsList.Contains(symbol))
                {
                    symbolsList.Add(symbol);
                }
            }

            bool hasHdrpSymbol = symbolsList.Contains("UNITY_HDRP");
            bool hasUrpSymbol = symbolsList.Contains("UNITY_URP");

            string renderPipelineAssetType;

#if UNITY_6000_0_OR_NEWER
            renderPipelineAssetType =
                GraphicsSettings.defaultRenderPipeline != null
                    ? GraphicsSettings.defaultRenderPipeline.GetType().ToString()
                    : string.Empty;
#else
            renderPipelineAssetType =
                GraphicsSettings.renderPipelineAsset != null
                    ? GraphicsSettings.renderPipelineAsset.GetType().ToString()
                    : string.Empty;
#endif

            bool isHdrpPipeline = renderPipelineAssetType.IndexOf("HDRenderPipelineAsset") >= 0;
            bool isUrpPipeline = renderPipelineAssetType.IndexOf("UniversalRenderPipelineAsset") >= 0;
            bool isBuiltInPipeline = string.IsNullOrEmpty(renderPipelineAssetType);

            if (isHdrpPipeline)
            {
                if (!hasHdrpSymbol)
                {
                    symbolsList.Add("UNITY_HDRP");
                }

                symbolsList.Remove("UNITY_URP");
            }
            else if (isUrpPipeline)
            {
                if (!hasUrpSymbol)
                {
                    symbolsList.Add("UNITY_URP");
                }

                symbolsList.Remove("UNITY_HDRP");
            }
            else if (isBuiltInPipeline)
            {
                symbolsList.Remove("UNITY_HDRP");
                symbolsList.Remove("UNITY_URP");
            }

            string newScriptingDefines = string.Join(";", symbolsList.ToArray());

            if (scriptingDefines != newScriptingDefines)
            {
#if UNITY_2021_2_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(
                    UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup),
                    newScriptingDefines);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    buildTargetGroup,
                    newScriptingDefines);
#endif
            }
        }
    }

#endif
}