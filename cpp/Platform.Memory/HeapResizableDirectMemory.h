namespace Platform::Memory
{
    namespace Internal
    {
        auto as_bytes(void* raw_pointer, std::size_t capacity)
        {
            auto pointer = static_cast<std::byte*>(raw_pointer);
            return std::ranges::subrange(pointer, pointer + capacity);
        }

        void ZeroBlock(void* unsafe_pointer, std::size_t capacity)
        {
            auto range = as_bytes(unsafe_pointer, capacity);
            std::for_each(
                std::execution::par_unseq,
                range.begin(), range.end(),
            [](auto&& byte) {
                byte = std::byte{0};
            });
        }
    }

    class HeapResizableDirectMemory final : public ResizableDirectMemoryBase
    {
        using ResizableDirectMemoryBase::capacity_t;
        //protected: override std::string ObjectName
        //{
        //    get => std::string("Heap stored memory block at ").append(Platform::Converters::To<std::string>(Pointer)).append(" address.");
        //}

        public: HeapResizableDirectMemory(capacity_t minimumReservedCapacity = MinimumCapacity)
        {
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            ReservedCapacity(minimumReservedCapacity);
            UsedCapacity(0);
        }

        HeapResizableDirectMemory(const HeapResizableDirectMemory& other)
        {
            auto mem = Internal::as_bytes(other.Pointer(), other.UsedCapacity());
            Pointer() = std::copy(
                std::execution::par_unseq,
                mem.begin(), mem.end(),
                static_cast<std::byte*>(Pointer())
            );
            ReservedCapacity(other.ReservedCapacity());
            UsedCapacity(other.UsedCapacity());
        }

        protected: void OnReservedCapacityChanged(capacity_t oldReservedCapacity, capacity_t newReservedCapacity) final
        {
            if (Pointer() == nullptr)
            {
                Pointer() = std::malloc(newReservedCapacity);
                Internal::ZeroBlock(Pointer(), newReservedCapacity);
            }
            else
            {
                Pointer() = std::realloc(Pointer(), newReservedCapacity);
                auto pointer = static_cast<std::byte*>(Pointer()) + oldReservedCapacity;
                Internal::ZeroBlock(pointer, newReservedCapacity - oldReservedCapacity);
            }
        }

        public: ~HeapResizableDirectMemory() final
        {
            delete static_cast<std::byte*>(Pointer());
        }
    };
}