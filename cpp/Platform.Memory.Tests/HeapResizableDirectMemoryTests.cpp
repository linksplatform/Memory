namespace Platform::Memory::Tests
{
    public unsafe TEST_CLASS(HeapResizableDirectMemoryTests)
    {
        public: TEST_METHOD(CorrectMemoryReallocationTest)
        {
            using auto heapMemory = HeapResizableDirectMemory();
            auto value1 = GetLastByte(heapMemory);
            heapMemory.ReservedCapacity *= 2;
            auto value2 = GetLastByte(heapMemory);
            Assert::AreEqual(value1, value2);
            Assert::AreEqual(0, value1);
        }

        private: static std::uint8_t GetLastByte(HeapResizableDirectMemory heapMemory)
        {
            auto pointer1 = (void*)heapMemory.Pointer;
            return *((std::uint8_t*)pointer1 + heapMemory.ReservedCapacity - 1);
        }
    };
}
