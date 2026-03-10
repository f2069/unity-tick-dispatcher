using System;
using System.Collections.Generic;

namespace UnityTickDispatcher
{
    internal sealed class TickPool
    {
        private const int InitialPoolCapacity = 500;

        private readonly Stack<TickData> _pool = new Stack<TickData>(InitialPoolCapacity);
        private readonly object _poolLock = new object();
        private long _versionSeed;

        public TickData Retrieve(Action action)
        {
            TickData data;
            lock (_poolLock)
            {
                data = _pool.Count > 0 ? _pool.Pop() : new TickData();
                _versionSeed++;
                if (_versionSeed == long.MaxValue)
                {
                    _versionSeed = 1;
                }

                data.Init(action, _versionSeed);
            }

            return data;
        }

        public void Store(List<TickData> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            lock (_poolLock)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var data = list[i];
                    if (data == null)
                    {
                        continue;
                    }

                    data.ResetData();
                    _pool.Push(data);
                }
            }
        }

        public void Store(TickData data)
        {
            if (data == null)
            {
                return;
            }

            data.ResetData();

            lock (_poolLock)
            {
                _pool.Push(data);
            }
        }
    }
}