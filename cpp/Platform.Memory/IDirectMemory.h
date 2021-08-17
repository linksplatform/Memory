namespace Platform::Memory
{
    class IDirectMemory : public IMemory, public IDisposable
    {
    public:
        virtual IntPtr Pointer() = 0;
    };
}