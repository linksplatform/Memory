namespace Platform::Memory
{
    template<typename T>
    struct PropertySetup
    {
        PropertySetup() = default;
        PropertySetup(const PropertySetup&) = delete;
        PropertySetup(PropertySetup&&) = delete;
        auto&& self() { return reinterpret_cast<T&>(*this); }
    };

    template <typename ...> class FileArrayMemory;
    template <typename TElement> class FileArrayMemory<TElement> final : public IArrayMemory<TElement>
        //where TElement : struct
    {
        using Self = FileArrayMemory<TElement>;
        private: std::shared_ptr<std::fstream> _file;
        private: std::filesystem::path path;

        public: std::size_t Size() final
        {
            return std::filesystem::file_size(path);
        }

        std::size_t current_index = 0;

        [[no_unique_address]] struct : PropertySetup<Self> {
            using PropertySetup<Self>::self;

            operator TElement()
            {
                self()._file->seekg(self().current_index);
                TElement element;
                self()._file->read(reinterpret_cast<char*>(&element), sizeof(TElement));
                return element;
            }

            auto& operator=(TElement value)
            {
                self()._file->seekg(self().current_index);
                self()._file->write(reinterpret_cast<char*>(&value), sizeof(TElement));
                return *this;
            }
        } _Index;

        auto&& operator[](std::size_t index)
        {
            current_index = index;
            return _Index;
        }

        //protected: override std::string ObjectName
        //{
        //    get => "File stored memory block at '{_file.Name}' path.";
        //}

        public: FileArrayMemory(auto&& file, auto&& path)
            : _file(std::forward<decltype(file)>(file)), path(std::forward<decltype(path)>(path)) { }

        public: explicit FileArrayMemory(auto&& path)
            : FileArrayMemory(std::make_shared<std::fstream>(path), path) { }

    };
}