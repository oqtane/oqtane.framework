using Microsoft.IO;

namespace Oqtane.Helpers
{
    public static class StreamHelpers
    {
        public static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new(new RecyclableMemoryStreamManager.Options()
        {
            BlockSize = RecyclableMemoryStreamManager.DefaultBlockSize,
            LargeBufferMultiple = RecyclableMemoryStreamManager.DefaultLargeBufferMultiple,
            MaximumBufferSize = RecyclableMemoryStreamManager.DefaultMaximumBufferSize,
            GenerateCallStacks = false,
            AggressiveBufferReturn = false,
            MaximumLargePoolFreeBytes = 16 * 1024 * 1024 * 4,
            MaximumSmallPoolFreeBytes = 100 * 1024
        });
    }
}
