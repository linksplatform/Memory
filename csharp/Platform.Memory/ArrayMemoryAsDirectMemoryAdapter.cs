using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Disposables;
using Platform.Exceptions;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents adapter from a memory block with access via indexer to direct memory access.</para>
    /// <para>Представляет адаптер от блока памяти с доступом через индексатор к прямому доступу к памяти.</para>
    /// </summary>
    /// <typeparam name="TElement"><para>Element type.</para><para>Тип элемента.</para></typeparam>
    public class ArrayMemoryAsDirectMemoryAdapter<TElement> : DisposableBase, IDirectMemory
        where TElement : struct
    {
        #region Fields
        private readonly ArrayMemory<TElement> _arrayMemory;
        private readonly GCHandle _pinnedHandle;
        private readonly IntPtr _pointer;

        #endregion

        #region Properties

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IMemory.Size"]/*'/>
        public long Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _arrayMemory.Size * Structure<TElement>.Size;
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IDirectMemory.Pointer"]/*'/>
        public IntPtr Pointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pointer;
        }

        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Pinned array memory at '{_pointer}' address.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="ArrayMemoryAsDirectMemoryAdapter{TElement}"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="ArrayMemoryAsDirectMemoryAdapter{TElement}"/>.</para>
        /// </summary>
        /// <param name="arrayMemory"><para>An object implementing <see cref="ArrayMemory{TElement}"/> class.</para><para>Объект, реализующий класс <see cref="ArrayMemory{TElement}"/>.</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayMemoryAsDirectMemoryAdapter(ArrayMemory<TElement> arrayMemory)
        {
            Ensure.Always.ArgumentNotNull(arrayMemory, nameof(arrayMemory));
            _arrayMemory = arrayMemory;
            
            // Use reflection to get the underlying array from ArrayMemory
            var field = typeof(ArrayMemory<TElement>).GetField("_array", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Ensure.Always.ArgumentMeetsCriteria(field, f => f != null, nameof(arrayMemory), "Cannot access internal array field of ArrayMemory.");
            var array = (TElement[])field!.GetValue(_arrayMemory)!;
            
            // Pin the array in memory
            _pinnedHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            _pointer = _pinnedHandle.AddrOfPinnedObject();
        }

        #endregion

        #region DisposableBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed && _pinnedHandle.IsAllocated)
            {
                _pinnedHandle.Free();
            }
        }

        #endregion
    }
}