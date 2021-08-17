namespace Platform::Memory
{
    class TemporaryFileMappedResizableDirectMemory : public FileMappedResizableDirectMemory
    {
        protected: override std::string ObjectName
        {
            get => std::string("Temporary file stored memory block at '").append(Platform::Converters::To<std::string>(Path)).append("' path.");
        }

        public: TemporaryFileMappedResizableDirectMemory(std::int64_t minimumReservedCapacity) : base(System::IO::Path::GetTempFileName(), minimumReservedCapacity) { }

        public: TemporaryFileMappedResizableDirectMemory() : this(MinimumCapacity) { }

        protected: void Dispose(bool manual, bool wasDisposed) override
        {
            base.Dispose(manual, wasDisposed);
            if (!wasDisposed)
            {
                File.Delete(Path);
            }
        }
    };
}
