#include <gtest/gtest.h>
#include <Platform.Memory.h>

//#include "HeapResizableDirectMemoryTests.cpp"

using namespace Platform::Memory;


auto main() -> int {
//
    //FileArrayMemory<int> a("db.links");
//
    //auto size = sizeof(int);
//
    //a[0*size] = 1337;
    //a[1*size] = 228;
    //a[2*size] = 177013;
    //std::cout << a[0*size] << "\n";
    //std::cout << a[1*size] << "\n";
    //std::cout << a[2*size] << "\n";

    //HeapResizableDirectMemory a{};

    //{ std::ofstream{"db.txt"}; }

    //TemporaryFileMappedResizableDirectMemory a { 4096 };
    //{ std::ofstream{"лооооол.txt"}; }
    {
        std::ofstream f{ "ll.txt" };
        f.flush();
        f.close();
    }

    FileMappedResizableDirectMemory a { "ll.txt", 4096 };
    DirectMemoryAsArrayMemoryAdapter<int> b(a);

    auto size = sizeof(int);

    b[0*size] = 1337;
    a.Flush();
    b[1*size] = 228;
    a.Flush();
    b[2*size] = 177013;
    a.Flush();
    std::cout << b[0*size] << "\n";
    std::cout << b[1*size] << "\n";
    std::cout << b[2*size] << "\n";
}