using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class HeapResizableDirectMemoryTests
    {
        [Fact]
        public void CorrectMemoryReallocationTest()
        {
            using (var heapMemory = new HeapResizableDirectMemory())
            {
                void* pointer1 = (void*)heapMemory.Pointer;
                var value1 = System.Runtime.CompilerServices.Unsafe.Read<byte>((byte*)pointer1 + heapMemory.ReservedCapacity - 1);

                heapMemory.ReservedCapacity *= 2;

                void* pointer2 = (void*)heapMemory.Pointer;
                var value2 = System.Runtime.CompilerServices.Unsafe.Read<byte>((byte*)pointer2 + heapMemory.ReservedCapacity - 1);

                Assert.Equal(value1, value2);
                Assert.Equal(0, value1);
            }
        }
    }
}
