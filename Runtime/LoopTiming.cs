using System;

namespace UnityTickDispatcher
{
    [Flags]
    public enum LoopTiming
    {
        None = 0,
        FixedUpdate = 1 << 3,

        Update = 1 << 6,
        LastUpdate = 1 << 7,

        LateUpdate = 1 << 9,
        LastLateUpdate = 1 << 10,
    }
}