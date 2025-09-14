using System;
using Xunit;
using Platform.Unsafe;

namespace Platform.Memory.Tests
{
    public unsafe class ArrayMemoryAsDirectMemoryAdapterTests
    {
        [Fact]
        public void BasicFunctionalityTest()
        {
            var arrayMemory = new ArrayMemory<int>(10);
            arrayMemory[0] = 42;
            arrayMemory[9] = 84;
            
            using var adapter = new ArrayMemoryAsDirectMemoryAdapter<int>(arrayMemory);
            
            Assert.Equal(10 * sizeof(int), adapter.Size);
            Assert.NotEqual(IntPtr.Zero, adapter.Pointer);
            
            var pointer = (int*)adapter.Pointer;
            Assert.Equal(42, pointer[0]);
            Assert.Equal(84, pointer[9]);
        }

        [Fact]
        public void PointerStabilityTest()
        {
            var arrayMemory = new ArrayMemory<long>(100);
            using var adapter = new ArrayMemoryAsDirectMemoryAdapter<long>(arrayMemory);
            
            var pointer1 = adapter.Pointer;
            var pointer2 = adapter.Pointer;
            
            Assert.Equal(pointer1, pointer2);
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var pointer3 = adapter.Pointer;
            Assert.Equal(pointer1, pointer3);
        }

        [Fact]
        public void WriteAccessTest()
        {
            var arrayMemory = new ArrayMemory<byte>(5);
            using var adapter = new ArrayMemoryAsDirectMemoryAdapter<byte>(arrayMemory);
            
            var pointer = (byte*)adapter.Pointer;
            pointer[0] = 255;
            pointer[4] = 128;
            
            Assert.Equal(255, arrayMemory[0]);
            Assert.Equal(128, arrayMemory[4]);
        }

        [Fact]
        public void SizeCalculationTest()
        {
            var arrayMemory = new ArrayMemory<double>(25);
            using var adapter = new ArrayMemoryAsDirectMemoryAdapter<double>(arrayMemory);
            
            Assert.Equal(25 * sizeof(double), adapter.Size);
        }

        [Fact]
        public void DisposalTest()
        {
            var arrayMemory = new ArrayMemory<int>(10);
            var adapter = new ArrayMemoryAsDirectMemoryAdapter<int>(arrayMemory);
            
            var pointer = adapter.Pointer;
            Assert.NotEqual(IntPtr.Zero, pointer);
            
            adapter.Dispose();
            
            // After disposal, the pointer should still be the same value,
            // but the GCHandle should be freed (we can't easily test this without internals access)
            Assert.NotEqual(IntPtr.Zero, adapter.Pointer);
        }
    }
}