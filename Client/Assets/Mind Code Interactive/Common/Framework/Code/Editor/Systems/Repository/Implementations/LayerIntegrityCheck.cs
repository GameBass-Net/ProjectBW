/// <summary>
/// Project : Mind Code Interactive
/// Class : LayerIntegrityCheck.cs
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
    [IntegrityCheck(priority: 10)]
    public sealed class LayerIntegrityCheck : IIntegrityCheck
    {
        public string Id => "Layer Validation";
        public string Description => "Validates required layers.";
        public int Priority => 10;
        public RepositoryManifest Manifest { get; set; }
        public string FailReason { get; private set; }

        public bool ShouldRun(RepositoryManifest manifest)
            => manifest.RequiredLayers != null && manifest.RequiredLayers.Length > 0;

        public bool RunCheck()
        {
            string[] existingLayers = InternalEditorUtility.layers;
            string[] missingLayers = Manifest.RequiredLayers
                .Where(requiredLayer => !existingLayers.Contains(requiredLayer))
                .ToArray();

            if (missingLayers.Length > 0)
            {
                FailReason = "Missing layers: " + string.Join(", ", missingLayers);
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
            SerializedProperty layersProperty = tagManagerSO.FindProperty("layers");
            tagManagerSO.Update();

            foreach (string requiredLayer in Manifest.RequiredLayers)
            {
                if (InternalEditorUtility.layers.Contains(requiredLayer))
                {
                    continue;
                }

                int availableSlotIndex = -1;

                for (int i = 3; i < layersProperty.arraySize; i++)
                {
                    SerializedProperty layerElementProperty = layersProperty.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(layerElementProperty.stringValue))
                    {
                        availableSlotIndex = i;
                        break;
                    }
                }

                if (availableSlotIndex != -1)
                {
                    layersProperty.GetArrayElementAtIndex(availableSlotIndex).stringValue = requiredLayer;
                }
                else
                {
                    FailReason = "No available user layer slots.";
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