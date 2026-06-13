/// <summary>
/// Project : Mind Code Interactive
/// Class : TagIntegrityCheck.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Linq;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes;
using MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations
{
    [IntegrityCheck(priority: 15)]
    public sealed class TagIntegrityCheck : IIntegrityCheck
    {
        public string Id => "Tag Validation";
        public string Description => "Validates required tags.";
        public int Priority => 15;
        public RepositoryManifest Manifest { get; set; }
        public string FailReason { get; private set; }

        public bool ShouldRun(RepositoryManifest manifest)
            => manifest.RequiredTags != null && manifest.RequiredTags.Length > 0;

        public bool RunCheck()
        {
            string[] missingTags = Manifest.RequiredTags
                .Where(requiredTag => !InternalEditorUtility.tags.Contains(requiredTag))
                .ToArray();

            if (missingTags.Length > 0)
            {
                FailReason = "Missing tags: " + string.Join(", ", missingTags);
                return false;
            }

            FailReason = null;
            return true;
        }

        public bool RunFix()
        {
            Object tagManagerAsset = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");

            if (tagManagerAsset == null)
            {
                FailReason = "TagManager asset not found.";
                return false;
            }

            SerializedObject tagManagerSO = new SerializedObject(tagManagerAsset);
            SerializedProperty tagsProperty = tagManagerSO.FindProperty("tags");
            tagManagerSO.Update();

            foreach (string requiredTag in Manifest.RequiredTags)
            {
                if (InternalEditorUtility.tags.Contains(requiredTag))
                {
                    continue;
                }

                bool slotFound = false;
                for (int i = 0; i < tagsProperty.arraySize; i++)
                {
                    SerializedProperty tagElementProperty = tagsProperty.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(tagElementProperty.stringValue))
                    {
                        tagElementProperty.stringValue = requiredTag;
                        slotFound = true;
                        break;
                    }
                }

                if (!slotFound)
                {
                    FailReason = "No available tag slots.";
                    return false;
                }
            }

            tagManagerSO.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            FailReason = null;
            return true;
        }
    }
}