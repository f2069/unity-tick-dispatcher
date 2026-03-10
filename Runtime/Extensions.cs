using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityTickDispatcher
{
    public static class Extensions
    {
        public static void DisposeTickHandle(this Component _, ref TickHandle handle)
        {
            handle.Dispose();
            handle = default;
        }

        internal static bool Contains(this LoopTiming whole, LoopTiming part) => (whole & part) == part;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveNulls<T>(this List<T> target)
        {
            for (var i = target.Count - 1; i >= 0; i--)
            {
                if (target[i] == null)
                {
                    target.RemoveAt(i);
                }
            }
        }
    }
}