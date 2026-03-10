using System;

namespace UnityTickDispatcher
{
    public sealed class TickSubscription : IDisposable
    {
        private TickHandle _handle;

        public TickSubscription(TickHandle handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            var handle = _handle;
            _handle = default;
            handle.Dispose();
        }
    }
}