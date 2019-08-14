using System;
using System.Threading;
using Platform.Exceptions;
using Platform.Disposables;
using Platform.Ranges;

namespace Platform.Memory
{
    /// <summary>
    /// Provides a base implementation for the resizable memory block with direct access (via unmanaged pointers).
    /// Предоставляет базовую реализацию для блока памяти c изменяемым размером и прямым доступом (через неуправляемые указатели).
    /// </summary>
    public abstract class ResizableDirectMemoryBase : DisposableBase, IResizableDirectMemory
    {
        #region Constants

        public static readonly long MinimumCapacity = 4096;

        #endregion

        #region Fields

        private IntPtr _pointer;
        private long _reservedCapacity;
        private long _usedCapacity;

        #endregion

        #region Properties

        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
        public long Size
        {
            get
            {
                Ensure.Always.NotDisposed(this);
                return UsedCapacity;
            }
        }

        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
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

        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Attempted to set the reserved capacity to a value that is less than the used capacity. Была выполнена попытка установить зарезервированную емкость на значение, которое меньше используемой емкости.</exception>
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

        /// <exception cref="ObjectDisposedException">The memory block is disposed. Блок памяти уже высвобожден.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Attempted to set the used capacity to a value that is greater than the reserved capacity or less than zero. Была выполнена попытка установить используемую емкость на значение, которое больше, чем зарезервированная емкость или меньше нуля.</exception>
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

        protected override bool AllowMultipleDisposeCalls => true;

        #endregion

        #region Methods

        protected abstract void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity);

        protected abstract void DisposePointer(IntPtr pointer, long size);

        #endregion

        #region DisposableBase Methods

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
