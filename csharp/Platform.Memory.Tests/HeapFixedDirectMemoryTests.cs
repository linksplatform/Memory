using System;
using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class HeapFixedDirectMemoryTests
    {
        [Fact]
        public void ConstructorWithCapacityTest()
        {
            const long capacity = 8192;
            using var heapMemory = new HeapFixedDirectMemory(capacity);
            
            Assert.Equal(capacity, heapMemory.Capacity);
            Assert.Equal(capacity, heapMemory.Size);
            Assert.NotEqual(IntPtr.Zero, heapMemory.Pointer);
        }
        
        [Fact]
        public void DefaultConstructorTest()
        {
            using var heapMemory = new HeapFixedDirectMemory();
            
            Assert.Equal(FixedDirectMemoryBase.MinimumCapacity, heapMemory.Capacity);
            Assert.Equal(FixedDirectMemoryBase.MinimumCapacity, heapMemory.Size);
            Assert.NotEqual(IntPtr.Zero, heapMemory.Pointer);
        }
        
        [Fact]
        public void MinimumCapacityEnforcementTest()
        {
            const long smallCapacity = 1;
            using var heapMemory = new HeapFixedDirectMemory(smallCapacity);
            
            Assert.Equal(FixedDirectMemoryBase.MinimumCapacity, heapMemory.Capacity);
            Assert.Equal(FixedDirectMemoryBase.MinimumCapacity, heapMemory.Size);
        }
        
        [Fact]
        public void MemoryIsZeroInitializedTest()
        {
            const long capacity = 8192;
            using var heapMemory = new HeapFixedDirectMemory(capacity);
            
            var pointer = (byte*)heapMemory.Pointer;
            for (long i = 0; i < capacity; i++)
            {
                Assert.Equal(0, pointer[i]);
            }
        }
        
        [Fact]
        public void WriteAndReadTest()
        {
            const long capacity = 8192;
            using var heapMemory = new HeapFixedDirectMemory(capacity);
            
            var pointer = (byte*)heapMemory.Pointer;
            const byte testValue = 42;
            
            // Write test value at different positions
            pointer[0] = testValue;
            pointer[capacity / 2] = testValue;
            pointer[capacity - 1] = testValue;
            
            // Verify values
            Assert.Equal(testValue, pointer[0]);
            Assert.Equal(testValue, pointer[capacity / 2]);
            Assert.Equal(testValue, pointer[capacity - 1]);
        }
        
        [Fact]
        public void DisposeTest()
        {
            var heapMemory = new HeapFixedDirectMemory(8192);
            var pointer = heapMemory.Pointer;
            
            Assert.NotEqual(IntPtr.Zero, pointer);
            
            heapMemory.Dispose();
            
            Assert.Throws<ObjectDisposedException>(() => heapMemory.Pointer);
            Assert.Throws<ObjectDisposedException>(() => heapMemory.Capacity);
            Assert.Throws<ObjectDisposedException>(() => heapMemory.Size);
        }
    }
}