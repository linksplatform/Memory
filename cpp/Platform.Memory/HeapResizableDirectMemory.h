namespace Platform::Memory
{
    public unsafe class HeapResizableDirectMemory : public ResizableDirectMemoryBase
    {
        protected: override std::string ObjectName
        {
            get => std::string("Heap stored memory block at ").append(Platform::Converters::To<std::string>(Pointer)).append(" address.");
        }

        public: HeapResizableDirectMemory(std::int64_t minimumReservedCapacity)
        {
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        public: HeapResizableDirectMemory() : this(MinimumCapacity) { }

        protected: void DisposePointer(IntPtr pointer, std::int64_t usedCapacity) override { Marshal.FreeHGlobal(pointer); }

        protected: void OnReservedCapacityChanged(std::int64_t oldReservedCapacity, std::int64_t newReservedCapacity) override
        {
            if (Pointer == IntPtr.0)
            {
                Pointer = Marshal.AllocHGlobal(this->IntPtr(newReservedCapacity));
                MemoryBlock.0((void*)Pointer, newReservedCapacity);
            }
            else
            {
                Pointer = Marshal.ReAllocHGlobal(Pointer, this->IntPtr(newReservedCapacity));
                auto pointer = (std::uint8_t*)Pointer + oldReservedCapacity;
                MemoryBlock.0(pointer, newReservedCapacity - oldReservedCapacity);
            }
        }
    };
}