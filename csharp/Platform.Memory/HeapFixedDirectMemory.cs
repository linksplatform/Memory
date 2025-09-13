using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a fixed-size memory block allocated in Heap.</para>
    /// <para>Представляет блок памяти фиксированного размера, выделенный в "куче".</para>
    /// </summary>
    public unsafe class HeapFixedDirectMemory : FixedDirectMemoryBase
    {
        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Fixed heap stored memory block at {Pointer} address.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="HeapFixedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="HeapFixedDirectMemory"/>.</para>
        /// </summary>
        /// <param name="capacity"><para>Fixed capacity in bytes.</para><para>Фиксированный размер в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HeapFixedDirectMemory(long capacity) : base(capacity) { }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="HeapFixedDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="HeapFixedDirectMemory"/>.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HeapFixedDirectMemory() : this(MinimumCapacity) { }

        #endregion

        #region FixedDirectMemoryBase Methods

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="M:Platform.Memory.FixedDirectMemoryBase.AllocateMemory(System.Int64)"]/*'/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void AllocateMemory(long capacity)
        {
            Pointer = Marshal.AllocHGlobal(new IntPtr(capacity));
            MemoryBlock.Zero((void*)Pointer, capacity);
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="M:Platform.Memory.FixedDirectMemoryBase.DisposePointer(System.IntPtr,System.Int64)"]/*'/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DisposePointer(IntPtr pointer, long capacity) => Marshal.FreeHGlobal(pointer);

        #endregion
    }
}