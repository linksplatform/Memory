#pragma once

#include <Platform.Collections.h>

#ifdef WIN32
    #include <windows.h>
    #include <sysinfoapi.h>
#endif
#include <execution>
#include <fstream>
#include <filesystem>
#include <gsl/gsl>

#include "memory_mapped_file.hpp"
#include "memory_mapped_file.cpp"

#include "IMemory.h"
#include "IDirectMemory.h"
#include "IArrayMemory.h"
#include "ArrayMemory.h"
#include "FileArrayMemory.h"

#include "IResizableDirectMemory.h"
#include "ResizableDirectMemoryBase.h"
#include "HeapResizableDirectMemory.h"

#include "FileMappedResizableDirectMemory.h"
#include "TemporaryFileMappedResizableDirectMemory.h"
#include "DirectMemoryAsArrayMemoryAdapter.h"
