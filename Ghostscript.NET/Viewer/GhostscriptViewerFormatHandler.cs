//
// GhostscriptViewerFormatHandler.cs
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

namespace Ghostscript.NET.Viewer;

internal abstract class GhostscriptViewerFormatHandler : IDisposable
{
    #region Constructor

    public GhostscriptViewerFormatHandler(GhostscriptViewer viewer)
    {
        Viewer = viewer;
    }

    #endregion

    #region Viewer

    public GhostscriptViewer Viewer { get; }

    #endregion

    #region FirstPageNumber

    public int FirstPageNumber { get; set; }

    #endregion

    #region LastPageNumber

    public int LastPageNumber { get; set; }

    #endregion

    #region CurrentPageNumber

    public int CurrentPageNumber { get; internal set; } = 1;

    #endregion

    #region MediaBox

    public GhostscriptRectangle MediaBox { get; set; } = GhostscriptRectangle.Empty;

    #endregion

    #region BoundingBox

    public GhostscriptRectangle BoundingBox { get; set; } = GhostscriptRectangle.Empty;

    #endregion

    #region CropBox

    public GhostscriptRectangle CropBox { get; set; } = GhostscriptRectangle.Empty;

    #endregion

    #region IsMediaBoxSet

    public bool IsMediaBoxSet => MediaBox != GhostscriptRectangle.Empty;

    #endregion

    #region IsBoundingBoxSet

    public bool IsBoundingBoxSet => BoundingBox != GhostscriptRectangle.Empty;

    #endregion

    #region IsCropBoxSet

    public bool IsCropBoxSet => CropBox != GhostscriptRectangle.Empty;

    #endregion

    #region PageOrientation

    public GhostscriptPageOrientation PageOrientation { get; set; } = GhostscriptPageOrientation.Portrait;

    #endregion

    #region ShowPageInvoked

    internal bool ShowPagePostScriptCommandInvoked { get; set; }

    #endregion

    #region Destructor

    ~GhostscriptViewerFormatHandler()
    {
        Dispose(false);
    }

    #endregion

    #region Execute

    public int Execute(string str)
    {
        return Viewer.Interpreter.Run(str);
    }

    #endregion

    #region Execute

    internal int Execute(IntPtr str)
    {
        return Viewer.Interpreter.Run(str);
    }

    #endregion

    #region Private variables

    private bool _disposed;

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
            }

            _disposed = true;
        }
    }

    #endregion

    #endregion

    #region Abstract methods

    public abstract void Initialize();
    public abstract void Open(string filePath);
    public abstract void StdInput(out string input, int count);
    public abstract void StdOutput(string message);
    public abstract void StdError(string message);
    public abstract void InitPage(int pageNumber);
    public abstract void ShowPage(int pageNumber);

    #endregion
}