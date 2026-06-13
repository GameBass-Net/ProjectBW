/// <summary>
/// Project : Mind Code Interactive
/// Class : BaseInspectorEditor.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using Object = UnityEngine.Object;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors
{
    public abstract class BaseInspectorEditor<T> : UnityEditor.Editor where T : Object
    {
        private EditorInstanceCache m_editorInstanceCache = new EditorInstanceCache();
        private ChildrenCollectorRegistry<T> m_childrenCollectorRegistry = new ChildrenCollectorRegistry<T>();

        public T Target { get; private set; }
        public T[] TargetsTyped { get; private set; }
        public PropertyCollection Properties { get; private set; }

        protected virtual void OnEnable()
        {
            Target = target as T;
            TargetsTyped = ExtractTypedTargets();
            Properties = PropertyCollection.CreateFrom(serializedObject);
            OnInspectorEnable();
        }

        protected virtual void OnDisable()
        {
            OnInspectorDisable();
            m_editorInstanceCache?.Dispose();
            m_childrenCollectorRegistry?.Clear();
        }

        public sealed override void OnInspectorGUI()
        {
            if (serializedObject == null ||
                    serializedObject.targetObject == null ||
                    serializedObject.targetObject.Equals(null))
            {
                return;
            }

            serializedObject.Update();
            OnInspectorDraw();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnInspectorEnable() { }
        protected virtual void OnInspectorDisable() { }
        protected virtual void OnInspectorDraw() { }

        protected void RegisterChildrenListAccessor<TChild>(Func<T, IList<TChild>> childrenAccessor) where TChild : Object
        {
            if (childrenAccessor != null)
            {
                m_childrenCollectorRegistry.Register(childrenAccessor);
            }
        }

        protected UnityEditor.Editor GetOrCreateChildMultiEditor(Object targetObject)
        {
            if (!targetObject || TargetsTyped.Length == 0)
            {
                return null;
            }

            Type targetObjectType = targetObject.GetType();
            if (m_editorInstanceCache.TryGetMultiEditor(targetObjectType, out UnityEditor.Editor cachedEditor) && cachedEditor)
            {
                return cachedEditor;
            }

            List<Object> collectedChildren = CollectChildrenOfType(targetObjectType);
            if (collectedChildren == null || collectedChildren.Count == 0)
            {
                return null;
            }

            Object[] filteredChildren = collectedChildren.Where(child => child && child.GetType() == targetObjectType).ToArray();
            if (filteredChildren.Length == 0)
            {
                return null;
            }

            UnityEditor.Editor newEditor = CreateEditor(filteredChildren);
            m_editorInstanceCache.CacheMultiEditor(targetObjectType, newEditor);
            return newEditor;
        }

        protected UnityEditor.Editor GetOrCreateEditor(Object targetObject)
        {
            if (!targetObject)
            {
                return null;
            }

#pragma warning disable CS0618
            int instanceId = targetObject.GetInstanceID();
#pragma warning restore CS0618

            if (m_editorInstanceCache.TryGetSingleEditor(instanceId, out UnityEditor.Editor cachedEditor) && ReferenceEquals(cachedEditor.target, targetObject))
            {
                return cachedEditor;
            }

            UnityEditor.Editor newEditor = CreateEditor(targetObject);
            m_editorInstanceCache.CacheSingleEditor(instanceId, newEditor);
            return newEditor;
        }

        private T[] ExtractTypedTargets()
        {
            if (targets == null || targets.Length == 0)
            {
                return Array.Empty<T>();
            }

            List<T> typedTargetsList = new List<T>();
            foreach (Object targetObject in targets)
            {
                if (targetObject is T typedTarget)
                {
                    typedTargetsList.Add(typedTarget);
                }
            }
            return typedTargetsList.ToArray();
        }

        private List<Object> CollectChildrenOfType(Type targetType)
        {
            List<Object> collectedChildren = new List<Object>();
            foreach (T parentTarget in TargetsTyped)
            {
                if (parentTarget)
                {
                    m_childrenCollectorRegistry.CollectFromParent(parentTarget, targetType, collectedChildren);
                }
            }
            return collectedChildren;
        }
    }

    internal class EditorInstanceCache : IDisposable
    {
        private readonly Dictionary<Type, UnityEditor.Editor> m_multiEditors = new Dictionary<Type, UnityEditor.Editor>();
        private readonly Dictionary<int, UnityEditor.Editor> m_singleEditors = new Dictionary<int, UnityEditor.Editor>();

        public bool TryGetMultiEditor(Type editorType, out UnityEditor.Editor editor)
        {
            if (m_multiEditors.TryGetValue(editorType, out editor) && editor)
            {
                return true;
            }

            editor = null;
            return false;
        }

        public bool TryGetSingleEditor(int instanceId, out UnityEditor.Editor editor)
        {
            if (m_singleEditors.TryGetValue(instanceId, out editor) && editor)
            {
                return true;
            }

            editor = null;
            return false;
        }

        public void CacheMultiEditor(Type editorType, UnityEditor.Editor editor)
        {
            if (editor)
            {
                m_multiEditors[editorType] = editor;
            }
        }

        public void CacheSingleEditor(int instanceId, UnityEditor.Editor editor)
        {
            if (editor)
            {
                m_singleEditors[instanceId] = editor;
            }
        }

        public void Dispose()
        {
            foreach (UnityEditor.Editor cachedEditor in m_multiEditors.Values)
            {
                if (cachedEditor)
                {
                    Object.DestroyImmediate(cachedEditor);
                }
            }

            foreach (UnityEditor.Editor cachedEditor in m_singleEditors.Values)
            {
                if (cachedEditor)
                {
                    Object.DestroyImmediate(cachedEditor);
                }
            }

            m_multiEditors.Clear();
            m_singleEditors.Clear();
        }
    }

    internal class ChildrenCollectorRegistry<T> where T : Object
    {
        private readonly List<IChildrenCollector> m_collectorsList = new List<IChildrenCollector>();

        public void Register<TChild>(Func<T, IList<TChild>> childrenAccessor) where TChild : Object
        {
            if (childrenAccessor != null)
            {
                m_collectorsList.Add(new ChildrenCollector<TChild>(childrenAccessor));
            }
        }

        public void CollectFromParent(T parentTarget, Type targetType, List<Object> outputChildren)
        {
            foreach (IChildrenCollector collector in m_collectorsList)
            {
                if (collector.TryCollect(parentTarget, targetType, outputChildren))
                {
                    return;
                }
            }
        }

        public void Clear() => m_collectorsList.Clear();

        private interface IChildrenCollector
        {
            bool TryCollect(T parentTarget, Type targetType, List<Object> outputChildren);
        }

        private class ChildrenCollector<TChild> : IChildrenCollector where TChild : Object
        {
            private readonly Func<T, IList<TChild>> m_childrenAccessor;

            public ChildrenCollector(Func<T, IList<TChild>> childrenAccessor) => m_childrenAccessor = childrenAccessor;

            public bool TryCollect(T parentTarget, Type targetType, List<Object> outputChildren)
            {
                if (!typeof(TChild).IsAssignableFrom(targetType))
                {
                    return false;
                }

                IList<TChild> childrenList = m_childrenAccessor(parentTarget);
                if (childrenList == null || childrenList.Count == 0)
                {
                    return false;
                }

                foreach (TChild child in childrenList)
                {
                    if (child)
                    {
                        outputChildren.Add(child);
                    }
                }

                return true;
            }
        }
    }
}