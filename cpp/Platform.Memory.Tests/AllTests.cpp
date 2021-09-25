#include <gtest/gtest.h>
#include <Platform.Memory.h>

#include "HeapResizableDirectMemoryTests.cpp"

auto main_() -> int {
    std::uint8_t bytes[1024];
    for (int i = 0; i < std::size(bytes); i++) {
        bytes[i] = i;
    }

    Platform::Memory::Internal::ZeroBlock(bytes, std::size(bytes));

    for (auto byte : bytes) {
        std::cout << (int)byte;
    }
}