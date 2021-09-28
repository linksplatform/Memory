namespace Platform::Memory
{
    template <typename ...> class DirectMemoryAsArrayMemoryAdapter;
    template <typename TElement> class DirectMemoryAsArrayMemoryAdapter<TElement> :
        public IArrayMemory<TElement>, public IDirectMemory
    {
        using Self = DirectMemoryAsArrayMemoryAdapter<TElement>;
        using IDirectMemory::pointer_t;

        private: IDirectMemory& _memory;

        public: std::size_t Size() final
        {
            return _memory.Size();
        }

        public: pointer_t& Pointer()
        {
            return _memory.Pointer();
        }

        public: const pointer_t& Pointer() const
        {
            return _memory.Pointer();
        }

        //public: TElement this[std::int64_t index]
        //{
        //    get => Pointer.ReadElementValue<TElement>(index);
        //    set => Pointer.WriteElementValue(index, value);
        //}

        //protected: override std::string ObjectName
        //{
        //    get => std::string("Array as memory block at '").append(Platform::Converters::To<std::string>(Pointer)).append("' address.");
        //}

        std::size_t current_index = 0;

        [[no_unique_address]] struct : PropertySetup<Self> {
            using PropertySetup<Self>::self;

            operator TElement&() { return *reinterpret_cast<TElement*>(self().Pointer() + self().current_index); }

            auto& operator=(TElement value)
            {
                TElement& ref = *this;
                ref = value;
                return *this;
            }
        } _Index;

        public: auto&& operator[](std::size_t index)
        {
            current_index = index;
            return _Index;
        }

        public: DirectMemoryAsArrayMemoryAdapter(IDirectMemory &memory)
            :_memory(memory)
        {
            using namespace Platform::Exceptions::Ensure;
            Always::ArgumentMeetsCriteria(
                memory,
                [](auto& m) { return (m.Size() % sizeof(TElement)) == 0; },
                "memory",
                "Memory is not aligned to element size."
            );
        }
    };
}
