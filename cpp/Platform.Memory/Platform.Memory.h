#pragma once

#include <cstddef>
#include <cstdlib>
#include <vector>
#include <ranges>
#include <algorithm>
#include <execution>
#include <limits>
#include <memory>
#include <stdexcept>

#ifdef WIN32
    #include <windows.h>
    #include <sysinfoapi.h>
#else
    #include <unistd.h>
#endif

// Minimal Range and Exception handling for compilation
namespace Platform::Ranges {
    template<typename T>
    struct Range {
        T min, max;
        Range(T min_val, T max_val) : min(min_val), max(max_val) {}
    };
    
    namespace Ensure::Always {
        template<typename T>
        void ArgumentInRange(T value, const Range<T>& range) {
            if (value < range.min || value > range.max) {
                throw std::out_of_range("Argument out of range");
            }
        }
    }
}

// Include core memory interfaces and implementations
#include "IMemory.h"
#include "IDirectMemory.h"
#include "IArrayMemory.h"
#include "ArrayMemory.h"

#include "IResizableDirectMemory.h"
#include "ResizableDirectMemoryBase.h"
#include "HeapResizableDirectMemory.h"

#include "DirectMemoryAsArrayMemoryAdapter.h"

// File-based implementations require external dependencies - included separately
// #include "memory_mapped_file.hpp"
// #include "memory_mapped_file.cpp"
// #include "FileArrayMemory.h" 
// #include "FileMappedResizableDirectMemory.h"
// #include "TemporaryFileMappedResizableDirectMemory.h"
