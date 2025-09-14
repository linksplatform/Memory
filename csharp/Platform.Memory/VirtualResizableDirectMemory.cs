using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a memory block allocated using virtual memory API.</para>
    /// <para>Представляет блок памяти, выделенный с использованием API виртуальной памяти.</para>
    /// </summary>
    public unsafe class VirtualResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Native API Constants
        
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint PAGE_READWRITE = 0x04;
        
        private const int PROT_READ = 0x1;
        private const int PROT_WRITE = 0x2;
        private const int MAP_PRIVATE = 0x02;
        private const int MAP_ANONYMOUS = 0x20;
        private const int MAP_FAILED = -1;
        
        #endregion
        
        #region Native API Declarations
        
        // Windows Virtual Memory API
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFree(IntPtr lpAddress, nuint dwSize, uint dwFreeType);
        
        // Unix/Linux/macOS Memory Mapping API
        [DllImport("libc", SetLastError = true)]
        private static extern IntPtr mmap(IntPtr addr, nuint length, int prot, int flags, int fd, nint offset);
        
        [DllImport("libc", SetLastError = true)]
        private static extern int munmap(IntPtr addr, nuint length);
        
        // Linux-specific mremap (not available on macOS)
        [DllImport("libc", SetLastError = true)]
        private static extern IntPtr mremap(IntPtr old_address, nuint old_size, nuint new_size, int flags);
        
        private const int MREMAP_MAYMOVE = 1;
        
        #endregion
        
        #region Fields
        
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
        private long _allocatedSize;
        
        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Virtual memory block at {Pointer} address.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="VirtualResizableDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="VirtualResizableDirectMemory"/>.</para>
        /// </summary>
        /// <param name="minimumReservedCapacity"><para>Minimum memory size in bytes.</para><para>Минимальный размер памяти в байтах.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VirtualResizableDirectMemory(long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="VirtualResizableDirectMemory"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="VirtualResizableDirectMemory"/>.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VirtualResizableDirectMemory() : this(MinimumCapacity) { }

        #endregion

        #region ResizableDirectMemoryBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void DisposePointer(IntPtr pointer, long usedCapacity)
        {
            if (IsWindows)
            {
                if (!VirtualFree(pointer, 0, MEM_RELEASE))
                {
                    throw new InvalidOperationException($"Failed to free virtual memory at {pointer}. Error: {Marshal.GetLastWin32Error()}");
                }
            }
            else
            {
                if (munmap(pointer, (nuint)_allocatedSize) != 0)
                {
                    throw new InvalidOperationException($"Failed to unmap virtual memory at {pointer}. Error: {Marshal.GetLastPInvokeError()}");
                }
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            if (Pointer == IntPtr.Zero)
            {
                // Initial allocation
                Pointer = AllocateVirtualMemory(newReservedCapacity);
                _allocatedSize = newReservedCapacity;
                MemoryBlock.Zero((void*)Pointer, newReservedCapacity);
            }
            else
            {
                // Resize existing allocation
                if (IsLinux && oldReservedCapacity < newReservedCapacity)
                {
                    // Try to use mremap on Linux for efficient resizing
                    try
                    {
                        var remappedPointer = mremap(Pointer, (nuint)oldReservedCapacity, (nuint)newReservedCapacity, MREMAP_MAYMOVE);
                        if (remappedPointer != new IntPtr(MAP_FAILED))
                        {
                            Pointer = remappedPointer;
                            _allocatedSize = newReservedCapacity;
                            // Zero the new memory region
                            var pointer = (byte*)Pointer + oldReservedCapacity;
                            MemoryBlock.Zero(pointer, newReservedCapacity - oldReservedCapacity);
                            return;
                        }
                    }
                    catch
                    {
                        // Fall back to manual allocation and copy
                    }
                }
                
                // Manual resize: allocate new, copy old, free old
                var newPointer = AllocateVirtualMemory(newReservedCapacity);
                var copySize = Math.Min(oldReservedCapacity, newReservedCapacity);
                Buffer.MemoryCopy((void*)Pointer, (void*)newPointer, newReservedCapacity, copySize);
                
                // Zero the additional memory if growing
                if (newReservedCapacity > oldReservedCapacity)
                {
                    var pointer = (byte*)newPointer + oldReservedCapacity;
                    MemoryBlock.Zero(pointer, newReservedCapacity - oldReservedCapacity);
                }
                
                DisposePointer(Pointer, oldReservedCapacity);
                Pointer = newPointer;
                _allocatedSize = newReservedCapacity;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr AllocateVirtualMemory(long size)
        {
            if (IsWindows)
            {
                var pointer = VirtualAlloc(IntPtr.Zero, (nuint)size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (pointer == IntPtr.Zero)
                {
                    throw new OutOfMemoryException($"Failed to allocate virtual memory of size {size}. Error: {Marshal.GetLastWin32Error()}");
                }
                return pointer;
            }
            else
            {
                var pointer = mmap(IntPtr.Zero, (nuint)size, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
                if (pointer == new IntPtr(MAP_FAILED))
                {
                    throw new OutOfMemoryException($"Failed to allocate virtual memory of size {size}. Error: {Marshal.GetLastPInvokeError()}");
                }
                return pointer;
            }
        }

        #endregion
    }
}