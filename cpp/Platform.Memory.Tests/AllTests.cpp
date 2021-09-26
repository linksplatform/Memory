#include <gtest/gtest.h>
#include <Platform.Memory.h>

//#include "HeapResizableDirectMemoryTests.cpp"

using namespace Platform::Memory;

auto main() -> int {
    { std::ofstream{"db.links"}; }

    FileArrayMemory<int> a("db.links");

    auto size = sizeof(int);

    a[0*size] = 1337;
    a[1*size] = 228;
    a[2*size] = 177013;
    std::cout << a[0*size] << "\n";
    std::cout << a[1*size] << "\n";
    std::cout << a[2*size] << "\n";
}