﻿//
// GhostscriptRasterizer.cs
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

using System.Drawing;
using Ghostscript.NET.Viewer;

namespace Ghostscript.NET.Rasterizer;

public class GhostscriptRasterizer : IDisposable
{
    #region Constructor - viewerInstance

    public GhostscriptRasterizer(GhostscriptViewer viewerInstance)
    {
        if (viewerInstance == null) throw new ArgumentNullException("viewerInstance");

        _viewer = viewerInstance;
        _gsViewState = _viewer.SaveState();
        _viewer.ProgressiveUpdate = false;
        _viewer.DisplayPage += _viewer_DisplayPage;
    }

    #endregion

    #region PageCount

    /// <summary>
    /// Gets PDF page count.
    /// </summary>
    public int PageCount => _viewer.LastPageNumber;

    #endregion

    #region GraphicsAlphaBits

    public int GraphicsAlphaBits
    {
        get => _viewer.GraphicsAlphaBits;
        set => _viewer.GraphicsAlphaBits = value;
    }

    #endregion

    #region TextAlphaBits

    public int TextAlphaBits
    {
        get => _viewer.TextAlphaBits;
        set => _viewer.TextAlphaBits = value;
    }

    #endregion

    #region EPSClip

    public bool EpsClip
    {
        get => _viewer.EpsClip;
        set => _viewer.EpsClip = value;
    }

    #endregion

    #region CustomSwitches

    public List<string> CustomSwitches
    {
        get => _viewer.CustomSwitches;
        set => _viewer.CustomSwitches = value;
    }

    #endregion

    #region Destructor

    ~GhostscriptRasterizer()
    {
        Dispose(false);
    }

    #endregion

    #region Open - stream

    public void Open(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        Open(stream, GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.Gpl | GhostscriptLicense.Afpl, GhostscriptLicense.Gpl), false);
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

        if (_gsViewState == null) _viewer.Open(stream, versionInfo, dllFromMemory);
    }

    #endregion

    #region Open - path, versionInfo, dllFromMemory

    public void Open(string path, GhostscriptVersionInfo versionInfo, bool dllFromMemory)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Could not find input file.", path);

        if (versionInfo == null) throw new ArgumentNullException("versionInfo");

        if (_gsViewState == null) _viewer.Open(path, versionInfo, dllFromMemory);
    }

    #endregion

    #region Open - stream, library

    public void Open(Stream stream, byte[] library)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        if (library == null) throw new ArgumentNullException("library");

        if (_gsViewState == null) _viewer.Open(stream, library);
    }

    #endregion

    #region Open - path, library

    public void Open(string path, byte[] library)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Couldn't find input file.", path);

        if (library == null) throw new ArgumentNullException("library");

        if (_gsViewState == null) _viewer.Open(path, library);
    }

    #endregion

    #region Close

    /// <summary>
    /// Close the GhostscriptRasterizer.
    /// </summary>
    public void Close()
    {
        if (_gsViewState == null) _viewer.Close();
    }

    #endregion

    #region GetPage

    /// <summary>
    /// Gets PDF page as System.Drawing.Image.
    /// </summary>
    /// <param name="dpi">Desired dpi.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <returns>PDF page represented as System.Drawing.Image.</returns>
    public Image GetPage(int dpi, int pageNumber)
    {
        _viewer.Dpi = dpi;
        _viewer.ShowPage(pageNumber, true);
        return _lastRasterizedImage;
    }

    #endregion

    #region _viewer_DisplayPage

    private void _viewer_DisplayPage(object sender, GhostscriptViewerViewEventArgs e)
    {
        if (e.Image != null) _lastRasterizedImage = e.Image.Clone() as Image;
    }

    #endregion

    #region Private variables

    private bool _disposed;
    private GhostscriptViewer _viewer;
    private Image _lastRasterizedImage;
    private readonly GhostscriptViewerState _gsViewState;

    #endregion

    #region Constructor

    public GhostscriptRasterizer(GhostscriptStdIo stdIo)
    {
        _viewer = new GhostscriptViewer();
        _viewer.ShowPageAfterOpen = false;
        _viewer.ProgressiveUpdate = false;
        _viewer.DisplayPage += _viewer_DisplayPage;

        if (stdIo != null) _viewer.AttachStdIo(stdIo);
    }

    public GhostscriptRasterizer()
        : this(default(GhostscriptStdIo))
    {
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
                if (_gsViewState == null)
                {
                    if (_viewer != null)
                    {
                        _viewer.Dispose();
                        _viewer = null;
                    }
                }
                else
                {
                    _viewer.RestoreState(_gsViewState);
                }
            }

            _disposed = true;
        }
    }

    #endregion

    #endregion
}