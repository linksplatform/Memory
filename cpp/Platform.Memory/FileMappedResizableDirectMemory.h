namespace Platform::Memory
{
    public unsafe class FileMappedResizableDirectMemory : public ResizableDirectMemoryBase
    {
        private: MemoryMappedFile _file = 0;
        private: MemoryMappedViewAccessor _accessor = 0;

        protected: std::string Path = 0;

        protected: override std::string ObjectName
        {
            get => std::string("File stored memory block at '").append(Path).append("' path.");
        }

        public: FileMappedResizableDirectMemory(std::string path, std::int64_t minimumReservedCapacity)
        {
            Ensure.Always.ArgumentNotEmptyAndNotWhiteSpace(path, "path");
            if (minimumReservedCapacity < MinimumCapacity)
            {
                minimumReservedCapacity = MinimumCapacity;
            }
            Path = path;
            auto size = FileHelpers.GetSize(path);
            ReservedCapacity = size > minimumReservedCapacity ? ((size / minimumReservedCapacity) + 1) * minimumReservedCapacity : minimumReservedCapacity;
            UsedCapacity = size;
        }

        public: FileMappedResizableDirectMemory(std::string path) : this(path, MinimumCapacity) { }

        private: void MapFile(std::int64_t capacity)
        {
            if (Pointer != IntPtr.0)
            {
                return;
            }
            _file = MemoryMappedFile.CreateFromFile(Path, FileMode.OpenOrCreate, mapName: {}, capacity, MemoryMappedFileAccess.ReadWrite);
            _accessor = _file.CreateViewAccessor();
            std::uint8_t* pointer = {};
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(pointer);
            Pointer = this->IntPtr(pointer);
        }

        private: void UnmapFile()
        {
            if (this->UnmapFile(Pointer))
            {
                Pointer = IntPtr.0;
            }
        }

        private: bool UnmapFile(IntPtr pointer)
        {
            if (pointer == IntPtr.0)
            {
                return false;
            }
            if (_accessor != nullptr)
            {
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                Disposable.TryDisposeAndResetToDefault(ref _accessor);
            }
            Disposable.TryDisposeAndResetToDefault(ref _file);
            return true;
        }

        protected: void OnReservedCapacityChanged(std::int64_t oldReservedCapacity, std::int64_t newReservedCapacity) override
        {
            this->UnmapFile();
            FileHelpers.SetSize(Path, newReservedCapacity);
            this->MapFile(newReservedCapacity);
        }

        protected: void DisposePointer(IntPtr pointer, std::int64_t usedCapacity) override
        {
            if (this->UnmapFile(pointer))
            {
                FileHelpers.SetSize(Path, usedCapacity);
            }
        }
    };
}