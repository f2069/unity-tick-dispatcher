using System;

namespace UnityTickDispatcher
{
    public sealed class TickData
    {
        public bool IsValid => !_isDisposed && Action != null;

        public Action Action { get; private set; }
        public long Version { get; private set; }

        private bool _isDisposed;

        public void Init(Action action, long version)
        {
            Action = action;
            Version = version;
            _isDisposed = false;
        }

        public void ResetData()
        {
            Action = null;
            Version = 0;
            _isDisposed = false;
        }

        public void Dispose(long version)
        {
            if (Version != version)
            {
                return;
            }

            _isDisposed = true;
            Action = null;
        }
    }
}