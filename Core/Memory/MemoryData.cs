namespace WinCry.Memory
{
    class MemoryData
    {
        public ulong CachedRAMGreaterThan { get; set; }
        public ulong FreeRAMLessThan { get; set; }
        public int ServiceThreadSleepSeconds { get; set; }
    }
}
