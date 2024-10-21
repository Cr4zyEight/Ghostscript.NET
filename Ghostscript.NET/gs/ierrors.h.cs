//
// ierrors.h.cs
// This file is part of Ghostscript.NET library
//
// Author: Josip Habjan= habjan@gmail.com, http://www.linkedin.com/in/habjan; 
// Copyright (c) 2013-2016 by Josip Habjan. All rights reserved.
//
// Author ported parts of this code from AFPL Ghostscript. 
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files= the
// "Software";, to deal in the Software without restriction, including
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

namespace Ghostscript.NET
{
    public partial class Ierrors
    {
        static Ierrors()
        {
            ErrorNames.Add("not error");
            ErrorNames.AddRange(Level1ErrorNames);
            ErrorNames.AddRange(Level2ErrorNames);
            ErrorNames.AddRange(DpsErrorNames);
        }

        // PostScript Level 1 errors

        public const int EUnknownerror = -1;	/* unknown error */
        public const int EDictfull = -2;
        public const int EDictstackoverflow = -3;
        public const int EDictstackunderflow = -4;
        public const int EExecstackoverflow = -5;
        public const int EInterrupt = -6;
        public const int EInvalidaccess = -7;
        public const int EInvalidexit = -8;
        public const int EInvalidfileaccess = -9;
        public const int EInvalidfont = -10;
        public const int EInvalidrestore = -11;
        public const int EIoerror = -12;
        public const int ELimitcheck = -13;
        public const int ENocurrentpoint = -14;
        public const int ERangecheck = -15;
        public const int EStackoverflow = -16;
        public const int EStackunderflow = -17;
        public const int ESyntaxerror = -18;
        public const int ETimeout = -19;
        public const int ETypecheck = -20;
        public const int EUndefined = -21;
        public const int EUndefinedfilename = -22;
        public const int EUndefinedresult = -23;
        public const int EUnmatchedmark = -24;
        public const int EVMerror = -25;		/* must be the last Level 1 error */

        public static string[] Level1ErrorNames = {
                 "unknownerror", "dictfull", "dictstackoverflow", "dictstackunderflow",
                 "execstackoverflow", "interrupt", "invalidaccess", "invalidexit",
                 "invalidfileaccess", "invalidfont", "invalidrestore", "ioerror",
                 "limitcheck", "nocurrentpoint", "rangecheck", "stackoverflow",
                 "stackunderflow", "syntaxerror", "timeout", "typecheck", "undefined",
                 "undefinedfilename", "undefinedresult", "unmatchedmark", "VMerror" };


        // Additional Level 2 errors (also in DPS)

        public const int EConfigurationerror = -26;
        public const int EUndefinedresource = -27;
        public const int EUnregistered = -28;

        public static string[] Level2ErrorNames = { "configurationerror", "undefinedresource", "unregistered" };

        // Additional DPS errors

        public const int EInvalidcontext = -29;

        // invalidid is for the NeXT DPS extension
        
        public const int EInvalidid = -30;

        public static string[] DpsErrorNames = { "invalidcontext", "invalidid" };

        public static List<string> ErrorNames = new List<string>();


        // Pseudo-errors used internally

        /// <summary>
        /// Internal code for a fatal error.
        /// gs_interpret also returns this for a .quit with a positive exit code.
        /// </summary>
        public const int EFatal = -100;

        /// <summary>
        /// Internal code for the .quit operator.
        /// The real quit code is an integer on the operand stack.
        /// gs_interpret returns this only for a .quit with a zero exit code.
        /// </summary>
        public const int EQuit = -101;

        /// <summary>
        /// Internal code for a normal exit from the interpreter.
        /// Do not use outside of interp.c.
        /// </summary>
        public const int EInterpreterExit = -102;

        /// <summary>
        /// Internal code that indicates that a procedure has been stored in the
        /// remap_proc of the graphics state, and should be called before retrying
        /// the current token.  This is used for color remapping involving a call
        /// back into the interpreter -- inelegant, but effective.
        /// </summary>
        public const int ERemapColor = -103;

        /// <summary>
        /// Internal code to indicate we have underflowed the top block
        /// of the e-stack.
        /// </summary>
        public const int EExecStackUnderflow = -104;

        /// <summary>
        /// Internal code for the vmreclaim operator with a positive operand.
        /// We need to handle this as an error because otherwise the interpreter
        /// won't reload enough of its state when the operator returns.
        /// </summary>
        public const int EVMreclaim = -105;

        /// <summary>
        /// Internal code for requesting more input from run_string.
        /// </summary>
        public const int ENeedInput = -106;

        /// <summary>
        /// Internal code for a normal exit when usage info is displayed.
        /// This allows Window versions of Ghostscript to pause until
        /// the message can be read.
        /// </summary>
        public const int EInfo = -110;

    }
}
