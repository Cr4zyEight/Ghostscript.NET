//
// GhostscriptLibrary.cs
// This file is part of Ghostscript.NET library
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
using Microsoft.WinAny.Interop;

namespace Ghostscript.NET;

/// <summary>
/// Represents a native Ghostscript library.
/// </summary>
public class GhostscriptLibrary : IDisposable
{
    #region Constructor - buffer

    /// <summary>
    /// Initializes a new instance of the Ghostscript.NET.GhostscriptLibrary class
    /// from the native library represented as the memory buffer.
    /// </summary>
    /// <param name="library">Memory buffer representing native Ghostscript library.</param>
    public GhostscriptLibrary(byte[] library)
    {
        if (library == null) throw new ArgumentNullException("library");

        // check if library is compatibile with a running process
        if (Environment.Is64BitProcess != NativeLibraryHelper.Is64BitLibrary(library))
            // throw friendly gsdll incompatibility message
            ThrowIncompatibileNativeGhostscriptLibraryException();

        // create DynamicNativeLibrary instance from the memory buffer
        _library = new DynamicNativeLibrary(library);

        // set the flag that the library is loaded from the memory
        _loadedFromMemory = true;

        // get and map native library symbols
        Initialize();
    }

    #endregion

    #region Constructor - version

    /// <summary>
    /// Initializes a new instance of the Ghostscript.NET.GhostscriptLibrary class
    /// from the GhostscriptVersionInfo object.
    /// </summary>
    /// <param name="version">GhostscriptVersionInfo instance that tells which Ghostscript library to use.</param>
    public GhostscriptLibrary(GhostscriptVersionInfo version) : this(version, false)
    {
    }

    #endregion

    #region Constructor - version, fromMemory

    /// <summary>
    /// Initializes a new instance of the Ghostscript.NET.GhostscriptLibrary class
    /// from the GhostscriptVersionInfo object.
    /// </summary>
    /// <param name="version">GhostscriptVersionInfo instance that tells which Ghostscript library to use.</param>
    /// <param name="fromMemory">Tells if the Ghostscript should be loaded from the memory or directly from the disk.</param>
    public GhostscriptLibrary(GhostscriptVersionInfo version, bool fromMemory)
    {
        // check if Ghostscript version is specified
        if (version == null) throw new ArgumentNullException("version");

        // check if specified Ghostscript native library exist on the disk
        if (!File.Exists(version.DllPath)) throw new DllNotFoundException("Ghostscript native library could not be found.");

        _version = version;
        _loadedFromMemory = fromMemory;

        // check if library is compatibile with a running process
        if (Environment.Is64BitProcess != NativeLibraryHelper.Is64BitLibrary(version.DllPath))
            // throw friendly gsdll incompatibility message
            ThrowIncompatibileNativeGhostscriptLibraryException();

        // check wether we need to load Ghostscript native library from the memory or a disk
        if (fromMemory)
        {
            // load native Ghostscript library into the memory
            byte[] buffer = File.ReadAllBytes(version.DllPath);

            // create DynamicNativeLibrary instance from the memory buffer
            _library = new DynamicNativeLibrary(buffer);
        }
        else
        {
            // create DynamicNativeLibrary instance from the local disk file
            _library = new DynamicNativeLibrary(version.DllPath);
        }

        // get and map native library symbols
        Initialize();
    }

    #endregion

    #region Revision

    public int Revision { get; private set; }

    #endregion

    #region is_gsapi_set_arg_encoding

    public bool IsGsapiSetArgEncodingSupported
    {
        get
        {
            if (Revision >= 910)
                return true;
            return false;
        }
    }

    #endregion

    #region Destructor

    ~GhostscriptLibrary()
    {
        Dispose(false);
    }

    #endregion

    #region Initialize

    /// <summary>
    /// Get the native library symbols and map them to the appropriate functions/delegates.
    /// </summary>
    private void Initialize()
    {
        string symbolMappingError = "Delegate of an exported function couldn't be created for symbol '{0}'";

        GsapiRevision = _library.GetDelegateForFunction<GsapiRevision>("gsapi_revision");

        if (GsapiRevision == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_revision"));

        GsapiRevisionS rev = new();
        if (GsapiRevision(ref rev, Marshal.SizeOf(rev)) == 0) Revision = rev.revision;

        GsapiNewInstance = _library.GetDelegateForFunction<GsapiNewInstance>("gsapi_new_instance");

        if (GsapiNewInstance == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_new_instance"));

        GsapiDeleteInstance = _library.GetDelegateForFunction<GsapiDeleteInstance>("gsapi_delete_instance");

        if (GsapiDeleteInstance == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_delete_instance"));

        GsapiSetStdio = _library.GetDelegateForFunction<GsapiSetStdio>("gsapi_set_stdio");

        if (GsapiSetStdio == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_set_stdio"));

        GsapiSetPoll = _library.GetDelegateForFunction<GsapiSetPoll>("gsapi_set_poll");

        if (GsapiSetPoll == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_set_poll"));

        GsapiSetDisplayCallback = _library.GetDelegateForFunction<GsapiSetDisplayCallback>("gsapi_set_display_callback");

        if (GsapiSetDisplayCallback == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_set_display_callback"));

        if (IsGsapiSetArgEncodingSupported)
        {
            GsapiSetArgEncoding = _library.GetDelegateForFunction<GsapiSetArgEncoding>("gsapi_set_arg_encoding");

            if (GsapiSetArgEncoding == null)
                throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_set_arg_encoding"));
        }

        GsapiInitWithArgs = _library.GetDelegateForFunction<GsapiInitWithArgs>("gsapi_init_with_args");

        if (GsapiInitWithArgs == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_init_with_args"));

        GsapiRunStringBegin = _library.GetDelegateForFunction<GsapiRunStringBegin>("gsapi_run_string_begin");

        if (GsapiRunStringBegin == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string_begin"));

        GsapiRunStringContinue = _library.GetDelegateForFunction<GsapiRunStringContinue>("gsapi_run_string_continue");

        if (GsapiRunStringContinue == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string_continue"));

        GsapiRunStringEnd = _library.GetDelegateForFunction<GsapiRunStringEnd>("gsapi_run_string_end");

        if (GsapiRunStringEnd == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string_end"));

        GsapiRunStringWithLength = _library.GetDelegateForFunction<GsapiRunStringWithLength>("gsapi_run_string_with_length");

        if (GsapiRunStringWithLength == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string_with_length"));

        GsapiRunString = _library.GetDelegateForFunction<GsapiRunString>("gsapi_run_string");

        if (GsapiRunString == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string"));

        GsapiRunPtrString = _library.GetDelegateForFunction<GsapiRunPtrString>("gsapi_run_string");

        if (GsapiRunPtrString == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_string"));

        GsapiRunFile = _library.GetDelegateForFunction<GsapiRunFile>("gsapi_run_file");

        if (GsapiRunFile == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_run_file"));

        GsapiExit = _library.GetDelegateForFunction<GsapiExit>("gsapi_exit");

        if (GsapiExit == null)
            throw new GhostscriptException(string.Format(symbolMappingError, "gsapi_exit"));
    }

    #endregion

    #region ThrowIncompatibileNativeGhostscriptLibraryException

    /// <summary>
    /// Throws friendly gsdll incompatibility message.
    /// </summary>
    private void ThrowIncompatibileNativeGhostscriptLibraryException()
    {
        throw new BadImageFormatException(Environment.Is64BitProcess
            ? "You are using native Ghostscript library (gsdll32.dll) compiled for 32bit systems in a 64bit process. You need to use gsdll64.dll. " +
              "64bit native Ghostscript library can be downloaded from http://www.ghostscript.com/download/gsdnld.html"
            : "You are using native Ghostscript library (gsdll64.dll) compiled for 64bit systems in a 32bit process. You need to use gsdll32.dll. " +
              "32bit native Ghostscript library can be downloaded from http://www.ghostscript.com/download/gsdnld.html");
    }

    #endregion

    #region Private variables

    private bool _disposed;
    private DynamicNativeLibrary _library;
    private GhostscriptVersionInfo _version;
    private bool _loadedFromMemory;

    #endregion

    #region Dispose

    #region Dispose

    /// <summary>
    /// Releases all resources used by the Ghostscript.NET.GhostscriptLibrary instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Dispose - disposing

    /// <summary>
    /// Releases all resources used by the Ghostscript.NET.GhostscriptLibrary instance.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _library.Dispose();
                _library = null;
            }

            _disposed = true;
        }
    }

    #endregion

    #endregion

    #region Ghostscript functions

    public GsapiRevision GsapiRevision;
    public GsapiNewInstance GsapiNewInstance;
    public GsapiDeleteInstance GsapiDeleteInstance;
    public GsapiSetStdio GsapiSetStdio;
    public GsapiSetPoll GsapiSetPoll;
    public GsapiSetDisplayCallback GsapiSetDisplayCallback;
    public GsapiSetArgEncoding GsapiSetArgEncoding;
    public GsapiInitWithArgs GsapiInitWithArgs;
    public GsapiRunStringBegin GsapiRunStringBegin;
    public GsapiRunStringContinue GsapiRunStringContinue;
    public GsapiRunStringEnd GsapiRunStringEnd;
    public GsapiRunStringWithLength GsapiRunStringWithLength;
    public GsapiRunString GsapiRunString;
    public GsapiRunPtrString GsapiRunPtrString;
    public GsapiRunFile GsapiRunFile;
    public GsapiExit GsapiExit;

    #endregion
}