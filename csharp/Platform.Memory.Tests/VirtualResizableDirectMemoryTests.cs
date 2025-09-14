using System;
using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class VirtualResizableDirectMemoryTests
    {
        [Fact]
        public void CorrectMemoryAllocationTest()
        {
            using var virtualMemory = new VirtualResizableDirectMemory();
            Assert.True(virtualMemory.Pointer != IntPtr.Zero);
            Assert.True(virtualMemory.ReservedCapacity >= VirtualResizableDirectMemory.MinimumCapacity);
            Assert.Equal(0, virtualMemory.UsedCapacity);
        }

        [Fact]
        public void CorrectMemoryReallocationTest()
        {
            using var virtualMemory = new VirtualResizableDirectMemory();
            var value1 = GetLastByte(virtualMemory);
            virtualMemory.ReservedCapacity *= 2;
            var value2 = GetLastByte(virtualMemory);
            Assert.Equal(value1, value2);
            Assert.Equal(0, value1);
        }

        [Fact]
        public void MemoryZeroingTest()
        {
            using var virtualMemory = new VirtualResizableDirectMemory(8192);
            var pointer = (byte*)virtualMemory.Pointer;
            
            // Check that initial memory is zeroed
            for (int i = 0; i < 8192; i++)
            {
                Assert.Equal(0, pointer[i]);
            }
        }

        [Fact]
        public void MemoryResizePreservesDataTest()
        {
            using var virtualMemory = new VirtualResizableDirectMemory(4096);
            var pointer = (byte*)virtualMemory.Pointer;
            
            // Write test pattern
            for (int i = 0; i < 1024; i++)
            {
                pointer[i] = (byte)(i % 256);
            }
            
            // Resize to larger capacity
            virtualMemory.ReservedCapacity = 8192;
            pointer = (byte*)virtualMemory.Pointer;
            
            // Verify data is preserved
            for (int i = 0; i < 1024; i++)
            {
                Assert.Equal((byte)(i % 256), pointer[i]);
            }
            
            // Verify new memory is zeroed
            for (int i = 4096; i < 8192; i++)
            {
                Assert.Equal(0, pointer[i]);
            }
        }

        [Fact]
        public void UsedCapacitySetTest()
        {
            using var virtualMemory = new VirtualResizableDirectMemory(4096);
            Assert.Equal(0, virtualMemory.UsedCapacity);
            
            virtualMemory.UsedCapacity = 2048;
            Assert.Equal(2048, virtualMemory.UsedCapacity);
            
            virtualMemory.UsedCapacity = 4096;
            Assert.Equal(4096, virtualMemory.UsedCapacity);
        }

        [Fact]
        public void InitialCapacityTest()
        {
            using var virtualMemory1 = new VirtualResizableDirectMemory();
            Assert.Equal(VirtualResizableDirectMemory.MinimumCapacity, virtualMemory1.ReservedCapacity);
            
            using var virtualMemory2 = new VirtualResizableDirectMemory(8192);
            Assert.Equal(8192, virtualMemory2.ReservedCapacity);
            
            // Test minimum capacity enforcement
            using var virtualMemory3 = new VirtualResizableDirectMemory(100);
            Assert.Equal(VirtualResizableDirectMemory.MinimumCapacity, virtualMemory3.ReservedCapacity);
        }

        private static byte GetLastByte(VirtualResizableDirectMemory virtualMemory)
        {
            var pointer = (void*)virtualMemory.Pointer;
            return *((byte*)pointer + virtualMemory.ReservedCapacity - 1);
        }
    }
}