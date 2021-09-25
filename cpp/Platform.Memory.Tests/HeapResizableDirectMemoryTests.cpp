namespace Platform::Memory::Tests
{
    std::byte GetLastByte(const HeapResizableDirectMemory& heapMemory)
    {
        auto pointer1 = heapMemory.Pointer();
        return *((std::byte*)pointer1 + heapMemory.ReservedCapacity() - 1);
    }

    std::byte GetLastByteByCopy(HeapResizableDirectMemory heapMemory)
    {
        auto pointer1 = heapMemory.Pointer();
        return *((std::byte*)pointer1 + heapMemory.ReservedCapacity() - 1);
    }

    TEST(HeapResizableDirectMemory, CorrectMemoryReallocation)
    {
        auto heapMemory = HeapResizableDirectMemory();
        auto value1 = GetLastByte(heapMemory);
        heapMemory.ReservedCapacity(heapMemory.ReservedCapacity() * 2);
        auto value2 = GetLastByteByCopy(heapMemory);
        ASSERT_EQ(value1, value2);
        ASSERT_EQ(std::byte{0}, value1);

        ASSERT_NE(&value1, &value2);
    }
}
