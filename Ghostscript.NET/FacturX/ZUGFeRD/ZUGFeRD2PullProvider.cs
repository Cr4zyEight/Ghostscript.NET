using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Ghostscript.NET.FacturX.ZUGFeRD
{
    public class ZUGFeRD2PullProvider : ZugFeRdXmlWriter
    {
        private const string germanDateFormat = "dd.MM.yyyy";
        private const string invoiceDateFormat = "yyyyMMdd";
        private string _paymentTermsDescription;
        protected internal TransactionCalculator Calc;
        protected internal Profile Profile = Profiles.GetByName("EN16931");
        protected internal IExportableTransaction Trans;
        protected internal byte[] ZugferdData;

        /// <summary>
        /// Enables the flag to indicate a test invoice in the XML structure
        /// </summary>
        public void SetTest()
        {
        }

        /// <summary>
        /// Formats VAT value to a 2-decimal string
        /// </summary>
        private string VatFormat(decimal value) => XmlTools.ScaleDecimal(value, 2);

        /// <summary>
        /// Formats currency value to a 2-decimal string
        /// </summary>
        private string CurrencyFormat(decimal value) => XmlTools.ScaleDecimal(value, 2);

        /// <summary>
        /// Formats price value to a 4-decimal string
        /// </summary>
        private string PriceFormat(decimal value) => XmlTools.ScaleDecimal(value, 4);

        /// <summary>
        /// Formats quantity value to a 4-decimal string
        /// </summary>
        private string QuantityFormat(decimal value) => XmlTools.ScaleDecimal(value, 4);

        /// <summary>
        /// Retrieves the generated XML data as byte array
        /// </summary>
        public byte[] GetXml() => ZugferdData;

        /// <summary>
        /// Retrieves the current profile
        /// </summary>
        public Profile GetProfile() => Profile;

        /// <summary>
        /// Sets the current profile
        /// </summary>
        /// <param name="profile">The p.</param>
        public void SetProfile(Profile profile) => Profile = profile;

        /// <summary>
        /// Generates the XML structure for the invoice document
        /// </summary>
        /// <param name="trans">The transaction data</param>
        public void GenerateXml(IExportableTransaction trans)
        {
            Trans = trans;
            Calc = new TransactionCalculator(trans);
            _paymentTermsDescription = trans.GetPaymentTermDescription() ?? $"Zahlbar ohne Abzug bis {((DateTime)trans.GetDueDate()).ToString(germanDateFormat)}";

            XElement xml = new XElement($"{RsmNamespace.Prefix}:CrossIndustryInvoice",
                new XAttribute(XNamespace.Xmlns + XsiNamespace.Prefix, XsiNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + RsmNamespace.Prefix, RsmNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + RamNamespace.Prefix, RamNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + UdtNamespace.Prefix, UdtNamespace.Namespace),

                new XElement($"{RsmNamespace.Prefix}:ExchangedDocumentContext",
                    new XElement($"{RamNamespace.Prefix}:GuidelineSpecifiedDocumentContextParameter",
                        new XElement($"{RamNamespace.Prefix}:ID", GetProfile().GetId()))),
                new XElement($"{RsmNamespace.Prefix}:ExchangedDocument",
                    new XElement($"{RamNamespace.Prefix}:ID", XmlTools.EncodeXml(trans.GetNumber())),
                    new XElement($"{RamNamespace.Prefix}:TypeCode", "380"),
                    new XElement($"{RamNamespace.Prefix}:IssueDateTime",
                        new XElement($"{UdtNamespace.Prefix}:DateTimeString", ((DateTime)trans.GetIssueDate()).ToString(invoiceDateFormat))),
                    BuildNotesSection(trans),
                    BuildRebateAgreement(trans),
                    BuildSubjectNoteSection(trans)),
                new XElement($"{RsmNamespace.Prefix}:SupplyChainTradeTransaction",
                    BuildTradeLineItems(trans),
                    BuildApplicableHeaderTradeAgreement(trans),
                    BuildApplicableHeaderTradeDelivery(trans),
                    BuildApplicableHeaderTradeSettlement(trans))
            );

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] zugferdRaw = encoding.GetBytes(xml.ToString());
            ZugferdData = XmlTools.RemoveBom(zugferdRaw);
        }

        /// <summary>
        /// Builds the notes section for the XML
        /// </summary>
        private XElement BuildNotesSection(IExportableTransaction trans)
        {
            XElement notesSection = new XElement("Notes");
            if (trans.GetNotes() != null)
            {
                foreach (string note in trans.GetNotes())
                {
                    notesSection.Add(new XElement($"{RamNamespace.Prefix}:IncludedNote",
                        new XElement($"{RamNamespace.Prefix}:Content", XmlTools.EncodeXml(note))
                    ));
                }
            }
            return notesSection;
        }

        /// <summary>
        /// Builds the rebate agreement section for the XML
        /// </summary>
        private XElement BuildRebateAgreement(IExportableTransaction trans)
        {
            return trans.RebateAgreementExists()
                ? new XElement($"{RamNamespace.Prefix}:IncludedNote",
                    new XElement($"{RamNamespace.Prefix}:Content", "Es bestehen Rabatt- und Bonusvereinbarungen."),
                    new XElement($"{RamNamespace.Prefix}:SubjectCode", "AAK"))
                : null;
        }

        /// <summary>
        /// Builds the subject note section for the XML
        /// </summary>
        private XElement BuildSubjectNoteSection(IExportableTransaction trans)
        {
            return trans.GetSubjectNote() != null
                ? new XElement($"{RamNamespace.Prefix}:IncludedNote",
                    new XElement($"{RamNamespace.Prefix}:Content", XmlTools.EncodeXml(trans.GetSubjectNote())))
                : null;
        }

        /// <summary>
        /// Builds the trade line items section for the XML
        /// </summary>
        private XElement BuildTradeLineItems(IExportableTransaction trans)
        {
            List<XElement> lineItems = new List<XElement>();
            int lineId = 0;
            foreach (IZUGFeRDExportableItem item in trans.GetZfItems())
            {
                lineId++;
                XElement lineItem = new XElement($"{RamNamespace.Prefix}:IncludedSupplyChainTradeLineItem",
                    new XElement($"{RamNamespace.Prefix}:AssociatedDocumentLineDocument",
                        new XElement($"{RamNamespace.Prefix}:LineID", lineId)
                    ),
                    new XElement($"{RamNamespace.Prefix}:SpecifiedTradeProduct",
                        new XElement($"{RamNamespace.Prefix}:Name", XmlTools.EncodeXml(item.GetProduct().GetName())),
                        new XElement($"{RamNamespace.Prefix}:Description", XmlTools.EncodeXml(item.GetProduct().GetDescription()))
                    ),
                    BuildPriceDetails(item),
                    BuildDeliveryDetails(item),
                    BuildSettlementDetails(item)
                );

                lineItems.Add(lineItem);
            }

            return new XElement("TradeLineItems", lineItems);
        }

        /// <summary>
        /// Builds the price details for a trade item
        /// </summary>
        private XElement BuildPriceDetails(IZUGFeRDExportableItem item)
        {
            LineCalculator lc = new LineCalculator(item);
            return new XElement($"{RamNamespace.Prefix}:SpecifiedLineTradeAgreement",
                new XElement($"{RamNamespace.Prefix}:GrossPriceProductTradePrice",
                    new XElement($"{RamNamespace.Prefix}:ChargeAmount", PriceFormat(lc.GetPriceGross())),
                    new XElement($"{RamNamespace.Prefix}:BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
                ),
                new XElement($"{RamNamespace.Prefix}:NetPriceProductTradePrice",
                    new XElement($"{RamNamespace.Prefix}:ChargeAmount", PriceFormat(lc.GetPrice())),
                    new XElement($"{RamNamespace.Prefix}:BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
                )
            );
        }

        /// <summary>
        /// Builds the delivery details for a trade item
        /// </summary>
        private XElement BuildDeliveryDetails(IZUGFeRDExportableItem item)
        {
            return new XElement($"{RamNamespace.Prefix}:SpecifiedLineTradeDelivery",
                new XElement($"{RamNamespace.Prefix}:BilledQuantity", QuantityFormat(item.GetQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
            );
        }

        /// <summary>
        /// Builds the settlement details for a trade item
        /// </summary>
        private XElement BuildSettlementDetails(IZUGFeRDExportableItem item)
        {
            return new XElement($"{RamNamespace.Prefix}:SpecifiedLineTradeSettlement",
                new XElement($"{RamNamespace.Prefix}:ApplicableTradeTax",
                    new XElement($"{RamNamespace.Prefix}:TypeCode", "VAT"),
                    new XElement($"{RamNamespace.Prefix}:CategoryCode", item.GetProduct().GetTaxCategoryCode()),
                    new XElement($"{RamNamespace.Prefix}:RateApplicablePercent", VatFormat(item.GetProduct().GetVatPercent()))
                ),
                new XElement($"{RamNamespace.Prefix}:SpecifiedTradeSettlementLineMonetarySummation",
                    new XElement($"{RamNamespace.Prefix}:LineTotalAmount", CurrencyFormat(new LineCalculator(item).GetItemTotalNetAmount()))
                )
            );
        }

        /// <summary>
        /// Builds the applicable header trade agreement for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeAgreement(IExportableTransaction trans)
        {
            return new XElement($"{RamNamespace.Prefix}:ApplicableHeaderTradeAgreement",
                new XElement($"{RamNamespace.Prefix}:SellerTradeParty",
                    GetTradePartyAsXml(trans.GetSender(), true, false)),
                new XElement($"{RamNamespace.Prefix}:BuyerTradeParty",
                    GetTradePartyAsXml(trans.GetRecipient(), false, false)),
                trans.GetReferenceNumber() != null
                    ? new XElement($"{RamNamespace.Prefix}:BuyerReference", XmlTools.EncodeXml(trans.GetReferenceNumber()))
                    : null,
                trans.GetBuyerOrderReferencedDocumentId() != null
                    ? new XElement($"{RamNamespace.Prefix}:BuyerOrderReferencedDocument",
                        new XElement($"{RamNamespace.Prefix}:IssuerAssignedID", XmlTools.EncodeXml(trans.GetBuyerOrderReferencedDocumentId())))
                    : null,
                trans.GetContractReferencedDocument() != null
                    ? new XElement($"{RamNamespace.Prefix}:ContractReferencedDocument",
                        new XElement($"{RamNamespace.Prefix}:IssuerAssignedID", XmlTools.EncodeXml(trans.GetContractReferencedDocument())))
                    : null
            );
        }

        /// <summary>
        /// Builds the applicable header trade delivery section for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeDelivery(IExportableTransaction trans)
        {
            return new XElement($"{RamNamespace.Prefix}:ApplicableHeaderTradeDelivery",
                trans.GetDeliveryAddress() != null
                    ? new XElement($"{RamNamespace.Prefix}:ShipToTradeParty",
                        GetTradePartyAsXml(trans.GetDeliveryAddress(), false, true))
                    : null,
                new XElement($"{RamNamespace.Prefix}:ActualDeliverySupplyChainEvent",
                    new XElement($"{RamNamespace.Prefix}:OccurrenceDateTime",
                        new XElement($"{UdtNamespace.Prefix}:DateTimeString",
                            new XAttribute("format", "102"),
                            trans.GetDeliveryDate()?.ToString(invoiceDateFormat) ?? throw new InvalidOperationException("No delivery date provided")
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Builds the applicable header trade settlement for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeSettlement(IExportableTransaction trans)
        {
            List<XElement> settlementElements = new List<XElement>();

            if (trans.GetTradeSettlementPayment() != null)
            {
                foreach (IZUGFeRDTradeSettlementPayment payment in trans.GetTradeSettlementPayment())
                {
                    settlementElements.Add(payment.GetSettlementXml());
                }
            }

            XElement paymentTerms = trans.GetPaymentTerms() == null
                ? new XElement($"{RamNamespace.Prefix}:SpecifiedTradePaymentTerms",
                    new XElement($"{RamNamespace.Prefix}:Description", _paymentTermsDescription),
                    trans.GetDueDate() != null
                        ? new XElement($"{RamNamespace.Prefix}:DueDateDateTime",
                            new XElement($"{UdtNamespace.Prefix}:DateTimeString",
                                new XAttribute("format", "102"),
                                ((DateTime)trans.GetDueDate()).ToString(invoiceDateFormat)))
                        : null
                )
                : BuildPaymentTermsXml();

            return new XElement($"{RamNamespace.Prefix}:ApplicableHeaderTradeSettlement",
                new XElement($"{RamNamespace.Prefix}:PaymentReference", XmlTools.EncodeXml(trans.GetNumber())),
                new XElement($"{RamNamespace.Prefix}:InvoiceCurrencyCode", trans.GetCurrency()),
                settlementElements,
                paymentTerms,
                BuildVatSummary(trans)
            );
        }

        /// <summary>
        /// Builds the VAT summary section
        /// </summary>
        private XElement BuildVatSummary(IExportableTransaction trans)
        {
            Dictionary<decimal, VatAmount> vatPercentAmountMap = Calc.GetVatPercentAmountMap();
            List<XElement> vatSummaryElements = new List<XElement>();

            foreach (decimal taxPercent in vatPercentAmountMap.Keys)
            {
                VatAmount amount = vatPercentAmountMap[taxPercent];
                if (amount != null)
                {
                    vatSummaryElements.Add(new XElement($"{RamNamespace.Prefix}:ApplicableTradeTax",
                        new XElement($"{RamNamespace.Prefix}:CalculatedAmount", CurrencyFormat(amount.GetCalculated())),
                        new XElement($"{RamNamespace.Prefix}:TypeCode", "VAT"),
                        new XElement($"{RamNamespace.Prefix}:BasisAmount", CurrencyFormat(amount.GetBasis())),
                        new XElement($"{RamNamespace.Prefix}:CategoryCode", amount.GetCategoryCode()),
                        new XElement($"{RamNamespace.Prefix}:RateApplicablePercent", VatFormat(taxPercent))
                    ));
                }
            }

            return new XElement($"{RamNamespace.Prefix}:ApplicableTradeTaxes", vatSummaryElements);
        }

        /// <summary>
        /// Builds the payment terms XML
        /// </summary>
        private XElement BuildPaymentTermsXml()
        {
            IZUGFeRDPaymentTerms paymentTerms = Trans.GetPaymentTerms();
            DateTime? dueDate = paymentTerms.GetDueDate();

            XElement paymentTermsElement = new XElement($"{RamNamespace.Prefix}:SpecifiedTradePaymentTerms",
                new XElement($"{RamNamespace.Prefix}:Description", paymentTerms.GetDescription())
            );

            if (dueDate.HasValue)
            {
                paymentTermsElement.Add(
                    new XElement($"{RamNamespace.Prefix}:DueDateDateTime",
                        new XElement($"{UdtNamespace.Prefix}:DateTimeString", new XAttribute("format", "102"), dueDate.Value.ToString(invoiceDateFormat))
                    )
                );
            }

            return paymentTermsElement;
        }

        /// <summary>
        /// Builds the XML representation for a trade party
        /// </summary>
        private XElement GetTradePartyAsXml(IZUGFeRDExportableTradeParty party, bool isSender, bool isShipToTradeParty)
        {
            XElement tradePartyElement = new XElement($"{RamNamespace.Prefix}:TradeParty");

            if (party.GetId() != null)
            {
                tradePartyElement.Add(new XElement($"{RamNamespace.Prefix}:ID", XmlTools.EncodeXml(party.GetId())));
            }
            else if (party.GetGlobalIdScheme() != null && party.GetGlobalId() != null)
            {
                tradePartyElement.Add(new XElement($"{RamNamespace.Prefix}:GlobalID",
                    new XAttribute("schemeID", XmlTools.EncodeXml(party.GetGlobalIdScheme())),
                    XmlTools.EncodeXml(party.GetGlobalId())
                ));
            }

            tradePartyElement.Add(new XElement($"{RamNamespace.Prefix}:Name", XmlTools.EncodeXml(party.GetName())));

            if (party.GetContact() != null && (isSender || Profile == Profiles.GetByName("Extended")))
            {
                XElement contactElement = new XElement($"{RamNamespace.Prefix}:DefinedTradeContact",
                    new XElement($"{RamNamespace.Prefix}:PersonName", XmlTools.EncodeXml(party.GetContact().GetName())));

                if (party.GetContact().GetPhone() != null)
                {
                    contactElement.Add(new XElement($"{RamNamespace.Prefix}:TelephoneUniversalCommunication",
                        new XElement($"{RamNamespace.Prefix}:CompleteNumber", XmlTools.EncodeXml(party.GetContact().GetPhone()))));
                }

                if (party.GetContact().GetFax() != null && Profile == Profiles.GetByName("Extended"))
                {
                    contactElement.Add(new XElement($"{RamNamespace.Prefix}:FaxUniversalCommunication",
                        new XElement($"{RamNamespace.Prefix}:CompleteNumber", XmlTools.EncodeXml(party.GetContact().GetFax()))));
                }

                if (party.GetContact().GetEMail() != null)
                {
                    contactElement.Add(new XElement($"{RamNamespace.Prefix}:EmailURIUniversalCommunication",
                        new XElement($"{RamNamespace.Prefix}:URIID", XmlTools.EncodeXml(party.GetContact().GetEMail()))));
                }

                tradePartyElement.Add(contactElement);
            }

            XElement postalAddress = new XElement($"{RamNamespace.Prefix}:PostalTradeAddress",
                new XElement($"{RamNamespace.Prefix}:PostcodeCode", XmlTools.EncodeXml(party.GetZip())),
                new XElement($"{RamNamespace.Prefix}:LineOne", XmlTools.EncodeXml(party.GetStreet())));

            if (party.GetAdditionalAddress() != null)
            {
                postalAddress.Add(new XElement($"{RamNamespace.Prefix}:LineTwo", XmlTools.EncodeXml(party.GetAdditionalAddress())));
            }

            postalAddress.Add(
                new XElement($"{RamNamespace.Prefix}:CityName", XmlTools.EncodeXml(party.GetLocation())),
                new XElement($"{RamNamespace.Prefix}:CountryID", XmlTools.EncodeXml(party.GetCountry()))
            );

            tradePartyElement.Add(postalAddress);

            if (party.GetVatid() != null && !isShipToTradeParty)
            {
                tradePartyElement.Add(new XElement($"{RamNamespace.Prefix}:SpecifiedTaxRegistration",
                    new XElement($"{RamNamespace.Prefix}:ID", new XAttribute("schemeID", "VA"), XmlTools.EncodeXml(party.GetVatid()))));
            }

            if (party.GetTaxId() != null && !isShipToTradeParty)
            {
                tradePartyElement.Add(new XElement($"{RamNamespace.Prefix}:SpecifiedTaxRegistration",
                    new XElement($"{RamNamespace.Prefix}:ID", new XAttribute("schemeID", "FC"), XmlTools.EncodeXml(party.GetTaxId()))));
            }

            return tradePartyElement;
        }
    }
}
