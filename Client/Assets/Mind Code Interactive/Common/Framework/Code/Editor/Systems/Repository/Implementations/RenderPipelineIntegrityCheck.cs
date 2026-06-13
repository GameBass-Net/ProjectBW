/// <summary>
/// Project : Mind Code Interactive
/// Class : RenderPipelineIntegrityCheck.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces;
using MindCodeInteractive.Common.Framework.Code.Editor.Core.RenderPipelines;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.RenderPipelines;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
{
    [IntegrityCheck(priority: 20)]
    public sealed class RenderPipelineIntegrityCheck : IIntegrityCheck
    {
        private const string URP_CONVERTED_KEY = "MCI_EBS_URPConverted";
        private const string HDRP_CONVERTED_KEY = "MCI_EBS_HDRPConverted";

        private enum PipelineKind { BuiltIn, URP, HDRP, Unknown }

        public string Id => "Render Pipeline Support";
        public string Description => "Applies scoped material upgrades for the active SRP.";
        public int Priority => 20;
        public RepositoryManifest Manifest { get; set; }
        public string FailReason { get; private set; }

        public bool ShouldRun(RepositoryManifest manifest) => true;

        public bool RunCheck()
        {
            PipelineKind kind = ResolvePipelineKind();

            if (kind == PipelineKind.BuiltIn)
            {
                FailReason = null;
                return true;
            }

            if (kind == PipelineKind.Unknown)
            {
                FailReason = "Unknown render pipeline.";
                return false;
            }

            RenderPipelineUpgradeSnapshot snapshot = FindSnapshot();
            if (!snapshot)
            {
                FailReason = "RenderPipelineUpgradeSnapshot not found.";
                return false;
            }

            if (kind == PipelineKind.URP && !EditorPrefs.GetBool(URP_CONVERTED_KEY, false))
            {
                FailReason = "Project assets not yet converted to URP.";
                return false;
            }

            if (kind == PipelineKind.HDRP && !EditorPrefs.GetBool(HDRP_CONVERTED_KEY, false))
            {
                FailReason = "Project assets not yet converted to HDRP.";
                return false;
            }

            FailReason = null;
            return true;
        }

        public bool RunFix()
        {
            PipelineKind kind = ResolvePipelineKind();

            if (kind == PipelineKind.BuiltIn)
            {
                FailReason = null;
                return true;
            }

            if (kind == PipelineKind.Unknown)
            {
                FailReason = "Unknown render pipeline.";
                return false;
            }

            RenderPipelineUpgradeSnapshot snapshot = FindSnapshot();
            if (!snapshot)
            {
                FailReason = "RenderPipelineUpgradeSnapshot not found.";
                return false;
            }

            if (kind == PipelineKind.URP)
            {
                ImportPackage(snapshot.URPPackage);
                RenderPipelineUpgrader.ConvertToURP(snapshot);
                EditorPrefs.SetBool(URP_CONVERTED_KEY, true);
            }
            else
            {
                ImportPackage(snapshot.HDRPPackage);
                RenderPipelineUpgrader.ConvertToHDRP(snapshot);
                EditorPrefs.SetBool(HDRP_CONVERTED_KEY, true);
            }

            FailReason = null;
            return true;
        }

        private static void ImportPackage(DefaultAsset packageAsset)
        {
            if (!packageAsset)
            {
                return;
            }

            string packagePath = AssetDatabase.GetAssetPath(packageAsset);
            if (string.IsNullOrEmpty(packagePath) || !packagePath.EndsWith(".unitypackage"))
            {
                return;
            }

            AssetDatabase.ImportPackage(packagePath, false);
        }

        private static RenderPipelineUpgradeSnapshot FindSnapshot()
        {
            string[] guids = AssetDatabase.FindAssets("t:RenderPipelineUpgradeSnapshot");
            if (guids == null || guids.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RenderPipelineUpgradeSnapshot snapshot = AssetDatabase.LoadAssetAtPath<RenderPipelineUpgradeSnapshot>(path);
                if (snapshot)
                {
                    return snapshot;
                }
            }

            return null;
        }

        private static PipelineKind ResolvePipelineKind()
        {
            RenderPipelineContext.RenderPipeline activePipeline = RenderPipelineContext.GetActiveRenderPipeline();

            return activePipeline switch
            {
                RenderPipelineContext.RenderPipeline.BuiltIn => PipelineKind.BuiltIn,
                RenderPipelineContext.RenderPipeline.URP => PipelineKind.URP,
                RenderPipelineContext.RenderPipeline.HDRP => PipelineKind.HDRP,
                _ => PipelineKind.Unknown
            };
        }
    }
}