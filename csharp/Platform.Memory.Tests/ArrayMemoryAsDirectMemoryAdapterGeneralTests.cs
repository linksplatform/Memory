using System;
using Xunit;
using Platform.Unsafe;

namespace Platform.Memory.Tests
{
    public unsafe class ArrayMemoryAsDirectMemoryAdapterGeneralTests
    {
        [Fact]
        public void BasicFunctionalityWithArrayMemoryTest()
        {
            var arrayMemory = new ArrayMemory<int>(10);
            arrayMemory[0] = 42;
            arrayMemory[9] = 84;
            
            using var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<int>(arrayMemory);
            
            Assert.Equal(10 * sizeof(int), adapter.Size);
            Assert.NotEqual(IntPtr.Zero, adapter.Pointer);
            
            var pointer = (int*)adapter.Pointer;
            Assert.Equal(42, pointer[0]);
            Assert.Equal(84, pointer[9]);
        }

        [Fact]
        public void ReadOnlyModeTest()
        {
            var arrayMemory = new ArrayMemory<int>(5);
            arrayMemory[0] = 100;
            
            var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<int>(arrayMemory, isReadOnly: true);
            
            var pointer = (int*)adapter.Pointer;
            Assert.Equal(100, pointer[0]);
            
            // Modify through pointer
            pointer[0] = 200;
            
            // In read-only mode, changes should not sync back to original
            adapter.Dispose();
            
            Assert.Equal(100, arrayMemory[0]); // Original should be unchanged
        }

        [Fact]
        public void ReadWriteModeTest()
        {
            var arrayMemory = new ArrayMemory<int>(5);
            arrayMemory[0] = 100;
            
            using var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<int>(arrayMemory, isReadOnly: false);
            
            var pointer = (int*)adapter.Pointer;
            Assert.Equal(100, pointer[0]);
            
            // Modify through pointer
            pointer[0] = 200;
            
            // Manually sync changes back
            adapter.SyncToArrayMemory();
            
            Assert.Equal(200, arrayMemory[0]);
        }

        [Fact]
        public void SyncOnDisposeTest()
        {
            var arrayMemory = new ArrayMemory<byte>(3);
            arrayMemory[1] = 50;
            
            var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<byte>(arrayMemory, isReadOnly: false);
            
            var pointer = (byte*)adapter.Pointer;
            pointer[1] = 75;
            
            // Changes should sync back on dispose
            adapter.Dispose();
            
            Assert.Equal(75, arrayMemory[1]);
        }

        [Fact]
        public void WorksWithFileArrayMemoryTest()
        {
            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                using var fileArrayMemory = new FileArrayMemory<int>(tempFile);
                
                // Initialize some data
                fileArrayMemory[0] = 123;
                
                using var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<int>(fileArrayMemory, isReadOnly: true);
                
                Assert.NotEqual(IntPtr.Zero, adapter.Pointer);
                
                var pointer = (int*)adapter.Pointer;
                Assert.Equal(123, pointer[0]);
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void PointerStabilityTest()
        {
            var arrayMemory = new ArrayMemory<long>(50);
            using var adapter = new ArrayMemoryAsDirectMemoryAdapterGeneral<long>(arrayMemory);
            
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
    }
}