using System;
using System.IO;
using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class FileMappedFixedDirectMemoryTests : IDisposable
    {
        private readonly string _testFilePath;
        
        public FileMappedFixedDirectMemoryTests()
        {
            _testFilePath = Path.GetTempFileName();
        }
        
        public void Dispose()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        
        [Fact]
        public void ConstructorWithCapacityTest()
        {
            const long capacity = 8192;
            using var fileMemory = new FileMappedFixedDirectMemory(_testFilePath, capacity);
            
            Assert.Equal(capacity, fileMemory.Capacity);
            Assert.Equal(capacity, fileMemory.Size);
            Assert.NotEqual(IntPtr.Zero, fileMemory.Pointer);
            Assert.True(File.Exists(_testFilePath));
            Assert.Equal(capacity, new FileInfo(_testFilePath).Length);
        }
        
        [Fact]
        public void DefaultCapacityConstructorTest()
        {
            using var fileMemory = new FileMappedFixedDirectMemory(_testFilePath);
            
            Assert.Equal(FileMappedFixedDirectMemory.MinimumCapacity, fileMemory.Capacity);
            Assert.Equal(FileMappedFixedDirectMemory.MinimumCapacity, fileMemory.Size);
            Assert.NotEqual(IntPtr.Zero, fileMemory.Pointer);
            Assert.True(File.Exists(_testFilePath));
            Assert.Equal(FileMappedFixedDirectMemory.MinimumCapacity, new FileInfo(_testFilePath).Length);
        }
        
        [Fact]
        public void MinimumCapacityEnforcementTest()
        {
            const long smallCapacity = 1;
            using var fileMemory = new FileMappedFixedDirectMemory(_testFilePath, smallCapacity);
            
            Assert.Equal(FileMappedFixedDirectMemory.MinimumCapacity, fileMemory.Capacity);
            Assert.Equal(FileMappedFixedDirectMemory.MinimumCapacity, fileMemory.Size);
        }
        
        [Fact]
        public void WriteAndReadTest()
        {
            const long capacity = 8192;
            using var fileMemory = new FileMappedFixedDirectMemory(_testFilePath, capacity);
            
            var pointer = (byte*)fileMemory.Pointer;
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
        public void PersistenceTest()
        {
            const long capacity = 8192;
            const byte testValue = 123;
            
            // Write data in first instance
            using (var fileMemory1 = new FileMappedFixedDirectMemory(_testFilePath, capacity))
            {
                var pointer1 = (byte*)fileMemory1.Pointer;
                pointer1[100] = testValue;
                pointer1[capacity - 100] = testValue;
            }
            
            // Read data in second instance
            using (var fileMemory2 = new FileMappedFixedDirectMemory(_testFilePath, capacity))
            {
                var pointer2 = (byte*)fileMemory2.Pointer;
                Assert.Equal(testValue, pointer2[100]);
                Assert.Equal(testValue, pointer2[capacity - 100]);
            }
        }
        
        [Fact]
        public void EmptyPathArgumentTest()
        {
            Assert.Throws<ArgumentException>(() => new FileMappedFixedDirectMemory(""));
            Assert.Throws<ArgumentException>(() => new FileMappedFixedDirectMemory("   "));
        }
        
        [Fact]
        public void DisposeTest()
        {
            var fileMemory = new FileMappedFixedDirectMemory(_testFilePath, 8192);
            var pointer = fileMemory.Pointer;
            
            Assert.NotEqual(IntPtr.Zero, pointer);
            
            fileMemory.Dispose();
            
            Assert.Throws<ObjectDisposedException>(() => fileMemory.Pointer);
            Assert.Throws<ObjectDisposedException>(() => fileMemory.Capacity);
            Assert.Throws<ObjectDisposedException>(() => fileMemory.Size);
        }
    }
}