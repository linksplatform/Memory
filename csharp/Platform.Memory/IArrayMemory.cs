using System.Runtime.CompilerServices;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents a memory block interface with access via indexer.</para>
    /// <para>Представляет интерфейс блока памяти с доступом через индексатор.</para>
    /// </summary>
    /// <typeparam name="TElement"><para>Element type.</para><para>Тип элемента.</para></typeparam>
    public interface IArrayMemory<TElement> : IMemory
    {
        /// <summary>
        /// <para>Gets or sets the element at the specified index.</para>
        /// <para>Возвращает или устанавливает элемент по указанному индексу.</para>
        /// </summary>
        /// <param name="index"><para>The index of the element to get or set.</para><para>Индекс элемента, который нужно получить или установить.</para></param>
        TElement this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }
    }
}
