using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Platform.Exceptions;
using Platform.Disposables;
using Platform.Collections;
using Platform.Ranges;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a memory block distributed across multiple storage locations (drives or machines).</para>
    /// <para>Представляет блок памяти, распределённый по нескольким местам хранения (дискам или машинам).</para>
    /// </summary>
    public unsafe class DistributedDirectMemory : ResizableDirectMemoryBase
    {
        #region Fields
        private readonly List<FileMappedResizableDirectMemory> _segments;
        private readonly List<string> _segmentPaths;
        private readonly string[] _basePaths;
        private readonly long _segmentSize;
        private readonly string _namePrefix;
        
        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Distributed memory block across {_basePaths.Length} locations with {_segments.Count} segments.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DistributedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="DistributedDirectMemory"/>.</para>
        /// </summary>
        /// <param name="basePaths"><para>Array of base directory paths where memory segments will be stored.</para><para>Массив базовых путей к директориям, где будут храниться сегменты памяти.</para></param>
        /// <param name="namePrefix"><para>Prefix for segment file names.</para><para>Префикс для имён файлов сегментов.</para></param>
        /// <param name="segmentSize"><para>Size of each memory segment in bytes.</para><para>Размер каждого сегмента памяти в байтах.</para></param>
        /// <param name="minimumReservedCapacity"><para>Minimum reserved capacity in bytes.</para><para>Минимальный зарезервированный размер в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DistributedDirectMemory(string[] basePaths, string namePrefix, long segmentSize, long minimumReservedCapacity)
        {
            Ensure.Always.ArgumentNotNull(basePaths, nameof(basePaths));
            Ensure.Always.ArgumentNotEmptyAndNotWhiteSpace(namePrefix, nameof(namePrefix));
            Ensure.Always.ArgumentInRange(basePaths.Length, new Range<int>(1, int.MaxValue), nameof(basePaths));
            
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            if (segmentSize < MinimumCapacity)
            {
                segmentSize = MinimumCapacity;
            }
            
            // Ensure all base paths exist
            foreach (var basePath in basePaths)
            {
                Ensure.Always.ArgumentNotEmptyAndNotWhiteSpace(basePath, nameof(basePaths));
                Directory.CreateDirectory(basePath);
            }
            
            _basePaths = (string[])basePaths.Clone();
            _namePrefix = namePrefix;
            _segmentSize = segmentSize;
            _segments = new List<FileMappedResizableDirectMemory>();
            _segmentPaths = new List<string>();
            
            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DistributedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="DistributedDirectMemory"/>.</para>
        /// </summary>
        /// <param name="basePaths"><para>Array of base directory paths where memory segments will be stored.</para><para>Массив базовых путей к директориям, где будут храниться сегменты памяти.</para></param>
        /// <param name="namePrefix"><para>Prefix for segment file names.</para><para>Префикс для имён файлов сегментов.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DistributedDirectMemory(string[] basePaths, string namePrefix) 
            : this(basePaths, namePrefix, MinimumCapacity * 16, MinimumCapacity) { }

        #endregion

        #region Methods

        /// <summary>
        /// <para>Gets the segment index and offset within segment for the given absolute offset.</para>
        /// <para>Получает индекс сегмента и смещение внутри сегмента для заданного абсолютного смещения.</para>
        /// </summary>
        /// <param name="absoluteOffset"><para>Absolute offset in bytes.</para><para>Абсолютное смещение в байтах.</para></param>
        /// <param name="segmentIndex"><para>Index of the segment.</para><para>Индекс сегмента.</para></param>
        /// <param name="segmentOffset"><para>Offset within the segment.</para><para>Смещение внутри сегмента.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetSegmentLocation(long absoluteOffset, out int segmentIndex, out long segmentOffset)
        {
            segmentIndex = (int)(absoluteOffset / _segmentSize);
            segmentOffset = absoluteOffset % _segmentSize;
        }

        /// <summary>
        /// <para>Ensures that the specified segment exists.</para>
        /// <para>Обеспечивает существование указанного сегмента.</para>
        /// </summary>
        /// <param name="segmentIndex"><para>Index of the segment to ensure.</para><para>Индекс сегмента для обеспечения.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSegment(int segmentIndex)
        {
            while (_segments.Count <= segmentIndex)
            {
                var pathIndex = _segments.Count % _basePaths.Length;
                var segmentFileName = $"{_namePrefix}_segment_{_segments.Count:D8}.dat";
                var segmentPath = Path.Combine(_basePaths[pathIndex], segmentFileName);
                
                var segment = new FileMappedResizableDirectMemory(segmentPath, _segmentSize);
                _segments.Add(segment);
                _segmentPaths.Add(segmentPath);
            }
        }

        /// <summary>
        /// <para>Gets a pointer to the specified absolute offset in the distributed memory.</para>
        /// <para>Получает указатель на указанное абсолютное смещение в распределённой памяти.</para>
        /// </summary>
        /// <param name="absoluteOffset"><para>Absolute offset in bytes.</para><para>Абсолютное смещение в байтах.</para></param>
        /// <returns><para>Pointer to the memory location.</para><para>Указатель на место в памяти.</para></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetPointerAt(long absoluteOffset)
        {
            Ensure.Always.NotDisposed(this);
            Ensure.Always.ArgumentInRange(absoluteOffset, new Range<long>(0, ReservedCapacity - 1));
            
            GetSegmentLocation(absoluteOffset, out int segmentIndex, out long segmentOffset);
            EnsureSegment(segmentIndex);
            
            return new IntPtr(_segments[segmentIndex].Pointer.ToInt64() + segmentOffset);
        }

        /// <summary>
        /// <para>Reads data from the distributed memory at the specified offset.</para>
        /// <para>Читает данные из распределённой памяти по указанному смещению.</para>
        /// </summary>
        /// <param name="absoluteOffset"><para>Absolute offset to read from.</para><para>Абсолютное смещение для чтения.</para></param>
        /// <param name="buffer"><para>Buffer to read data into.</para><para>Буфер для чтения данных.</para></param>
        /// <param name="bufferOffset"><para>Offset in the buffer to start writing.</para><para>Смещение в буфере для начала записи.</para></param>
        /// <param name="count"><para>Number of bytes to read.</para><para>Количество байт для чтения.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(long absoluteOffset, byte[] buffer, int bufferOffset, int count)
        {
            Ensure.Always.NotDisposed(this);
            Ensure.Always.ArgumentNotNull(buffer, nameof(buffer));
            Ensure.Always.ArgumentInRange(absoluteOffset, new Range<long>(0, UsedCapacity - count));
            Ensure.Always.ArgumentInRange(bufferOffset, new Range<int>(0, buffer.Length - count));
            
            var remainingBytes = count;
            var currentOffset = absoluteOffset;
            var currentBufferOffset = bufferOffset;
            
            while (remainingBytes > 0)
            {
                GetSegmentLocation(currentOffset, out int segmentIndex, out long segmentOffset);
                EnsureSegment(segmentIndex);
                
                var bytesToReadFromSegment = Math.Min(remainingBytes, (int)(_segmentSize - segmentOffset));
                var sourcePtr = (byte*)(_segments[segmentIndex].Pointer.ToInt64() + segmentOffset);
                
                fixed (byte* destPtr = &buffer[currentBufferOffset])
                {
                    for (int i = 0; i < bytesToReadFromSegment; i++)
                    {
                        destPtr[i] = sourcePtr[i];
                    }
                }
                
                remainingBytes -= bytesToReadFromSegment;
                currentOffset += bytesToReadFromSegment;
                currentBufferOffset += bytesToReadFromSegment;
            }
        }

        /// <summary>
        /// <para>Writes data to the distributed memory at the specified offset.</para>
        /// <para>Записывает данные в распределённую память по указанному смещению.</para>
        /// </summary>
        /// <param name="absoluteOffset"><para>Absolute offset to write to.</para><para>Абсолютное смещение для записи.</para></param>
        /// <param name="buffer"><para>Buffer containing data to write.</para><para>Буфер, содержащий данные для записи.</para></param>
        /// <param name="bufferOffset"><para>Offset in the buffer to start reading.</para><para>Смещение в буфере для начала чтения.</para></param>
        /// <param name="count"><para>Number of bytes to write.</para><para>Количество байт для записи.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(long absoluteOffset, byte[] buffer, int bufferOffset, int count)
        {
            Ensure.Always.NotDisposed(this);
            Ensure.Always.ArgumentNotNull(buffer, nameof(buffer));
            Ensure.Always.ArgumentInRange(absoluteOffset, new Range<long>(0, ReservedCapacity - count));
            Ensure.Always.ArgumentInRange(bufferOffset, new Range<int>(0, buffer.Length - count));
            
            var remainingBytes = count;
            var currentOffset = absoluteOffset;
            var currentBufferOffset = bufferOffset;
            
            while (remainingBytes > 0)
            {
                GetSegmentLocation(currentOffset, out int segmentIndex, out long segmentOffset);
                EnsureSegment(segmentIndex);
                
                var bytesToWriteToSegment = Math.Min(remainingBytes, (int)(_segmentSize - segmentOffset));
                var destPtr = (byte*)(_segments[segmentIndex].Pointer.ToInt64() + segmentOffset);
                
                fixed (byte* sourcePtr = &buffer[currentBufferOffset])
                {
                    for (int i = 0; i < bytesToWriteToSegment; i++)
                    {
                        destPtr[i] = sourcePtr[i];
                    }
                }
                
                remainingBytes -= bytesToWriteToSegment;
                currentOffset += bytesToWriteToSegment;
                currentBufferOffset += bytesToWriteToSegment;
            }
            
            // Update used capacity if we wrote beyond current used capacity
            var newUsedCapacity = Math.Max(UsedCapacity, absoluteOffset + count);
            if (newUsedCapacity > UsedCapacity)
            {
                UsedCapacity = newUsedCapacity;
            }
        }

        #endregion

        #region ResizableDirectMemoryBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            var requiredSegments = (int)((newReservedCapacity + _segmentSize - 1) / _segmentSize);
            
            // Ensure we have enough segments
            EnsureSegment(requiredSegments - 1);
            
            // Resize the last segment if necessary
            if (requiredSegments > 0)
            {
                var lastSegmentIndex = requiredSegments - 1;
                var lastSegmentSize = newReservedCapacity - (lastSegmentIndex * _segmentSize);
                if (lastSegmentSize != _segmentSize && lastSegmentIndex < _segments.Count)
                {
                    _segments[lastSegmentIndex].ReservedCapacity = Math.Max(lastSegmentSize, MinimumCapacity);
                }
            }
            
            // Remove excess segments if shrinking
            while (_segments.Count > requiredSegments)
            {
                var lastIndex = _segments.Count - 1;
                _segments[lastIndex].Dispose();
                _segments.RemoveAt(lastIndex);
                
                // Clean up segment file
                var segmentPath = _segmentPaths[lastIndex];
                _segmentPaths.RemoveAt(lastIndex);
                try
                {
                    if (File.Exists(segmentPath))
                    {
                        File.Delete(segmentPath);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            
            // Update pointer to first segment if available
            if (_segments.Count > 0)
            {
                Pointer = _segments[0].Pointer;
            }
            else
            {
                Pointer = IntPtr.Zero;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DisposePointer(IntPtr pointer, long usedCapacity)
        {
            // Dispose all segments
            foreach (var segment in _segments)
            {
                try
                {
                    segment?.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            _segments.Clear();
            
            // Clean up segment files if they're empty or not needed
            for (int i = 0; i < _segmentPaths.Count; i++)
            {
                try
                {
                    var segmentPath = _segmentPaths[i];
                    var segmentUsedCapacity = Math.Max(0, Math.Min(_segmentSize, usedCapacity - (i * _segmentSize)));
                    
                    if (segmentUsedCapacity == 0 && File.Exists(segmentPath))
                    {
                        File.Delete(segmentPath);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            _segmentPaths.Clear();
        }

        #endregion
    }
}