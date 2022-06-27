namespace Platform::Memory
{
    class IDirectMemory : public IMemory
    {
    protected:
        using pointer_t = void*;
    public:
        virtual pointer_t& Pointer() = 0;

        virtual const pointer_t& Pointer() const = 0;
    };
}