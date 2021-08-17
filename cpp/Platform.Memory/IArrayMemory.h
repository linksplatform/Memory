namespace Platform::Memory
{
    template <typename ...> class IArrayMemory;
    template <typename TElement> class IArrayMemory<TElement> : public IMemory
    {
    public:
        TElement this[std::int64_t index]
        {
            get;
            set;
        }
    };
}
