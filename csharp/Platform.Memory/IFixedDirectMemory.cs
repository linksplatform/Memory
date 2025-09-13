using System.Runtime.CompilerServices;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a fixed-size memory block interface with direct access (via unmanaged pointers).</para>
    /// <para>Представляет интерфейс блока памяти фиксированного размера с прямым доступом (через неуправляемые указатели).</para>
    /// </summary>
    public interface IFixedDirectMemory : IDirectMemory
    {
        /// <summary>
        /// <para>Gets the capacity in bytes of this fixed memory block.</para>
        /// <para>Возвращает размер блока памяти фиксированного размера в байтах.</para>
        /// </summary>
        long Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }
    }
}