/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroup.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;
using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Implementations.Collapse;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Groups
{
    [ExecuteAlways]
    public class BuildingGroup : RegisterableUniqueObject, IDebuggable
    {
        [SerializeField] protected bool m_dontDestroyGroup = false;
        [SerializeField] protected List<BuildingPart> m_groupedParts = new List<BuildingPart>();
        [SerializeField] protected Bounds m_groupBounds;

        private GameObject m_groupCombinedObject;
        private bool m_isDynamic = false;
        private bool m_isBatched = false;

        public bool DontDestroyGroup { get => m_dontDestroyGroup; set => m_dontDestroyGroup = value; }

        public List<BuildingPart> GroupedParts => m_groupedParts;

        public Bounds GroupBounds => m_groupBounds;

        public bool IsDynamic { get => m_isDynamic; set => m_isDynamic = value; }

        public bool IsBatched => m_isBatched;

        public override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Register(this);
            }

            SynchronizeChildrenParts();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Application.isPlaying)
            {
                DebugRendererManager.Unregister(this);
            }
        }

        protected virtual void LateUpdate()
        {
            if (m_isDynamic)
            {
                RecenterPivot();
            }
        }

        private void OnDestroy()
        {
            DisposeCombinedMeshes();
        }

        private void OnValidate()
        {
        }

        public void AddPart(BuildingPart part)
        {
            if (part == null || m_groupedParts.Contains(part))
            {
                return;
            }

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(part.gameObject))
            {
                return;
            }
#endif

            part.AttachToGroup(this);
            m_groupedParts.Add(part);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.SetTransformParent(part.transform, transform, true, "Group Building Part");
            }
            else
#endif
            {
                part.transform.SetParent(transform, true);
            }

            if (part.CacheSystem != null &&
                part.CacheSystem.Rigidbodies != null && part.CacheSystem.Rigidbodies.Length > 0)
            {
                m_isDynamic = true;
            }

            RecenterPivot();
            EventPublisher.Publish(new BuildingGroupEvent.UpdatedEventArgs(this, part));
        }

        public void RemovePart(BuildingPart part)
        {
            if (!m_groupedParts.Remove(part))
            {
                return;
            }

            UpdateBounds();

            if (IsEmpty())
            {
                DestroyGroup();
            }
            else
            {
                EventPublisher.Publish(new BuildingGroupEvent.UpdatedEventArgs(this, part));
            }
        }

        public bool IsEmpty() => m_groupedParts == null || m_groupedParts.Count == 0;

        public Bounds UpdateBounds()
        {
            Bounds boundingBox = new Bounds(transform.position, Vector3.zero);
            bool hasAnyPart = false;
            for (int i = 0; i < m_groupedParts.Count; i++)
            {
                BuildingPart part = m_groupedParts[i];
                if (part == null)
                {
                    continue;
                }

                Bounds partBounds = part.RendererSystem.Active.GetWorldBounds();
                if (!hasAnyPart)
                {
                    boundingBox = partBounds;
                    hasAnyPart = true;
                }
                else
                {
                    boundingBox.Encapsulate(partBounds);
                }
            }
            m_groupBounds = hasAnyPart ? boundingBox : new Bounds(transform.position, Vector3.zero);
            return m_groupBounds;
        }

        public Vector3 GetClosestPoint(Vector3 position)
        {
            UpdateBounds();
            return m_groupBounds.ClosestPoint(position);
        }

        public void BatchGroup()
        {
            if (m_isBatched || m_groupedParts == null || m_groupedParts.Count == 0)
            {
                return;
            }

            for (int i = 0; i < m_groupedParts.Count; i++)
            {
                BuildingPart part = m_groupedParts[i];

                if (!part)
                {
                    continue;
                }

                if (!part.IsPlaced)
                {
                    return;
                }

                if (IsPartBusy(part))
                {
                    return;
                }
            }

            ClearCombinedObject();

            Dictionary<Material, List<CombineInstance>> materialGroups = CollectMeshes();
            if (materialGroups.Count == 0)
            {
                return;
            }

            CreateCombinedMesh(materialGroups);

            m_isBatched = true;

            for (int i = 0; i < m_groupedParts.Count; i++)
            {
                BuildingPart part = m_groupedParts[i];

                if (!part)
                {
                    continue;
                }

                part.RendererSystem.SetVariantVisibility(false);
            }
        }

        public void UnbatchGroup()
        {
            if (!m_isBatched)
            {
                return;
            }

            DisposeCombinedMeshes();

            for (int i = 0; i < m_groupedParts.Count; i++)
            {
                BuildingPart part = m_groupedParts[i];
                if (!part)
                {
                    continue;
                }

                part.RendererSystem.SetVariantVisibility(true);
            }

            m_isBatched = false;
        }

        private bool IsPartBusy(BuildingPart part)
        {
            if (!part)
            {
                return false;
            }

            if (!part.enabled)
            {
                return true;
            }

            if (part.IsDynamic)
            {
                return true;
            }

            BuildingCollapseCondition physicsCondition =
                part.ConditionSystem?.GetCondition(typeof(BuildingCollapseCondition)) as BuildingCollapseCondition;
            if (physicsCondition != null && physicsCondition.IsFalling)
            {
                return true;
            }

            BuildingAnimationBehavior animationBehavior =
                part.BehaviorSystem?.GetBehavior(typeof(BuildingAnimationBehavior)) as BuildingAnimationBehavior;
            if (animationBehavior != null && animationBehavior.HasActiveAnimation)
            {
                return true;
            }

            return false;
        }

        private void ClearCombinedObject()
        {
            if (!m_groupCombinedObject)
            {
                return;
            }

            Transform container = m_groupCombinedObject.transform;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                GameObject child = container.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private Dictionary<Material, List<CombineInstance>> CollectMeshes()
        {
            Dictionary<Material, List<CombineInstance>> materialGroups = new Dictionary<Material, List<CombineInstance>>();

            for (int i = 0; i < m_groupedParts.Count; i++)
            {
                BuildingPart part = m_groupedParts[i];
                if (!part)
                {
                    continue;
                }

                part.PlacementSystem?.RestoreMaterials();

                Parts.Implementations.Renderers.Data.RendererVariantData currentVariant = part.RendererSystem != null ? part.RendererSystem.Active : null;
                if (currentVariant != null)
                {
                    currentVariant.SetMaterialSet(currentVariant.ActiveMaterialIndex);
                }

                part.RendererSystem.RefreshVariant();

                Parts.Implementations.Renderers.Data.RendererVariantData activeVariant = part.RendererSystem.Active;
                if (activeVariant?.Renderers == null || activeVariant.Renderers.Length == 0)
                {
                    continue;
                }

                Renderer[] partRenderers = activeVariant.Renderers;

                for (int r = 0; r < partRenderers.Length; r++)
                {
                    Renderer renderer = partRenderers[r];
                    if (!renderer || !renderer.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    Mesh extractedMesh = ExtractMeshFromRenderer(renderer);
                    if (!extractedMesh || !extractedMesh.isReadable)
                    {
                        continue;
                    }

                    Material[] rendererMaterials = renderer.sharedMaterials;
                    int subMeshCount = extractedMesh.subMeshCount > 0 ? extractedMesh.subMeshCount : 1;

                    for (int s = 0; s < subMeshCount; s++)
                    {
                        Material subMeshMaterial = (rendererMaterials != null && s < rendererMaterials.Length)
                            ? rendererMaterials[s]
                            : null;

                        if (!materialGroups.TryGetValue(subMeshMaterial, out List<CombineInstance> combineList))
                        {
                            combineList = new List<CombineInstance>();
                            materialGroups[subMeshMaterial] = combineList;
                        }

                        combineList.Add(new CombineInstance
                        {
                            mesh = extractedMesh,
                            subMeshIndex = s,
                            transform = Matrix4x4.TRS(
                                transform.InverseTransformPoint(renderer.transform.position),
                                Quaternion.Inverse(transform.rotation) * renderer.transform.rotation,
                                renderer.transform.lossyScale)
                        });
                    }
                }
            }

            return materialGroups;
        }

        private Mesh ExtractMeshFromRenderer(Renderer renderer)
        {
            SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedRenderer != null)
            {
                return skinnedRenderer.sharedMesh;
            }

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return meshFilter.sharedMesh;
            }

            return null;
        }

        private void CreateCombinedMesh(Dictionary<Material, List<CombineInstance>> materialGroups)
        {
            if (!m_groupCombinedObject)
            {
                m_groupCombinedObject = new GameObject("Combined Mesh");
                m_groupCombinedObject.transform.SetParent(transform, false);
            }

            m_groupCombinedObject.transform.localPosition = Vector3.zero;
            m_groupCombinedObject.transform.localRotation = Quaternion.identity;
            m_groupCombinedObject.transform.localScale = Vector3.one;

            int materialIndex = 0;
            foreach (KeyValuePair<Material, List<CombineInstance>> materialGroup in materialGroups)
            {
                if (materialGroup.Value.Count == 0)
                {
                    continue;
                }

                GameObject materialObject = new GameObject("Material_" + materialIndex);
                materialObject.transform.SetParent(m_groupCombinedObject.transform, false);

                MeshFilter meshFilter = materialObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = materialObject.AddComponent<MeshRenderer>();

                Mesh combinedMesh = new Mesh();
                combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                combinedMesh.CombineMeshes(materialGroup.Value.ToArray(), true, true);

                meshFilter.sharedMesh = combinedMesh;
                meshRenderer.sharedMaterial = materialGroup.Key;
                meshRenderer.enabled = true;

                materialIndex++;
            }
        }

        private void DisposeCombinedMeshes()
        {
            if (!m_groupCombinedObject)
            {
                return;
            }

            Transform container = m_groupCombinedObject.transform;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                MeshFilter meshFilter = container.GetChild(i).GetComponent<MeshFilter>();
                if (meshFilter && meshFilter.sharedMesh)
                {
                    Destroy(meshFilter.sharedMesh);
                }
            }
        }

        private void SynchronizeChildrenParts()
        {
            if (m_groupedParts == null)
            {
                m_groupedParts = new List<BuildingPart>();
            }

            BuildingPart[] childrenParts = GetComponentsInChildren<BuildingPart>();

            for (int i = 0; i < childrenParts.Length; i++)
            {
                BuildingPart childPart = childrenParts[i];
                if (childPart != null && !m_groupedParts.Contains(childPart))
                {
                    m_groupedParts.Add(childPart);
                    childPart.AttachToGroup(this);
                }
            }

            for (int i = m_groupedParts.Count - 1; i >= 0; i--)
            {
                BuildingPart part = m_groupedParts[i];
                if (part == null || part.transform.parent != this.transform)
                {
                    m_groupedParts.RemoveAt(i);
                }
            }

            UpdateBounds();
        }

        public virtual void RecenterPivot()
        {
            RecenterPivot(GroupPivotMode.Center);
        }

        public virtual void RecenterPivot(GroupPivotMode pivotMode)
        {
            List<BuildingPart> groupedPartsCopy = new List<BuildingPart>(m_groupedParts);
            if (groupedPartsCopy.Count == 0)
            {
                return;
            }

            Vector3 newCenterPosition;

            switch (pivotMode)
            {
                case GroupPivotMode.GroupTransform:
                    return;

                case GroupPivotMode.FirstPart:
                    {
                        BuildingPart first = null;
                        for (int i = 0; i < groupedPartsCopy.Count; i++)
                        {
                            if (groupedPartsCopy[i] != null)
                            {
                                first = groupedPartsCopy[i];
                                break;
                            }
                        }
                        if (first == null)
                        {
                            return;
                        }
                        newCenterPosition = first.transform.position;
                        break;
                    }

                case GroupPivotMode.Ground:
                    {
                        Bounds b = UpdateBounds();
                        newCenterPosition = new Vector3(b.center.x, 0f, b.center.z);
                        break;
                    }

                case GroupPivotMode.Center:
                default:
                    {
                        Vector3 positionSum = Vector3.zero;
                        int validPartCount = 0;

                        for (int i = 0; i < groupedPartsCopy.Count; i++)
                        {
                            BuildingPart part = groupedPartsCopy[i];
                            if (part == null)
                            {
                                continue;
                            }

                            positionSum += part.transform.position;
                            validPartCount++;
                        }

                        if (validPartCount == 0)
                        {
                            return;
                        }

                        newCenterPosition = positionSum / validPartCount;
                        break;
                    }
            }

            int count = groupedPartsCopy.Count;
            Vector3[] savedWorldPositions = new Vector3[count];
            Quaternion[] savedWorldRotations = new Quaternion[count];

            for (int i = 0; i < count; i++)
            {
                BuildingPart part = groupedPartsCopy[i];
                if (part == null)
                {
                    continue;
                }

                savedWorldPositions[i] = part.transform.position;
                savedWorldRotations[i] = part.transform.rotation;
            }

            transform.position = newCenterPosition;

            for (int i = 0; i < count; i++)
            {
                BuildingPart part = groupedPartsCopy[i];
                if (part == null)
                {
                    continue;
                }

                part.transform.position = savedWorldPositions[i];
                part.transform.rotation = savedWorldRotations[i];
            }

            UpdateBounds();
        }

        public void DetachPart(BuildingPart part)
        {
            if (!m_groupedParts.Remove(part))
            {
                return;
            }

            part.AttachToGroup(null);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.SetTransformParent(part.transform, null, true, "Detach Building Part");
            }
            else
#endif
            {
                part.transform.SetParent(null, true);
            }

            UpdateBounds();

            if (IsEmpty())
            {
                DestroyGroup();
            }
            else
            {
                EventPublisher.Publish(new BuildingGroupEvent.UpdatedEventArgs(this, part));
            }
        }

        public void DestroyGroup()
        {
            if (m_dontDestroyGroup)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
#if UNITY_EDITOR
                if (m_groupedParts.Count == 0)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (this != null)
                        {
                            DestroyImmediate(gameObject);
                        }
                    };
                }
#endif
            }
        }

        #region IDebuggable

        [SerializeField] private DebugRenderer.ViewFlags m_debugFlags = DebugRenderer.ViewFlags.None;

        public bool DebugEnabled => isActiveAndEnabled;

        public DebugRenderer.ViewFlags DebugFlags
        {
            get => m_debugFlags;
            set => m_debugFlags = value;
        }

        public bool RequireSelection => false;

        public virtual void OnDebugRender()
        {
            Color wireframeColor = m_isBatched ? Color.white : Color.cyan;
            Color fillColor = m_isBatched ? new Color(1f, 1f, 1f, 0.1f) : new Color(0f, 1f, 1f, 0.1f);

            DebugRenderer.DrawWireCube(m_groupBounds.center, m_groupBounds.size, transform.rotation, wireframeColor, 0f, 1f, false);
            DebugRenderer.DrawCube(m_groupBounds.center, m_groupBounds.size, transform.rotation, fillColor, 0f, false);
        }

        #endregion
    }
}