//
// GhostscriptViewer.cs
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

using System.Globalization;
using Ghostscript.NET.Interpreter;

namespace Ghostscript.NET.Viewer;

public class GhostscriptViewer : IDisposable
{
    #region Constructor

    #endregion

    #region Internal properties

    #region ShowPageAfterOpen

    public bool ShowPageAfterOpen { get; set; } = true;

    #endregion

    #endregion

    #region Internal properties

    #region FormatHandler

    internal GhostscriptViewerFormatHandler FormatHandler { get; private set; }

    #endregion

    #endregion

    #region Destructor

    ~GhostscriptViewer()
    {
        Dispose(false);
    }

    #endregion

    #region Open - stream

    public void Open(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        string path = StreamHelper.WriteToTemporaryFile(stream);

        _fileCleanupHelper.Add(path);

        Open(path);
    }

    #endregion

    #region Open - path

    public void Open(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find input file.", path);

        Open(path, GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.Gpl | GhostscriptLicense.Afpl, GhostscriptLicense.Gpl), false);
    }

    #endregion

    #region Open - stream, versionInfo, dllFromMemory

    public void Open(Stream stream, GhostscriptVersionInfo versionInfo, bool dllFromMemory)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        if (versionInfo == null) throw new ArgumentNullException("versionInfo");

        string path = StreamHelper.WriteToTemporaryFile(stream);

        _fileCleanupHelper.Add(path);

        Open(path, versionInfo, dllFromMemory);
    }

    #endregion

    #region Open - path, versionInfo, dllFromMemory

    public void Open(string path, GhostscriptVersionInfo versionInfo, bool dllFromMemory)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find input file.", path);

        if (versionInfo == null) throw new ArgumentNullException("versionInfo");

        Close();

        _filePath = path;

        Interpreter = new GhostscriptInterpreter(versionInfo, dllFromMemory);

        Open();
    }

    #endregion

    #region Open - stream, library

    public void Open(Stream stream, byte[] library)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        string path = StreamHelper.WriteToTemporaryFile(stream);

        _fileCleanupHelper.Add(path);

        Open(path, library);
    }

    #endregion

    #region Open - path, library

    public void Open(string path, byte[] library)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find input file.", path);

        if (library == null) throw new ArgumentNullException("library");

        Close();

        _filePath = path;

        Interpreter = new GhostscriptInterpreter(library);

        Open();
    }

    #endregion

    #region Open - versionInfo, dllFromMemory

    public void Open(GhostscriptVersionInfo versionInfo, bool dllFromMemory)
    {
        if (versionInfo == null) throw new ArgumentNullException("versionInfo");

        Close();

        _filePath = string.Empty;

        Interpreter = new GhostscriptInterpreter(versionInfo, dllFromMemory);

        Open();
    }

    #endregion

    #region Open - library

    public void Open(byte[] library)
    {
        if (library == null) throw new ArgumentNullException("library");

        Close();

        _filePath = string.Empty;

        Interpreter = new GhostscriptInterpreter(library);

        Open();
    }

    #endregion

    #region Open

    private void Open()
    {
        string extension = Path.GetExtension(_filePath).ToLower();

        if (!string.IsNullOrWhiteSpace(_filePath) && string.IsNullOrWhiteSpace(extension))
            using (FileStream srm = new(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                extension = StreamHelper.GetStreamExtension(srm);
            }

        switch (extension)
        {
            case ".pdf":
            {
                FormatHandler = new GhostscriptViewerPdfFormatHandler(this);
                break;
            }
            case ".ps":
            {
                FormatHandler = new GhostscriptViewerPsFormatHandler(this);
                break;
            }
            case ".eps":
            {
                FormatHandler = new GhostscriptViewerEpsFormatHandler(this);
                break;
            }
            default:
            {
                FormatHandler = new GhostscriptViewerDefaultFormatHandler(this);
                break;
            }
        }

        Interpreter.Setup(new GhostscriptViewerStdIoHandler(this, FormatHandler), new GhostscriptViewerDisplayHandler(this));

        List<string> args = new();
        args.Add("-gsnet");
        args.Add("-sDEVICE=display");

        if (Environment.Is64BitProcess)
            args.Add("-sDisplayHandle=0");
        else
            args.Add("-dDisplayHandle=0");

        args.Add("-dDisplayFormat=" +
                 ((int)DisplayFormatColor.DisplayColorsRgb |
                  (int)DisplayFormatAlpha.DisplayAlphaNone |
                  (int)DisplayFormatDepth.DisplayDepth8 |
                  (int)DisplayFormatEndian.DisplayLittleendian |
                  (int)DisplayFormatFirstrow.DisplayBottomfirst));


        if (Interpreter.LibraryRevision > 950) args.Add("--permit-file-read=" + _filePath);

        args.Add("-dDOINTERPOLATE");
        args.Add("-dGridFitTT=0");

        // fixes bug: http://bugs.ghostscript.com/show_bug.cgi?id=695180
        if (Interpreter.LibraryRevision > 910) args.Add("-dMaxBitmap=1g");

        foreach (string customSwitch in CustomSwitches) args.Add(customSwitch);

        Interpreter.InitArgs(args.ToArray());

        FormatHandler.Initialize();

        FormatHandler.Open(_filePath);

        if (ShowPageAfterOpen) ShowPage(FormatHandler.FirstPageNumber, true);
    }

    #endregion

    #region Close

    public void Close()
    {
        if (FormatHandler != null) FormatHandler = null;

        if (Interpreter != null)
        {
            Interpreter.Dispose();
            Interpreter = null;
        }
    }

    #endregion

    #region AttachStdIO

    public void AttachStdIo(GhostscriptStdIo stdIoCallback)
    {
        _stdIoCallback = stdIoCallback;
    }

    #endregion

    #region ShowPage - pageNumber

    public void ShowPage(int pageNumber)
    {
        ShowPage(pageNumber, false);
    }

    #endregion

    #region ShowPage - pageNumber, refresh

    public void ShowPage(int pageNumber, bool refresh)
    {
        if (!IsEverythingInitialized)
            return;

        if (refresh == false && pageNumber == CurrentPageNumber)
            return;

        FormatHandler.InitPage(pageNumber);


        Interpreter.Run(
            "%%BeginPageSetup\n" +
            "<<\n");

        Interpreter.Run(string.Format("/HWResolution [{0} {1}]\n", ZoomXDpi, ZoomYDpi));

        GhostscriptRectangle mediaBox = FormatHandler.MediaBox;
        GhostscriptRectangle boundingBox = FormatHandler.BoundingBox;
        GhostscriptRectangle cropBox = FormatHandler.CropBox;

        float pageWidth = 0;
        float pageHeight = 0;

        if (FormatHandler.GetType() == typeof(GhostscriptViewerEpsFormatHandler) && EpsClip && boundingBox != GhostscriptRectangle.Empty)
        {
            pageWidth = boundingBox.Urx - boundingBox.Llx;
            pageHeight = boundingBox.Ury - boundingBox.Lly;
        }
        else
        {
            if (cropBox != GhostscriptRectangle.Empty)
            {
                pageWidth = cropBox.Urx - cropBox.Llx;
                pageHeight = cropBox.Ury - cropBox.Lly;
            }
            else
            {
                pageWidth = mediaBox.Urx - mediaBox.Llx;
                pageHeight = mediaBox.Ury - mediaBox.Lly;
            }
        }

        pageWidth = Math.Abs(pageWidth);
        pageHeight = Math.Abs(pageHeight);

        if (pageWidth > 0 && pageHeight > 0)
            Interpreter.Run(string.Format("/PageSize [{0} {1}]\n",
                pageWidth.ToString("0.00", CultureInfo.InvariantCulture),
                pageHeight.ToString("0.00", CultureInfo.InvariantCulture)));

        if (cropBox != GhostscriptRectangle.Empty) mediaBox = cropBox;

        if (mediaBox == GhostscriptRectangle.Empty && boundingBox != GhostscriptRectangle.Empty) mediaBox = boundingBox;

        if (mediaBox != GhostscriptRectangle.Empty && FormatHandler.GetType() != typeof(GhostscriptViewerPsFormatHandler))
        {
            if (FormatHandler.PageOrientation == GhostscriptPageOrientation.Portrait)
                Interpreter.Run(string.Format("/PageOffset  [{0} {1}]\n",
                    (-mediaBox.Llx).ToString("0.00", CultureInfo.InvariantCulture),
                    (-mediaBox.Lly).ToString("0.00", CultureInfo.InvariantCulture)));
            else if (FormatHandler.PageOrientation == GhostscriptPageOrientation.Landscape)
                Interpreter.Run(string.Format("/PageOffset  [{0} {1}]\n",
                    (-mediaBox.Lly).ToString("0.00", CultureInfo.InvariantCulture),
                    mediaBox.Llx.ToString("0.00", CultureInfo.InvariantCulture)));
            else if (FormatHandler.PageOrientation == GhostscriptPageOrientation.UpsideDown)
                Interpreter.Run(string.Format("/PageOffset  [{0} {1}]\n",
                    mediaBox.Llx.ToString("0.00", CultureInfo.InvariantCulture),
                    mediaBox.Lly.ToString("0.00", CultureInfo.InvariantCulture)));
            else if (FormatHandler.PageOrientation == GhostscriptPageOrientation.Seascape)
                Interpreter.Run(string.Format("/PageOffset  [{0} {1}]\n",
                    mediaBox.Lly.ToString("0.00", CultureInfo.InvariantCulture),
                    (-mediaBox.Llx).ToString("0.00", CultureInfo.InvariantCulture)));
        }

        Interpreter.Run(string.Format("/GraphicsAlphaBits {0}\n", GraphicsAlphaBits));
        Interpreter.Run(string.Format("/TextAlphaBits {0}\n", TextAlphaBits));

        Interpreter.Run(">> setpagedevice\n");

        Interpreter.Run(
            "%%EndPageSetup\n");

        FormatHandler.ShowPage(pageNumber);
    }

    #endregion

    #region ShowFirstPage

    public void ShowFirstPage()
    {
        if (CurrentPageNumber == FirstPageNumber)
            return;

        ShowPage(FirstPageNumber);
    }

    #endregion

    #region ShowNextPage

    public void ShowNextPage()
    {
        if (CurrentPageNumber + 1 <= LastPageNumber) ShowPage(CurrentPageNumber + 1);
    }

    #endregion

    #region ShowPreviousPage

    public void ShowPreviousPage()
    {
        if (CurrentPageNumber - 1 >= FirstPageNumber) ShowPage(CurrentPageNumber - 1);
    }

    #endregion

    #region ShowLastPage

    public void ShowLastPage()
    {
        if (CurrentPageNumber == LastPageNumber)
            return;

        ShowPage(LastPageNumber);
    }

    #endregion

    #region RefreshPage

    public void RefreshPage()
    {
        if (IsEverythingInitialized) ShowPage(CurrentPageNumber, true);
    }

    #endregion

    #region IsPageNumberValid

    public bool IsPageNumberValid(int pageNumber)
    {
        if (pageNumber >= FirstPageNumber && pageNumber <= LastPageNumber)
            return true;
        return false;
    }

    #endregion

    #region Zoom

    public bool Zoom(float scale, bool test = false)
    {
        int tmpZoopX = (int)(ZoomXDpi * scale + 0.5);
        int tmpZoomY = (int)(ZoomYDpi * scale + 0.5);

        if (tmpZoopX < 39)
            return false;

        if (tmpZoopX > 496)
            return false;

        if (!test)
        {
            ZoomXDpi = tmpZoopX;
            ZoomYDpi = tmpZoomY;
        }

        return true;
    }

    #endregion

    #region ZoomIn

    public void ZoomIn()
    {
        if (IsEverythingInitialized)
        {
            Zoom(1.2f);
            RefreshPage();
        }
    }

    #endregion

    #region ZoomOut

    public void ZoomOut()
    {
        if (IsEverythingInitialized)
        {
            Zoom(0.8333333f);
            RefreshPage();
        }
    }

    #endregion

    #region SaveState

    public GhostscriptViewerState SaveState()
    {
        GhostscriptViewerState state = new();
        state.XDpi = ZoomXDpi;
        state.YDpi = ZoomYDpi;
        state.CurrentPage = FormatHandler.CurrentPageNumber;
        state.ProgressiveUpdate = ProgressiveUpdate;
        return state;
    }

    #endregion

    #region RestoreState

    public void RestoreState(GhostscriptViewerState state)
    {
        ZoomXDpi = state.XDpi;
        ZoomYDpi = state.YDpi;
        FormatHandler.CurrentPageNumber = state.CurrentPage;
        ProgressiveUpdate = state.ProgressiveUpdate;
    }

    #endregion

    #region Private variables

    private bool _disposed;
    private string _filePath;
    private GhostscriptStdIo _stdIoCallback;
    private readonly FileCleanupHelper _fileCleanupHelper = new();

    #endregion

    #region Public events

    public event GhostscriptViewerViewEventHandler DisplaySize;
    public event GhostscriptViewerViewEventHandler DisplayUpdate;
    public event GhostscriptViewerViewEventHandler DisplayPage;

    #region OnDisplaySize

    protected virtual void OnDisplaySize(GhostscriptViewerViewEventArgs e)
    {
        if (DisplaySize != null) DisplaySize(this, e);
    }

    #endregion

    #region OnDisplayUpdate

    protected virtual void OnDisplayUpdate(GhostscriptViewerViewEventArgs e)
    {
        if (DisplayUpdate != null) DisplayUpdate(this, e);
    }

    #endregion

    #region OnDisplayPage

    protected virtual void OnDisplayPage(GhostscriptViewerViewEventArgs e)
    {
        if (DisplayPage != null) DisplayPage(this, e);
    }

    #endregion

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
                if (FormatHandler != null)
                {
                    FormatHandler.Dispose();
                    FormatHandler = null;
                }

                if (Interpreter != null)
                {
                    Interpreter.Dispose();
                    Interpreter = null;
                }
            }

            _fileCleanupHelper.Cleanup();

            _disposed = true;
        }
    }

    #endregion

    #endregion

    #region Internal methods

    #region StdInput

    internal void StdInput(out string input, int count)
    {
        input = null;

        if (_stdIoCallback != null) _stdIoCallback.StdIn(out input, count);
    }

    #endregion

    #region StdOutput

    internal void StdOutput(string message)
    {
        if (_stdIoCallback != null) _stdIoCallback.StdOut(message);
    }

    #endregion

    #region StdError

    internal void StdError(string message)
    {
        if (_stdIoCallback != null) _stdIoCallback.StdError(message);
    }

    #endregion

    #region RaiseDisplaySize

    internal void RaiseDisplaySize(GhostscriptViewerViewEventArgs e)
    {
        OnDisplaySize(e);
    }

    #endregion

    #region RaiseDisplayPage

    internal void RaiseDisplayPage(GhostscriptViewerViewEventArgs e)
    {
        OnDisplayPage(e);
    }

    #endregion

    #region RaiseDisplayUpdate

    internal void RaiseDisplayUpdate(GhostscriptViewerViewEventArgs e)
    {
        OnDisplayUpdate(e);
    }

    #endregion

    #region ZoomXDpi

    internal int ZoomXDpi { get; set; } = 96;

    #endregion

    #region ZoomYDpi

    internal int ZoomYDpi { get; set; } = 96;

    #endregion

    #endregion

    #region Public properties

    #region Interpreter

    public GhostscriptInterpreter Interpreter { get; private set; }

    #endregion

    #region IsEverythingInitialized

    public bool IsEverythingInitialized => FormatHandler != null;

    #endregion

    #region FilePath

    public string FilePath
    {
        get
        {
            if (IsEverythingInitialized)
                return _filePath;
            return null;
        }
    }

    #endregion

    #region CurrentPageNumber

    public int CurrentPageNumber
    {
        get
        {
            if (IsEverythingInitialized)
                return FormatHandler.CurrentPageNumber;
            return 0;
        }
    }

    #endregion

    #region FirstPageNumber

    public int FirstPageNumber
    {
        get
        {
            if (IsEverythingInitialized)
                return FormatHandler.FirstPageNumber;
            return 0;
        }
    }

    #endregion

    #region LastPageNumber

    public int LastPageNumber
    {
        get
        {
            if (IsEverythingInitialized)
                return FormatHandler.LastPageNumber;
            return 0;
        }
    }

    #endregion

    #region ProgressiveUpdate

    public bool ProgressiveUpdate { get; set; } = true;

    #endregion

    #region ProgressiveUpdateInterval

    public int ProgressiveUpdateInterval { get; set; } = 100;

    #endregion

    #region CanShowFirstPage

    public bool CanShowFirstPage => CurrentPageNumber != FirstPageNumber;

    #endregion

    #region CanShowPreviousPage

    public bool CanShowPreviousPage => CurrentPageNumber > FirstPageNumber;

    #endregion

    #region CanShowNextPage

    public bool CanShowNextPage => CurrentPageNumber < LastPageNumber;

    #endregion

    #region CanShowLastPage

    public bool CanShowLastPage => CurrentPageNumber != LastPageNumber;

    #endregion

    #region CanZoomIn

    public bool CanZoomIn => Zoom(1.2f, true);

    #endregion

    #region CanZoomOut

    public bool CanZoomOut => Zoom(0.8333333f, true);

    #endregion

    #region GraphicsAlphaBits

    public int GraphicsAlphaBits { get; set; } = 4;

    #endregion

    #region TextAlphaBits

    public int TextAlphaBits { get; set; } = 4;

    #endregion

    #region EPSClip

    public bool EpsClip { get; set; } = true;

    #endregion

    #region CurrentPageOrientation

    public GhostscriptPageOrientation CurrentPageOrientation
    {
        get
        {
            if (IsEverythingInitialized)
                return FormatHandler.PageOrientation;
            return GhostscriptPageOrientation.Landscape;
        }
    }

    #endregion

    #region CustomSwitches

    public List<string> CustomSwitches { get; set; } = new();

    #endregion

    #region DPI

    public int Dpi
    {
        get => ZoomXDpi;
        set
        {
            ZoomXDpi = value;
            ZoomYDpi = value;
        }
    }

    #endregion

    #endregion
}