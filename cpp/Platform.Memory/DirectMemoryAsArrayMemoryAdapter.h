namespace Platform::Memory
{
    template <typename ...> class DirectMemoryAsArrayMemoryAdapter;
    template <typename TElement> class DirectMemoryAsArrayMemoryAdapter<TElement> : public DisposableBase, IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        private: IDirectMemory _memory = 0;

        public: std::int64_t Size()
        {
            return _memory.Size;
        }

        public: IntPtr Pointer()
        {
            return _memory.Pointer;
        }

        public: TElement this[std::int64_t index]
        {
            get => Pointer.ReadElementValue<TElement>(index);
            set => Pointer.WriteElementValue(index, value);
        }

        protected: override std::string ObjectName
        {
            get => std::string("Array as memory block at '").append(Platform::Converters::To<std::string>(Pointer)).append("' address.");
        }

        public: DirectMemoryAsArrayMemoryAdapter(IDirectMemory &memory)
        {
            Platform::Exceptions::EnsureExtensions::ArgumentNotNull(Platform::Exceptions::Ensure::Always, memory, "memory");
            Platform::Exceptions::EnsureExtensions::ArgumentMeetsCriteria(Platform::Exceptions::Ensure::Always, memory, m => (m.Size % Structure<TElement>.Size) == 0, "memory", "Memory is not aligned to element size.");
            _memory = memory;
        }

        protected: void Dispose(bool manual, bool wasDisposed) override
        {
            if (!wasDisposed)
            {
                _memory.DisposeIfPossible();
            }
        }
    }
}
