namespace Platform::Memory
{
    template <typename ...> class FileArrayMemory;
    template <typename TElement> class FileArrayMemory<TElement> : public DisposableBase, IArrayMemory<TElement>
        where TElement : struct
    {
        private: FileStream _file = 0;

        public: std::int64_t Size()
        {
            return _file.Length;
        }

        public: TElement this[std::int64_t index]
        {
            get
            {
                _file.Seek(Structure<TElement>.Size * index, SeekOrigin.Begin);
                return _file.ReadOrDefault<TElement>();
            }
            set
            {
                _file.Seek(Structure<TElement>.Size * index, SeekOrigin.Begin);
                _file.Write(value);
            }
        }

        protected: override std::string ObjectName
        {
            get => "File stored memory block at '{_file.Name}' path.";
        }

        public: FileArrayMemory(FileStream file) { _file = file; }

        public: FileArrayMemory(std::string path) : this(File.Open(path, FileMode.OpenOrCreate)) { }

        protected: void Dispose(bool manual, bool wasDisposed) override
        {
            if (!wasDisposed)
            {
                _file.DisposeIfPossible();
            }
        }
    }
}