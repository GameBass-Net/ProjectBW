/// <summary>
/// Project : Easy Build System
/// Class : HubStartup.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager
{
    [InitializeOnLoad]
    static class HubStartup
    {
        private const string k_sessionInitKey = "EBS_HubStartup_Initialized";
        private const string k_validationKey = "MCI_EBS_REPOSITORY_INIT";

        static HubStartup()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            if (RepositoryManager.IsValidationInitialized(k_validationKey))
            {
                return;
            }

            bool alreadyRanThisSession = SessionState.GetBool(k_sessionInitKey, false);

            if (!alreadyRanThisSession)
            {
                HubManagerWindow.OpenWindowToPage("ebs.integrityCheck");
            }

            SessionState.SetBool(k_sessionInitKey, true);

            RepositoryManifest manifest = Resources.Load<RepositoryManifest>("EbsRepositoryManifest");

            RepositoryManager.InitializeProject(manifest, k_validationKey, null, (id, msg, valid) =>
            {
                Debug.Log("[Easy Build System] System initialization completed!");

                if (!alreadyRanThisSession)
                {
                    HubManagerWindow.OpenWindowToPage("ebs.gettingStarted");
                }
            });
        }
    }
}