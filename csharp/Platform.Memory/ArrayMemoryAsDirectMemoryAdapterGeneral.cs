using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Disposables;
using Platform.Exceptions;
using Platform.Unsafe;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Represents adapter from any IArrayMemory implementation to direct memory access.</para>
    /// <para>Представляет адаптер от любой реализации IArrayMemory к прямому доступу к памяти.</para>
    /// </summary>
    /// <typeparam name="TElement"><para>Element type.</para><para>Тип элемента.</para></typeparam>
    public class ArrayMemoryAsDirectMemoryAdapterGeneral<TElement> : DisposableBase, IDirectMemory
        where TElement : struct
    {
        #region Fields
        private readonly IArrayMemory<TElement> _arrayMemory;
        private readonly TElement[] _pinnedArray;
        private readonly GCHandle _pinnedHandle;
        private readonly IntPtr _pointer;
        private readonly bool _isReadOnly;

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
            get
            {
                SyncFromArrayMemory();
                return _pointer;
            }
        }

        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override string ObjectName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => $"Pinned array memory adapter at '{_pointer}' address.";
        }

        #endregion

        #region Constructors

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="ArrayMemoryAsDirectMemoryAdapterGeneral{TElement}"/> class.</para>
        /// <para>Инициализирует новый экземпляр класса <see cref="ArrayMemoryAsDirectMemoryAdapterGeneral{TElement}"/>.</para>
        /// </summary>
        /// <param name="arrayMemory"><para>An object implementing <see cref="IArrayMemory{TElement}"/> interface.</para><para>Объект, реализующий интерфейс <see cref="IArrayMemory{TElement}"/>.</para></param>
        /// <param name="isReadOnly"><para>Whether the adapter is read-only (won't sync changes back).</para><para>Является ли адаптер только для чтения (не будет синхронизировать изменения обратно).</para></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArrayMemoryAsDirectMemoryAdapterGeneral(IArrayMemory<TElement> arrayMemory, bool isReadOnly = false)
        {
            Ensure.Always.ArgumentNotNull(arrayMemory, nameof(arrayMemory));
            _arrayMemory = arrayMemory;
            _isReadOnly = isReadOnly;
            
            // Create a managed array copy
            _pinnedArray = new TElement[_arrayMemory.Size];
            
            // Copy data from the source
            for (long i = 0; i < _arrayMemory.Size; i++)
            {
                _pinnedArray[i] = _arrayMemory[i];
            }
            
            // Pin the array in memory
            _pinnedHandle = GCHandle.Alloc(_pinnedArray, GCHandleType.Pinned);
            _pointer = _pinnedHandle.AddrOfPinnedObject();
        }

        #endregion

        #region Methods

        /// <summary>
        /// <para>Synchronizes changes from the original array memory to the pinned copy.</para>
        /// <para>Синхронизирует изменения из исходной памяти массива в закрепленную копию.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SyncFromArrayMemory()
        {
            for (long i = 0; i < _arrayMemory.Size; i++)
            {
                _pinnedArray[i] = _arrayMemory[i];
            }
        }

        /// <summary>
        /// <para>Synchronizes changes from the pinned copy back to the original array memory.</para>
        /// <para>Синхронизирует изменения из закрепленной копии обратно в исходную память массива.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncToArrayMemory()
        {
            if (_isReadOnly)
            {
                return;
            }
            
            for (long i = 0; i < _arrayMemory.Size; i++)
            {
                _arrayMemory[i] = _pinnedArray[i];
            }
        }

        #endregion

        #region DisposableBase Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                if (!_isReadOnly)
                {
                    SyncToArrayMemory();
                }
                
                if (_pinnedHandle.IsAllocated)
                {
                    _pinnedHandle.Free();
                }
            }
        }

        #endregion
    }
}