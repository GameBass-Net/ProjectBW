/// <summary>
/// Project : Mind Code Interactive
/// Class : EventPublisher.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem
{
    public static class EventPublisher
    {
        private static readonly Dictionary<Type, CallbackListBase> s_subscribers = new Dictionary<Type, CallbackListBase>();
        private static readonly object s_lockObject = new object();

        private abstract class CallbackListBase
        {
            public abstract void Invoke(IBaseEvent data);
            public abstract void Clear();
            public abstract int Count { get; }
        }

        private class CallbackList<T> : CallbackListBase where T : IBaseEvent
        {
            private List<Action<T>> m_callbacks = new List<Action<T>>(16);

            public override int Count => m_callbacks.Count;

            public void Subscribe(Action<T> callback)
            {
                if (callback == null)
                {
                    return;
                }

                if (!m_callbacks.Contains(callback))
                {
                    m_callbacks.Add(callback);
                }
            }

            public void Unsubscribe(Action<T> callback)
            {
                if (callback == null)
                {
                    return;
                }

                int index = m_callbacks.IndexOf(callback);
                if (index >= 0)
                {
                    m_callbacks.RemoveAt(index);
                }
            }

            public override void Invoke(IBaseEvent data)
            {
                T typedData = (T)data;

                for (int i = m_callbacks.Count - 1; i >= 0; i--)
                {
                    Action<T> callback = m_callbacks[i];

                    if (callback == null)
                    {
                        m_callbacks.RemoveAt(i);
                        continue;
                    }

                    object target = callback.Target;

                    if (target is UnityEngine.Object uObj && uObj == null)
                    {
                        m_callbacks.RemoveAt(i);
                        continue;
                    }

                    try
                    {
                        callback.Invoke(typedData);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError("Error invoking event callback: " + exception);
                    }
                }
            }

            public override void Clear() => m_callbacks.Clear();
        }

        public static void Subscribe<T>(Action<T> callback) where T : IBaseEvent
        {
            if (callback == null)
            {
                return;
            }

            Type eventType = typeof(T);
            lock (s_lockObject)
            {
                if (!s_subscribers.ContainsKey(eventType))
                {
                    s_subscribers[eventType] = new CallbackList<T>();
                } ((CallbackList<T>)s_subscribers[eventType]).Subscribe(callback);
            }
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : IBaseEvent
        {
            if (callback == null)
            {
                return;
            }

            Type eventType = typeof(T);
            lock (s_lockObject)
            {
                if (s_subscribers.ContainsKey(eventType))
                {
                    ((CallbackList<T>)s_subscribers[eventType]).Unsubscribe(callback);
                    if (s_subscribers[eventType].Count == 0)
                    {
                        s_subscribers.Remove(eventType);
                    }
                }
            }
        }

        public static void Publish<T>(T eventData) where T : IBaseEvent
        {
            if (eventData == null)
            {
                return;
            }

            Type eventType = typeof(T);
            CallbackListBase callbacks = null;

            lock (s_lockObject)
            {
                if (s_subscribers.ContainsKey(eventType))
                {
                    callbacks = s_subscribers[eventType];
                }
            }

            callbacks?.Invoke(eventData);
        }

        public static bool HasSubscribers<T>() where T : IBaseEvent => GetSubscriberCount<T>() > 0;

        public static int GetSubscriberCount<T>() where T : IBaseEvent
        {
            Type eventType = typeof(T);
            lock (s_lockObject)
            {
                if (s_subscribers.ContainsKey(eventType))
                {
                    return s_subscribers[eventType].Count;
                }

                return 0;
            }
        }

        public static void ClearAll()
        {
            lock (s_lockObject)
            {
                foreach (CallbackListBase callbackList in s_subscribers.Values)
                {
                    callbackList.Clear();
                }

                s_subscribers.Clear();
            }
        }
    }
}