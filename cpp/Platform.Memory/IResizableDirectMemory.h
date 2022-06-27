namespace Platform::Memory
{
    class IResizableDirectMemory : public IDirectMemory
    {
    protected:
        using capacity_t = std::size_t;
    public:
        virtual capacity_t ReservedCapacity() const = 0;
        virtual void ReservedCapacity(capacity_t) = 0;


        virtual capacity_t UsedCapacity() const = 0;
        virtual void UsedCapacity(capacity_t) = 0;
    };
}