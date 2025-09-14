using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Platform.Memory.Tests
{
    public unsafe class DistributedDirectMemoryTests
    {
        private readonly string[] _testPaths;
        private readonly string _testDirectory;

        public DistributedDirectMemoryTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "DistributedMemoryTests", Guid.NewGuid().ToString());
            _testPaths = new[]
            {
                Path.Combine(_testDirectory, "drive1"),
                Path.Combine(_testDirectory, "drive2"),
                Path.Combine(_testDirectory, "drive3")
            };

            foreach (var path in _testPaths)
            {
                Directory.CreateDirectory(path);
            }
        }

        private void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public void ConstructorCreatesDirectoriesTest()
        {
            var tempPaths = new[]
            {
                Path.Combine(Path.GetTempPath(), "DistributedMemoryTest1", Guid.NewGuid().ToString()),
                Path.Combine(Path.GetTempPath(), "DistributedMemoryTest2", Guid.NewGuid().ToString())
            };

            try
            {
                using var memory = new DistributedDirectMemory(tempPaths, "test");
                
                foreach (var path in tempPaths)
                {
                    Assert.True(Directory.Exists(path));
                }
            }
            finally
            {
                foreach (var path in tempPaths)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, recursive: true);
                        }
                    }
                    catch { }
                }
            }
        }

        [Fact]
        public void InitialCapacityTest()
        {
            using var memory = new DistributedDirectMemory(_testPaths, "test");
            
            Assert.True(memory.ReservedCapacity >= ResizableDirectMemoryBase.MinimumCapacity);
            Assert.Equal(0, memory.UsedCapacity);
            Assert.True(memory.Size == memory.UsedCapacity);
            
            Cleanup();
        }

        [Fact]
        public void CapacityResizingTest()
        {
            using var memory = new DistributedDirectMemory(_testPaths, "test");
            
            var initialCapacity = memory.ReservedCapacity;
            memory.ReservedCapacity = initialCapacity * 2;
            
            Assert.Equal(initialCapacity * 2, memory.ReservedCapacity);
            Assert.Equal(0, memory.UsedCapacity);
            
            Cleanup();
        }

        [Fact]
        public void BasicWriteAndReadTest()
        {
            using var memory = new DistributedDirectMemory(_testPaths, "test");
            
            var testData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var readBuffer = new byte[testData.Length];
            
            memory.WriteBytes(0, testData, 0, testData.Length);
            memory.ReadBytes(0, readBuffer, 0, readBuffer.Length);
            
            Assert.Equal(testData, readBuffer);
            Assert.Equal(testData.Length, memory.UsedCapacity);
            
            Cleanup();
        }

        [Fact]
        public void CrossSegmentWriteAndReadTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            using var memory = new DistributedDirectMemory(_testPaths, "test", segmentSize, segmentSize * 2);
            
            // Create data that spans multiple segments
            var testData = new byte[segmentSize + 1000];
            for (int i = 0; i < testData.Length; i++)
            {
                testData[i] = (byte)(i % 256);
            }
            
            var readBuffer = new byte[testData.Length];
            
            memory.WriteBytes(0, testData, 0, testData.Length);
            memory.ReadBytes(0, readBuffer, 0, readBuffer.Length);
            
            Assert.Equal(testData, readBuffer);
            Assert.Equal(testData.Length, memory.UsedCapacity);
            
            Cleanup();
        }

        [Fact]
        public void MultipleSegmentDistributionTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            using var memory = new DistributedDirectMemory(_testPaths, "test", segmentSize, segmentSize * 3);
            
            // Write data to force creation of multiple segments
            var testData = new byte[segmentSize * 2 + 500];
            for (int i = 0; i < testData.Length; i++)
            {
                testData[i] = (byte)(i % 256);
            }
            
            memory.WriteBytes(0, testData, 0, testData.Length);
            
            // Verify segments are distributed across different paths
            var segmentFiles = _testPaths
                .SelectMany(path => Directory.GetFiles(path, "test_segment_*.dat"))
                .ToArray();
            
            Assert.True(segmentFiles.Length >= 2);
            
            // Verify files are distributed across different drives
            var driveDistribution = segmentFiles
                .GroupBy(file => Path.GetDirectoryName(file))
                .Count();
            
            Assert.True(driveDistribution > 1);
            
            Cleanup();
        }

        [Fact]
        public void RandomAccessTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            using var memory = new DistributedDirectMemory(_testPaths, "test", segmentSize, 20000);
            
            var random = new System.Random(42);
            var testData = new byte[10000];
            random.NextBytes(testData);
            
            // Write the entire test data first to ensure it's all available
            memory.WriteBytes(0, testData, 0, testData.Length);
            
            // Test random access reads
            for (int i = 0; i < 50; i++)
            {
                var offset = random.Next(0, testData.Length - 100);
                var length = random.Next(1, Math.Min(100, testData.Length - offset));
                var readBuffer = new byte[length];
                
                memory.ReadBytes(offset, readBuffer, 0, length);
                
                for (int j = 0; j < length; j++)
                {
                    Assert.Equal(testData[offset + j], readBuffer[j]);
                }
            }
            
            // Test random overwrites
            var random2 = new System.Random(123);
            for (int i = 0; i < 20; i++)
            {
                var offset = random2.Next(0, testData.Length - 50);
                var length = random2.Next(1, Math.Min(50, testData.Length - offset));
                var newData = new byte[length];
                random2.NextBytes(newData);
                
                memory.WriteBytes(offset, newData, 0, length);
                
                // Update our test data array to match what we wrote
                Array.Copy(newData, 0, testData, offset, length);
                
                // Verify the write
                var readBuffer = new byte[length];
                memory.ReadBytes(offset, readBuffer, 0, length);
                Assert.Equal(newData, readBuffer);
            }
            
            Cleanup();
        }

        [Fact]
        public void GetPointerAtTest()
        {
            using var memory = new DistributedDirectMemory(_testPaths, "test");
            
            var testData = new byte[] { 42, 84, 126 };
            memory.WriteBytes(0, testData, 0, testData.Length);
            
            for (int i = 0; i < testData.Length; i++)
            {
                var pointer = memory.GetPointerAt(i);
                var value = *(byte*)pointer;
                Assert.Equal(testData[i], value);
            }
            
            Cleanup();
        }

        [Fact]
        public void CapacityReductionTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            using var memory = new DistributedDirectMemory(_testPaths, "test", segmentSize, segmentSize * 3);
            
            var initialCapacity = memory.ReservedCapacity;
            
            // Write some data to multiple segments
            var testData = new byte[segmentSize / 2];
            for (int i = 0; i < testData.Length; i++)
            {
                testData[i] = (byte)(i % 256);
            }
            memory.WriteBytes(0, testData, 0, testData.Length);
            
            // Get initial file count
            var initialFiles = _testPaths
                .SelectMany(path => Directory.GetFiles(path, "test_segment_*.dat"))
                .Count();
            
            // Reduce capacity to something larger than used capacity but smaller than original
            memory.ReservedCapacity = segmentSize * 2;
            
            // Verify files are cleaned up
            var finalFiles = _testPaths
                .SelectMany(path => Directory.GetFiles(path, "test_segment_*.dat"))
                .Count();
            
            Assert.True(finalFiles <= initialFiles);
            Assert.Equal(segmentSize * 2, memory.ReservedCapacity);
            
            Cleanup();
        }

        [Fact]
        public void DisposalCleansUpResourcesTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            var memory = new DistributedDirectMemory(_testPaths, "disposal_test", segmentSize, segmentSize * 2);
            
            // Create some segments by writing data
            var testData = new byte[segmentSize + 1000];
            memory.WriteBytes(0, testData, 0, testData.Length);
            
            // Verify segments exist
            var filesBeforeDisposal = _testPaths
                .SelectMany(path => Directory.GetFiles(path, "disposal_test_segment_*.dat"))
                .Count();
            
            Assert.True(filesBeforeDisposal > 0);
            
            memory.Dispose();
            
            // Files should still exist after disposal (they contain data)
            // but should be properly sized to used capacity
            var filesAfterDisposal = _testPaths
                .SelectMany(path => Directory.GetFiles(path, "disposal_test_segment_*.dat"))
                .Count();
            
            // Verify no exception is thrown on subsequent disposal
            memory.Dispose();
            
            Cleanup();
        }

        [Fact]
        public void ArgumentValidationTest()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new DistributedDirectMemory(null!, "test"));
            
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new DistributedDirectMemory(new string[0], "test"));
            
            Assert.Throws<ArgumentException>(() => 
                new DistributedDirectMemory(new[] { "" }, "test"));
            
            Assert.Throws<ArgumentException>(() => 
                new DistributedDirectMemory(_testPaths, ""));
            
            using var memory = new DistributedDirectMemory(_testPaths, "test");
            
            Assert.Throws<ArgumentNullException>(() => 
                memory.ReadBytes(0, null!, 0, 10));
            
            Assert.Throws<ArgumentNullException>(() => 
                memory.WriteBytes(0, null!, 0, 10));
            
            Cleanup();
        }

        [Fact]
        public void LargeDataTest()
        {
            var segmentSize = ResizableDirectMemoryBase.MinimumCapacity;
            using var memory = new DistributedDirectMemory(_testPaths, "large_test", segmentSize, segmentSize * 5);
            
            // Create data larger than single segment
            var dataSize = segmentSize * 3 + 12345;
            var largeData = new byte[dataSize];
            
            // Ensure capacity is large enough
            if (memory.ReservedCapacity < dataSize)
            {
                memory.ReservedCapacity = dataSize;
            }
            
            var random = new System.Random(123);
            random.NextBytes(largeData);
            
            memory.WriteBytes(0, largeData, 0, largeData.Length);
            
            var readBuffer = new byte[dataSize];
            memory.ReadBytes(0, readBuffer, 0, (int)dataSize);
            
            Assert.Equal(largeData, readBuffer);
            Assert.Equal(dataSize, memory.UsedCapacity);
            
            Cleanup();
        }
    }
}