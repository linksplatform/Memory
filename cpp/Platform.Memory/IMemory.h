namespace Platform::Memory
{
    class IMemory
    {
    public:
        virtual std::size_t Size() = 0;

        std::size_t size() { return Size(); }
    };
}