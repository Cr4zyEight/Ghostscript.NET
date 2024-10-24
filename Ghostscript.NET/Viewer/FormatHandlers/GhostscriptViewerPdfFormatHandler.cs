//
// GhostscriptViewerPdfFormatHandler.cs
// This file is part of Ghostscript.NET library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2023 by Josip Habjan. All rights reserved.
//
// Author ported some parts of this code from GSView. 
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

using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Ghostscript.NET.Exceptions;
using Ghostscript.NET.gs;
using Ghostscript.NET.Helpers;

namespace Ghostscript.NET.Viewer.FormatHandlers;

internal class GhostscriptViewerPdfFormatHandler : GhostscriptViewerFormatHandler
{
    #region Constructor

    public GhostscriptViewerPdfFormatHandler(GhostscriptViewer viewer) : base(viewer)
    {
    }

    #endregion

    #region Initialize

    public override void Initialize()
    {
        // define our routine for preparing to show a page.  
        // This writes out some tags which we capture in the 
        // callback to obtain the page size and orientation. 
        Execute(string.Format(@"
                /GSNETViewer_PDFpage {{ 
        	        ({0}) print dup == flush 
        	        pdfgetpage /Page exch store 
                    Page /MediaBox pget 
                    {{ ({1}) print == flush  }} 
        	        if 
                    Page /CropBox pget 
         	        {{ ({2}) print == flush }} 
        	        if 
                    Page /Rotate pget not {{ 0 }} if 
        	        ({3}) print == flush 
                }} def", PdfPageTag, PdfMediaTag, PdfCropTag, PdfRotateTag));

        // put these in userdict so we can write to them later
        Execute(@"
                    /Page null def
                    /Page# 0 def
                    /PDFSave null def
                    /DSCPageCount 0 def
                ");

        if (Viewer.Interpreter.LibraryRevision < 927)
            // this should be executed only for gs versions below 9.27
            // open PDF support dictionaries
            Execute(@"
                    GS_PDF_ProcSet begin
                    pdfdict begin");
    }

    #endregion

    #region Open

    public override void Open(string filePath)
    {
        int res = 0;

        if (StringHelper.HasNonAsciiChars(filePath))
        {
            IntPtr ptrStr = StringHelper.NativeUtf8FromString(string.Format("({0}) (r) file runpdfbegin", filePath.Replace("\\", "/")));

            // open PDF file
            res = Execute(ptrStr);

            Marshal.FreeHGlobal(ptrStr);
        }
        else
        {
            // open PDF file
            res = Execute(string.Format("({0}) (r) file runpdfbegin", filePath.Replace("\\", "/")));
        }

        if (res == Ierrors.EIoerror)
            throw new GhostscriptException("IO error for file: '" + filePath + "'", Ierrors.EIoerror);
        if (res == Ierrors.EInvalidfileaccess) throw new GhostscriptException("IO security problem (access control failure) for file: '" + filePath + "'", Ierrors.EIoerror);

        Execute("/FirstPage where { pop FirstPage } { 1 } ifelse");
        Execute("/LastPage where { pop LastPage } { pdfpagecount } ifelse");

        // flush stdout and then send PDF page marker to stdout where we capture the page numbers via callback
        Execute(string.Format("flush ({0}) print exch =only ( ) print =only (\n) print flush", PdfPagesTag));

        // fixes problem with the invisible layers
        // if we don't run that code, then optional content groups will be left unmarked and always processed
        Execute("process_trailer_attrs\n");
    }

    #endregion

    #region StdInput

    public override void StdInput(out string input, int count)
    {
        input = string.Empty;
    }

    #endregion

    #region StdOutput

    public override void StdOutput(string message)
    {
        if (message.Contains(PdfTag))
        {
            int startPos = message.IndexOf(PdfTag);
            int endPos = message.IndexOf(": ");

            string tag = message.Substring(startPos, endPos - startPos + 2);
            string rest = message.Substring(endPos + 2, message.Length - endPos - 2);

            switch (tag)
            {
                case PdfPagesTag:
                {
                    string[] pages = rest.Split(' ');

                    int first, last;

                    if (pages.Length >= 2 && int.TryParse(pages[0], out first) && int.TryParse(pages[1], out last))
                    {
                        FirstPageNumber = first;
                        LastPageNumber = last;
                    }

                    break;
                }
                case PdfPageTag:
                {
                    int number;

                    if (int.TryParse(rest, out number)) CurrentPageNumber = number;

                    break;
                }
                case PdfMediaTag:
                {
                    string[] mb = rest.Split(' ');

                    float llx, lly, urx, ury;

                    if (mb.Length >= 4 &&
                        float.TryParse(mb[0].TrimStart('['), NumberStyles.Float, CultureInfo.InvariantCulture, out llx) &&
                        float.TryParse(mb[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lly) &&
                        float.TryParse(mb[2], NumberStyles.Float, CultureInfo.InvariantCulture, out urx) &&
                        float.TryParse(mb[3].TrimEnd(']'), NumberStyles.Float, CultureInfo.InvariantCulture, out ury))
                        MediaBox = new GhostscriptRectangle(llx, lly, urx, ury);

                    break;
                }
                case PdfCropTag:
                {
                    string[] cb = rest.Split(' ');

                    float llx, lly, urx, ury;

                    if (cb.Length >= 4 &&
                        float.TryParse(cb[0].TrimStart('['), NumberStyles.Float, CultureInfo.InvariantCulture, out llx) &&
                        float.TryParse(cb[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lly) &&
                        float.TryParse(cb[2], NumberStyles.Float, CultureInfo.InvariantCulture, out urx) &&
                        float.TryParse(cb[3].TrimEnd(']'), NumberStyles.Float, CultureInfo.InvariantCulture, out ury))
                        CropBox = new GhostscriptRectangle(llx, lly, urx, ury);

                    break;
                }
                case PdfRotateTag:
                {
                    int rotate;

                    if (int.TryParse(rest, out rotate))
                    {
                        while (rotate < 0) rotate += 360;

                        while (rotate >= 360) rotate -= 360;

                        switch (rotate)
                        {
                            case 90:
                                PageOrientation = GhostscriptPageOrientation.Landscape;
                                break;
                            case 180:
                                PageOrientation = GhostscriptPageOrientation.UpsideDown;
                                break;
                            case 270:
                                PageOrientation = GhostscriptPageOrientation.Seascape;
                                break;
                            default:
                                PageOrientation = GhostscriptPageOrientation.Portrait;
                                break;
                        }
                    }

                    break;
                }
            }
        }
    }

    #endregion

    #region StdError

    public override void StdError(string message)
    {
        Debug.WriteLine($"GS:StdError > {message}");
    }

    #endregion

    #region InitPage

    public override void InitPage(int pageNumber)
    {
        if (pageNumber >= FirstPageNumber && pageNumber <= LastPageNumber)
        {
            Execute(string.Format("{0} GSNETViewer_PDFpage", pageNumber));

            if (Viewer.Interpreter.LibraryRevision >= 10010) Execute("Page pdfshowpage_init");
        }
        else
        {
            throw new GhostscriptException("The page number falls outside the range of valid page numbers!");
        }
    }

    #endregion

    #region ShowPage

    public override void ShowPage(int pageNumber)
    {
        if (pageNumber >= FirstPageNumber && pageNumber <= LastPageNumber)
        {
            if (Viewer.Interpreter.LibraryRevision >= 10010)
                Execute("Page pdfshowpage_finish");
            else
                Execute("Page pdfshowpage");
        }
        else
        {
            throw new GhostscriptException("The page number falls outside the range of valid page numbers!");
        }
    }

    #endregion

    #region Private constants

    private const string PdfTag = "%GSNET";
    private const string PdfPagesTag = "%GSNET_VIEWER_PDF_PAGES: ";
    private const string PdfPageTag = "%GSNET_VIEWER_PDF_PAGE: ";
    private const string PdfMediaTag = "%GSNET_VIEWER_PDF_MEDIA: ";
    private const string PdfCropTag = "%GSNET_VIEWER_PDF_CROP: ";
    private const string PdfRotateTag = "%GSNET_VIEWER_PDF_ROTATE: ";
    private const string PdfDoneTag = "%GSNET_VIEWER_PDF_DONE: ";
    private const string PdfMarkTag = "%GSNET_VIEWER_PDF_MARK: ";

    #endregion
}