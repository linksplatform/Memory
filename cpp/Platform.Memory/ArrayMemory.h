namespace Platform::Memory
{
    template <typename ...> class ArrayMemory;
    template <typename TElement> class ArrayMemory<TElement> : public IArrayMemory<TElement>
    {
        private: std::vector<TElement> _array{};

        public: size_t Size()
        {
            return _array.size();
        }

        TElement& operator[](std::size_t index) { return _array[index]; }
        const TElement& operator[](std::size_t index) const { return _array[index]; }

        public: ArrayMemory() = default;

        public: explicit ArrayMemory(std::size_t size) : _array(size) {}
    };
}
