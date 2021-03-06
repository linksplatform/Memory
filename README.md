[![NuGet Version and Downloads count](https://buildstats.info/nuget/Platform.Memory)](https://www.nuget.org/packages/Platform.Memory)
[![Actions Status](https://github.com/linksplatform/Memory/workflows/CD/badge.svg)](https://github.com/linksplatform/Memory/actions?workflow=CD)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/9aaabf24441141439ea8419c71484feb)](https://app.codacy.com/app/drakonard/Memory?utm_source=github.com&utm_medium=referral&utm_content=linksplatform/Memory&utm_campaign=Badge_Grade_Dashboard)
[![CodeFactor](https://www.codefactor.io/repository/github/linksplatform/memory/badge)](https://www.codefactor.io/repository/github/linksplatform/memory)

# [Memory](https://github.com/linksplatform/Memory)

LinksPlatform's Platform.Memory Class Library contains classes for memory management simplification. Here you can find multiple implementations of [IMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.IMemory.html) interface.

The data can be accessed using [the raw pointer](https://linksplatform.github.io/Memory/api/Platform.Memory.IDirectMemory.html) or [by element's index](https://linksplatform.github.io/Memory/api/Platform.Memory.IArrayMemory-1.html) and can be stored in volatile memory:
* [HeapResizableDirect](https://linksplatform.github.io/Memory/api/Platform.Memory.HeapResizableDirectMemory.html),
* [ArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.ArrayMemory-1.html)

or in non-volatile memory:
* [FileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileMappedResizableDirectMemory.html),
* [TemporaryFileMappedResizableDirectMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.TemporaryFileMappedResizableDirectMemory.html),
* [FileArrayMemory](https://linksplatform.github.io/Memory/api/Platform.Memory.FileArrayMemory-1.html).

Namespace: [Platform.Memory](https://linksplatform.github.io/Memory/csharp/api/Platform.Memory.html)

Forked from: [Konard/LinksPlatform/Platform/Platform.Memory](https://github.com/Konard/LinksPlatform/tree/1af617ce19994e78e7ed5c980075c18f8f6cf7f9/Platform/Platform.Memory)

NuGet package: [Platform.Memory](https://www.nuget.org/packages/Platform.Memory)

## [Documentation](https://linksplatform.github.io/Memory)
[PDF file](https://linksplatform.github.io/Memory/csharp/Platform.Memory.pdf) with code for e-readers.

## Depend on
*   [System.IO.MemoryMappedFiles](https://www.nuget.org/packages/System.IO.MemoryMappedFiles)
*   [Platform.IO](https://github.com/linksplatform/IO)

## Dependent libraries
*   [Platform.Data.Doublets](https://github.com/linksplatform/Data.Doublets)
