//
// AddWatermarkSample.cs
// This file is part of Ghostscript.NET.Samples project
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
using System.Diagnostics;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.IO;

using Ghostscript.NET.FacturX.ZUGFeRD;


namespace Ghostscript.NET.Samples
{
    public class FacturXWriteSample : ISample
    {


        public void Start()
        {
            
            Invoice i = (new Invoice()).SetDueDate(DateTime.Now).SetIssueDate(DateTime.Now).SetDeliveryDate(DateTime.Now).SetSender((new TradeParty("Test company", "teststr", "55232", "teststadt", "DE")).AddTaxId("DE4711").AddVatId("DE0815").SetContact(new Contact("Hans Test", "+49123456789", "test@example.org")).AddBankDetails(new BankDetails("DE12500105170648489890", "COBADEFXXX"))).SetRecipient(new TradeParty("Franz Müller", "teststr.12", "55232", "Entenhausen", "DE")).SetReferenceNumber("991-01484-64").SetNumber("123").
                    AddItem(new Item(new Product("Testprodukt", "", "C62", new decimal(19)), decimal.One, decimal.One));

            ZUGFeRD2PullProvider zf2P = new ZUGFeRD2PullProvider();
            zf2P.SetProfile(Profiles.GetByName("XRechnung"));
            zf2P.GenerateXml(i);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string outfilename = "xrechnung.xml";
            File.WriteAllBytes(outfilename, zf2P.GetXml());
        }

    }
}
