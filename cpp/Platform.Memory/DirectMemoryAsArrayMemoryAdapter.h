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

        public: TElement& operator[](std::size_t index) override
        {
            return reinterpret_cast<TElement*>(_memory.Pointer())[index];
        }

        public: const TElement& operator[](std::size_t index) const override
        {
            return reinterpret_cast<const TElement*>(_memory.Pointer())[index];
        }

        public: DirectMemoryAsArrayMemoryAdapter(IDirectMemory &memory)
            :_memory(memory)
        {
            if ((memory.Size() % sizeof(TElement)) != 0) {
                throw std::invalid_argument("Memory is not aligned to element size.");
            }
        }
    };
}
