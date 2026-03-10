namespace UnityTickDispatcher
{
    public readonly struct TickHandle
    {
        private readonly TickData _data;
        private readonly long _version;

        public TickHandle(TickData data)
        {
            _data = data;
            _version = data.Version;
        }

        public void Dispose()
        {
            _data?.Dispose(_version);
        }
    }
}