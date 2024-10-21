//
// GhostscriptDisplayDeviceHandler.cs
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
using System.Runtime.InteropServices;

namespace Ghostscript.NET
{
    /// <summary>
    /// Represents a base Ghostscript Display Device handler.
    /// </summary>
    public abstract class GhostscriptDisplayDeviceHandler
    {

        #region Internal variables

        internal DisplayCallback Callback;

        internal GhostscriptLibrary Gs;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Ghostscript.NET.GhostscriptDisplayDeviceHandler class.
        /// </summary>
        public GhostscriptDisplayDeviceHandler(GhostscriptLibrary gs)
        {
            Gs = gs;

            if (gs.Revision > 951)
            {
                Callback = new DisplayCallbackV3();
                Callback.version_minor = Gdevdsp.DisplayVersionMinorV3;
                Callback.version_major = Gdevdsp.DisplayVersionMajorV3;
                Callback.size = Marshal.SizeOf(typeof(DisplayCallbackV3));
            }
            else
            {
                Callback = new DisplayCallback();
                Callback.version_minor = Gdevdsp.DisplayVersionMinorV2;
                Callback.version_major = Gdevdsp.DisplayVersionMajorV2;
                Callback.size = Marshal.SizeOf(typeof(DisplayCallback));
            }

            Callback.display_open = new DisplayOpenCallback(display_open);
            Callback.display_preclose = new DisplayPrecloseCallback(display_preclose);
            Callback.display_close = new DisplayCloseCallback(display_close);
            Callback.display_presize = new DisplayPresizeCallback(display_presize);
            Callback.display_size = new DisplaySizeCallback(display_size);
            Callback.display_sync = new DisplaySyncCallback(display_sync);
            Callback.display_page = new DisplayPageCallback(display_page);
            Callback.display_update = new DisplayUpdateCallback(display_update);

            //_callback.display_memalloc = new display_memalloc_callback(display_memalloc);
            //_callback.display_memfree = new display_memfree_callback(display_memfree);
            //_callback.display_separation = new display_separation_callback(display_separation);
        }

        #endregion

        #region display_open

        private int display_open(IntPtr handle, IntPtr device)
        {
            return this.Open(handle, device);
        }

        #endregion

        #region display_preclose

        private int display_preclose(IntPtr handle, IntPtr device)
        {
            return this.Preclose(handle, device);
        }

        #endregion

        #region display_close

        private int display_close(IntPtr handle, IntPtr device)
        {
            return this.Close(handle, device);
        }

        #endregion

        #region display_presize

        private int display_presize(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format)
        {
            return this.Presize(handle, device, width, height, raster, format);
        }

        #endregion

        #region display_size

        private int display_size(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format, IntPtr pimage)
        {
            return this.Size(handle, device, width, height, raster, format, pimage);
        }

        #endregion

        #region display_sync

        private int display_sync(IntPtr handle, IntPtr device)
        {
            return this.Sync(handle, device);
        }

        #endregion

        #region display_page

        private unsafe int display_page(IntPtr handle, IntPtr device, Int32 copies, Int32 flush)
        {
            return this.Page(handle, device, copies, flush);
        }

        #endregion

        #region display_update

        private int display_update(IntPtr handle, IntPtr device, Int32 x, Int32 y, Int32 w, Int32 h)
        {
            return this.Update(handle, device, x, y, w, h);
        }

        #endregion

        #region display_memalloc

        private void display_memalloc(IntPtr handle, IntPtr device, UInt32 size)
        {
            
        }

        #endregion

        #region display_memfree

        private int display_memfree(IntPtr handle, IntPtr device, IntPtr mem)
        {
            return 0;
        }

        #endregion

        #region display_separation

        private int display_separation(IntPtr handle, IntPtr device, Int32 component, string componentName, UInt16 c, UInt16 m, UInt16 y, UInt16 k)
        {
            return 0;
        }

        #endregion

        #region Abstract functions

        public abstract int Open(IntPtr handle, IntPtr device);
        public abstract int Preclose(IntPtr handle, IntPtr device);
        public abstract int Close(IntPtr handle, IntPtr device);
        public abstract int Presize(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format);
        public abstract int Size(IntPtr handle, IntPtr device, Int32 width, Int32 height, Int32 raster, UInt32 format, IntPtr pimage);
        public abstract int Sync(IntPtr handle, IntPtr device);
        public abstract int Page(IntPtr handle, IntPtr device, Int32 copies, Int32 flush);
        public abstract int Update(IntPtr handle, IntPtr device, Int32 x, Int32 y, Int32 w, Int32 h);

        #endregion

    }
}
