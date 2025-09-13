using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform.Disposables;
using Platform.Exceptions;
using Platform.Collections;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a fixed-size memory block stored as a file on disk.</para>
    /// <para>Представляет блок памяти фиксированного размера, хранящийся в виде файла на диске.</para>
    /// </summary>
    public unsafe class FileMappedFixedDirectMemory : DisposableBase, IFixedDirectMemory
    {
        #region Constants

        /// <summary>
        /// <para>Gets minimum capacity in bytes.</para>
        /// <para>Возвращает минимальную емкость в байтах.</para>
        /// </summary>
        public static readonly long MinimumCapacity = Environment.SystemPageSize;

        #endregion

        #region Fields
        private MemoryMappedFile _file;
        private MemoryMappedViewAccessor _accessor;
        private IntPtr _pointer;
        private readonly long _capacity;

        /// <summary>
        /// <para>Gets path to memory mapped file.</para>
        /// <para>Получает путь к отображенному в памяти файлу.</para>
        /// </summary>
        protected readonly string Path;

        #endregion

        #region Properties

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IMemory.Size"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Ensure.Always.NotDisposed(this);
                return _capacity;
            }
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IFixedDirectMemory.Capacity"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        public long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Ensure.Always.NotDisposed(this);
                return _capacity;
            }
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IDirectMemory.Pointer"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        public IntPtr Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Ensure.Always.NotDisposed(this);
                return _pointer;
            }
        }

        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override bool AllowMultipleDisposeCalls
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Fixed file stored memory block at '{Path}' path.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="FileMappedFixedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="FileMappedFixedDirectMemory"/>.</para>
        /// </summary>
        /// <param name="path"><para>An path to file.</para><para>Путь к файлу.</para></param>
        /// <param name="capacity"><para>Fixed capacity in bytes.</para><para>Фиксированный размер в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FileMappedFixedDirectMemory(string path, long capacity)
        {
            Ensure.Always.ArgumentNotEmptyAndNotWhiteSpace(path, nameof(path));
            Path = path;
            
            _capacity = capacity < MinimumCapacity ? MinimumCapacity : capacity;
            AllocateMemory(_capacity);
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="FileMappedFixedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="FileMappedFixedDirectMemory"/>.</para>
        /// </summary>
        /// <param name="path"><para>An path to file.</para><para>Путь к файлу.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FileMappedFixedDirectMemory(string path) : this(path, MinimumCapacity) { }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AllocateMemory(long capacity)
        {
            SetFileSize(Path, capacity);
            _file = MemoryMappedFile.CreateFromFile(Path, FileMode.Open, mapName: null, capacity, MemoryMappedFileAccess.ReadWrite);
            _accessor = _file.CreateViewAccessor();
            byte* pointer = null;
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
            _pointer = new IntPtr(pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetFileSize(string path, long size)
        {
            using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            fileStream.SetLength(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool UnmapFile(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return false;
            }
            if (_accessor != null)
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                Disposable.TryDisposeAndResetToDefault(ref _accessor);
            }
            Disposable.TryDisposeAndResetToDefault(ref _file);
            return true;
        }

        #endregion

        #region DisposableBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero);
                if (pointer != IntPtr.Zero)
                {
                    UnmapFile(pointer);
                }
            }
        }

        #endregion
    }
}