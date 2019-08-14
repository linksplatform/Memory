using System;
using Platform.Disposables;
using Platform.Exceptions;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// Represents adapter to a memory block with access via indexer.
    /// Представляет адаптер к блоку памяти с доступом через индексатор.
    /// </summary>
    /// <typeparam name="TElement">Element type. Тип элемента.</typeparam>
    public class DirectMemoryAsArrayMemoryAdapter<TElement> : DisposableBase, IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        #region Fields

        private readonly IDirectMemory _memory;

        #endregion

        #region Properties

        public long Size => _memory.Size;

        public IntPtr Pointer => _memory.Pointer;

        public TElement this[long index]
        {
            get => Pointer.GetElement(Structure<TElement>.Size, index).GetValue<TElement>();
            set => Pointer.GetElement(Structure<TElement>.Size, index).SetValue(value);
        }

        #endregion

        #region DisposableBase Properties

        protected override string ObjectName => $"Array as memory block at '{Pointer}' address.";

        #endregion

        #region Constructors

        public DirectMemoryAsArrayMemoryAdapter(IDirectMemory memory)
        {
            Ensure.Always.ArgumentMeetsCriteria(m => (m.Size % Structure<TElement>.Size) == 0, memory, nameof(memory), "Memory is not aligned to element size.");
            _memory = memory;
        }

        #endregion

        #region DisposableBase Methods

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
