//
// GhostscriptInterpreter.cs
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

using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace Ghostscript.NET.Interpreter
{
    /// <summary>
    /// Represents a Ghostscript interpreter.
    /// </summary>
    public class GhostscriptInterpreter : IDisposable
    {

        #region Private constants

        private const int RunStringMaxLength = 65535;

        #endregion

        #region Private variables

        private bool _disposed = false;
        private GhostscriptLibrary _gs = null;
        private IntPtr _gsInstance = IntPtr.Zero;
        private GhostscriptStdIo _stdIo = null;
        private GhostscriptDisplayDeviceHandler _displayDevice = null;
        private IntPtr _displayDeviceCallbackHandle = IntPtr.Zero;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptInterpreter class.
        /// </summary>
        public GhostscriptInterpreter()
            : this(GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.Gpl | GhostscriptLicense.Afpl, GhostscriptLicense.Gpl), false)
        { }

        #endregion

        #region Constructor - library

        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptInterpreter class.
        /// </summary>
        /// <param name="library">Memory buffer representing native Ghostscript library.</param>
        public GhostscriptInterpreter(byte[] library)
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            // load ghostscript native library
            _gs = new GhostscriptLibrary(library);

            // initialize Ghostscript interpreter
            this.Initialize();
        }

        #endregion

        #region Constructor - version

        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptInterpreter class.
        /// </summary>
        /// <param name="version">GhostscriptVersionInfo instance that tells which Ghostscript library to use.</param>
        public GhostscriptInterpreter(GhostscriptVersionInfo version) : this(version, false)
        { }

        #endregion

        #region Constructor - version, fromMemory

        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptInterpreter class.
        /// </summary>
        /// <param name="version">GhostscriptVersionInfo instance that tells which Ghostscript library to use.</param>
        /// <param name="fromMemory">Tells if the Ghostscript should be loaded from the memory or directly from the disk.</param>
        public GhostscriptInterpreter(GhostscriptVersionInfo version, bool fromMemory)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            // load ghostscript native library
            _gs = new GhostscriptLibrary(version, fromMemory);

            // initialize Ghostscript interpreter
            this.Initialize();
        }

        #endregion

        #region Destructor

        ~GhostscriptInterpreter()
        {
            Dispose(false);
        }

        #endregion

        #region Dispose

        #region Dispose

        /// <summary>
        /// Releases all resources used by the Ghostscript.NET.GhostscriptInterpreter instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Dispose - disposing

        /// <summary>
        /// Releases all resources used by the Ghostscript.NET.GhostscriptInterpreter instance.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // GSAPI: exit the interpreter
                    _gs.GsapiExit(_gsInstance);

                    // GSAPI: destroy an instance of Ghostscript
                    _gs.GsapiDeleteInstance(_gsInstance);

                    // release all resource used by Ghostscript library
                    _gs.Dispose();
                }

                // check if the display device callback handler is attached
                if (_displayDeviceCallbackHandle != IntPtr.Zero)
                {
                    // free earlier allocated memory used for the display device callback
                    Marshal.FreeCoTaskMem(_displayDeviceCallbackHandle);
                }

                _disposed = true;
            }
        }

        #endregion

        #endregion

        #region Initialize

        /// <summary>
        /// Initializes a new instance of Ghostscript interpreter.
        /// </summary>
        private void Initialize()
        {
            // GSAPI: create a new instance of Ghostscript
            int rcIns = _gs.GsapiNewInstance(out _gsInstance, IntPtr.Zero);

            if (Ierrors.IsError(rcIns))
            {
                throw new GhostscriptApiCallException("gsapi_new_instance", rcIns);
            }
        }

        #endregion

        #region Setup

        /// <summary>
        /// Sets the stdio and display device callback handlers.
        /// </summary>
        /// <param name="stdIo">Stdio callback handler.</param>
        /// <param name="displayDevice">DisplayDevice callback handler.</param>
        public void Setup(GhostscriptStdIo stdIo, GhostscriptDisplayDeviceHandler displayDevice)
        {
            // check if we need to set stdio handler
            if (stdIo != null)
            {
                // check if stdio handler is not already set
                if (_stdIo == null)
                {
                    // GSAPI: set the stdio callback handlers
                    int rcStdio = _gs.GsapiSetStdio(_gsInstance,
                                            stdIo != null ? stdIo.StdInCallback : null,
                                            stdIo != null ? stdIo.StdOutCallback : null,
                                            stdIo != null ? stdIo.StdErrCallback : null);

                    // check if the stdio callback handlers are set correctly
                    if (Ierrors.IsError(rcStdio))
                    {
                        throw new GhostscriptApiCallException("gsapi_set_stdio", rcStdio);
                    }

                    // remember it
                    _stdIo = stdIo;
                }
                else
                {
                    throw new GhostscriptException("StdIO callback handler is already set.");
                }
            }

            // check if a custom display device needs to be used
            if (displayDevice != null)
            {
                // check if display device is already set
                if (_displayDevice == null)
                {
                    // allocate a memory for the display device callback handler
                    _displayDeviceCallbackHandle = Marshal.AllocCoTaskMem(displayDevice.Callback.size);

                    // copy display device callback structure content to the pre-allocated block of memory
                    Marshal.StructureToPtr(displayDevice.Callback, _displayDeviceCallbackHandle, true);

                    // GSAPI: set the display device callback handler
                    int rcDev = _gs.GsapiSetDisplayCallback(_gsInstance, _displayDeviceCallbackHandle);

                    // check if the display callback handler is set correctly
                    if (Ierrors.IsError(rcDev))
                    {
                        throw new GhostscriptApiCallException("gsapi_set_display_callback", rcDev);
                    }

                    // remember it
                    _displayDevice = displayDevice;

                }
                else
                {
                    throw new GhostscriptException("DisplayDevice callback is already set!");
                }
            }

        }

        #endregion

        #region InitArgs

        /// <summary>
        /// Initializes the interpreter.
        /// </summary>
        public void InitArgs(string[] args)
        {
            if (_gs.IsGsapiSetArgEncodingSupported)
            {
                // set the encoding to UTF8
                int rcEnc = _gs.GsapiSetArgEncoding(_gsInstance, GsArgEncoding.Utf8);
            }

            string[] utf8Args = new string[args.Length];

            for(int i = 0; i < args.Length; i++)
            {
                utf8Args[i] = StringHelper.ToUtf8String(args[i]);
            }
            
            // GSAPI: initialize the interpreter
            int rcInit = _gs.GsapiInitWithArgs(_gsInstance, utf8Args.Length, utf8Args);

            // check if the interpreter is initialized correctly
            if (Ierrors.IsError(rcInit))
            {
                throw new GhostscriptApiCallException("gsapi_init_with_args", rcInit);
            }
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs a string.
        /// </summary>
        public int Run(string str)
        {
            lock (this)
            {
                int exitCode;

                // check if the string we are trying to run doesn't exceed max length for the 'run_string' function
                if (str.Length < RunStringMaxLength)
                {
                    // GSAPI: run the string
                    int rcRun = _gs.GsapiRunString(_gsInstance, str, 0, out exitCode);

                    if (Ierrors.IsFatalIgnoreNeedInput(rcRun))
                    {
                        throw new GhostscriptApiCallException("gsapi_run_string", rcRun);
                    }

                    return rcRun;
                }
                else // we need to split a string into chunks
                {
                    // GSAPI: prepare a Ghostscript for running string in chunks
                    int rcRunBeg = _gs.GsapiRunStringBegin(_gsInstance, 0, out exitCode);

                    if (Ierrors.IsFatalIgnoreNeedInput(rcRunBeg))
                    {
                        throw new GhostscriptApiCallException("gsapi_run_string_begin", rcRunBeg);
                    }

                    int chunkStart = 0;

                    // start splitting a string into chunks
                    for (int size = str.Length; size > 0; size -= RunStringMaxLength)
                    {
                        int chunkSize = (size < RunStringMaxLength) ? size : RunStringMaxLength;
                        string chunk = str.Substring(chunkStart, chunkSize);

                        // GSAPI: run a chunk
                        int rcRunCon = _gs.GsapiRunStringContinue(_gsInstance, chunk, (uint)chunkSize, 0, out exitCode);

                        if (Ierrors.IsFatalIgnoreNeedInput(rcRunCon))
                        {
                            throw new GhostscriptApiCallException("gsapi_run_string_continue", rcRunCon);
                        }

                        chunkStart += chunkSize;
                    }

                    // GSAPI: notify Ghostscript we are done with running chunked string
                    int rcRunEnd = _gs.GsapiRunStringEnd(_gsInstance, 0, out exitCode);

                    if (Ierrors.IsFatalIgnoreNeedInput(rcRunEnd))
                    {
                        throw new GhostscriptApiCallException("gsapi_run_string_end", rcRunEnd);
                    }

                    return rcRunEnd;
                }
            }
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs a string.
        /// </summary>
        internal int Run(IntPtr str)
        {
            lock (this)
            {
                int exitCode;

                int rcRun = _gs.GsapiRunPtrString(_gsInstance, str, 0, out exitCode);

                if (Ierrors.IsFatalIgnoreNeedInput(rcRun))
                {
                    throw new GhostscriptApiCallException("gsapi_run_string", rcRun);
                }

                return rcRun;
            }
        }

        #endregion

        #region RunFile

        /// <summary>
        /// Runs a PostScript file.
        /// </summary>
        public void RunFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Couldn't find input file.", path);
            }

            int exitCode;

            // GSAPI: tell a Ghostscript to run a file
            int rcRun = _gs.GsapiRunFile(_gsInstance, path, 0, out exitCode);

            if (Ierrors.IsFatal(rcRun))
            {
                throw new GhostscriptApiCallException("gsapi_run_file", rcRun);
            }
        }

        #endregion

        #region LibraryRevision

        public int LibraryRevision
        {
            get { return _gs.Revision; }
        }

        #endregion

        #region GhostscriptLibrary

        public GhostscriptLibrary GhostscriptLibrary
        {
            get { return _gs; }
        }

        #endregion

    }
}
