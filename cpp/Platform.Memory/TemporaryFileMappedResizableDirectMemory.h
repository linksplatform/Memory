namespace Platform::Memory
{
    class TemporaryFileMappedResizableDirectMemory final : public ResizableDirectMemoryBase
    {
        using base_t = FileMappedResizableDirectMemory;
        base_t base;

        pointer_t& Pointer() override
        {
            return base.Pointer();
        }

        const pointer_t& Pointer() const override
        {
            return base.Pointer();
        }

        //protected: override std::string ObjectName
        //{
        //    get => std::string("Temporary file stored memory block at '").append(Platform::Converters::To<std::string>(Path)).append("' path.");
        //}

        void OnReservedCapacityChanged(capacity_t oldReservedCapacity, capacity_t newReservedCapacity) override
        {
            base.OnReservedCapacityChanged(oldReservedCapacity, newReservedCapacity);
        }

        public: explicit TemporaryFileMappedResizableDirectMemory(std::size_t minimumReservedCapacity) : base((std::filesystem::current_path() / std::tmpnam(nullptr)).string(), minimumReservedCapacity){}

        public: TemporaryFileMappedResizableDirectMemory() : TemporaryFileMappedResizableDirectMemory(base_t::MinimumCapacity) { }

        public: ~TemporaryFileMappedResizableDirectMemory() final
        {
            base.Close();
            std::cout << base.Path;
            std::filesystem::remove(base.Path);
        }
    };
}
