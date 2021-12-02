namespace Platform::Memory
{
    namespace Internal
    {
        std::size_t page_size() noexcept
        {
            std::size_t page_size;
        #if defined(WIN32)
            SYSTEM_INFO sSysInfo;
            GetSystemInfo(&sSysInfo);
            page_size = sSysInfo.dwPageSize;
        #elif defined(PAGESIZE)
            page_size = PAGESIZE;
        #else
            page_size = sysconf(_SC_PAGESIZE);
        #endif
            return page_size;
        }

        auto PageSize = page_size();
    }

    class ResizableDirectMemoryBase : public IResizableDirectMemory
    {
        protected: using IResizableDirectMemory::pointer_t;
        protected: using IResizableDirectMemory::capacity_t;

        public: inline static const std::size_t MinimumCapacity = Internal::PageSize;

        private: pointer_t _pointer = nullptr;
        private: capacity_t _reservedCapacity = 0;
        private: capacity_t _usedCapacity = 0;

        public: std::size_t Size()
        {
            return UsedCapacity();
        }

        public: pointer_t& Pointer()
        {
            return _pointer;
        }

        public: const pointer_t& Pointer() const
        {
            return _pointer;
        }

        public: capacity_t ReservedCapacity() const
        {
            return _reservedCapacity;
        }

        public: void ReservedCapacity(capacity_t value)
        {
            using namespace Platform::Ranges;
            using namespace Platform::Exceptions;

            if (value != _reservedCapacity)
            {
                Ranges::Always::ArgumentInRange(value, Range{_usedCapacity, std::numeric_limits<capacity_t>::max()});
                OnReservedCapacityChanged(_reservedCapacity, value);
                _reservedCapacity = value;
            }
        }

        public: capacity_t UsedCapacity() const
        {
            return _usedCapacity;
        }

        public: void UsedCapacity(capacity_t value)
        {
            using namespace Platform::Ranges;

            if (value != _usedCapacity)
            {
                // TODO: Use modernize Ranges version
                Always::ArgumentInRange(value, Range(0, _reservedCapacity));
                _usedCapacity = value;
            }
        }

        protected: virtual void OnReservedCapacityChanged(capacity_t oldReservedCapacity, capacity_t newReservedCapacity) = 0;

        protected: virtual ~ResizableDirectMemoryBase()
        {
            //_pointer = nullptr;
        }
    };
}
