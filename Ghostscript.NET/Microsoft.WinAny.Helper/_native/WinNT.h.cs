//
// WinNT.h.cs
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

using System.Runtime.InteropServices;

namespace Ghostscript.NET.Microsoft.WinAny.Helper._native;

internal static unsafe class WinNt
{
    #region Helpers

    #region IMAGE_FIRST_SECTION

    public static ImageSectionHeader* IMAGE_FIRST_SECTION(byte* ptrImageNtHeaders)
    {
        if (Environment.Is64BitProcess)
        {
            ImageNtHeaders64* imageNtHeaders = (ImageNtHeaders64*)ptrImageNtHeaders;
            return (ImageSectionHeader*)((long)imageNtHeaders +
                                         (long)Marshal.OffsetOf(typeof(ImageNtHeaders64), "OptionalHeader") +
                                         imageNtHeaders->FileHeader.SizeOfOptionalHeader);
        }
        else
        {
            ImageNtHeaders32* imageNtHeaders = (ImageNtHeaders32*)ptrImageNtHeaders;
            return (ImageSectionHeader*)((long)imageNtHeaders +
                                         (long)Marshal.OffsetOf(typeof(ImageNtHeaders32), "OptionalHeader") +
                                         imageNtHeaders->FileHeader.SizeOfOptionalHeader);
        }
    }

    #endregion

    #region IMAGE_SNAP_BY_ORDINAL

    public static bool IMAGE_SNAP_BY_ORDINAL(IntPtr* ordinal)
    {
        if (Environment.Is64BitProcess)
            return ((ulong)*ordinal & ImageOrdinalFlag64) != 0;
        return ((uint)*ordinal & ImageOrdinalFlag32) != 0;
    }

    #endregion

    #region IMAGE_ORDINAL

    public static ulong IMAGE_ORDINAL(IntPtr* ordinal)
    {
        return (ulong)*ordinal & 0xffff;
    }

    #endregion

    #endregion

    #region Constants

    public const uint ImageDosSignature = 0x5A4D; // MZ
    public const uint ImageOs2Signature = 0x454E; // NE
    public const uint ImageOs2SignatureLe = 0x454C; // LE
    public const uint ImageVxdSignature = 0x454C; // LE
    public const uint ImageNtSignature = 0x00004550; // PE00

    public const int ImageSizeofShortName = 8;

    public const int ImageNumberofDirectoryEntries = 16;

    public const ulong ImageOrdinalFlag64 = 0x8000000000000000;
    public const uint ImageOrdinalFlag32 = 0x80000000;

    public const uint ImageScnTypeNoPad = 0x00000008; // Reserved.

    public const uint ImageScnCntCode = 0x00000020; // Section contains code.
    public const uint ImageScnCntInitializedData = 0x00000040; // Section contains initialized data.
    public const uint ImageScnCntUninitializedData = 0x00000080; // Section contains uninitialized data.

    public const uint ImageScnLnkOther = 0x00000100; // Reserved.
    public const uint ImageScnLnkInfo = 0x00000200; // Section contains comments or some other type of information.

    public const uint ImageScnLnkRemove = 0x00000800; // Section contents will not become part of image.
    public const uint ImageScnLnkComdat = 0x00001000; // Section contents comdat.

    public const uint ImageScnNoDeferSpecExc = 0x00004000; // Reset speculative exceptions handling bits in the TLB entries for this section.
    public const uint ImageScnGprel = 0x00008000; // Section content can be accessed relative to GP
    public const uint ImageScnMemFardata = 0x00008000;

    public const uint ImageScnMemPurgeable = 0x00020000;
    public const uint ImageScnMem16Bit = 0x00020000;
    public const uint ImageScnMemLocked = 0x00040000;
    public const uint ImageScnMemPreload = 0x00080000;

    public const uint ImageScnAlign1Bytes = 0x00100000; //
    public const uint ImageScnAlign2Bytes = 0x00200000; //
    public const uint ImageScnAlign4Bytes = 0x00300000; //
    public const uint ImageScnAlign8Bytes = 0x00400000; //
    public const uint ImageScnAlign16Bytes = 0x00500000; // Default alignment if no others are specified.
    public const uint ImageScnAlign32Bytes = 0x00600000; //
    public const uint ImageScnAlign64Bytes = 0x00700000; //
    public const uint ImageScnAlign128Bytes = 0x00800000; //
    public const uint ImageScnAlign256Bytes = 0x00900000; //
    public const uint ImageScnAlign512Bytes = 0x00A00000; //
    public const uint ImageScnAlign1024Bytes = 0x00B00000; //
    public const uint ImageScnAlign2048Bytes = 0x00C00000; //
    public const uint ImageScnAlign4096Bytes = 0x00D00000; //

    public const uint ImageScnAlign8192Bytes = 0x00E00000; //

    // Unused                                    0x00F00000;
    public const uint ImageScnAlignMask = 0x00F00000;

    public const uint ImageScnLnkNrelocOvfl = 0x01000000; // Section contains extended relocations.
    public const uint ImageScnMemDiscardable = 0x02000000; // Section can be discarded.
    public const uint ImageScnMemNotCached = 0x04000000; // Section is not cachable.
    public const uint ImageScnMemNotPaged = 0x08000000; // Section is not pageable.
    public const uint ImageScnMemShared = 0x10000000; // Section is shareable.
    public const uint ImageScnMemExecute = 0x20000000; // Section is executable.
    public const uint ImageScnMemRead = 0x40000000; // Section is readable.
    public const uint ImageScnMemWrite = 0x80000000; // Section is writeable.

    public const uint PageNoaccess = 0x01;
    public const uint PageReadonly = 0x02;
    public const uint PageReadwrite = 0x04;
    public const uint PageWritecopy = 0x08;
    public const uint PageExecute = 0x10;
    public const uint PageExecuteRead = 0x20;
    public const uint PageExecuteReadwrite = 0x40;
    public const uint PageExecuteWritecopy = 0x80;
    public const uint PageGuard = 0x100;
    public const uint PageNocache = 0x200;
    public const uint PageWritecombine = 0x400;

    public const uint MemCommit = 0x1000;
    public const uint MemReserve = 0x2000;
    public const uint MemDecommit = 0x4000;
    public const uint MemRelease = 0x8000;
    public const uint MemFree = 0x10000;
    public const uint MemPrivate = 0x20000;
    public const uint MemMapped = 0x40000;
    public const uint MemReset = 0x80000;
    public const uint MemTopDown = 0x100000;
    public const uint MemWriteWatch = 0x200000;
    public const uint MemPhysical = 0x400000;
    public const uint MemRotate = 0x800000;
    public const uint MemLargePages = 0x20000000;
    public const uint Mem4MbPages = 0x80000000;
    public const uint MemImage = SecImage;

    public const uint SecFile = 0x800000;
    public const uint SecImage = 0x1000000;
    public const uint SecProtectedImage = 0x2000000;
    public const uint SecReserve = 0x4000000;
    public const uint SecCommit = 0x8000000;
    public const uint SecNocache = 0x10000000;
    public const uint SecWritecombine = 0x40000000;
    public const uint SecLargePages = 0x80000000;

    public const int WriteWatchFlagReset = 0x01;

    // Directory Entries

    public const int ImageDirectoryEntryExport = 0; // Export Directory
    public const int ImageDirectoryEntryImport = 1; // Import Directory
    public const int ImageDirectoryEntryResource = 2; // Resource Directory
    public const int ImageDirectoryEntryException = 3; // Exception Directory
    public const int ImageDirectoryEntrySecurity = 4; // Security Directory
    public const int ImageDirectoryEntryBasereloc = 5; // Base Relocation Table
    public const int ImageDirectoryEntryDebug = 6; // Debug Directory
    public const int ImageDirectoryEntryArchitecture = 7; // Architecture Specific Data
    public const int ImageDirectoryEntryGlobalptr = 8; // RVA of GP
    public const int ImageDirectoryEntryTls = 9; // TLS Directory
    public const int ImageDirectoryEntryLoadConfig = 10; // Load Configuration Directory
    public const int ImageDirectoryEntryBoundImport = 11; // Bound Import Directory in headers
    public const int ImageDirectoryEntryIat = 12; // Import Address Table
    public const int ImageDirectoryEntryDelayImport = 13; // Delay Load Import Descriptors
    public const int ImageDirectoryEntryComDescriptor = 14; // COM Runtime descriptor

    public const int ImageRelBasedAbsolute = 0;
    public const int ImageRelBasedHigh = 1;
    public const int ImageRelBasedLow = 2;
    public const int ImageRelBasedHighlow = 3;
    public const int ImageRelBasedHighadj = 4;
    public const int ImageRelBasedMipsJmpaddr = 5;
    public const int ImageRelBasedMipsJmpaddr16 = 9;
    public const int ImageRelBasedIa64Imm64 = 9;
    public const int ImageRelBasedDir64 = 10;


    public const uint DllProcessAttach = 1;
    public const uint DllThreadAttach = 2;
    public const uint DllThreadDetach = 3;
    public const uint DllProcessDetach = 0;

    /* These are the settings of the Machine field. */
    public const ushort ImageFileMachineUnknown = 0;
    public const ushort ImageFileMachineI860 = 0x014d;
    public const ushort ImageFileMachineI386 = 0x014c;
    public const ushort ImageFileMachineR3000 = 0x0162;
    public const ushort ImageFileMachineR4000 = 0x0166;
    public const ushort ImageFileMachineR10000 = 0x0168;
    public const ushort ImageFileMachineWcemipsv2 = 0x0169;
    public const ushort ImageFileMachineAlpha = 0x0184;
    public const ushort ImageFileMachineSh3 = 0x01a2;
    public const ushort ImageFileMachineSh3Dsp = 0x01a3;
    public const ushort ImageFileMachineSh3E = 0x01a4;
    public const ushort ImageFileMachineSh4 = 0x01a6;
    public const ushort ImageFileMachineSh5 = 0x01a8;
    public const ushort ImageFileMachineArm = 0x01c0;
    public const ushort ImageFileMachineThumb = 0x01c2;
    public const ushort ImageFileMachineArmnt = 0x01c4;
    public const ushort ImageFileMachineArm64 = 0xaa64;
    public const ushort ImageFileMachineAm33 = 0x01d3;
    public const ushort ImageFileMachinePowerpc = 0x01f0;
    public const ushort ImageFileMachinePowerpcfp = 0x01f1;
    public const ushort ImageFileMachineIa64 = 0x0200;
    public const ushort ImageFileMachineMips16 = 0x0266;
    public const ushort ImageFileMachineAlpha64 = 0x0284;
    public const ushort ImageFileMachineMipsfpu = 0x0366;
    public const ushort ImageFileMachineMipsfpu16 = 0x0466;
    public const ushort ImageFileMachineAxp64 = ImageFileMachineAlpha64;
    public const ushort ImageFileMachineTricore = 0x0520;
    public const ushort ImageFileMachineCef = 0x0cef;
    public const ushort ImageFileMachineEbc = 0x0ebc;
    public const ushort ImageFileMachineAmd64 = 0x8664;
    public const ushort ImageFileMachineM32R = 0x9041;
    public const ushort ImageFileMachineCee = 0xc0ee;

    #endregion

    #region Structures

    #region IMAGE_DOS_HEADER

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageDosHeader // DOS .EXE header
    {
        public ushort e_magic; // Magic number
        public ushort e_cblp; // Bytes on last page of file
        public ushort e_cp; // Pages in file
        public ushort e_crlc; // Relocations
        public ushort e_cparhdr; // Size of header in paragraphs
        public ushort e_minalloc; // Minimum extra paragraphs needed
        public ushort e_maxalloc; // Maximum extra paragraphs needed
        public ushort e_ss; // Initial (relative) SS value
        public ushort e_sp; // Initial SP value
        public ushort e_csum; // Checksum
        public ushort e_ip; // Initial IP value
        public ushort e_cs; // Initial (relative) CS value
        public ushort e_lfarlc; // File address of relocation table
        public ushort e_ovno; // Overlay number
        public fixed ushort e_res[4]; // Reserved ushorts
        public ushort e_oemid; // OEM identifier (for e_oeminfo)
        public ushort e_oeminfo; // OEM information; e_oemid specific
        public fixed ushort e_res2[10]; // Reserved ushorts
        public uint e_lfanew; // File address of new exe header
    }

    #endregion

    #region IMAGE_NT_HEADERS32

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageNtHeaders32
    {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader32 OptionalHeader;
    }

    #endregion

    #region IMAGE_NT_HEADERS64

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageNtHeaders64
    {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader64 OptionalHeader;
    }

    #endregion

    #region IMAGE_FILE_HEADER

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageFileHeader
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    #endregion

    #region IMAGE_OPTIONAL_HEADER32

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageOptionalHeader32
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public IntPtr ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public fixed ulong DataDirectory[ImageNumberofDirectoryEntries];
    }

    #endregion

    #region IMAGE_OPTIONAL_HEADER64

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageOptionalHeader64
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public IntPtr ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public fixed ulong DataDirectory[ImageNumberofDirectoryEntries];
    }

    #endregion

    #region IMAGE_DATA_DIRECTORY

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageDataDirectory
    {
        public uint VirtualAddress;
        public uint Size;
    }

    #endregion

    #region IMAGE_SECTION_HEADER

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageSectionHeader
    {
        public fixed byte Name[ImageSizeofShortName];
        public uint PhysicalAddress;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }

    #endregion

    #region IMAGE_BASE_RELOCATION

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageBaseRelocation
    {
        public uint VirtualAddress;
        public uint SizeOfBlock;
    }

    #endregion

    #region IMAGE_IMPORT_DESCRIPTOR

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageImportDescriptor
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunk;
    }

    #endregion

    #region IMAGE_IMPORT_BY_NAME

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageImportByName
    {
        public ushort Hint;
        public fixed byte Name[1];
    }

    #endregion

    #region IMAGE_EXPORT_DIRECTORY

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageExportDirectory
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Name;
        public uint Base;
        public uint NumberOfFunctions;
        public uint NumberOfNames;
        public uint AddressOfFunctions; // RVA from base of image
        public uint AddressOfNames; // RVA from base of image
        public uint AddressOfNameOrdinals; // RVA from base of image
    }

    #endregion

    #endregion
}