#ifndef PLATFORM_MEMORY
#define PLATFORM_MEMORY

#include <Platform.Collections.h>

#ifdef WIN32
    #include <sysinfoapi.h>
#endif
#include <execution>
#include <fstream>
#include <filesystem>

#include "FileStream.h"

#include "IMemory.h"
#include "IDirectMemory.h"
#include "IArrayMemory.h"
#include "ArrayMemory.h"
#include "FileArrayMemory.h"

#include "IResizableDirectMemory.h"
#include "ResizableDirectMemoryBase.h"
#include "HeapResizableDirectMemory.h"

#include "DirectMemoryAsArrayMemoryAdapter.h"

#endif //PLATFORM_MEMORY
