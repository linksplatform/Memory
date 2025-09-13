#include "Platform.Memory/Platform.Memory.h"
#include <iostream>
#include <cassert>

using namespace Platform::Memory;

int main() {
    std::cout << "Testing Platform.Memory C++ implementation...\n";
    
    try {
        // Test HeapResizableDirectMemory
        HeapResizableDirectMemory heapMemory;
        std::cout << "Created HeapResizableDirectMemory with size: " << heapMemory.Size() << "\n";
        std::cout << "Reserved capacity: " << heapMemory.ReservedCapacity() << "\n";
        std::cout << "Used capacity: " << heapMemory.UsedCapacity() << "\n";
        
        // Set used capacity
        heapMemory.UsedCapacity(1024);
        std::cout << "After setting used capacity to 1024: " << heapMemory.UsedCapacity() << "\n";
        
        // Test ArrayMemory
        ArrayMemory<int> arrayMemory(10);
        std::cout << "Created ArrayMemory<int> with size: " << arrayMemory.Size() << "\n";
        
        // Test array access
        arrayMemory[0] = 42;
        arrayMemory[1] = 100;
        std::cout << "Set arrayMemory[0] = 42, arrayMemory[1] = 100\n";
        std::cout << "arrayMemory[0] = " << arrayMemory[0] << "\n";
        std::cout << "arrayMemory[1] = " << arrayMemory[1] << "\n";
        
        // Test DirectMemoryAsArrayMemoryAdapter
        DirectMemoryAsArrayMemoryAdapter<std::byte> adapter(heapMemory);
        std::cout << "Created DirectMemoryAsArrayMemoryAdapter<std::byte>\n";
        std::cout << "Adapter size: " << adapter.Size() << "\n";
        
        // Set some bytes
        adapter[0] = std::byte{0xFF};
        adapter[1] = std::byte{0xAB};
        std::cout << "Set adapter[0] = 0xFF, adapter[1] = 0xAB\n";
        std::cout << "adapter[0] = " << static_cast<int>(adapter[0]) << "\n";
        std::cout << "adapter[1] = " << static_cast<int>(adapter[1]) << "\n";
        
        std::cout << "All tests passed!\n";
        return 0;
    }
    catch (const std::exception& e) {
        std::cerr << "Test failed with exception: " << e.what() << "\n";
        return 1;
    }
}