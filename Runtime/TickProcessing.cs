using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityTickDispatcher
{
    internal sealed class TickProcessing
    {
        private const int InitialTicksCapacity = 500;
        private const int InitialBufferCapacity = 100;

        private readonly TickPool _pool;
        private readonly List<TickData> _ticks = new List<TickData>(InitialTicksCapacity);
        private readonly List<TickData> _returnToPoolBuffer = new List<TickData>(InitialBufferCapacity);

        private Queue<TickData> _queueWrite = new Queue<TickData>(100);
        private Queue<TickData> _queueRead = new Queue<TickData>(100);

        private readonly object _queueLock = new object();
        private int _tail;
        private bool _canBeTrim;

        public TickProcessing(TickPool pool)
        {
            _pool = pool;
        }

        public IDisposable Add(Action action)
        {
            var handle = AddHandle(action);
            return new TickSubscription(handle);
        }

        public TickHandle AddHandle(Action action)
        {
            var data = _pool.Retrieve(action);

            lock (_queueLock)
            {
                _queueWrite.Enqueue(data);
            }

            return new TickHandle(data);
        }

        public void Run()
        {
            _returnToPoolBuffer.Clear();

            var last = _tail - 1;
            var i = 0;

            while (i <= last)
            {
                var tickData = _ticks[i];
                if (tickData is not { IsValid: true })
                {
                    _ticks[i] = _ticks[last];
                    _ticks[last] = null;
                    last--;

                    if (tickData != null)
                    {
                        _returnToPoolBuffer.Add(tickData);
                    }

                    continue;
                }

                try
                {
                    tickData.Action.Invoke();
                    i++;
                }
                catch (Exception e)
                {
                    _ticks[i] = _ticks[last];
                    _ticks[last] = null;
                    last--;

                    _returnToPoolBuffer.Add(tickData);

                    Debug.LogException(e);
                }
            }

            _tail = last + 1;

            lock (_queueLock)
            {
                (_queueRead, _queueWrite) = (_queueWrite, _queueRead);
            }

            while (_queueRead.TryDequeue(out var data))
            {
                if (data is not { IsValid: true })
                {
                    _returnToPoolBuffer.Add(data);
                    continue;
                }

                AddInternal(data);
            }

            _pool.Store(_returnToPoolBuffer);
            TryTrimLists();
        }

        public void Optimize()
        {
            _canBeTrim = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(TickData data)
        {
            if (_ticks.Capacity <= _tail)
            {
                _ticks.Capacity = checked(_tail * 2);
            }

            if (_ticks.Count == _tail)
            {
                _ticks.Add(data);
            }
            else
            {
                _ticks[_tail] = data;
            }

            _tail++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryTrimLists()
        {
            if (!_canBeTrim)
            {
                return;
            }

            _canBeTrim = false;

            RemoveNulls(_ticks);
            _ticks.TrimExcess();

            _ticks.Capacity = _ticks.Capacity < InitialTicksCapacity
                ? InitialTicksCapacity
                : _ticks.Capacity;

            _tail = _ticks.Count;

            _returnToPoolBuffer.Clear();
            _returnToPoolBuffer.TrimExcess();

            _returnToPoolBuffer.Capacity = _returnToPoolBuffer.Capacity < InitialBufferCapacity
                ? InitialBufferCapacity
                : _returnToPoolBuffer.Capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveNulls<T>(List<T> target)
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