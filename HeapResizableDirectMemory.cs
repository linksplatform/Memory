using System;
using System.Runtime.InteropServices;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a memory block allocated in Heap.</para>
    /// <para>Представляет блок памяти, выделенный в "куче".</para>
    /// </summary>
    public unsafe class HeapResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName => $"Heap stored memory block at {Pointer} address.";

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="HeapResizableDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="HeapResizableDirectMemory"/>.</para>
        /// </summary>
        /// <param name="minimumReservedCapacity"><para>Minimum file size in bytes.</para><para>Минимальный размер файла в байтах.</para></param>
        public HeapResizableDirectMemory(long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="HeapResizableDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="HeapResizableDirectMemory"/>.</para>
        /// </summary>
        public HeapResizableDirectMemory() : this(MinimumCapacity) { }

        #endregion

        #region ResizableDirectMemoryBase Methods

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="M:Platform.Memory.ResizableDirectMemoryBase.DisposePointer(System.IntPtr,System.Int64)"]/*'/>
        protected override void DisposePointer(IntPtr pointer, long usedCapacity) => Marshal.FreeHGlobal(pointer);

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="M:Platform.Memory.ResizableDirectMemoryBase.OnReservedCapacityChanged(System.Int64,System.Int64)"]/*'/>
        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            if (Pointer == IntPtr.Zero)
            {
                Pointer = Marshal.AllocHGlobal(new IntPtr(newReservedCapacity));
                MemoryBlock.Zero(Pointer.ToPointer(), newReservedCapacity);
            }
            else
            {
                Pointer = Marshal.ReAllocHGlobal(Pointer, new IntPtr(newReservedCapacity));
            }
        }

        #endregion
    }
}