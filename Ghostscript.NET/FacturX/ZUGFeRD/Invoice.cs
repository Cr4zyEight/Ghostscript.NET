/**
 * *********************************************************************
 * <p>
 * Copyright 2018 Jochen Staerk
 * <p>
 * Use is subject to license terms.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at http://www.apache.org/licenses/LICENSE-2.0.
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * <p>
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * <p>
 * **********************************************************************
 */
using System;
using System.Collections.Generic;


namespace Ghostscript.NET.FacturX.ZUGFeRD
{
    /// <summary>
    ///*
    /// An invoice, with fluent setters </summary>
    /// <seealso cref="IExportableTransaction if you want to implement an interface instead"/>
    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class Invoice implements IExportableTransaction
    public class Invoice : IExportableTransaction
    {
        protected internal string DocumentName = null, DocumentCode = null, Number = null, OwnOrganisationFullPlaintextInfo = null, ReferenceNumber = null, ShipToOrganisationId = null, ShipToOrganisationName = null, ShipToStreet = null, ShipToZip = null, ShipToLocation = null, ShipToCountry = null, BuyerOrderReferencedDocumentId = null, InvoiceReferencedDocumentId = null, BuyerOrderReferencedDocumentIssueDateTime = null, OwnForeignOrganisationId = null, OwnOrganisationName = null, Currency = null, PaymentTermDescription = null;
        protected internal DateTime? IssueDate = null, DueDate = null;
        protected DateTime? DeliveryDate = null;
        protected internal decimal? TotalPrepaidAmount = null;
        protected internal TradeParty Sender = null, Recipient = null, DeliveryAddress = null;
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @JsonDeserialize(contentAs=Item.class) protected java.util.ArrayList<IZUGFeRDExportableItem> ZFItems = null;
        protected internal List<IZUGFeRDExportableItem> ZfItems = null;
        protected internal List<string> Notes = null;
        protected internal string SellerOrderReferencedDocumentId;
        protected internal string ContractReferencedDocument = null;
        //protected internal List<FileAttachment> xmlEmbeddedFiles = null;

        protected internal DateTime? DetailedDeliveryDateStart = null;
        protected internal DateTime? DetailedDeliveryPeriodEnd = null;

        //protected internal List<IZUGFeRDAllowanceCharge> Allowances = new List<IZUGFeRDAllowanceCharge>(), Charges = new List<IZUGFeRDAllowanceCharge>(), LogisticsServiceCharges = new List<IZUGFeRDAllowanceCharge>();
        protected internal IZUGFeRDPaymentTerms PaymentTerms = null;
        protected internal DateTime InvoiceReferencedIssueDate;
        protected internal string SpecifiedProcuringProjectId = null;
        protected internal string SpecifiedProcuringProjectName = null;

        public Invoice()
        {
            ZfItems = new List<IZUGFeRDExportableItem>();
            SetCurrency("EUR");
        }




        public string GetDocumentName()
        {
            return !string.IsNullOrWhiteSpace(DocumentName) ? DocumentName : "RECHNUNG";
        }

        public string GetContractReferencedDocument()
        {
            return ContractReferencedDocument;
        }

        public virtual Invoice SetDocumentName(string documentName)
        {
            this.DocumentName = documentName;
            return this;
        }

        public virtual Invoice SetDocumentCode(string documentCode)
        {
            this.DocumentCode = documentCode;
            return this;
        }
        /*
            public virtual Invoice embedFileInXML(FileAttachment fa)
            {
                if (xmlEmbeddedFiles == null)
                {
                    xmlEmbeddedFiles = new List<>();
                }
                xmlEmbeddedFiles.add(fa);
                return this;
            }

            public override FileAttachment[] getAdditionalReferencedDocuments()
            {

                    if (xmlEmbeddedFiles == null)
                    {
                        return null;
                    }
                    return xmlEmbeddedFiles.toArray(new FileAttachment[0]);


            }
            */
        public IZUGFeRDExportableTradeParty GetRecipient()
        {
            return Recipient;
        }

        public IZUGFeRDTradeSettlementPayment[] GetTradeSettlementPayment()
        {
            return null;
        }

        /// <summary>
        /// required.
        /// sets the invoice receiving institution = invoicee </summary>
        /// <param name="recipient"> the invoicee organisation </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetRecipient(TradeParty recipient)
        {
            this.Recipient = recipient;
            return this;
        }


        public string GetNumber()
        {
            return Number;
        }

        public virtual Invoice SetNumber(string number)
        {
            this.Number = number;
            return this;
        }


        /***
         * switch type to invoice correction and refer to document number.
         * Please note that the quantities need to be negative, if you
         * e.g. delivered 100 and take 50 back the quantity should be -50 in the
         * corrected invoice, which will result in negative VAT and a negative payment amount
         * @param number the invoice number to be corrected
         * @return this object (fluent setter)
         */
        public virtual Invoice SetCorrection(string number)
        {
            SetInvoiceReferencedDocumentId(number);
            DocumentCode = "384";
            return this;
        }
        public virtual Invoice SetCreditNote()
        {
            DocumentCode = "381";
            return this;
        }

        public string GetOwnOrganisationFullPlaintextInfo()
        {
            return OwnOrganisationFullPlaintextInfo;
        }

        public virtual Invoice SetOwnOrganisationFullPlaintextInfo(string ownOrganisationFullPlaintextInfo)
        {
            this.OwnOrganisationFullPlaintextInfo = ownOrganisationFullPlaintextInfo;
            return this;
        }

        public bool RebateAgreementExists()
        {
            return false;
        }

        public string GetReferenceNumber()
        {
            return ReferenceNumber;
        }

        public virtual Invoice SetReferenceNumber(string referenceNumber)
        {
            this.ReferenceNumber = referenceNumber;
            return this;
        }

        public string GetShipToOrganisationId()
        {
            return ShipToOrganisationId;

        }

        public string GetShipToOrganisationName()
        {
            return ShipToOrganisationName;
        }

        public virtual Invoice SetShipToOrganisationId(string shipToOrganisationId)
        {
            this.ShipToOrganisationId = shipToOrganisationId;
            return this;
        }

        public virtual Invoice SetShipToOrganisationName(string shipToOrganisationName)
        {
            this.ShipToOrganisationName = shipToOrganisationName;
            return this;
        }

        public string GetShipToStreet()
        {

            return ShipToStreet;

        }


        public virtual Invoice SetShipToStreet(string shipToStreet)
        {
            this.ShipToStreet = shipToStreet;
            return this;
        }

        public string GetShipToZip()
        {

            return ShipToZip;

        }

        public string GetShipToLocation()
        {
            return ShipToLocation;
        }

        public string GetShipToCountry()
        {
            return ShipToCountry;
        }

        public string GetSellerOrderReferencedDocumentId()
        {
            return SellerOrderReferencedDocumentId;
        }

        public string GetBuyerOrderReferencedDocumentId()
        {
            return BuyerOrderReferencedDocumentId;
        }

        public virtual Invoice SetShipToZip(string shipToZip)
        {
            this.ShipToZip = shipToZip;
            return this;
        }

        public virtual Invoice SetShipToLocation(string shipToLocation)
        {
            this.ShipToLocation = shipToLocation;
            return this;
        }

        public virtual Invoice SetShipToCountry(string shipToCountry)
        {
            this.ShipToCountry = shipToCountry;
            return this;
        }

        public virtual Invoice SetSellerOrderReferencedDocumentId(string sellerOrderReferencedDocumentId)
        {
            this.SellerOrderReferencedDocumentId = sellerOrderReferencedDocumentId;
            return this;
        }

        /// <summary>
        ///*
        /// usually the order number </summary>
        /// <param name="buyerOrderReferencedDocumentId"> string with number </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetBuyerOrderReferencedDocumentId(string buyerOrderReferencedDocumentId)
        {
            this.BuyerOrderReferencedDocumentId = buyerOrderReferencedDocumentId;
            return this;
        }

        /// <summary>
        ///*
        /// usually in case of a correction the original invoice number </summary>
        /// <param name="invoiceReferencedDocumentId"> string with number </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetInvoiceReferencedDocumentId(string invoiceReferencedDocumentId)
        {
            this.InvoiceReferencedDocumentId = invoiceReferencedDocumentId;
            return this;
        }
        public string GetInvoiceReferencedDocumentId()
        {

            return InvoiceReferencedDocumentId;

        }

        public DateTime? GetInvoiceReferencedIssueDate()
        {
            return InvoiceReferencedIssueDate;
        }

        public virtual Invoice SetInvoiceReferencedIssueDate(DateTime issueDate)
        {
            this.InvoiceReferencedIssueDate = issueDate;
            return this;
        }

        public string GetBuyerOrderReferencedDocumentIssueDateTime()
        {

            return BuyerOrderReferencedDocumentIssueDateTime;

        }

        /// <summary>
        ///*
        /// when the order (or whatever reference in BuyerOrderReferencedDocumentID) was issued (@todo switch to date?) </summary>
        /// <param name="buyerOrderReferencedDocumentIssueDateTime">  IssueDateTime in format CCYY-MM-DDTHH:MM:SS </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetBuyerOrderReferencedDocumentIssueDateTime(string buyerOrderReferencedDocumentIssueDateTime)
        {
            this.BuyerOrderReferencedDocumentIssueDateTime = buyerOrderReferencedDocumentIssueDateTime;
            return this;
        }
        public IZUGFeRDExportableTradeParty GetSender()
        {
            return Sender;
        }

        public string GetSubjectNote()
        {
            return null;
        }

        public string GetOwnTaxId()
        {
            return GetSender()?.GetTaxId();
        }


        [Obsolete]
        public virtual Invoice SetOwnTaxId(string ownTaxId)
        {
            Sender.AddTaxId(ownTaxId);
            return this;
        }

        public string GetOwnVatid()
        {

            return GetSender()?.GetVatid();

        }

        [Obsolete]
        public virtual Invoice SetOwnVatid(string ownVatid)
        {
            Sender.AddVatid(ownVatid);
            return this;
        }

        public string GetOwnForeignOrganisationId()
        {

            return OwnForeignOrganisationId;

        }




        public virtual void SetZfItems(List<IZUGFeRDExportableItem> ims)
        {
            ZfItems = ims;
        }

        [Obsolete]
        public virtual Invoice SetOwnForeignOrganisationId(string ownForeignOrganisationId)
        {
            this.OwnForeignOrganisationId = ownForeignOrganisationId;
            return this;
        }

        public string GetOwnOrganisationName()
        {

            return OwnOrganisationName;

        }


        [Obsolete]
        public virtual Invoice SetOwnOrganisationName(string ownOrganisationName)
        {
            this.OwnOrganisationName = ownOrganisationName;
            return this;
        }

        public string GetOwnStreet()
        {

            return Sender.GetStreet();

        }


        public string GetOwnZip()
        {

            return Sender.GetZip();

        }


        public string GetOwnLocation()
        {

            return Sender.GetLocation();

        }


        public string GetOwnCountry()
        {

            return Sender.GetCountry();

        }


        public string[] GetNotes()
        {
            return Notes?.ToArray();
        }

        public string GetCurrency()
        {
            return !string.IsNullOrWhiteSpace(Currency) ? Currency : "EUR";
        }


        public virtual Invoice SetCurrency(string currency)
        {
            this.Currency = currency;
            return this;
        }

        public string GetPaymentTermDescription()
        {

            return PaymentTermDescription;

        }

        public virtual Invoice SetPaymentTermDescription(string paymentTermDescription)
        {
            this.PaymentTermDescription = paymentTermDescription;
            return this;
        }

        public DateTime? GetIssueDate()
        {

            return IssueDate;

        }

        public virtual Invoice SetIssueDate(DateTime issueDate)
        {
            this.IssueDate = issueDate;
            return this;
        }

        public DateTime? GetDueDate()
        {

            return DueDate;

        }

        public virtual Invoice SetDueDate(DateTime dueDate)
        {
            this.DueDate = dueDate;
            return this;
        }

        public DateTime? GetDeliveryDate()
        {

            return DeliveryDate;

        }

        public virtual Invoice SetDeliveryDate(DateTime deliveryDate)
        {
            this.DeliveryDate = deliveryDate;
            return this;
        }

        public decimal GetTotalPrepaidAmount()
        {
            return TotalPrepaidAmount ?? decimal.Zero;
        }

        public virtual Invoice SetTotalPrepaidAmount(decimal totalPrepaidAmount)
        {
            this.TotalPrepaidAmount = totalPrepaidAmount;
            return this;
        }


        /// <summary>
        ///*
        /// sets a named sender contact </summary>
        /// @deprecated use setSender 
        /// <seealso cref="Contact"/>
        /// <param name="ownContact"> the sender contact </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetOwnContact(Contact ownContact)
        {
            this.Sender.SetContact(ownContact);
            return this;
        }

        /// <summary>
        /// required.
        /// sets the invoicing institution = invoicer </summary>
        /// <param name="sender"> the invoicer </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetSender(TradeParty sender)
        {
            this.Sender = sender;
            /*		if ((sender.getBankDetails() != null) && (sender.getBankDetails().size() > 0))
                    {
                        // convert bankdetails

                    }*/
            return this;
        }
        /*
            public IZUGFeRDAllowanceCharge[] getZFAllowances()
            {

                    if (Allowances.isEmpty())
                    {
                        return null;
                    }
                    else
                    {
                  return Allowances.toArray(new IZUGFeRDAllowanceCharge[0]);
                    }

            }


            public IZUGFeRDAllowanceCharge[] getZFCharges()
            {

                    if (Charges.isEmpty())
                    {
                        return null;
                    }
                    else
                    {
                  return Charges.toArray(new IZUGFeRDAllowanceCharge[0]);
                    }

            }


            public IZUGFeRDAllowanceCharge[] getZFLogisticsServiceCharges()
            {

                    if (LogisticsServiceCharges.isEmpty())
                    {
                        return null;
                    }
                    else
                    {
                  return LogisticsServiceCharges.toArray(new IZUGFeRDAllowanceCharge[0]);
                    }

            }

        */
        public IZUGFeRDTradeSettlement[]? GetTradeSettlement()
        {
            if (GetSender() == null)
            {
                return null;
            }

            return ((TradeParty)GetSender()).GetAsTradeSettlement();
        }


        public IZUGFeRDPaymentTerms GetPaymentTerms()
        {

            return PaymentTerms;

        }

        public virtual Invoice SetPaymentTerms(IZUGFeRDPaymentTerms paymentTerms)
        {
            this.PaymentTerms = paymentTerms;
            return this;
        }

        public IZUGFeRDExportableTradeParty GetDeliveryAddress()
        {

            return DeliveryAddress;

        }

        /// <summary>
        ///*
        /// if the delivery address is not the recipient address, it can be specified here </summary>
        /// <param name="deliveryAddress"> the goods receiving organisation </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetDeliveryAddress(TradeParty deliveryAddress)
        {
            this.DeliveryAddress = deliveryAddress;
            return this;
        }

        public IZUGFeRDExportableItem[] GetZfItems()
        {
            return ZfItems.ToArray();

        }


        /// <summary>
        /// required
        /// adds invoice "lines" :-) </summary>
        /// <seealso cref="Item"/>
        /// <param name="item"> the invoice line </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice AddItem(IZUGFeRDExportableItem item)
        {
            ZfItems.Add(item);
            return this;
        }


        /// <summary>
        ///*
        /// checks if all required items are set in order to be able to export it </summary>
        /// <returns> true if all required items are set </returns>
        public virtual bool Valid
        {
            get
            {
                return (DueDate != null) && (Sender != null) && (Sender.GetTaxId() != null) && (Sender.GetVatid() != null) && (Recipient != null);
                //contact
                //		this.phone = phone;
                //		this.email = email;
                //		this.street = street;
                //		this.zip = zip;
                //		this.location = location;
                //		this.country = country;
            }
        }


        /// <summary>
        ///*
        /// adds a document level addition to the price </summary>
        /// <seealso cref="Charge"/>
        /// <param name="izac"> the charge to be applied </param>
        /// <returns> fluent setter </returns>
        /*
            public virtual Invoice addCharge(IZUGFeRDAllowanceCharge izac)
            {
                Charges.add(izac);
                return this;
            }
        */
        /// <summary>
        ///*
        /// adds a document level rebate </summary>
        /// <seealso cref="Allowance"/>
        /// <param name="izac"> the allowance to be applied </param>
        /// <returns> fluent setter </returns>
        /*
            public virtual Invoice addAllowance(IZUGFeRDAllowanceCharge izac)
            {
                Allowances.add(izac);
                return this;
            }
        */
        /// <summary>
        ///*
        /// adds the ID of a contract referenced in the invoice </summary>
        /// <param name="s"> the contract number </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetContractReferencedDocument(string s)
        {
            ContractReferencedDocument = s;
            return this;
        }


        /// <summary>
        ///*
        /// sets a document level delivery period,
        /// which is optional additional to the mandatory deliverydate
        /// and which will become a BillingSpecifiedPeriod-Element </summary>
        /// <param name="start"> the date of first delivery </param>
        /// <param name="end"> the date of last delivery </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice SetDetailedDeliveryPeriod(DateTime start, DateTime end)
        {
            DetailedDeliveryDateStart = start;
            DetailedDeliveryPeriodEnd = end;

            return this;
        }


        public DateTime? GetDetailedDeliveryPeriodFrom()
        {

            return DetailedDeliveryDateStart;

        }

        public DateTime? GetDetailedDeliveryPeriodTo()
        {

            return DetailedDeliveryPeriodEnd;

        }


        /// <summary>
        /*
        /// adds a free text paragraph, which will become a includedNote element </summary>
        /// <param name="text"> freeform UTF8 plain text </param>
        /// <returns> fluent setter </returns>
        public virtual Invoice addNote(string text)
        {
            if (notes == null)
            {
                notes = new List<>();
            }
            notes.add(text);
            return this;
        }*/

        public string GetSpecifiedProcuringProjectId()
        {

            return SpecifiedProcuringProjectId;

        }

        public virtual Invoice SetSpecifiedProcuringProjectId(string specifiedProcuringProjectId)
        {
            this.SpecifiedProcuringProjectId = specifiedProcuringProjectId;
            return this;
        }

        public string GetSpecifiedProcuringProjectName()
        {

            return SpecifiedProcuringProjectName;

        }

        public virtual Invoice SetSpecifiedProcuringProjectName(string specifiedProcuringProjectName)
        {
            this.SpecifiedProcuringProjectName = specifiedProcuringProjectName;
            return this;
        }
    }
}
