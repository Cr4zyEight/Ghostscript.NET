//
// GhostscriptViewerPsFormatHandler.cs
// This file is part of Ghostscript.NET library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013-2016 by Josip Habjan. All rights reserved.
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

using System.Globalization;
using Ghostscript.NET.Viewer.DSC;

namespace Ghostscript.NET.Viewer;

internal class GhostscriptViewerPsFormatHandler : GhostscriptViewerFormatHandler
{
    #region Constructor

    public GhostscriptViewerPsFormatHandler(GhostscriptViewer viewer) : base(viewer)
    {
    }

    #endregion

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            if (_tokenizer != null)
            {
                _tokenizer.Dispose();
                _tokenizer = null;
            }

        base.Dispose(disposing);
    }

    #endregion

    #region Initialize

    public override void Initialize()
    {
    }

    #endregion

    #region Open

    public override void Open(string filePath)
    {
        OpenPsFile(filePath);
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
    }

    #endregion

    #region StdError

    public override void StdError(string message)
    {
    }

    #endregion

    #region InitPage

    public override void InitPage(int pageNumber)
    {
    }

    #endregion

    #region ShowPage

    public override void ShowPage(int pageNumber)
    {
        CurrentPageNumber = pageNumber;

        DscToken pageToken = _pageTokens[pageNumber];
        DscToken pageEndToken;

        if (pageNumber == _pageTokens.Count)
            pageEndToken = _lastPageEnding;
        else
            pageEndToken = _pageTokens[pageNumber + 1];

        int pageSize = (int)(pageEndToken.StartPosition - pageToken.StartPosition);
        string pageContent = _tokenizer.ReadContent((int)pageToken.StartPosition, pageSize);

        Execute(pageContent);
    }

    #endregion

    #region OpenPsFile

    private void OpenPsFile(string path)
    {
        _tokenizer = new DscTokenizer(path);

        FirstPageNumber = 1;

        DscToken token = null;

        // loop through all DSC comments based on keyword
        while ((token = _tokenizer.GetNextDscKeywordToken()) != null)
            switch (token.Text)
            {
                case DscPages: // %%Pages: <numpages> | (atend)
                {
                    token = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace | DscTokenEnding.LineEnd);

                    // check if we need to ignore this comment because it's set at the end of the file
                    if (!string.IsNullOrWhiteSpace(token.Text) && token.Text != "(atend)" && !token.Text.StartsWith("%"))
                        // we got it, memorize it
                        LastPageNumber = int.Parse(token.Text);

                    break;
                }
                case DscBoundingbox: // { %%BoundingBox: <llx> <lly> <urx> <ury> } | (atend)
                {
                    try
                    {
                        DscToken llx = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace | DscTokenEnding.LineEnd);

                        if (!string.IsNullOrWhiteSpace(llx.Text) && llx.Text != "(atend)" && !llx.Text.StartsWith("%"))
                        {
                            DscToken lly = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace);
                            DscToken urx = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace);
                            DscToken ury = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace | DscTokenEnding.LineEnd);

                            BoundingBox = new GhostscriptRectangle(
                                float.Parse(llx.Text, CultureInfo.InvariantCulture),
                                float.Parse(lly.Text, CultureInfo.InvariantCulture),
                                float.Parse(urx.Text, CultureInfo.InvariantCulture),
                                float.Parse(ury.Text, CultureInfo.InvariantCulture));
                        }
                    }
                    catch
                    {
                    }

                    break;
                }
                case DscPage: // %%Page: <label> <ordinal>
                {
                    // label can be anything, we need to get oridinal which is the last
                    // value of the line
                    DscToken pageNumberToken;

                    // loop through each comment value
                    while ((pageNumberToken = _tokenizer.GetNextDscValueToken(DscTokenEnding.Whitespace | DscTokenEnding.LineEnd)) != null)
                        // check if this is the last comment value in this line
                        if (pageNumberToken.Ending == DscTokenEnding.LineEnd)
                        {
                            // we got it, add this comment keyword to the page list
                            _pageTokens.Add(int.Parse(pageNumberToken.Text), token);
                            break;
                        }

                    break;
                }
                case DscTrailer: // %%Trailer (no keywords)
                {
                    // if the postscript is well formatted, we should get this one
                    // save this comment so we can know the position when the last page is ending
                    _lastPageEnding = token;

                    break;
                }
                case DscEof: // %%EOF (no keywords)
                {
                    // check if we already know where the last page is ending
                    if (_lastPageEnding == null)
                        // we don't know, use start of the %%EOF comment as the last page ending position 
                        _lastPageEnding = token;

                    break;
                }
            }

        // check if we didn't find %%Trailer or %%EOF comment
        if (_lastPageEnding == null)
        {
            // it seems that the last page goes to the end of the file, set the last page ending
            // position to the complete file size value
            _lastPageEnding = new DscToken();
            _lastPageEnding.StartPosition = _tokenizer.FileSize;
        }

        // we did'n find %%Pages comment, set the last page number to 1
        if (LastPageNumber == 0) LastPageNumber = 1;

        // check if we didn't find any %%Page comment
        if (_pageTokens.Count == 0)
            // create dummy one that will point to the first byte in the file
            _pageTokens.Add(1, new DscToken { StartPosition = 0 });

        // hpd = Header, Procedure definitions, Document setup
        // start position of the first %%Page: comment is te hpd size
        int hpdSize = (int)_pageTokens[1].StartPosition;
        // get the hpd text
        string hpdContent = _tokenizer.ReadContent(0, hpdSize);

        // process header, procedure definitions and document setup

        if (string.IsNullOrWhiteSpace(hpdContent)) hpdContent = "%!PS-Adobe-3.0";

        Execute(hpdContent);
    }

    #endregion

    #region Private constants

    private const string DscPages = "%%Pages:";
    private const string DscPage = "%%Page:";
    private const string DscBoundingbox = "%%BoundingBox:";
    private const string DscPageboundingbox = "%%PageBoundingBox:";
    private const string DscTrailer = "%%Trailer";
    private const string DscEof = "%%EOF";

    #endregion

    #region Private variables

    private DscTokenizer _tokenizer;
    private readonly Dictionary<int, DscToken> _pageTokens = new();
    private DscToken _lastPageEnding;

    #endregion
}