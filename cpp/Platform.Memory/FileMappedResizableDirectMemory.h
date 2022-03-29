namespace Platform::Memory
{
    class FileMappedResizableDirectMemory final : public ResizableDirectMemoryBase
    {
      using base = ResizableDirectMemoryBase;
        protected: using base::capacity_t;

        private: mio::mmap_sink _file;

        //protected: override std::string ObjectName
        //{
        //    get => std::string("File stored memory block at '").append(Path).append("' path.");
        //}

        public:
          FileMappedResizableDirectMemory (const FileMappedResizableDirectMemory&) = delete;
          FileMappedResizableDirectMemory& operator= (const FileMappedResizableDirectMemory&) = delete;
          FileMappedResizableDirectMemory (FileMappedResizableDirectMemory&&) = default;

        public: FileMappedResizableDirectMemory(const std::string& path, capacity_t minimumReservedCapacity)
        {
            using namespace Platform::Collections;
            Expects(!IsWhiteSpace(path));
            if(!std::filesystem::exists(path))
            {
              std::ofstream file {path};
              file.close();
            }
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
            if(Pointer() != nullptr)
            {
                return;
            }
            _file = mio::mmap_sink {Path.c_str()};
            Pointer() = _file.data();
        }

        private: void UnmapFile()
        {
            _file.unmap();
            Pointer() = nullptr;
        }

        // TODO: maybe use rvalue friend function
        public: void Close()
        {

        }

        public: ~FileMappedResizableDirectMemory() final
        {
            Close();
        }
    };
}
