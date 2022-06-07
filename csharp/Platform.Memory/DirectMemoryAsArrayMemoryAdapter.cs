using System;
using System.Runtime.CompilerServices;
using Platform.Disposables;
using Platform.Exceptions;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents adapter to a memory block with access via indexer.</para>
    /// <para>Представляет адаптер к блоку памяти с доступом через индексатор.</para>
    /// </summary>
    /// <typeparam name="TElement"><para>Element type.</para><para>Тип элемента.</para></typeparam>
    public class DirectMemoryAsArrayMemoryAdapter<TElement> : DisposableBase, IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        #region Fields
        private readonly IDirectMemory _memory;

        #endregion

        #region Properties

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IMemory.Size"]/*'/>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _memory.Size;
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IDirectMemory.Pointer"]/*'/>
        public IntPtr Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _memory.Pointer;
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IArrayMemory`1.Item(System.Int64)"]/*'/>
        public TElement this[long index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Pointer.ReadElementValue<TElement>(index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Pointer.WriteElementValue(index, value);
        }

        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Array as memory block at '{Pointer}' address.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DirectMemoryAsArrayMemoryAdapter{TElement}"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="DirectMemoryAsArrayMemoryAdapter{TElement}"/>.</para>
        /// </summary>
        /// <param name="memory"><para>An object implementing <see cref="IDirectMemory"/> interface.</para><para>Объект, реализующий интерфейс <see cref="IDirectMemory"/>.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DirectMemoryAsArrayMemoryAdapter(IDirectMemory memory)
        {
            Ensure.Always.ArgumentNotNull(memory, nameof(memory));
            Ensure.Always.ArgumentMeetsCriteria(memory, m => (m.Size % Structure<TElement>.Size) == 0, nameof(memory), "Memory is not aligned to element size.");
            _memory = memory;
        }

        #endregion

        #region DisposableBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                _memory.DisposeIfPossible();
            }
        }

        #endregion
    }
}
