/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorCoroutines.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers
{
    public static class EditorCoroutines
    {
        private static readonly Dictionary<int, Stack<IEnumerator>> s_coroutines = new Dictionary<int, Stack<IEnumerator>>();

        static EditorCoroutines() => EditorApplication.update += Update;

        public static IEnumerator StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                return null;
            }

            int routineHash = routine.GetHashCode();
            Stack<IEnumerator> routineStack = new Stack<IEnumerator>();
            routineStack.Push(routine);
            s_coroutines[routineHash] = routineStack;

            return routine;
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                return;
            }

            s_coroutines.Remove(routine.GetHashCode());
        }

        public static void StopAllCoroutines() => s_coroutines.Clear();

        private static void Update()
        {
            foreach (KeyValuePair<int, Stack<IEnumerator>> coroutineEntry in s_coroutines.ToList())
            {
                Stack<IEnumerator> routineStack = coroutineEntry.Value;
                if (routineStack.Count == 0)
                {
                    s_coroutines.Remove(coroutineEntry.Key);
                    continue;
                }

                IEnumerator currentRoutine = routineStack.Peek();

                if (!currentRoutine.MoveNext())
                {
                    routineStack.Pop();
                    continue;
                }

                object currentValue = currentRoutine.Current;
                if (currentValue is IEnumerator nestedRoutine)
                {
                    routineStack.Push(nestedRoutine);
                }
            }
        }

        public class EditorWaitForSeconds : IEnumerator
        {
            private readonly double m_waitUntilTime;

            public object Current => null;

            public EditorWaitForSeconds(float seconds)
                => m_waitUntilTime = EditorApplication.timeSinceStartup + seconds;

            public bool MoveNext() => EditorApplication.timeSinceStartup < m_waitUntilTime;

            public void Reset() { }
        }
    }
}