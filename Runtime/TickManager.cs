using System;
using UnityEngine;

namespace UnityTickDispatcher
{
    public sealed class TickManager : MonoBehaviour
    {
        [SerializeField] private LoopTiming loopTiming = LoopTiming.FixedUpdate | LoopTiming.Update;

        public static bool IsInitialized => _instance != null;

        private static readonly ActionDisposable NoopDisposable = new ActionDisposable(null);
        private static TickManager _instance;

        private TickPool _tickPool;
        private TickProcessing _fixedProcessing;
        private TickProcessing _updateProcessing;
        private TickProcessing _lateUpdateProcessing;
        private TickProcessing _lastUpdateProcessing;
        private TickProcessing _lastLateUpdateProcessing;
        private LoopTiming _loopTiming;

        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticOnLoad()
        {
            _instance = null;
        }
        #endif

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _loopTiming = loopTiming;

            _tickPool = new TickPool();
            _fixedProcessing = _loopTiming.Contains(LoopTiming.FixedUpdate) ? new TickProcessing(_tickPool) : null;
            _updateProcessing = _loopTiming.Contains(LoopTiming.Update) ? new TickProcessing(_tickPool) : null;
            _lastUpdateProcessing = _loopTiming.Contains(LoopTiming.LastUpdate) ? new TickProcessing(_tickPool) : null;
            _lateUpdateProcessing = _loopTiming.Contains(LoopTiming.LateUpdate) ? new TickProcessing(_tickPool) : null;
            _lastLateUpdateProcessing = _loopTiming.Contains(LoopTiming.LastLateUpdate) ? new TickProcessing(_tickPool) : null;
        }

        private void FixedUpdate()
        {
            _fixedProcessing?.Run();
        }

        private void Update()
        {
            _updateProcessing?.Run();
            _lastUpdateProcessing?.Run();
        }

        private void LateUpdate()
        {
            _lateUpdateProcessing?.Run();
            _lastLateUpdateProcessing?.Run();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                // TODO clear local fields
                _instance = null;
            }
        }

        public static void Optimize()
        {
            if (!IsInitialized)
            {
                return;
            }

            _instance._fixedProcessing?.Optimize();
            _instance._updateProcessing?.Optimize();
            _instance._lastUpdateProcessing?.Optimize();
            _instance._lateUpdateProcessing?.Optimize();
            _instance._lastLateUpdateProcessing?.Optimize();
        }

        public static IDisposable Subscribe(Action action, LoopTiming loopTiming = LoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return NoopDisposable;
            }

            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _instance._fixedProcessing,
                LoopTiming.Update => _instance._updateProcessing,
                LoopTiming.LastUpdate => _instance._lastUpdateProcessing,
                LoopTiming.LateUpdate => _instance._lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _instance._lastLateUpdateProcessing,

                _ => null
            };

            return processing?.Add(action) ?? NoopDisposable;
        }

        public static TickHandle SubscribeAsHandle(Action action, LoopTiming loopTiming = LoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return default;
            }

            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _instance._fixedProcessing,
                LoopTiming.Update => _instance._updateProcessing,
                LoopTiming.LastUpdate => _instance._lastUpdateProcessing,
                LoopTiming.LateUpdate => _instance._lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _instance._lastLateUpdateProcessing,

                _ => null
            };

            return processing?.AddHandle(action) ?? default;
        }

        public static void SubscribeAsHandle(Action action, ref TickHandle handle, LoopTiming loopTiming = LoopTiming.Update)
        {
            handle.Dispose();

            var processing = loopTiming switch
            {
                LoopTiming.FixedUpdate => _instance._fixedProcessing,
                LoopTiming.Update => _instance._updateProcessing,
                LoopTiming.LastUpdate => _instance._lastUpdateProcessing,
                LoopTiming.LateUpdate => _instance._lateUpdateProcessing,
                LoopTiming.LastLateUpdate => _instance._lastLateUpdateProcessing,

                _ => null
            };

            handle = IsInitialized ? processing?.AddHandle(action) ?? default : default;
        }
    }
}