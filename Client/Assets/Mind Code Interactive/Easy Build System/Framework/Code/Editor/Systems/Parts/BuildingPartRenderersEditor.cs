/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartRenderersEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    public static class BuildingPartRenderersEditor
    {
        public static void Draw(
            BuildingPart target,
            PropertyCollection properties,
            SerializedObject serializedObject,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider,
            List<string> integrityMessages)
        {
            if (target == null)
            {
                return;
            }

            BuildingRendererSystem rendererSystem = target.RendererSystem;

            if (integrityMessages != null && integrityMessages.Count > 0)
            {
                string message = string.Join("\n", integrityMessages);
                EditorGUIExtended.HelpBox(message, EditorGUIElements.MessageType.Warning);
                if (EditorGUIExtended.WarningButton("Auto-Fix Issue(s)..."))
                {
                    RendererVariantData activeVariant = rendererSystem.Active;
                    if (activeVariant != null)
                    {
                        AutoFixIntegrity(target, activeVariant, integrityMessages);
                    }
                }
                EditorGUILayout.Space();
            }

            if (rendererSystem == null || rendererSystem.Count == 0)
            {
                EditorGUIExtended.Label("No Renderer Variant(s) added yet.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }
            else
            {
                for (int variantIndex = 0; variantIndex < rendererSystem.Count; variantIndex++)
                {
                    RendererVariantData variant = rendererSystem.GetVariant(variantIndex);
                    if (variant != null)
                    {
                        DrawRendererVariant(target, properties, serializedObject, editorProvider, rendererSystem, variant, variantIndex);
                    }
                }
            }

            EditorGUIExtended.Separator();

            EditorGUIExtended.DragAndDropArea("Drag & Drop GameObjects here to add renderer variants.",
                draggedObjects => HandleRendererDropped(target, draggedObjects),
                ValidateRendererObject, true, true);

            using (EditorGUIExtended.DisabledScope(!target.gameObject.scene.IsValid()))
            {
                if (GUILayout.Button("Refresh Renderer System..."))
                {
                    Undo.RecordObject(target, "Refresh Renderer System");
                    rendererSystem.Initialize(target);
                    EditorUtility.SetDirty(target);
                    Debug.Log("Renderer system refreshed for Building Part '" + target.Name + "'.");
                }
            }
        }

        public static void CheckIntegrity(BuildingPart target, List<string> integrityMessages)
        {
            if (integrityMessages == null)
            {
                return;
            }

            integrityMessages.Clear();

            if (target == null)
            {
                return;
            }

            BuildingRendererSystem rendererSystem = target.RendererSystem;
            if (rendererSystem == null)
            {
                return;
            }

            RendererVariantData activeVariant = rendererSystem.Active;
            if (activeVariant == null || activeVariant.Renderers == null)
            {
                return;
            }

            foreach (Renderer renderer in activeVariant.Renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh && !meshFilter.sharedMesh.isReadable)
                {
                    integrityMessages.Add("Mesh '" + meshFilter.sharedMesh.name + "' is not readable. Enable 'Read/Write' in import settings.");
                }

                Collider collider = renderer.GetComponent<Collider>();
                if (collider == null)
                {
                    integrityMessages.Add("Renderer '" + renderer.gameObject.name + "' has no collider.");
                }
            }
        }

        private static void DrawRendererVariant(
            BuildingPart target,
            PropertyCollection properties,
            SerializedObject serializedObject,
            Func<UnityEngine.Object, UnityEditor.Editor> editorProvider,
            BuildingRendererSystem rendererSystem,
            RendererVariantData variant,
            int index)
        {
            if (variant == null)
            {
                return;
            }

            string displayName = variant.Name;
            bool isActive = rendererSystem.ActiveIndex == index;
            Texture2D stateIcon = Resources.Load<Texture2D>(isActive ? "Editor/Icons/on" : "Editor/Icons/off");

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent("Renderer Variant : " + displayName, stateIcon),
                string.Empty,
                () =>
                {
                    UnityEngine.Object targetForEditor = variant.Root ? variant.Root.gameObject : null;
                    UnityEditor.Editor rendererEditor = editorProvider != null ? editorProvider(targetForEditor) : null;

                    GUILayout.BeginHorizontal();
                    if (rendererEditor != null)
                    {
                        WithRenderersTemporarilyEnabled(variant, () =>
                        {
                            rendererEditor.OnPreviewGUI(GUILayoutUtility.GetRect(80f, 80f), EditorStyles.whiteLabel);
                        });
                    }
                    GUILayout.Space(3f);
                    GUILayout.BeginVertical();
                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        EditorGUIExtended.Label("Transform : " + displayName, EditorGUILabels.LabelType.Bold);
                        GUILayout.Space(3f);
                        EditorGUILayout.ObjectField(variant.Root, typeof(Transform), true);
                        EditorGUILayout.BoundsField(variant.GetLocalBounds());
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        using (EditorGUIExtended.IndentScope())
                        {
                            GUILayout.Space(2f);
                            properties.Draw("m_rendererSystem.m_variants.Array.data[" + index + "]", new GUIContent("Cached Data"));
                        }
                    }

                    EditorGUIExtended.Separator("Material Sets");
                    for (int i = 0; i < variant.MaterialVariants.Count; i++)
                    {
                        DrawMaterialSet(target, serializedObject, variant, index, i);
                    }

                    if (GUILayout.Button("Add Material Set..."))
                    {
                        Undo.RecordObject(target, "Add Material Set");
                        variant.AddMaterialSet();
                        EditorUtility.SetDirty(target);
                    }
                },
                menu => DrawRendererVariantContextMenu(menu, target, rendererSystem, index, displayName),
                false);
        }

        private static void DrawRendererVariantContextMenu(
            GenericMenu menu,
            BuildingPart target,
            BuildingRendererSystem rendererSystem,
            int index,
            string displayName)
        {
            if (index >= 0 && rendererSystem.ActiveIndex != index)
            {
                EditorContextMenus.AddItem(menu, "Set Active", () =>
                {
                    Undo.RecordObject(target, "Set Active Variant");
                    rendererSystem.SetVariant(index);
                    EditorUtility.SetDirty(target);
                    Debug.Log("Activated variant '" + displayName + "' on '" + target.name + "'.");
                });
            }
            else
            {
                EditorContextMenus.AddDisabledItem(menu, "Active");
            }

            if (rendererSystem.Count > 1)
            {
                EditorContextMenus.Separator(menu);

                if (index > 0)
                {
                    EditorContextMenus.AddItem(menu, "Move Up", () =>
                    {
                        Undo.RecordObject(target, "Move Renderer Variant Up");
                        List<RendererVariantData> variants = rendererSystem.Variants;
                        (variants[index - 1], variants[index]) = (variants[index], variants[index - 1]);
                        rendererSystem.SetVariant(index - 1);
                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    });
                }

                if (index < rendererSystem.Count - 1)
                {
                    EditorContextMenus.AddItem(menu, "Move Down", () =>
                    {
                        Undo.RecordObject(target, "Move Renderer Variant Down");
                        List<RendererVariantData> variants = rendererSystem.Variants;
                        (variants[index + 1], variants[index]) = (variants[index], variants[index + 1]);
                        rendererSystem.SetVariant(index + 1);
                        EditorUtility.SetDirty(target);
                        SceneView.RepaintAll();
                    });
                }
            }

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddItem(menu, "Refresh Variant", () =>
            {
                Undo.RecordObject(target, "Refresh Variant Renderers");
                rendererSystem.RefreshVariant(rendererSystem.GetVariant(index));
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
            });

            EditorContextMenus.AddItem(menu, "Recalculate Bounds", () =>
            {
                Undo.RecordObject(target, "Recalculate Variant Bounds");
                rendererSystem.RecalculateBounds(rendererSystem.GetVariant(index));
                EditorUtility.SetDirty(target);
            });

            RendererVariantData currentVariant = rendererSystem.GetVariant(index);
            if (currentVariant?.Root != null)
            {
                EditorContextMenus.AddItem(menu, "Edit Offset", () =>
                {
                    BuildingPartRendererOffsetEditor.Open(target.transform, currentVariant.Root);
                });
            }

            EditorContextMenus.Separator(menu);

            EditorContextMenus.AddItem(menu, "Remove", () =>
            {
                if (!EditorUtility.DisplayDialog(
                    "Remove Variant",
                    "Are you sure you want to remove variant '" + displayName + "'?",
                    "Remove", "Cancel"))
                {
                    return;
                }

                Undo.RecordObject(target, "Remove Renderer Variant");
                rendererSystem.Variants.RemoveAt(index);

                int nextIndex = Mathf.Clamp(rendererSystem.ActiveIndex, 0, rendererSystem.Count - 1);
                rendererSystem.SetVariant(nextIndex);

                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
            });
        }

        private static void DrawMaterialSet(
            BuildingPart target,
            SerializedObject serializedObject,
            RendererVariantData variant,
            int variantIndex,
            int setIndex)
        {
            MaterialSetData set = variant.MaterialVariants[setIndex];
            bool isActive = variant.ActiveMaterialIndex == setIndex;
            bool isFallback = setIndex == 0;

            Texture2D stateIcon = Resources.Load<Texture2D>(isActive ? "Editor/Icons/on" : "Editor/Icons/off");
            string label = isFallback ? "[Fallback] Material Set" : $"[{setIndex}] Material Set";

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent(label, stateIcon),
                string.Empty,
                () =>
                {
                    if (variant.Renderers == null || variant.Renderers.Length == 0)
                    {
                        return;
                    }

                    if (isFallback)
                    {
                        EditorGUIExtended.HelpBox("Fallback material set cannot be edited.", EditorGUIElements.MessageType.None);
                        GUILayout.Space(3f);
                    }

                    for (int i = 0; i < variant.Renderers.Length; i++)
                    {
                        Renderer renderer = variant.Renderers[i];
                        if (!renderer)
                        {
                            continue;
                        }

                        using (EditorGUIExtended.IndentScope())
                        {
                            EditorGUIExtended.Label(renderer.gameObject.name, EditorGUILabels.LabelType.Bold);

                            if (isFallback)
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    Material[] materials = renderer.sharedMaterials;
                                    for (int m = 0; m < materials.Length; m++)
                                    {
                                        EditorGUILayout.ObjectField(
                                            $"Material {m}",
                                            materials[m],
                                            typeof(Material),
                                            false);
                                    }
                                }
                            }
                            else
                            {
                                if (set.RendererMaterials == null || i >= set.RendererMaterials.Length)
                                {
                                    continue;
                                }

                                SerializedProperty materialsProperty = serializedObject.FindProperty(
                                    $"m_rendererSystem.m_variants.Array.data[{variantIndex}].MaterialVariants.Array.data[{setIndex}].RendererMaterials.Array.data[{i}].Materials");

                                if (materialsProperty == null)
                                {
                                    continue;
                                }

                                EditorGUILayout.PropertyField(
                                    materialsProperty,
                                    new GUIContent("Materials"),
                                    true);

                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                },
                menu => DrawMaterialSetContextMenu(menu, target, serializedObject, variant, setIndex),
                false);
        }

        private static void DrawMaterialSetContextMenu(
            GenericMenu menu,
            BuildingPart target,
            SerializedObject serializedObject,
            RendererVariantData variant,
            int setIndex)
        {
            bool isActive = variant.ActiveMaterialIndex == setIndex;
            bool isFallback = setIndex == 0;

            if (!isActive)
            {
                EditorContextMenus.AddItem(menu, "Set Active", () =>
                {
                    Undo.RecordObject(target, "Set Material Set Active");
                    variant.SetMaterialSet(setIndex);
                    serializedObject.Update();
                    EditorUtility.SetDirty(target);
                    SceneView.RepaintAll();
                });
            }
            else
            {
                EditorContextMenus.AddDisabledItem(menu, "Active");
            }

            EditorContextMenus.Separator(menu);

            if (isFallback)
            {
                EditorContextMenus.AddItem(menu, "Refresh Fallback Materials", () =>
                {
                    Undo.RecordObject(target, "Refresh Fallback Materials");
                    RefreshFallbackMaterials(variant);
                    serializedObject.Update();
                    EditorUtility.SetDirty(target);
                    SceneView.RepaintAll();
                });

                EditorContextMenus.AddDisabledItem(menu, "Remove (Protected)");
                return;
            }

            EditorContextMenus.AddItem(menu, "Remove", () =>
            {
                if (!EditorUtility.DisplayDialog(
                    "Remove Material Set",
                    $"Remove material set [{setIndex}]?",
                    "Remove", "Cancel"))
                {
                    return;
                }

                Undo.RecordObject(target, "Remove Material Set");
                variant.RemoveMaterialSet(setIndex);

                if (variant.ActiveMaterialIndex >= variant.MaterialVariants.Count)
                {
                    int newIndex = Mathf.Max(0, variant.MaterialVariants.Count - 1);
                    variant.SetMaterialSet(newIndex);
                }

                serializedObject.Update();
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
            });
        }

        private static void RefreshFallbackMaterials(RendererVariantData variant)
        {
            if (variant == null || variant.Renderers == null)
            {
                return;
            }

            MaterialSetData fallback = variant.MaterialVariants[0];

            int count = variant.Renderers.Length;
            fallback.RendererMaterials = new SerializableRendererMaterials[count];

            for (int i = 0; i < count; i++)
            {
                Renderer r = variant.Renderers[i];
                if (!r)
                {
                    fallback.RendererMaterials[i] = new SerializableRendererMaterials();
                    continue;
                }

                fallback.RendererMaterials[i] = new SerializableRendererMaterials
                {
                    Materials = (Material[])r.sharedMaterials.Clone()
                };
            }
        }

        private static bool HandleRendererDropped(BuildingPart target, UnityEngine.Object[] draggedObjects)
        {
            if (target?.RendererSystem == null)
            {
                return false;
            }

            List<Transform> validRoots = new List<Transform>();

            foreach (UnityEngine.Object obj in draggedObjects)
            {
                GameObject go = obj as GameObject;
                if (!go || !ValidateRendererObject(go))
                {
                    continue;
                }

                Transform root = go.transform;

                if (target.RendererSystem.Variants.Exists(v => v?.Root == root))
                {
                    continue;
                }

                Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
                bool hasDuplicate = false;

                foreach (Renderer renderer in renderers)
                {
                    if (!renderer)
                    {
                        continue;
                    }

                    foreach (RendererVariantData variant in target.RendererSystem.Variants)
                    {
                        if (variant?.Renderers == null)
                        {
                            continue;
                        }

                        if (Array.Exists(variant.Renderers, r => r == renderer))
                        {
                            hasDuplicate = true;
                            break;
                        }
                    }

                    if (hasDuplicate)
                    {
                        break;
                    }
                }

                if (!hasDuplicate)
                {
                    validRoots.Add(root);
                }
            }

            if (validRoots.Count == 0)
            {
                return false;
            }

            Undo.RecordObject(target, "Add Variant");
            target.RendererSystem.AddVariantFromRoots(validRoots.ToArray());
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();

            return true;
        }

        private static bool ValidateRendererObject(UnityEngine.Object obj)
        {
            GameObject go = obj as GameObject;
            if (!go)
            {
                return false;
            }

            if (go.GetComponent<BuildingPart>() != null)
            {
                return false;
            }

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
            {
                return true;
            }

            return go.GetComponent<LODGroup>() != null;
        }

        private static void AutoFixIntegrity(BuildingPart target, RendererVariantData variant, List<string> integrityMessages)
        {
            foreach (Renderer renderer in variant.Renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                Collider collider = renderer.GetComponent<Collider>();
                if (collider == null)
                {
                    Undo.AddComponent<MeshCollider>(renderer.gameObject);
                }

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh && !meshFilter.sharedMesh.isReadable)
                {
                    string meshPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                    ModelImporter importer = AssetImporter.GetAtPath(meshPath) as ModelImporter;
                    if (importer != null)
                    {
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                    }
                }
            }

            CheckIntegrity(target, integrityMessages);
            EditorUtility.SetDirty(target);
        }

        private static void WithRenderersTemporarilyEnabled(RendererVariantData variant, Action drawAction)
        {
            if (drawAction == null || variant == null)
            {
                return;
            }

            List<(Renderer r, bool prev)> toggled = new List<(Renderer r, bool prev)>();
            List<(GameObject go, bool prev)> activatedObjects = new List<(GameObject go, bool prev)>();

            try
            {
                if (variant.Root != null)
                {
                    Transform t = variant.Root;
                    while (t != null)
                    {
                        if (!t.gameObject.activeSelf)
                        {
                            activatedObjects.Add((t.gameObject, false));
                            t.gameObject.SetActive(true);
                        }
                        t = t.parent;
                    }
                }

                foreach (LODGroup lodGroup in variant.LODGroups)
                {
                    if (!lodGroup)
                    {
                        continue;
                    }

                    foreach (LOD lodLevel in lodGroup.GetLODs())
                    {
                        foreach (Renderer renderer in lodLevel.renderers)
                        {
                            if (renderer) { toggled.Add((renderer, renderer.enabled)); renderer.enabled = true; }
                        }
                    }
                }

                foreach (Renderer renderer in variant.Renderers)
                {
                    if (renderer) { toggled.Add((renderer, renderer.enabled)); renderer.enabled = true; }
                }

                drawAction();
            }
            finally
            {
                foreach ((Renderer renderer, bool previousState) in toggled)
                {
                    if (renderer)
                    {
                        renderer.enabled = previousState;
                    }
                }

                for (int i = activatedObjects.Count - 1; i >= 0; i--)
                {
                    if (activatedObjects[i].go != null)
                    {
                        activatedObjects[i].go.SetActive(activatedObjects[i].prev);
                    }
                }
            }
        }
    }
}
