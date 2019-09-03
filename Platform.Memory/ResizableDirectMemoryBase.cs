using System;
using System.Threading;
using Platform.Exceptions;
using Platform.Disposables;
using Platform.Ranges;

namespace Platform.Memory
{
    /// <summary>
    /// <para>Provides a base implementation for the resizable memory block with direct access (via unmanaged pointers).</para>
    /// <para>Предоставляет базовую реализацию для блока памяти c изменяемым размером и прямым доступом (через неуправляемые указатели).</para>
    /// </summary>
    public abstract class ResizableDirectMemoryBase : DisposableBase, IResizableDirectMemory
    {
        #region Constants

        /// <summary>
        /// <para>Gets minimum capacity in bytes.</para>
        /// <para>Возвращает минимальную емкость в байтах.</para>
        /// </summary>
        public static readonly long MinimumCapacity = 4096;

        #endregion

        #region Fields

        private IntPtr _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        #endregion

        #region Properties

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IMemory.Size"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        public long Size
        {
            get
            {
                Ensure.Always.NotDisposed(this);
                return UsedCapacity;
            }
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IDirectMemory.Pointer"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        public IntPtr Pointer
        {
            get
            {
                Ensure.Always.NotDisposed(this);
                return _pointer;
            }
            protected set
            {
                Ensure.Always.NotDisposed(this);
                _pointer = value;
            }
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IResizableDirectMemory.ReservedCapacity"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        /// <exception cref="ArgumentOutOfRangeException"><para>Attempted to set the reserved capacity to a value that is less than the used capacity.</para><para>Была выполнена попытка установить зарезервированную емкость на значение, которое меньше используемой емкости.</para></exception>
        public long ReservedCapacity
        {
            get
            {
                Ensure.Always.NotDisposed(this);
                return _reservedCapacity;
            }
            set
            {
                Ensure.Always.NotDisposed(this);
                if (value != _reservedCapacity)
                {
                    Ensure.Always.ArgumentInRange(value, new Range<long>(_usedCapacity, long.MaxValue));
                    OnReservedCapacityChanged(_reservedCapacity, value);
                    _reservedCapacity = value;
                }
            }
        }

        /// <inheritdoc/>
        /// <include file='bin\Release\netstandard2.0\Platform.Memory.xml' path='doc/members/member[@name="P:Platform.Memory.IResizableDirectMemory.UsedCapacity"]/*'/>
        /// <exception cref="ObjectDisposedException"><para>The memory block is disposed.</para><para>Блок памяти уже высвобожден.</para></exception>
        /// <exception cref="ArgumentOutOfRangeException"><para>Attempted to set the used capacity to a value that is greater than the reserved capacity or less than zero.</para><para>Была выполнена попытка установить используемую емкость на значение, которое больше, чем зарезервированная емкость или меньше нуля.</para></exception>
        public long UsedCapacity
        {
            get
            {
                Ensure.Always.NotDisposed(this);
                return _usedCapacity;
            }
            set
            {
                Ensure.Always.NotDisposed(this);
                if (value != _usedCapacity)
                {
                    Ensure.Always.ArgumentInRange(value, new Range<long>(0, _reservedCapacity));
                    _usedCapacity = value;
                }
            }
        }

        #endregion

        #region DisposableBase Properties

        /// <inheritdoc/>
        protected override bool AllowMultipleDisposeCalls => true;

        #endregion

        #region Methods

        /// <summary>
        /// <para>Executed on the event of change for <see cref="ReservedCapacity"/> property.</para>
        /// <para>Выполняется в случае изменения свойства <see cref="ReservedCapacity"/>.</para>
        /// </summary>
        /// <param name="oldReservedCapacity"><para>The old reserved capacity of the memory block in bytes.</para><para>Старая зарезервированная емкость блока памяти в байтах.</para></param>
        /// <param name="newReservedCapacity"><para>The new reserved capacity of the memory block in bytes.</para><para>Новая зарезервированная емкость блока памяти в байтах.</para></param>
        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        /// <summary>
        /// <para>Executed when it is time to dispose <see cref="Pointer"/>.</para>
        /// <para>Выполняется, когда пришло время высвободить <see cref="Pointer"/>.</para>
        /// </summary>
        /// <param name="pointer"><para>The pointer to a memory block.</para><para>Указатель на блок памяти.</para></param>
        /// <param name="usedCapacity"><para>The used capacity of the memory block in bytes.</para><para>Используемая емкость блока памяти в байтах.</para></param>
        protected abstract void DisposePointer(IntPtr pointer, long usedCapacity);

        #endregion

        #region DisposableBase Methods

        /// <inheritdoc/>
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                var pointer = Interlocked.Exchange(ref _pointer, IntPtr.Zero);
                if (pointer != IntPtr.Zero)
                {
                    DisposePointer(pointer, _usedCapacity);
                }
            }
        }

        #endregion
    }
}
