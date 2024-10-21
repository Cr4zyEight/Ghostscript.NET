//
// GhostscriptProcessor.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostscript.NET.Processor
{
    public class GhostscriptProcessor : IDisposable
    {
        #region Private constants

        private readonly char[] _emptySpaceSplit = new char[] { ' ' };

        #endregion

        #region Private variables

        private bool _disposed = false;
        private bool _processorOwnsLibrary = true;
        private GhostscriptLibrary _gs;
        private GhostscriptStdIo _stdIoCallback;
        private GhostscriptProcessorInternalStdIoHandler _internalStdIoCallback;
        private GsapiPoolCallback _poolCallBack;
        private StringBuilder _outputMessages = new StringBuilder();
        private StringBuilder _errorMessages = new StringBuilder();
        private int _totalPages;
        private bool _isRunning = false;
        private bool _stopProcessing = false;

        #endregion

        #region Public events

        public event GhostscriptProcessorEventHandler Started;
        public event GhostscriptProcessorProcessingEventHandler Processing;
        public event GhostscriptProcessorErrorEventHandler Error;
        public event GhostscriptProcessorEventHandler Completed;

        #region Started

        protected void OnStarted(GhostscriptProcessorEventArgs e)
        {
            if (this.Started != null)
            {
                this.Started(this, e);
            }
        }

        #endregion

        #region OnProcessing

        protected void OnProcessing(GhostscriptProcessorProcessingEventArgs e)
        {
            if (this.Processing != null)
            {
                this.Processing(this, e);
            }
        }

        #endregion

        #region OnError

        protected void OnError(GhostscriptProcessorErrorEventArgs e)
        {
            if (this.Error != null)
            {
                this.Error(this, e);
            }
        }

        #endregion

        #region OnCompleted

        protected void OnCompleted(GhostscriptProcessorEventArgs e)
        {
            if (this.Completed != null)
            {
                this.Completed(this, e);
            }
        }

        #endregion

        #endregion

        #region Constructor

        public GhostscriptProcessor()
            : this(GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.Gpl | GhostscriptLicense.Afpl, GhostscriptLicense.Gpl), false)
        { }

        #endregion

        #region Constructor - library

        public GhostscriptProcessor(GhostscriptLibrary library, bool processorOwnsLibrary = false)
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }
            _processorOwnsLibrary = processorOwnsLibrary;
            _gs = library;
        }
        
        public GhostscriptProcessor(byte[] library)
        {
            if (library == null)
            {
                throw new ArgumentNullException("library");
            }

            _gs = new GhostscriptLibrary(library);
        }

        #endregion

        #region Constructor - version

        public GhostscriptProcessor(GhostscriptVersionInfo version) : this(version, false)
        { }

        #endregion

        #region Constructor - version, fromMemory

        public GhostscriptProcessor(GhostscriptVersionInfo version, bool fromMemory)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            _gs = new GhostscriptLibrary(version, fromMemory);
        }

        #endregion

        #region Destructor

        ~GhostscriptProcessor()
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
                    if (_processorOwnsLibrary)
                    {
                        _gs.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        #endregion

        #endregion

        #region Process - device

        public void Process(GhostscriptDevice device)
        {
            this.Process(device, null);
        }

        #endregion

        #region Process - args

        public void Process(string[] args)
        {
            this.Process(args, null);
        }

        #endregion

        #region Process - device, stdIO_callback

        public void Process(GhostscriptDevice device, GhostscriptStdIo stdIoCallback)
        {
            this.StartProcessing(device, stdIoCallback);
        }

        #endregion

        #region Process - args, stdIO_callback

        public void Process(string[] args, GhostscriptStdIo stdIoCallback)
        {
            this.StartProcessing(args, stdIoCallback);
        }

        #endregion

        #region StartProcessing - device, stdIO_callback

        public void StartProcessing(GhostscriptDevice device, GhostscriptStdIo stdIoCallback)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            this.StartProcessing(device.GetSwitches(), stdIoCallback);
        }

        #endregion

        #region StartProcessing - args, stdIO_callback

        /// <summary>
        /// Run Ghostscript.
        /// </summary>
        /// <param name="args">Command arguments</param>
        /// <param name="stdIO_callback">StdIO callback, can be set to null if you dont want to handle it.</param>
        public void StartProcessing(string[] args, GhostscriptStdIo stdIoCallback)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }

            if (args.Length < 3)
            {
                throw new ArgumentOutOfRangeException("args");
            }

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(args[i]));
            }

            _isRunning = true;

            IntPtr instance = IntPtr.Zero;

            int rcIns = _gs.GsapiNewInstance(out instance, IntPtr.Zero);

            if (Ierrors.IsError(rcIns))
            {
                throw new GhostscriptApiCallException("gsapi_new_instance", rcIns);
            }

            try
            {
                _stdIoCallback = stdIoCallback;

                _internalStdIoCallback = new GhostscriptProcessorInternalStdIoHandler(
                                                new StdInputEventHandler(OnStdIoInput), 
                                                new StdOutputEventHandler(OnStdIoOutput), 
                                                new StdErrorEventHandler(OnStdIoError));

                int rcStdio = _gs.GsapiSetStdio(instance,
                                        _internalStdIoCallback._std_in,
                                        _internalStdIoCallback._std_out,
                                        _internalStdIoCallback.StdErr);

                _poolCallBack = new GsapiPoolCallback(Pool);

                int rcPool = _gs.GsapiSetPoll(instance, _poolCallBack);

                if (Ierrors.IsError(rcPool))
                {
                    throw new GhostscriptApiCallException("gsapi_set_poll", rcPool);

                }

                if (Ierrors.IsError(rcStdio))
                {
                    throw new GhostscriptApiCallException("gsapi_set_stdio", rcStdio);
                }

                this.OnStarted(new GhostscriptProcessorEventArgs());

                _stopProcessing = false;

                if (_gs.IsGsapiSetArgEncodingSupported)
                {
                    int rcEnc = _gs.GsapiSetArgEncoding(instance, GsArgEncoding.Utf8);
                }

                int rcInit = _gs.GsapiInitWithArgs(instance, args.Length, args);

                if (Ierrors.IsErrorIgnoreQuit(rcInit))
                {
                    if (!Ierrors.IsInterrupt(rcInit))
                    {
                        throw new GhostscriptApiCallException("gsapi_init_with_args", rcInit);
                    }
                }

                int rcExit = _gs.GsapiExit(instance);

                if (Ierrors.IsErrorIgnoreQuit(rcExit))
                {
                    throw new GhostscriptApiCallException("gsapi_exit", rcExit);
                }
            }
            finally
            {
                _gs.GsapiDeleteInstance(instance);

                GC.Collect();

                _isRunning = false;

                this.OnCompleted(new GhostscriptProcessorEventArgs());
            }
        }

        #endregion

        #region StopProcessing

        public void StopProcessing()
        {
            _stopProcessing = true;
        }

        #endregion

        #region Pool

        private int Pool(IntPtr handle)
        {
            if (_stopProcessing)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region OnStdIoInput

        private void OnStdIoInput(out string input, int count)
        {
            if (_stdIoCallback != null)
            {
                _stdIoCallback.StdIn(out input, count);
            }
            else
            {
                input = string.Empty;
            }
        }

        #endregion

        #region OnStdIoOutput

        private void OnStdIoOutput(string output)
        {
            lock (_outputMessages)
            {
                _outputMessages.Append(output);

                int rIndex = _outputMessages.ToString().IndexOf("\r\n");

                while (rIndex > -1)
                {
                    string line = _outputMessages.ToString().Substring(0, rIndex);
                    _outputMessages = _outputMessages.Remove(0, rIndex + 2);

                    this.ProcessOutputLine(line);

                    rIndex = _outputMessages.ToString().IndexOf("\r\n");
                }

                if (_stdIoCallback != null)
                {
                    _stdIoCallback.StdOut(output);
                }
            }
        }

        #endregion

        #region OnStdIoError

        private void OnStdIoError(string error)
        {
            lock (_errorMessages)
            {
                _outputMessages.Append(error);

                int rIndex = _errorMessages.ToString().IndexOf("\r\n");

                while (rIndex > -1)
                {
                    string line = _errorMessages.ToString().Substring(0, rIndex);
                    _errorMessages = _errorMessages.Remove(0, rIndex + 2);

                    this.ProcessErrorLine(line);

                    rIndex = _errorMessages.ToString().IndexOf("\r\n");
                }

                if (_stdIoCallback != null)
                {
                    _stdIoCallback.StdError(error);
                }
            }
        }

        #endregion

        #region ProcessOutputLine

        private void ProcessOutputLine(string line)
        {
            if (line.StartsWith("Processing pages"))
            {
                string[] chunks = line.Split(_emptySpaceSplit);
                _totalPages = int.Parse(chunks[chunks.Length - 1].TrimEnd('.'));
            }
            else if (line.StartsWith("Page"))
            {
                string[] chunks = line.Split(_emptySpaceSplit);
                int currentPage = int.Parse(chunks[1]);

                this.OnProcessing(new GhostscriptProcessorProcessingEventArgs(currentPage, _totalPages));
            }
        }

        #endregion

        #region ProcessErrorLine

        private void ProcessErrorLine(string line)
        {
            this.OnError(new GhostscriptProcessorErrorEventArgs(line));
        }

        #endregion

        #region IsRunning

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        #endregion

        #region IsStopping

        public bool IsStopping
        {
            get { return _isRunning && _stopProcessing; }
        }

        #endregion

    }
}
