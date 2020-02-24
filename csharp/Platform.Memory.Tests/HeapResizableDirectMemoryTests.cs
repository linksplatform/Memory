using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class HeapResizableDirectMemoryTests
    {
        [Fact]
        public void CorrectMemoryReallocationTest()
        {
            using var heapMemory = new HeapResizableDirectMemory();
            var value1 = GetLastByte(heapMemory);
            heapMemory.ReservedCapacity *= 2;
            var value2 = GetLastByte(heapMemory);
            Assert.Equal(value1, value2);
            Assert.Equal(0, value1);
        }

        private static byte GetLastByte(HeapResizableDirectMemory heapMemory)
        {
            var pointer1 = (void*)heapMemory.Pointer;
            return *((byte*)pointer1 + heapMemory.ReservedCapacity - 1);
        }
    }
}
