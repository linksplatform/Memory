namespace Platform::Memory
{
    template <typename ...> class IArrayMemory;
    template <typename TElement> class IArrayMemory<TElement> : public IMemory
    {
    public:
        //virtual TElement& operator[](std::size_t index) {}

        // TODO: impl const
        //virtual const TElement& operator[](std::size_t index) const {}
    };
}
