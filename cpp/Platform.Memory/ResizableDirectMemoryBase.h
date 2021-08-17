namespace Platform::Memory
{
    class ResizableDirectMemoryBase : public DisposableBase, public IResizableDirectMemory
    {
        public: inline static const std::int64_t MinimumCapacity = Environment.SystemPageSize;

        private: IntPtr _pointer = 0;
        private: std::int64_t _reservedCapacity = 0;
        private: std::int64_t _usedCapacity = 0;

        public: std::int64_t Size
        {
            get
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                return UsedCapacity;
            }
        }

        public: IntPtr Pointer
        {
            get
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                return _pointer;
            }
            protected: set
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                _pointer = value;
            }
        }

        public: std::int64_t ReservedCapacity
        {
            get
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                return _reservedCapacity;
            }
            set
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                if (value != _reservedCapacity)
                {
                    Platform::Ranges::EnsureExtensions::ArgumentInRange(Platform::Exceptions::Ensure::Always, value, Range<std::int64_t>(_usedCapacity, std::numeric_limits<std::int64_t>::max()));
                    OnReservedCapacityChanged(_reservedCapacity, value);
                    _reservedCapacity = value;
                }
            }
        }

        public: std::int64_t UsedCapacity
        {
            get
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                return _usedCapacity;
            }
            set
            {
                Platform::Disposables::EnsureExtensions::NotDisposed(Platform::Exceptions::Ensure::Always, this);
                if (value != _usedCapacity)
                {
                    Platform::Ranges::EnsureExtensions::ArgumentInRange(Platform::Exceptions::Ensure::Always, value, Range<std::int64_t>(0, _reservedCapacity));
                    _usedCapacity = value;
                }
            }
        }

        protected: override bool AllowMultipleDisposeCalls
        {
            get => true;
        }

        protected: virtual void OnReservedCapacityChanged(std::int64_t oldReservedCapacity, std::int64_t newReservedCapacity) = 0;

        protected: virtual void DisposePointer(IntPtr pointer, std::int64_t usedCapacity) = 0;

        protected: void Dispose(bool manual, bool wasDisposed) override
        {
            if (!wasDisposed)
            {
                auto pointer = Interlocked.Exchange(ref _pointer, IntPtr.0);
                if (pointer != IntPtr.0)
                {
                    this->DisposePointer(pointer, _usedCapacity);
                }
            }
        }
    };
}
