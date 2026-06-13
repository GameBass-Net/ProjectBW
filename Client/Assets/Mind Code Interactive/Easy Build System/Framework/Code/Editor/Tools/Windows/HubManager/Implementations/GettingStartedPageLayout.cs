/// <summary>
/// Project : Easy Build System
/// Class : GettingStartedPageLayout.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.RenderPipelines;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools.Windows.HubManager.Implementations
{
    [EditorPage("ebs.gettingStarted", "Easy Build System", "Getting Started", "Editor/Icons/general", 0, true)]
    public class GettingStartedPageLayout : PageLayout
    {
        public override void DrawLayout()
        {
            DrawHeroSection();
            DrawAcknowledgementSection();
        }

        public static GUIStyle WordWrappedLabelCenter
        {
            get
            {
                GUIStyle centeredWordWrappedStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                    fontSize = 10
                };

                centeredWordWrappedStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white / 1.25f : Color.black / 1.25f;
                return centeredWordWrappedStyle;
            }
        }

        private void DrawHeroSection()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Width(650), GUILayout.Height(110));
            Rect bannerLogoRect = GUILayoutUtility.GetRect(1, 110);
            Texture2D bannerLogoTexture = Resources.Load<Texture2D>("Editor/Banner");
            GUI.DrawTexture(bannerLogoRect, bannerLogoTexture, ScaleMode.ScaleToFit);
            EditorGUILayout.Space(5f);

            EditorGUIExtended.BeginBorderLayoutHorizontal(true);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Space(-5f);
            EditorGUIExtended.Label("Package Version:");
            GUILayout.Space(-5f);
            EditorGUIExtended.ColoredLabel(RepositoryManifest.Get("EbsRepositoryManifest").Version, EditorGUIExtended.ColorPalette.Success);

            GUILayout.Space(-5f);
            EditorGUIExtended.Label(" | Render Pipeline:");
            GUILayout.Space(-5f);
            EditorGUIExtended.ColoredLabel(RenderPipelineContext.GetRenderPipelineAsString(), EditorGUIExtended.ColorPalette.Success);

            GUILayout.Space(-5f);
            EditorGUIExtended.Label(" | Unity Version:");
            GUILayout.Space(-5f);
            EditorGUIExtended.ColoredLabel(Application.unityVersion, EditorGUIExtended.ColorPalette.Success);

            GUILayout.Space(-5f);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUIExtended.EndBorderLayoutHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            EditorGUIExtended.Separator();
        }

        private void DrawAcknowledgementSection()
        {
            float contentLineSpacing = -5f;

            EditorGUIExtended.BeginVertical();
            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);
                EditorGUIExtended.ColoredLabel("Welcome", Color.white, EditorGUILabels.LabelType.Bold, EditorGUILabels.LabelAlignment.Center);
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Thank you for purchasing <b>Easy Build System</b> and supporting our work.", WordWrappedLabelCenter);
                GUILayout.Space(contentLineSpacing);
                EditorGUILayout.LabelField("If you enjoy the asset, please leave a review and share your thoughts, it helps a lot.", WordWrappedLabelCenter);
                GUILayout.Space(contentLineSpacing);
                EditorGUILayout.LabelField("We hope this asset helps you save time in your Unity projects.", WordWrappedLabelCenter);
            }
            EditorGUIExtended.EndVertical();

            EditorGUIExtended.BeginVertical();
            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);
                EditorGUIExtended.ColoredLabel("Getting Started", Color.white, EditorGUILabels.LabelType.Bold, EditorGUILabels.LabelAlignment.Center);
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Start by exploring the included sample scenes and reading the documentation.", WordWrappedLabelCenter);
                GUILayout.Space(contentLineSpacing);
                EditorGUILayout.LabelField("You will quickly learn how to set up the system and get the most out of its features.", WordWrappedLabelCenter);
                if (EditorGUIExtended.Button("Read Documentation"))
                {
                    Application.OpenURL("https://mindcodeinteractive.gitbook.io/easy-build-system/");
                }
                GUILayout.Space(3f);
            }
            EditorGUIExtended.EndVertical();

            EditorGUIExtended.BeginVertical();
            using (EditorGUIExtended.MarginScope())
            {
                GUILayout.Space(-5f);
                EditorGUIExtended.ColoredLabel("Support & Community", Color.white, EditorGUILabels.LabelType.Bold, EditorGUILabels.LabelAlignment.Center);
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Join our Discord community to get support, access hotfixes and stay up to date.", WordWrappedLabelCenter);
                GUILayout.Space(contentLineSpacing);
                EditorGUILayout.LabelField("You can also register your invoice number to unlock customer-only channels and archived versions.", WordWrappedLabelCenter);
                if (EditorGUIExtended.Button("Join Discord"))
                {
                    Application.OpenURL("https://discord.gg/7BavHZazh8");
                }
                GUILayout.Space(3f);
            }
            EditorGUIExtended.EndVertical();

            EditorGUIExtended.InspectorBottom();
        }
    }
}