using System;
using UnityEngine;

namespace UnityTickDispatcher
{
    [DefaultExecutionOrder(-1000)]
    public sealed class TickManager : MonoBehaviour
    {
        private static readonly ActionDisposable NoopDisposable = new ActionDisposable(null);
        private static bool IsInitialized => _instance != null;
        private static TickManager _instance;

        private TickPool _tickPool;
        private TickProcessing _fixedProcessing;
        private TickProcessing _updateProcessing;
        private TickProcessing _lateUpdateProcessing;
        private TickProcessing _lastUpdateProcessing;
        private TickProcessing _lastLateUpdateProcessing;

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

            _tickPool = new TickPool();
            _fixedProcessing = new TickProcessing(_tickPool);
            _updateProcessing = new TickProcessing(_tickPool);
            _lastUpdateProcessing = new TickProcessing(_tickPool);
            _lateUpdateProcessing = new TickProcessing(_tickPool);
            _lastLateUpdateProcessing = new TickProcessing(_tickPool);
        }

        private void FixedUpdate()
        {
            _fixedProcessing.Run();
        }

        private void Update()
        {
            _updateProcessing.Run();
            _lastUpdateProcessing.Run();
        }

        private void LateUpdate()
        {
            _lateUpdateProcessing.Run();
            _lastLateUpdateProcessing.Run();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Optimize()
        {
            _fixedProcessing.Optimize();
            _updateProcessing.Optimize();
            _lastUpdateProcessing.Optimize();
            _lateUpdateProcessing.Optimize();
            _lastLateUpdateProcessing.Optimize();
        }

        public static IDisposable Subscribe(Action action, PlayerLoopTiming loopTiming = PlayerLoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return NoopDisposable;
            }

            switch (loopTiming)
            {
                case PlayerLoopTiming.FixedUpdate:
                    _instance._fixedProcessing.Add(action);
                    break;
                case PlayerLoopTiming.Update:
                    _instance._updateProcessing.Add(action);
                    break;
                case PlayerLoopTiming.LastUpdate:
                    _instance._lastUpdateProcessing.Add(action);
                    break;
                case PlayerLoopTiming.LateUpdate:
                    _instance._lateUpdateProcessing.Add(action);
                    break;
                case PlayerLoopTiming.LastLateUpdate:
                    _instance._lastLateUpdateProcessing.Add(action);
                    break;
                default:
                    Debug.LogError($"LoopTiming {loopTiming} not implemented.");
                    break;
            }

            return NoopDisposable;
        }

        public static TickHandle SubscribeAsHandle(Action action, PlayerLoopTiming loopTiming = PlayerLoopTiming.Update)
        {
            if (!IsInitialized)
            {
                return default;
            }

            switch (loopTiming)
            {
                case PlayerLoopTiming.FixedUpdate:
                    _instance._fixedProcessing.AddHandle(action);
                    break;
                case PlayerLoopTiming.Update:
                    _instance._updateProcessing.AddHandle(action);
                    break;
                case PlayerLoopTiming.LastUpdate:
                    _instance._lastUpdateProcessing.AddHandle(action);
                    break;
                case PlayerLoopTiming.LateUpdate:
                    _instance._lateUpdateProcessing.AddHandle(action);
                    break;
                case PlayerLoopTiming.LastLateUpdate:
                    _instance._lastLateUpdateProcessing.AddHandle(action);
                    break;
                default:
                    Debug.LogError($"LoopTiming {loopTiming} not implemented.");
                    break;
            }

            return default;
        }
    }
}