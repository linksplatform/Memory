namespace Platform::Memory
{
    class IResizableDirectMemory : public IDirectMemory
    {
    public:
        std::int64_t ReservedCapacity
        {
            get;
            set;
        }

        std::int64_t UsedCapacity
        {
            get;
            set;
        }
    };
}