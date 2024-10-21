//
// DynamicNativeLibrary.cs
// This file is part of Microsoft.WinAny.Helper library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2016 by Josip Habjan. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Parts of this source code are ported from C to C# by Josip Habjan
//
// The Original Code is MemoryModule.c 
// (https://github.com/fancycode/MemoryModule/blob/master/MemoryModule.c);
// and it's under Mozilla Public License Version 1.1 
// (http://www.mozilla.org/MPL/) 
// The Initial Developer of the Original Code is Joachim Bauch
// Copyright (C) 2004-2012 Joachim Bauch (mail@joachim-bauch.de). 

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WinAny.Interop
{
    /// <summary>
    /// Class that helps you to load and use native/unmanaged dynamic-link libraries dinamically.
    /// It provides ability to load libraries from the memory or disk. 
    /// It's compatibile with x86 and x64 libraries.
    /// </summary>
    public unsafe class DynamicNativeLibrary : IDisposable
    {

        #region Private variables

        private IntPtr  _loadedModuleHandle;
        private bool    _loadedFromMemory;
        private bool    _disposed = false;

        private uint[,,] _protectionFlags = new uint[2, 2, 2]
                    {
                        { /* not executable */ {WinNt.PageNoaccess, WinNt.PageWritecopy}, {WinNt.PageReadonly, WinNt.PageReadwrite}, }, 
                        { /* executable */ {WinNt.PageExecute, WinNt.PageExecuteWritecopy}, {WinNt.PageExecuteRead, WinNt.PageExecuteReadwrite}, },
                    };

        #endregion

        #region Constructor - fileName

        /// <summary>
        /// Initializes a new instance of the NativeLibrary class from a native module stored on disk.
        /// </summary>
        /// <param name="lpLibFileName">Native module file name.</param>
        public DynamicNativeLibrary(string fileName)
        {
            _loadedModuleHandle = WinBase.LoadLibrary(fileName);

            if (_loadedModuleHandle == IntPtr.Zero)
                throw new Exception("Module could not be loaded.");

            _loadedFromMemory = false;
        }

        #endregion

        #region Constructor - buffer

        /// <summary>
        /// Initializes a new instance of the NativeLibrary class from a native module byte array.
        /// </summary>
        /// <param name="buffer">Native module byte array.</param>
        public DynamicNativeLibrary(byte[] buffer)
        {
            _loadedModuleHandle = MemoryLoadLibrary(buffer);

            if (_loadedModuleHandle == IntPtr.Zero)
                throw new Exception("Module could not be loaded.");

            _loadedFromMemory = true;
        }

        #endregion

        #region Destructor

        ~DynamicNativeLibrary()
        {
            Dispose(false);
        }

        #endregion

        #region Dispose

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Dispose - disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free managed resources
                }

                if (_loadedModuleHandle != IntPtr.Zero)
                {
                    if (_loadedFromMemory)
                    {
                        this.MemoryFreeLibrary(_loadedModuleHandle);
                    }
                    else
                    {
                        WinBase.FreeLibrary(_loadedModuleHandle);
                    }

                    _loadedModuleHandle = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        #endregion

        #endregion

        #region MemoryLoadLibrary

        /// <summary>
        /// Loads the specified native module from a byte array into the address space of the calling process.
        /// </summary>
        /// <param name="data">Native module byte array.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.</returns>
        private IntPtr MemoryLoadLibrary(byte[] data)
        {
            fixed (byte* ptrData = data)
            {
                WinNt.ImageDosHeader* dosHeader = (WinNt.ImageDosHeader*)ptrData;

                if (dosHeader->e_magic != WinNt.ImageDosSignature)
                {
                    throw new NotSupportedException();
                }

                byte* ptrOldHeader;
                uint oldHeaderOhSizeOfImage;
                uint oldHeaderOhSizeOfHeaders;
                int imageNtHeadersSize;
                IntPtr oldHeaderOhImageBase;

                if (Environment.Is64BitProcess)
                {
                    WinNt.ImageNtHeaders64* oldHeader = (WinNt.ImageNtHeaders64*)(ptrData + dosHeader->e_lfanew);
                    if (oldHeader->Signature != WinNt.ImageNtSignature)
                    {
                        throw new NotSupportedException();
                    }

                    oldHeaderOhSizeOfImage = oldHeader->OptionalHeader.SizeOfImage;
                    oldHeaderOhSizeOfHeaders = oldHeader->OptionalHeader.SizeOfHeaders;
                    oldHeaderOhImageBase = oldHeader->OptionalHeader.ImageBase;
                    ptrOldHeader = (byte*)oldHeader;

                    imageNtHeadersSize = sizeof(WinNt.ImageNtHeaders64);
                }
                else
                {
                    WinNt.ImageNtHeaders32* oldHeader = (WinNt.ImageNtHeaders32*)(ptrData + dosHeader->e_lfanew);
                    if (oldHeader->Signature != WinNt.ImageNtSignature)
                    {
                        throw new NotSupportedException();
                    }

                    oldHeaderOhSizeOfImage = oldHeader->OptionalHeader.SizeOfImage;
                    oldHeaderOhSizeOfHeaders = oldHeader->OptionalHeader.SizeOfHeaders;
                    oldHeaderOhImageBase = oldHeader->OptionalHeader.ImageBase;
                    ptrOldHeader = (byte*)oldHeader;

                    imageNtHeadersSize = sizeof(WinNt.ImageNtHeaders32);
                }

                IntPtr codeBase = IntPtr.Zero;

                if (!Environment.Is64BitProcess)
                {
                    codeBase = WinBase.VirtualAlloc(oldHeaderOhImageBase, oldHeaderOhSizeOfImage, WinNt.MemReserve, WinNt.PageReadwrite);
                }

                if (codeBase == IntPtr.Zero)
                    codeBase = WinBase.VirtualAlloc(IntPtr.Zero, oldHeaderOhSizeOfImage, WinNt.MemReserve, WinNt.PageReadwrite);

                if (codeBase == IntPtr.Zero)
                    return IntPtr.Zero;

                MemoryModule* memoryModule = (MemoryModule*)Marshal.AllocHGlobal(sizeof(MemoryModule));
                memoryModule->codeBase = (byte*)codeBase;
                memoryModule->numModules = 0;
                memoryModule->modules = null;
                memoryModule->initialized = 0;

                WinBase.VirtualAlloc(codeBase, oldHeaderOhSizeOfImage, WinNt.MemCommit, WinNt.PageReadwrite);

                IntPtr headers = WinBase.VirtualAlloc(codeBase, oldHeaderOhSizeOfHeaders, WinNt.MemCommit, WinNt.PageReadwrite);


                // copy PE header to code
                Memory.Memcpy((byte*)headers, (byte*)dosHeader, dosHeader->e_lfanew + oldHeaderOhSizeOfHeaders);
               
                memoryModule->headers = &((byte*)(headers))[dosHeader->e_lfanew];

                if (Environment.Is64BitProcess)
                {
                    WinNt.ImageNtHeaders64* mmHeaders64 = (WinNt.ImageNtHeaders64*)(memoryModule->headers);
                    mmHeaders64->OptionalHeader.ImageBase = codeBase;
                }
                else
                {
                    WinNt.ImageNtHeaders32* mmHeaders32 = (WinNt.ImageNtHeaders32*)(memoryModule->headers);
                    mmHeaders32->OptionalHeader.ImageBase = codeBase;
                }

                this.CopySections(ptrData, ptrOldHeader, memoryModule);

                ulong locationDelta = (ulong)((ulong)codeBase - (ulong)oldHeaderOhImageBase);

                if (locationDelta != 0)
                {
                    this.PerformBaseRelocation(memoryModule, locationDelta);
                }

                if (!this.BuildImportTable(memoryModule))
                {
                    goto error;
                }

                this.FinalizeSections(memoryModule);

                if (!this.CallDllEntryPoint(memoryModule, WinNt.DllProcessAttach))
                {
                    goto error;
                }

                return (IntPtr)memoryModule;

            error:
                MemoryFreeLibrary((IntPtr)memoryModule);
                return IntPtr.Zero;
            }
        }

        #endregion

        #region CopySections

        /// <summary>
        /// Copies sections from a native module file block to the new memory location.
        /// </summary>
        /// <param name="ptr_data">Pointer to a native module byte array.</param>
        /// <param name="ptr_old_headers">Pointer to a source native module headers.</param>
        /// <param name="memory_module">Pointer to a memory module.</param>
        private void CopySections(byte* ptrData, byte* ptrOldHeaders, MemoryModule* memoryModule)
        {
            byte* codeBase = memoryModule->codeBase;
            WinNt.ImageSectionHeader* section = WinNt.IMAGE_FIRST_SECTION(memoryModule->headers);

            ushort numberOfSections;
            uint sectionAlignment;

            if (Environment.Is64BitProcess)
            {
                WinNt.ImageNtHeaders64* newHeaders = (WinNt.ImageNtHeaders64*)memoryModule->headers;
                numberOfSections = newHeaders->FileHeader.NumberOfSections;

                WinNt.ImageNtHeaders64* oldHeaders = (WinNt.ImageNtHeaders64*)ptrOldHeaders;
                sectionAlignment = oldHeaders->OptionalHeader.SectionAlignment;
            }
            else
            {
                WinNt.ImageNtHeaders32* newHeaders = (WinNt.ImageNtHeaders32*)memoryModule->headers;
                numberOfSections = newHeaders->FileHeader.NumberOfSections;

                WinNt.ImageNtHeaders32* oldHeaders = (WinNt.ImageNtHeaders32*)ptrOldHeaders;
                sectionAlignment = oldHeaders->OptionalHeader.SectionAlignment;
            }

            uint index;
            byte* dest;

            for (index = 0; index < numberOfSections; index++, section++)
            {
                if (section->SizeOfRawData == 0)
                {
                    if (sectionAlignment > 0)
                    {
                        dest = (byte*)WinBase.VirtualAlloc((IntPtr)(codeBase + section->VirtualAddress), sectionAlignment, WinNt.MemCommit, WinNt.PageReadwrite);
                        section->PhysicalAddress = (uint)dest;
                        Memory.Memset(dest, 0, sectionAlignment);
                    }

                    continue;
                }

                // commit memory block and copy data from dll
                dest = (byte*)WinBase.VirtualAlloc((IntPtr)(codeBase + section->VirtualAddress), section->SizeOfRawData, WinNt.MemCommit, WinNt.PageReadwrite);
                Memory.Memcpy(dest, ptrData + section->PointerToRawData, section->SizeOfRawData);

                section->PhysicalAddress = (uint)dest;
            }
        }

        #endregion

        #region PerformBaseRelocation

        /// <summary>
        /// Adjusts base address of the imported data.
        /// </summary>
        /// <param name="memory_module">Pointer to a memory module.</param>
        /// <param name="delta">Adjustment delta value.</param>
        private void PerformBaseRelocation(MemoryModule* memoryModule, ulong delta)
        {
            WinNt.ImageDataDirectory* directory = this.GET_HEADER_DIRECTORY(memoryModule, WinNt.ImageDirectoryEntryBasereloc);

            if (directory->Size > 0)
            {
                WinNt.ImageBaseRelocation* relocation = (WinNt.ImageBaseRelocation*)(memoryModule->codeBase + directory->VirtualAddress);

                int sizeOfBaseRelocation = sizeof(WinNt.ImageBaseRelocation);

                int index;

                for (; relocation->VirtualAddress > 0; )
                {
                    byte* dest = (byte*)(memoryModule->codeBase + relocation->VirtualAddress);
                    ushort* relInfo = (ushort*)((byte*)relocation + sizeOfBaseRelocation);

                    for (index = 0; index < ((relocation->SizeOfBlock - sizeOfBaseRelocation) / 2); index++, relInfo++)
                    {
                        uint* patchAddrHl32;
                        ulong* patchAddrHl64;

                        uint type, offset;

                        // the upper 4 bits define the type of relocation
                        type = (uint)(*relInfo >> 12);

                        // the lower 12 bits define the offset
                        offset = (uint)(*relInfo & 0xfff);

                        switch (type)
                        {
                            case WinNt.ImageRelBasedAbsolute:
                                break;

                            case WinNt.ImageRelBasedHighlow:
                                patchAddrHl32 = (uint*)((uint)dest + offset);
                                *patchAddrHl32 += (uint)delta;
                                break;


                            case WinNt.ImageRelBasedDir64:
                                patchAddrHl64 = (ulong*)((ulong)dest + offset);
                                *patchAddrHl64 += delta;
                                break;

                            default:
                                break;
                        }
                    }

                    relocation = (WinNt.ImageBaseRelocation*)((byte*)relocation + relocation->SizeOfBlock);
                }
            }
        }

        #endregion

        #region BuildImportTable

        /// <summary>
        /// Loads required dlls and adjust function table of the imports.
        /// </summary>
        /// <param name="memory_module">Pointer to a memory module.</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        private bool BuildImportTable(MemoryModule* memoryModule)
        {
            bool result = true;

            WinNt.ImageDataDirectory* directory = this.GET_HEADER_DIRECTORY(memoryModule, WinNt.ImageDirectoryEntryImport);

            if (directory->Size > 0)
            {
                WinNt.ImageImportDescriptor* importDesc = (WinNt.ImageImportDescriptor*)(memoryModule->codeBase + directory->VirtualAddress);

                for (; importDesc->Name != 0; importDesc++)
                {
                    IntPtr* thunkRef;
                    IntPtr* funcRef;

                    string moduleName = Marshal.PtrToStringAnsi((IntPtr)(memoryModule->codeBase + importDesc->Name));
                    IntPtr handle = WinBase.LoadLibrary(moduleName);

                    if (handle == IntPtr.Zero)
                    {
                        result = false;
                        break;
                    }

                    int sizeOfPointer = sizeof(IntPtr);

                    memoryModule->modules = (IntPtr*)Memory.Realloc((byte*)memoryModule->modules,
                                                                     (uint)((memoryModule->numModules) * sizeOfPointer),
                                                                     (uint)((memoryModule->numModules + 1) * sizeOfPointer));


                    if (memoryModule->modules == null)
                    {
                        result = false;
                        break;
                    }

                    memoryModule->modules[memoryModule->numModules++] = handle;

                    if (importDesc->Characteristics != 0)
                    {
                        thunkRef = (IntPtr*)(memoryModule->codeBase + importDesc->Characteristics);
                        funcRef = (IntPtr*)(memoryModule->codeBase + importDesc->FirstThunk);
                    }
                    else
                    {
                        thunkRef = (IntPtr*)(memoryModule->codeBase + importDesc->FirstThunk);
                        funcRef = (IntPtr*)(memoryModule->codeBase + importDesc->FirstThunk);
                    }

                    for (; *thunkRef != IntPtr.Zero; thunkRef++, funcRef++)
                    {
                        if (WinNt.IMAGE_SNAP_BY_ORDINAL(thunkRef))
                        {
                            *funcRef = WinBase.GetProcAddress(handle, (byte*)WinNt.IMAGE_ORDINAL(thunkRef));
                        }
                        else
                        {
                            WinNt.ImageImportByName* thunkData = (WinNt.ImageImportByName*)(memoryModule->codeBase + (ulong)*thunkRef);
                            //string procName = Marshal.PtrToStringAnsi((IntPtr)(byte*)(thunkData) + 2);
                            IntPtr a = (IntPtr)(byte*)(thunkData);
                            string procName = Marshal.PtrToStringAnsi(new IntPtr(a.ToInt64() + 2));
                            *funcRef = WinBase.GetProcAddress(handle, procName);
                        }

                        if (*funcRef == IntPtr.Zero)
                        {
                            result = false;
                            break;
                        }
                    }

                    if (!result)
                        break;
                }
            }

            return result;
        }

        #endregion

        #region FinalizeSections

        /// <summary>
        /// Marks memory pages depending on section headers and release sections that are marked as "discardable".
        /// </summary>
        /// <param name="memory_module">Pointer to a memory module.</param>
        private void FinalizeSections(MemoryModule* memoryModule)
        {
            WinNt.ImageSectionHeader* section = WinNt.IMAGE_FIRST_SECTION(memoryModule->headers); ;

            ushort numberOfSections;
            uint sizeOfInitializedData;
            uint sizeOfUninitializedData;

            long imageOffset = 0;

            if (Environment.Is64BitProcess)
            {
                WinNt.ImageNtHeaders64* headers = (WinNt.ImageNtHeaders64*)memoryModule->headers;
                numberOfSections = headers->FileHeader.NumberOfSections;
                sizeOfInitializedData = headers->OptionalHeader.SizeOfInitializedData;
                sizeOfUninitializedData = headers->OptionalHeader.SizeOfUninitializedData;

                imageOffset = (long)((ulong)headers->OptionalHeader.ImageBase & 0xffffffff00000000);
            }
            else
            {
                WinNt.ImageNtHeaders32* headers = (WinNt.ImageNtHeaders32*)memoryModule->headers;
                numberOfSections = headers->FileHeader.NumberOfSections;
                sizeOfInitializedData = headers->OptionalHeader.SizeOfInitializedData;
                sizeOfUninitializedData = headers->OptionalHeader.SizeOfUninitializedData;
            }

            for (int i = 0; i < numberOfSections; i++, section++)
            {
                uint protect, oldProtect, rawDataSize;
                uint executable = Convert.ToUInt32((section->Characteristics & WinNt.ImageScnMemExecute) != 0);
                uint readable = Convert.ToUInt32((section->Characteristics & WinNt.ImageScnMemRead) != 0);
                uint writeable = Convert.ToUInt32((section->Characteristics & WinNt.ImageScnMemWrite) != 0);

                if ((section->Characteristics & WinNt.ImageScnMemDiscardable) != 0)
                {
                    // section is not needed any more and can safely be freed
                    WinBase.VirtualFree((IntPtr)(void*)((long)section->PhysicalAddress | (long)imageOffset), section->SizeOfRawData, WinNt.MemDecommit);
                    continue;
                }

                protect = _protectionFlags[executable, readable, writeable];

                if ((section->Characteristics & WinNt.ImageScnMemNotCached) != 0)
                    protect |= WinNt.PageNocache;

                // determine size of region
                rawDataSize = section->SizeOfRawData;

                if (rawDataSize == 0)
                {
                    if ((section->Characteristics & WinNt.ImageScnCntInitializedData) != 0)
                        rawDataSize = sizeOfInitializedData;

                    else if ((section->Characteristics & WinNt.ImageScnCntUninitializedData) != 0)
                        rawDataSize = sizeOfUninitializedData;
                }

                if (rawDataSize > 0)
                {
                    // change memory access flags
                    WinBase.VirtualProtect((IntPtr)(void*)((long)section->PhysicalAddress | (long)imageOffset), rawDataSize, protect, &oldProtect);
                }
            }
        }

        #endregion

        #region CallDllEntryPoint

        /// <summary>
        /// Calls module entry point.
        /// </summary>
        /// <param name="memory_module">Pointer to a memory module.</param>
        /// <param name="fdwReason"></param>
        /// <returns>If the function succeeds or if there is no entry point, the return value is true.</returns>
        private bool CallDllEntryPoint(MemoryModule* memoryModule, uint fdwReason)
        {
            uint addressOfEntryPoint;

            if (Environment.Is64BitProcess)
            {
                WinNt.ImageNtHeaders64* headers = (WinNt.ImageNtHeaders64*)memoryModule->headers;
                addressOfEntryPoint = headers->OptionalHeader.AddressOfEntryPoint;
            }
            else
            {
                WinNt.ImageNtHeaders32* headers = (WinNt.ImageNtHeaders32*)memoryModule->headers;
                addressOfEntryPoint = headers->OptionalHeader.AddressOfEntryPoint;
            }

            if (addressOfEntryPoint != 0)
            {
                IntPtr dllEntry = (IntPtr)(memoryModule->codeBase + addressOfEntryPoint);

                if (dllEntry == IntPtr.Zero)
                {
                    return false;
                }

                DllEntryProc dllEntryProc = (DllEntryProc)Marshal.GetDelegateForFunctionPointer(dllEntry, typeof(DllEntryProc));

                if (dllEntryProc((IntPtr)memoryModule->codeBase, fdwReason, 0))
                {
                    if (fdwReason == WinNt.DllProcessAttach)
                    {
                        memoryModule->initialized = 1;
                    }
                    else if (fdwReason == WinNt.DllProcessDetach)
                    {
                        memoryModule->initialized = 0;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region MemoryFreeLibrary

        /// <summary>
        /// Deattach from the process and do a cleanup.
        /// </summary>
        /// <param name="hModule">Pointer to a memory module.</param>
        private void MemoryFreeLibrary(IntPtr hModule)
        {
            if (hModule == IntPtr.Zero)
                return;

            MemoryModule* memoryModule = (MemoryModule*)hModule;

            if (memoryModule != null)
            {
                if (memoryModule->initialized != 0)
                {
                    this.CallDllEntryPoint(memoryModule, WinNt.DllProcessDetach);
                }

                if (memoryModule->modules != null)
                {
                    // free previously opened libraries
                    for (int index = 0; index < memoryModule->numModules; index++)
                    {
                        if (memoryModule->modules[index] != IntPtr.Zero)
                        {
                            WinBase.FreeLibrary(memoryModule->modules[index]);
                        }
                    }

                    Marshal.FreeHGlobal((IntPtr)memoryModule->modules);
                }

                if ((IntPtr)memoryModule->codeBase != IntPtr.Zero)
                {
                    // release memory of library
                    WinBase.VirtualFree((IntPtr)memoryModule->codeBase, 0, WinNt.MemRelease);
                }

                Marshal.FreeHGlobal((IntPtr)memoryModule);
            }
        }

        #endregion

        #region GetDelegateForFunction

        /// <summary>
        /// Retrieves a delegate of an exported function or variable from loaded module.
        /// </summary>
        /// <param name="procName">The function or variable name.</param>
        /// <param name="delegateType">The type of the delegate to be returned.</param>
        /// <returns>A function instance.</returns>
        public Delegate GetDelegateForFunction(string procName, Type delegateType)
        {
            IntPtr procAddress = this.GetProcAddress(procName);

            if (procAddress != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer(procAddress, delegateType);
            }

            return null;
        }

        #endregion

        #region GetDelegateForFunction


        /// <summary>
        /// Retrieves a delegate of an exported function or variable from loaded module.
        /// </summary>
        /// <typeparam name="T">Delegate type.</typeparam>
        /// <param name="procName">The function or variable name.</param>
        /// <returns>A function instance.</returns>
        public T GetDelegateForFunction<T>(string procName)
        {
            return (T)(object)GetDelegateForFunction(procName, typeof(T));
        }

        #endregion

        #region GetProcAddress

        /// <summary>
        /// Retrieves the address of an exported function or variable from loaded module.
        /// </summary>
        /// <param name="procName">The function or variable name.</param>
        /// <returns>
        /// If the function succeeds, the return value is the address of the exported function or variable.
        /// If the function fails, the return value is IntPtr.Zero.
        /// </returns>
        private IntPtr GetProcAddress(string procName)
        {
            if (_loadedModuleHandle == IntPtr.Zero)
                return IntPtr.Zero;

            if (!_loadedFromMemory)
            {
                return WinBase.GetProcAddress(_loadedModuleHandle, procName);
            }

            MemoryModule* memoryModule = (MemoryModule*)_loadedModuleHandle;

            byte* codeBase = memoryModule->codeBase;

            int idx = -1;
            uint i;

            uint* nameRef;
            ushort* ordinal;

            
            WinNt.ImageDataDirectory* directory = this.GET_HEADER_DIRECTORY(memoryModule, WinNt.ImageDirectoryEntryExport);

            if (directory->Size == 0)
                // no export table found
                return IntPtr.Zero;

            WinNt.ImageExportDirectory* exports = (WinNt.ImageExportDirectory*)(codeBase + directory->VirtualAddress);

            if (exports->NumberOfNames == 0 || exports->NumberOfFunctions == 0)
                // DLL doesn't export anything
                return IntPtr.Zero;

            // search function name in list of exported names
            nameRef = (uint*)(codeBase + exports->AddressOfNames);
            ordinal = (ushort*)(codeBase + exports->AddressOfNameOrdinals);

            for (i = 0; i < exports->NumberOfNames; i++, nameRef++, ordinal++)
            {
                IntPtr procNameHandle = (IntPtr)((byte*)((ulong)codeBase + *nameRef));
                string testProcName = Marshal.PtrToStringAnsi(procNameHandle);

                if (testProcName == procName)
                {
                    idx = *ordinal;
                    break;
                }
            }

            if (idx == -1)
                // exported symbol not found
                return IntPtr.Zero;

            if ((uint)idx > exports->NumberOfFunctions)
                // name <-> ordinal number don't match
                return IntPtr.Zero;

            // AddressOfFunctions contains the RVAs to the "real" functions
            //return (IntPtr)((uint)codeBase + *(uint*)((uint)codeBase + exports->AddressOfFunctions + (idx * 4)));
            return (IntPtr)(codeBase + *(uint*)(codeBase + exports->AddressOfFunctions + (idx * 4)));
        }

        #endregion

        #region GET_HEADER_DIRECTORY

        private WinNt.ImageDataDirectory* GET_HEADER_DIRECTORY(MemoryModule* memoryModule, uint index)
        {
            if (Environment.Is64BitProcess)
            {
                WinNt.ImageNtHeaders64* headers = (WinNt.ImageNtHeaders64*)memoryModule->headers;
                return (WinNt.ImageDataDirectory*)(&headers->OptionalHeader.DataDirectory[index]);
            }
            else
            {
                WinNt.ImageNtHeaders32* headers = (WinNt.ImageNtHeaders32*)memoryModule->headers;
                return (WinNt.ImageDataDirectory*)(&headers->OptionalHeader.DataDirectory[index]);
            }
        }

        #endregion

        #region MEMORY_MODULE

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MemoryModule
        {
            public byte* headers;
            public byte* codeBase;
            public IntPtr* modules;
            public int numModules;
            public int initialized;
        }

        #endregion

        #region DllEntryProc

        private delegate bool DllEntryProc(IntPtr hinstDll, uint fdwReason, uint lpReserved);

        #endregion

    }
}
