using System;
using System.Threading;
using System.Runtime.CompilerServices;
using Platform.Exceptions;
using Platform.Disposables;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Provides a base implementation for the fixed-size memory block with direct access (via unmanaged pointers).</para>
    /// <para>Предоставляет базовую реализацию для блока памяти фиксированного размера с прямым доступом (через неуправляемые указатели).</para>
    /// </summary>
    public abstract class FixedDirectMemoryBase : DisposableBase, IFixedDirectMemory
    {
        #region Constants

        /// <summary>
        /// <para>Gets minimum capacity in bytes.</para>
        /// <para>Возвращает минимальную емкость в байтах.</para>
        /// </summary>
        public static readonly long MinimumCapacity = Environment.SystemPageSize;

        #endregion

        #region Fields
        private IntPtr _pointer;
        private readonly long _capacity;

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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set
            {
                Ensure.Always.NotDisposed(this);
                _pointer = value;
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

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="FixedDirectMemoryBase"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="FixedDirectMemoryBase"/>.</para>
        /// </summary>
        /// <param name="capacity"><para>The capacity in bytes.</para><para>Размер в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected FixedDirectMemoryBase(long capacity)
        {
            if (capacity < MinimumCapacity)
            {
                capacity = MinimumCapacity;
            }
            _capacity = capacity;
            AllocateMemory(capacity);
        }

        #endregion

        #region Methods

        /// <summary>
        /// <para>Allocates the memory with the specified capacity.</para>
        /// <para>Выделяет память с указанной емкостью.</para>
        /// </summary>
        /// <param name="capacity"><para>The capacity of the memory block in bytes.</para><para>Емкость блока памяти в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void AllocateMemory(long capacity);

        /// <summary>
        /// <para>Executed when it is time to dispose <see cref="Pointer"/>.</para>
        /// <para>Выполняется, когда пришло время высвободить <see cref="Pointer"/>.</para>
        /// </summary>
        /// <param name="pointer"><para>The pointer to a memory block.</para><para>Указатель на блок памяти.</para></param>
        /// <param name="capacity"><para>The capacity of the memory block in bytes.</para><para>Емкость блока памяти в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void DisposePointer(IntPtr pointer, long capacity);

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
                    DisposePointer(pointer, _capacity);
                }
            }
        }

        #endregion
    }
}