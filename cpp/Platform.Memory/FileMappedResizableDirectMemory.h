namespace Platform::Memory
{
    class FileMappedResizableDirectMemory final : public ResizableDirectMemoryBase
    {
        protected: using ResizableDirectMemoryBase::capacity_t;

        private: memory_mapped_file::writable_mmf _file;

        //protected: override std::string ObjectName
        //{
        //    get => std::string("File stored memory block at '").append(Path).append("' path.");
        //}

        public: FileMappedResizableDirectMemory(const std::string& path, capacity_t minimumReservedCapacity)
            : _file(path.c_str(), memory_mapped_file::mmf_exists_mode::if_exists_just_open)
        {
            using namespace Platform::Collections::Ensure;
            Always::ArgumentNotEmptyAndNotWhiteSpace(path, "path", "");
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            Path = path;

            auto size = std::filesystem::file_size(Path);
            // TODO: cringe
            ReservedCapacity(size > minimumReservedCapacity ? ((size / minimumReservedCapacity) + 1) * minimumReservedCapacity : minimumReservedCapacity);
            UsedCapacity(size);
        }

        public: void OnReservedCapacityChanged(capacity_t oldReservedCapacity, capacity_t newReservedCapacity) final
        {
            this->UnmapFile();
            std::filesystem::resize_file(Path, newReservedCapacity);
            this->MapFile(newReservedCapacity);
        }

        public: std::filesystem::path Path;
        public: explicit FileMappedResizableDirectMemory(const std::string& path) : FileMappedResizableDirectMemory(path, MinimumCapacity) { }

        private: void MapFile(capacity_t capacity)
        {
            _file.map(0, _file.file_size());
            Pointer() = static_cast<void*>(_file.data());
        }

        private: void UnmapFile()
        {
            _file.unmap();
            Pointer() = nullptr;
        }

        // TODO: maybe use rvalue friend function
        public: void Close()
        {
            _file.unmap();
            _file.close();
        }

        public: ~FileMappedResizableDirectMemory() final
        {
            Close();
        }
    };
}