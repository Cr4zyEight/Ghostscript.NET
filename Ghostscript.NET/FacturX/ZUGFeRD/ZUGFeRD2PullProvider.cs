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
        private string VatFormat(decimal value) => XmlTools.ScaleDecimal(value, 2).Replace(",", ".");

        /// <summary>
        /// Formats currency value to a 2-decimal string
        /// </summary>
        private string CurrencyFormat(decimal value) => XmlTools.ScaleDecimal(value, 2).Replace(",", ".");

        /// <summary>
        /// Formats price value to a 4-decimal string
        /// </summary>
        private string PriceFormat(decimal value) => XmlTools.ScaleDecimal(value, 4).Replace(",", ".");

        /// <summary>
        /// Formats quantity value to a 4-decimal string
        /// </summary>
        private string QuantityFormat(decimal value) => XmlTools.ScaleDecimal(value, 4).Replace(",", ".");

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

            XElement xml = new XElement(RsmNamespace.Namespace + "CrossIndustryInvoice",
                new XAttribute(XNamespace.Xmlns + XsiNamespace.Prefix, XsiNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + RsmNamespace.Prefix, RsmNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + RamNamespace.Prefix, RamNamespace.Namespace),
                new XAttribute(XNamespace.Xmlns + UdtNamespace.Prefix, UdtNamespace.Namespace),

                new XElement(RsmNamespace.Namespace + "ExchangedDocumentContext",
                    new XElement(RamNamespace.Namespace + "GuidelineSpecifiedDocumentContextParameter",
                        new XElement(RamNamespace.Namespace + "ID", GetProfile().GetId()))),
                new XElement(RsmNamespace.Namespace + "ExchangedDocument",
                    new XElement(RamNamespace.Namespace + "ID", XmlTools.EncodeXml(trans.GetNumber())),
                    new XElement(RamNamespace.Namespace + "TypeCode", "380"),
                    new XElement(RamNamespace.Namespace + "IssueDateTime",
                        new XElement(UdtNamespace.Namespace + "DateTimeString", ((DateTime)trans.GetIssueDate()).ToString(invoiceDateFormat))),
                    BuildNotesSection(trans),
                    BuildRebateAgreement(trans),
                    BuildSubjectNoteSection(trans)),
                new XElement(RsmNamespace.Namespace + "SupplyChainTradeTransaction",
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
                    notesSection.Add(new XElement(RamNamespace.Namespace + "IncludedNote",
                        new XElement(RamNamespace.Namespace + "Content", XmlTools.EncodeXml(note))
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
                ? new XElement(RamNamespace.Namespace + "IncludedNote",
                    new XElement(RamNamespace.Namespace + "Content", "Es bestehen Rabatt- und Bonusvereinbarungen."),
                    new XElement(RamNamespace.Namespace + "SubjectCode", "AAK"))
                : null;
        }

        /// <summary>
        /// Builds the subject note section for the XML
        /// </summary>
        private XElement BuildSubjectNoteSection(IExportableTransaction trans)
        {
            return trans.GetSubjectNote() != null
                ? new XElement(RamNamespace.Namespace + "IncludedNote",
                    new XElement(RamNamespace.Namespace + "Content", XmlTools.EncodeXml(trans.GetSubjectNote())))
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
                XElement lineItem = new XElement(RamNamespace.Namespace + "IncludedSupplyChainTradeLineItem",
                    new XElement(RamNamespace.Namespace + "AssociatedDocumentLineDocument",
                        new XElement(RamNamespace.Namespace + "LineID", lineId)
                    ),
                    new XElement(RamNamespace.Namespace + "SpecifiedTradeProduct",
                        new XElement(RamNamespace.Namespace + "Name", XmlTools.EncodeXml(item.GetProduct().GetName())),
                        new XElement(RamNamespace.Namespace + "Description", XmlTools.EncodeXml(item.GetProduct().GetDescription()))
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
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeAgreement",
                new XElement(RamNamespace.Namespace + "GrossPriceProductTradePrice",
                    new XElement(RamNamespace.Namespace + "ChargeAmount", PriceFormat(lc.GetPriceGross())),
                    new XElement(RamNamespace.Namespace + "BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
                ),
                new XElement(RamNamespace.Namespace + "NetPriceProductTradePrice",
                    new XElement(RamNamespace.Namespace + "ChargeAmount", PriceFormat(lc.GetPrice())),
                    new XElement(RamNamespace.Namespace + "BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
                )
            );
        }

        /// <summary>
        /// Builds the delivery details for a trade item
        /// </summary>
        private XElement BuildDeliveryDetails(IZUGFeRDExportableItem item)
        {
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeDelivery",
                new XElement(RamNamespace.Namespace + "BilledQuantity", QuantityFormat(item.GetQuantity()), new XAttribute("unitCode", XmlTools.EncodeXml(item.GetProduct().GetUnit())))
            );
        }

        /// <summary>
        /// Builds the settlement details for a trade item
        /// </summary>
        private XElement BuildSettlementDetails(IZUGFeRDExportableItem item)
        {
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeSettlement",
                new XElement(RamNamespace.Namespace + "ApplicableTradeTax",
                    new XElement(RamNamespace.Namespace + "TypeCode", "VAT"),
                    new XElement(RamNamespace.Namespace + "CategoryCode", item.GetProduct().GetTaxCategoryCode()),
                    new XElement(RamNamespace.Namespace + "RateApplicablePercent", VatFormat(item.GetProduct().GetVatPercent()))
                ),
                new XElement(RamNamespace.Namespace + "SpecifiedTradeSettlementLineMonetarySummation",
                    new XElement(RamNamespace.Namespace + "LineTotalAmount", CurrencyFormat(new LineCalculator(item).GetItemTotalNetAmount()))
                )
            );
        }

        /// <summary>
        /// Builds the applicable header trade agreement for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeAgreement(IExportableTransaction trans)
        {
            return new XElement(RamNamespace.Namespace + "ApplicableHeaderTradeAgreement",
                new XElement(RamNamespace.Namespace + "SellerTradeParty",
                    GetTradePartyAsXml(trans.GetSender(), true, false)),
                new XElement(RamNamespace.Namespace + "BuyerTradeParty",
                    GetTradePartyAsXml(trans.GetRecipient(), false, false)),
                trans.GetReferenceNumber() != null
                    ? new XElement(RamNamespace.Namespace + "BuyerReference", XmlTools.EncodeXml(trans.GetReferenceNumber()))
                    : null,
                trans.GetBuyerOrderReferencedDocumentId() != null
                    ? new XElement(RamNamespace.Namespace + "BuyerOrderReferencedDocument",
                        new XElement(RamNamespace.Namespace + "IssuerAssignedID", XmlTools.EncodeXml(trans.GetBuyerOrderReferencedDocumentId())))
                    : null,
                trans.GetContractReferencedDocument() != null
                    ? new XElement(RamNamespace.Namespace + "ContractReferencedDocument",
                        new XElement(RamNamespace.Namespace + "IssuerAssignedID", XmlTools.EncodeXml(trans.GetContractReferencedDocument())))
                    : null
            );
        }

        /// <summary>
        /// Builds the applicable header trade delivery section for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeDelivery(IExportableTransaction trans)
        {
            return new XElement(RamNamespace.Namespace + "ApplicableHeaderTradeDelivery",
                trans.GetDeliveryAddress() != null
                    ? new XElement(RamNamespace.Namespace + "ShipToTradeParty",
                        GetTradePartyAsXml(trans.GetDeliveryAddress(), false, true))
                    : null,
                new XElement(RamNamespace.Namespace + "ActualDeliverySupplyChainEvent",
                    new XElement(RamNamespace.Namespace + "OccurrenceDateTime",
                        new XElement(UdtNamespace.Namespace + "DateTimeString",
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
                ? new XElement(RamNamespace.Namespace + "SpecifiedTradePaymentTerms",
                    new XElement(RamNamespace.Namespace + "Description", _paymentTermsDescription),
                    trans.GetDueDate() != null
                        ? new XElement(RamNamespace.Namespace + "DueDateDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"),
                                ((DateTime)trans.GetDueDate()).ToString(invoiceDateFormat)))
                        : null
                )
                : BuildPaymentTermsXml();

            return new XElement(RamNamespace.Namespace + "ApplicableHeaderTradeSettlement",
                new XElement(RamNamespace.Namespace + "PaymentReference", XmlTools.EncodeXml(trans.GetNumber())),
                new XElement(RamNamespace.Namespace + "InvoiceCurrencyCode", trans.GetCurrency()),
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
                    vatSummaryElements.Add(new XElement(RamNamespace.Namespace + "ApplicableTradeTax",
                        new XElement(RamNamespace.Namespace + "CalculatedAmount", CurrencyFormat(amount.GetCalculated())),
                        new XElement(RamNamespace.Namespace + "TypeCode", "VAT"),
                        new XElement(RamNamespace.Namespace + "BasisAmount", CurrencyFormat(amount.GetBasis())),
                        new XElement(RamNamespace.Namespace + "CategoryCode", amount.GetCategoryCode()),
                        new XElement(RamNamespace.Namespace + "RateApplicablePercent", VatFormat(taxPercent))
                    ));
                }
            }

            return new XElement(RamNamespace.Namespace + "ApplicableTradeTaxes", vatSummaryElements);
        }

        /// <summary>
        /// Builds the payment terms XML
        /// </summary>
        private XElement BuildPaymentTermsXml()
        {
            IZUGFeRDPaymentTerms paymentTerms = Trans.GetPaymentTerms();
            DateTime? dueDate = paymentTerms.GetDueDate();

            XElement paymentTermsElement = new XElement(RamNamespace.Namespace + "SpecifiedTradePaymentTerms",
                new XElement(RamNamespace.Namespace + "Description", paymentTerms.GetDescription())
            );

            if (dueDate.HasValue)
            {
                paymentTermsElement.Add(
                    new XElement(RamNamespace.Namespace + "DueDateDateTime",
                        new XElement(UdtNamespace.Namespace + "DateTimeString", new XAttribute("format", "102"), dueDate.Value.ToString(invoiceDateFormat))
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
            XElement tradePartyElement = new XElement(RamNamespace.Namespace + "TradeParty");

            if (party.GetId() != null)
            {
                tradePartyElement.Add(new XElement(RamNamespace.Namespace + "ID", XmlTools.EncodeXml(party.GetId())));
            }
            else if (party.GetGlobalIdScheme() != null && party.GetGlobalId() != null)
            {
                tradePartyElement.Add(new XElement(RamNamespace.Namespace + "GlobalID",
                    new XAttribute("schemeID", XmlTools.EncodeXml(party.GetGlobalIdScheme())),
                    XmlTools.EncodeXml(party.GetGlobalId())
                ));
            }

            tradePartyElement.Add(new XElement(RamNamespace.Namespace + "Name", XmlTools.EncodeXml(party.GetName())));

            if (party.GetContact() != null && (isSender || Profile == Profiles.GetByName("Extended")))
            {
                XElement contactElement = new XElement(RamNamespace.Namespace + "DefinedTradeContact",
                    new XElement(RamNamespace.Namespace + "PersonName", XmlTools.EncodeXml(party.GetContact().GetName())));

                if (party.GetContact().GetPhone() != null)
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "TelephoneUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "CompleteNumber", XmlTools.EncodeXml(party.GetContact().GetPhone()))));
                }

                if (party.GetContact().GetFax() != null && Profile == Profiles.GetByName("Extended"))
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "FaxUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "CompleteNumber", XmlTools.EncodeXml(party.GetContact().GetFax()))));
                }

                if (party.GetContact().GetEMail() != null)
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "EmailURIUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "URIID", XmlTools.EncodeXml(party.GetContact().GetEMail()))));
                }

                tradePartyElement.Add(contactElement);
            }

            XElement postalAddress = new XElement(RamNamespace.Namespace + "PostalTradeAddress",
                new XElement(RamNamespace.Namespace + "PostcodeCode", XmlTools.EncodeXml(party.GetZip())),
                new XElement(RamNamespace.Namespace + "LineOne", XmlTools.EncodeXml(party.GetStreet())));

            if (party.GetAdditionalAddress() != null)
            {
                postalAddress.Add(new XElement(RamNamespace.Namespace + "LineTwo", XmlTools.EncodeXml(party.GetAdditionalAddress())));
            }

            postalAddress.Add(
                new XElement(RamNamespace.Namespace + "CityName", XmlTools.EncodeXml(party.GetLocation())),
                new XElement(RamNamespace.Namespace + "CountryID", XmlTools.EncodeXml(party.GetCountry()))
            );

            tradePartyElement.Add(postalAddress);

            if (party.GetVatid() != null && !isShipToTradeParty)
            {
                tradePartyElement.Add(new XElement(RamNamespace.Namespace + "SpecifiedTaxRegistration",
                    new XElement(RamNamespace.Namespace + "ID", new XAttribute("schemeID", "VA"), XmlTools.EncodeXml(party.GetVatid()))));
            }

            if (party.GetTaxId() != null && !isShipToTradeParty)
            {
                tradePartyElement.Add(new XElement(RamNamespace.Namespace + "SpecifiedTaxRegistration",
                    new XElement(RamNamespace.Namespace + "ID", new XAttribute("schemeID", "FC"), XmlTools.EncodeXml(party.GetTaxId()))));
            }

            return tradePartyElement;
        }
    }
}
