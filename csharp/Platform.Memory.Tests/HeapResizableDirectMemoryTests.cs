using Xunit;

namespace Platform.Memory.Tests
{
    /// <summary>
    /// <para>
    /// Represents the heap resizable direct memory tests.
    /// </para>
    /// <para></para>
    /// </summary>
    public unsafe class HeapResizableDirectMemoryTests
    {
        /// <summary>
        /// <para>
        /// Tests that correct memory reallocation test.
        /// </para>
        /// <para></para>
        /// </summary>
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
