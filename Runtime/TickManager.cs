using Gatekeeper.Utility;
using System;
using UnityEngine;

namespace UnityTickDispatcher
{
    [DefaultExecutionOrder(-1000)]
    public sealed class TickManager : MonoSingleton<TickManager>
    {
        private static readonly ActionDisposable NoopDisposable = new ActionDisposable(null);

        private TickProcessing _fixedProcessing;
        private TickProcessing _updateProcessing;
        private TickProcessing _lateUpdateProcessing;
        private TickPool _tickPool;

        private void Awake()
        {
            InitializeSingleton();

            if (Instance != this)
            {
                return;
            }

            _tickPool = new TickPool();
            _fixedProcessing = new TickProcessing(_tickPool);
            _updateProcessing = new TickProcessing(_tickPool);
            _lateUpdateProcessing = new TickProcessing(_tickPool);
        }

        private void FixedUpdate()
        {
            _fixedProcessing.Run();
        }

        private void Update()
        {
            _updateProcessing.Run();
        }

        private void LateUpdate()
        {
            _lateUpdateProcessing.Run();
        }

        public void Optimize()
        {
            _fixedProcessing.Optimize();
            _updateProcessing.Optimize();
            _lateUpdateProcessing.Optimize();
        }

        public static IDisposable EveryFixedUpdate(Action action)
            => IsInitialized ? Instance._fixedProcessing.Add(action) : NoopDisposable;

        public static IDisposable EveryUpdate(Action action)
            => IsInitialized ? Instance._updateProcessing.Add(action) : NoopDisposable;

        public static IDisposable EveryLateUpdate(Action action)
            => IsInitialized ? Instance._lateUpdateProcessing.Add(action) : NoopDisposable;

        public static TickHandle EveryFixedUpdateAsHandle(Action action)
            => IsInitialized ? Instance._fixedProcessing.AddHandle(action) : default;

        public static TickHandle EveryUpdateAsHandle(Action action)
            => IsInitialized ? Instance._updateProcessing.AddHandle(action) : default;

        public static TickHandle EveryLateUpdateAsHandle(Action action)
            => IsInitialized ? Instance._lateUpdateProcessing.AddHandle(action) : default;

        public static void EveryFixedUpdateAsHandle(Action action, ref TickHandle handle)
        {
            handle.Dispose();
            handle = IsInitialized ? Instance._fixedProcessing.AddHandle(action) : default;
        }

        public static void EveryUpdateAsHandle(Action action, ref TickHandle handle)
        {
            handle.Dispose();
            handle = IsInitialized ? Instance._updateProcessing.AddHandle(action) : default;
        }

        public static void EveryLateUpdateAsHandle(Action action, ref TickHandle handle)
        {
            handle.Dispose();
            handle = IsInitialized ? Instance._lateUpdateProcessing.AddHandle(action) : default;
        }
    }
}