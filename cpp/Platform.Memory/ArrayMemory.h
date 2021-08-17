namespace Platform::Memory
{
    template <typename ...> class ArrayMemory;
    template <typename TElement> class ArrayMemory<TElement> : public IArrayMemory<TElement>
    {
        private: TElement _array[N] = { {0} };

        public: std::int64_t Size()
        {
            return _array.Length;
        }

        public: TElement this[std::int64_t index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public: ArrayMemory(std::int64_t size) { _array = TElement[size]; }
    };
}
