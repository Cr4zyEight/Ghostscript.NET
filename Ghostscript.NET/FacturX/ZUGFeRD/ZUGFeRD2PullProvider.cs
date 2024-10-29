using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Ghostscript.NET.Helpers;

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

        private XElement _exemptionReason;

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
            
            XDocument xml = new XDocument(
                new XElement(RsmNamespace.Namespace + "CrossIndustryInvoice",
                    new XAttribute(XNamespace.Xmlns + XsiNamespace.Prefix, XsiNamespace.Namespace),
                    new XAttribute(XNamespace.Xmlns + RsmNamespace.Prefix, RsmNamespace.Namespace),
                    new XAttribute(XNamespace.Xmlns + RamNamespace.Prefix, RamNamespace.Namespace),
                    new XAttribute(XNamespace.Xmlns + UdtNamespace.Prefix, UdtNamespace.Namespace),

                    new XElement(RsmNamespace.Namespace + "ExchangedDocumentContext",
                        new XElement(RamNamespace.Namespace + "GuidelineSpecifiedDocumentContextParameter",
                            new XElement(RamNamespace.Namespace + "ID", GetProfile().GetId()))),
                    new XElement(RsmNamespace.Namespace + "ExchangedDocument",
                        new XElement(RamNamespace.Namespace + "ID", trans.GetNumber()),
                        new XElement(RamNamespace.Namespace + "TypeCode", "380"),
                        new XElement(RamNamespace.Namespace + "IssueDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString", 
                                ((DateTime)trans.GetIssueDate()).ToString(invoiceDateFormat),
                                new XAttribute("format", "102")
                            )
                        ),
                        BuildNotesSection(trans.GetNotes()),
                        BuildSubjectNoteSection(trans)),
                        BuildRebateAgreement(trans),
                    new XElement(RsmNamespace.Namespace + "SupplyChainTradeTransaction",
                        BuildIncludedSupplyChainTradeLineItem(trans),
                        BuildApplicableHeaderTradeAgreement(trans),
                        BuildApplicableHeaderTradeDelivery(trans),
                        BuildApplicableHeaderTradeSettlement(trans))
                )
            );

            StringBuilder builder = new ();
            using Utf8StringWriter writer = new (builder);
            xml.Save(writer);

            UTF8Encoding encoding = new ();
            byte[] zugferdRaw = encoding.GetBytes(builder.ToString());
            ZugferdData = XmlTools.RemoveBom(zugferdRaw);
        }

        /// <summary>
        /// Builds the notes section for the XML
        /// </summary>
        private List<XElement> BuildNotesSection(string[] noteContents)
        {
            List<XElement> notesXml = [];

            foreach (string note in noteContents ?? [])
            {
                notesXml.Add(new XElement(RamNamespace.Namespace + "IncludedNote",
                    new XElement(RamNamespace.Namespace + "Content", note)
                ));
            }
            
            return notesXml;
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
                    new XElement(RamNamespace.Namespace + "Content", trans.GetSubjectNote()))
                : null;
        }

        /// <summary>
        /// Builds the IncludedSupplyChainTradeLineItem items section for the XML
        /// </summary>
        private List<XElement> BuildIncludedSupplyChainTradeLineItem(IExportableTransaction trans)
        {
            List<XElement> lineItems = new List<XElement>();
            int lineId = 0;
            foreach (IZUGFeRDExportableItem item in trans.GetZfItems() ?? [])
            {
                lineId++;
                XElement lineItem = new XElement(RamNamespace.Namespace + "IncludedSupplyChainTradeLineItem",
                    new XElement(RamNamespace.Namespace + "AssociatedDocumentLineDocument",
                        new XElement(RamNamespace.Namespace + "LineID", lineId),
                        BuildNotesSection(item.GetNotes())
                    ),
                    new XElement(RamNamespace.Namespace + "SpecifiedTradeProduct",
                        item.GetProduct().GetSellerAssignedId() != null ? new XElement(RamNamespace.Namespace + "SellerAssignedID", item.GetProduct().GetSellerAssignedId()) : null,
                        item.GetProduct().GetBuyerAssignedId() != null ? new XElement(RamNamespace.Namespace + "BuyerAssignedID", item.GetProduct().GetBuyerAssignedId()) : null,
                        new XElement(RamNamespace.Namespace + "Name", item.GetProduct().GetName()),
                        new XElement(RamNamespace.Namespace + "Description", item.GetProduct().GetDescription())
                    ),
                    BuildPriceDetails(item),
                    BuildDeliveryDetails(item),
                    BuildSettlementDetails(item)
                );

                lineItems.Add(lineItem);
            }

            return lineItems;
        }

        /// <summary>
        /// Builds the price details for a trade item
        /// </summary>
        private XElement BuildPriceDetails(IZUGFeRDExportableItem item)
        {
            LineCalculator lc = new (item);
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeAgreement",
                new XElement(RamNamespace.Namespace + "GrossPriceProductTradePrice",
                    new XElement(RamNamespace.Namespace + "ChargeAmount", PriceFormat(lc.GetPriceGross())),
                    new XElement(RamNamespace.Namespace + "BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", item.GetProduct().GetUnit()))
                ),
                new XElement(RamNamespace.Namespace + "NetPriceProductTradePrice",
                    new XElement(RamNamespace.Namespace + "ChargeAmount", PriceFormat(lc.GetPrice())),
                    new XElement(RamNamespace.Namespace + "BasisQuantity", QuantityFormat(item.GetBasisQuantity()), new XAttribute("unitCode", item.GetProduct().GetUnit()))
                )
            );
        }

        /// <summary>
        /// Builds the delivery details for a trade item
        /// </summary>
        private XElement BuildDeliveryDetails(IZUGFeRDExportableItem item)
        {
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeDelivery",
                new XElement(RamNamespace.Namespace + "BilledQuantity", QuantityFormat(item.GetQuantity()), new XAttribute("unitCode", item.GetProduct().GetUnit()))
            );
        }

        /// <summary>
        /// Builds the settlement details for a trade item
        /// </summary>
        private XElement BuildSettlementDetails(IZUGFeRDExportableItem item)
        {
            _exemptionReason = BuildExemptionReason(item);
            return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeSettlement",
                new XElement(RamNamespace.Namespace + "ApplicableTradeTax",
                    new XElement(RamNamespace.Namespace + "TypeCode", "VAT"),
                    _exemptionReason,
                    new XElement(RamNamespace.Namespace + "CategoryCode", item.GetProduct().GetTaxCategoryCode()),
                    new XElement(RamNamespace.Namespace + "RateApplicablePercent", VatFormat(item.GetProduct().GetVatPercent()))
                ),
                BuildBillingSpecifiedPeriod(item),
                BuildSpecifiedTradeSettlementLineMonetarySummation(item),
                BuildAdditionalReferencedDocument(item)
            );
        }

        /// <summary>
        /// Builds the exemption reason for a trade item
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private XElement BuildExemptionReason(IZUGFeRDExportableItem item)
        {
            if (item.GetProduct().GetTaxExemptionReason() != null)
            {
                return new XElement(RamNamespace.Namespace + "SpecifiedLineTradeSettlement", item.GetProduct().GetTaxExemptionReason());
            }

            return null;
        }

        /// <summary>
        /// Builds the BillingSpecifiedPeriod for a trade item
        /// </summary>
        private XElement BuildBillingSpecifiedPeriod(IZUGFeRDExportableItem item)
        {
            if (item.GetDetailedDeliveryPeriodFrom() != null || item.GetDetailedDeliveryPeriodTo() != null)
            {
                return new XElement(RamNamespace.Namespace + "BillingSpecifiedPeriod",
                    item.GetDetailedDeliveryPeriodFrom() != null ?
                        new XElement(RamNamespace.Namespace + "StartDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"), item.GetDetailedDeliveryPeriodFrom().Value.ToString(invoiceDateFormat)
                            )
                        )
                        : null,
                    item.GetDetailedDeliveryPeriodFrom() != null ?
                        new XElement(RamNamespace.Namespace + "EndDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"), item.GetDetailedDeliveryPeriodTo().Value.ToString(invoiceDateFormat)
                            )
                        )
                        : null
                );
            }

            return null;
        }

        /// <summary>
        /// Builds the BillingSpecifiedPeriod for a trade item
        /// </summary>
        private XElement BuildBillingSpecifiedPeriod(IExportableTransaction transaction)
        {
            if (transaction.GetDetailedDeliveryPeriodFrom() != null || transaction.GetDetailedDeliveryPeriodTo() != null)
            {
                return new XElement(RamNamespace.Namespace + "BillingSpecifiedPeriod",
                    transaction.GetDetailedDeliveryPeriodFrom() != null ?
                        new XElement(RamNamespace.Namespace + "StartDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"), transaction.GetDetailedDeliveryPeriodFrom().Value.ToString(invoiceDateFormat)
                            )
                        )
                        : null,
                    transaction.GetDetailedDeliveryPeriodFrom() != null ?
                        new XElement(RamNamespace.Namespace + "EndDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"), transaction.GetDetailedDeliveryPeriodTo().Value.ToString(invoiceDateFormat)
                            )
                        )
                        : null
                );
            }

            return null;
        }

        /// <summary>
        /// Builds the SpecifiedTradeSettlementLineMonetarySummation for a trade item
        /// </summary>
        private XElement BuildSpecifiedTradeSettlementLineMonetarySummation(IZUGFeRDExportableItem item)
        {
            LineCalculator lc = new (item);
            return new XElement(RamNamespace.Namespace + "SpecifiedTradeSettlementLineMonetarySummation", 
                new XElement(RamNamespace.Namespace + "LineTotalAmount", CurrencyFormat(lc.GetItemTotalNetAmount()))
            );
        }

        /// <summary>
        /// Builds the AdditionalReferencedDocument for a trade item
        /// </summary>
        private XElement BuildAdditionalReferencedDocument(IZUGFeRDExportableItem item)
        {
            if (item.GetAdditionalReferencedDocumentId() != null)
            {
                return new XElement(RamNamespace.Namespace + "AdditionalReferencedDocument",
                    new XElement(RamNamespace.Namespace + "IssuerAssignedID", item.GetAdditionalReferencedDocumentId()),
                    new XElement(RamNamespace.Namespace + "TypeCode", "130")
                );
            }

            return null;
        }

        /// <summary>
        /// Builds the applicable header trade agreement for the transaction
        /// </summary>
        private XElement BuildApplicableHeaderTradeAgreement(IExportableTransaction trans)
        {
            return new XElement(RamNamespace.Namespace + "ApplicableHeaderTradeAgreement",
                BuildBuyerReference(trans),
                new XElement(RamNamespace.Namespace + "SellerTradeParty",
                    GetTradePartyAsXml(trans.GetSender(), true, false)),
                new XElement(RamNamespace.Namespace + "BuyerTradeParty",
                    GetTradePartyAsXml(trans.GetRecipient(), false, false)),
                BuildBuyerOrderReferencedDocument(trans),
                BuildContractReferencedDocument(trans)
            );
        }

        /// <summary>
        /// Builds the buyer order referenced document.
        /// </summary>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        private XElement BuildBuyerOrderReferencedDocument(IExportableTransaction trans)
        {
            if (trans.GetContractReferencedDocument() != null)
            {
                return new XElement(RamNamespace.Namespace + "ContractReferencedDocument",
                    new XElement(RamNamespace.Namespace + "IssuerAssignedID", trans.GetContractReferencedDocument()));
            }

            return null;
        }

        /// <summary>
        /// Builds the contract referenced document.
        /// </summary>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        private XElement BuildContractReferencedDocument(IExportableTransaction trans)
        {
            if (trans.GetContractReferencedDocument() != null)
            {
                return new XElement(RamNamespace.Namespace + "ContractReferencedDocument",
                    new XElement(RamNamespace.Namespace + "IssuerAssignedID", trans.GetContractReferencedDocument()));
            }

            return null;
        }

        /// <summary>
        /// Builds the build buyer reference.
        /// </summary>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        private XElement BuildBuyerReference(IExportableTransaction trans)
        {
            if (trans.GetReferenceNumber() != null)
            {
                return new XElement(RamNamespace.Namespace + "BuyerReference", trans.GetReferenceNumber());
            }
            return null;
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
            // settlementElements
            List<XElement> settlementElements = [];
            bool hasDueDate = false;

            if (trans.GetTradeSettlementPayment() != null)
            {
                foreach (IZUGFeRDTradeSettlementPayment payment in trans.GetTradeSettlementPayment() ?? [])
                {
                    hasDueDate = true;
                    settlementElements.Add(payment.GetSettlementXml());
                }
            }

            if (trans.GetTradeSettlement() != null)
            {
                foreach (IZUGFeRDTradeSettlement payment in trans.GetTradeSettlement() ?? [])
                {
                    if (payment is IZUGFeRDTradeSettlementPayment) hasDueDate = true;
                    settlementElements.Add(payment.GetSettlementXml());
                }
            }
            // applicableTradeTaxElements
            List<XElement> applicableTradeTaxElements = [];

            Dictionary<decimal, VatAmount> vatPercentAmountMap = Calc.GetVatPercentAmountMap();
            foreach (decimal currentTaxPercent in vatPercentAmountMap.Keys)
            {
                VatAmount amount = vatPercentAmountMap[currentTaxPercent];
                if (amount != null)
                {
                    applicableTradeTaxElements.Add(
                        new XElement(RamNamespace.Namespace + "ApplicableTradeTax",
                                new XElement(RamNamespace.Namespace + "CalculatedAmount", CurrencyFormat(amount.GetCalculated())),
                                new XElement(RamNamespace.Namespace + "TypeCode", "VAT"),
                                _exemptionReason, //TODO: This does not seem right... _exemptionReason is build up new for every loop of an IZUGFeRDExportableItem in trans.GetZfItems(), so this will be set to the last item looped (it is the same as writing "BuildExemptionReason(trans.GetZfItems().Last())")
                                new XElement(RamNamespace.Namespace + "BasisAmount", CurrencyFormat(amount.GetBasis())),
                                new XElement(RamNamespace.Namespace + "CategoryCode", amount.GetCategoryCode()),
                                new XElement(RamNamespace.Namespace + "RateApplicablePercent", VatFormat(currentTaxPercent))
                             )
                        );
                }
            }

            // billingSpecifiedPeriod
            XElement billingSpecifiedPeriod = null;
            if (trans.GetDetailedDeliveryPeriodFrom() != null || trans.GetDetailedDeliveryPeriodTo() != null)
            {
                billingSpecifiedPeriod = BuildBillingSpecifiedPeriod(trans);
            }

            // paymentTerms
            XElement paymentTerms = trans.GetPaymentTerms() == null
                ? new XElement(RamNamespace.Namespace + "SpecifiedTradePaymentTerms",
                    new XElement(RamNamespace.Namespace + "Description", _paymentTermsDescription),
                    hasDueDate && trans.GetDueDate() != null
                        ? new XElement(RamNamespace.Namespace + "DueDateDateTime",
                            new XElement(UdtNamespace.Namespace + "DateTimeString",
                                new XAttribute("format", "102"),
                                ((DateTime)trans.GetDueDate()).ToString(invoiceDateFormat)))
                        : null
                )
                : BuildPaymentTermsXml();

            // specifiedTradeSettlementHeaderMonetarySummation
            XElement specifiedTradeSettlementHeaderMonetarySummation = 
                new XElement(RamNamespace.Namespace + "SpecifiedTradeSettlementHeaderMonetarySummation",
                    new XElement(RamNamespace.Namespace + "LineTotalAmount", CurrencyFormat(Calc.GetTotal())),
                    new XElement(RamNamespace.Namespace + "TaxBasisTotalAmount", CurrencyFormat(Calc.GetTaxBasis())),
                    new XElement(RamNamespace.Namespace + "TaxTotalAmount", CurrencyFormat(Calc.GetGrandTotal() - Calc.GetTaxBasis()),
                        new XAttribute("currencyID", trans.GetCurrency())
                    ),
                    new XElement(RamNamespace.Namespace + "GrandTotalAmount", CurrencyFormat(Calc.GetGrandTotal())),
                    new XElement(RamNamespace.Namespace + "TotalPrepaidAmount", CurrencyFormat(Calc.GetTotalPrepaid())),
                    new XElement(RamNamespace.Namespace + "DuePayableAmount", CurrencyFormat(Calc.GetGrandTotal() - Calc.GetTotalPrepaid()))
                );

            return new XElement(RamNamespace.Namespace + "ApplicableHeaderTradeSettlement",
                new XElement(RamNamespace.Namespace + "PaymentReference", trans.GetNumber()),
                new XElement(RamNamespace.Namespace + "InvoiceCurrencyCode", trans.GetCurrency()),
                settlementElements,
                applicableTradeTaxElements,
                billingSpecifiedPeriod,
                paymentTerms,
                specifiedTradeSettlementHeaderMonetarySummation
            );
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
        private List<XElement> GetTradePartyAsXml(IZUGFeRDExportableTradeParty party, bool isSender, bool isShipToTradeParty)
        {
            List<XElement> elements = [];

            if (party.GetId() != null)
            {
                elements.Add(new XElement(RamNamespace.Namespace + "ID", party.GetId()));
            }
            else if (party.GetGlobalIdScheme() != null && party.GetGlobalId() != null)
            {
                elements.Add(new XElement(RamNamespace.Namespace + "GlobalID",
                    new XAttribute("schemeID", party.GetGlobalIdScheme()),
                    party.GetGlobalId()
                ));
            }

            elements.Add(new XElement(RamNamespace.Namespace + "Name", party.GetName()));

            if (party.GetContact() != null && (isSender || Profile == Profiles.GetByName("Extended")))
            {
                XElement contactElement = new (RamNamespace.Namespace + "DefinedTradeContact",
                    new XElement(RamNamespace.Namespace + "PersonName", party.GetContact().GetName()));

                if (party.GetContact().GetPhone() != null)
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "TelephoneUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "CompleteNumber", party.GetContact().GetPhone())));
                }

                if (party.GetContact().GetFax() != null && Profile == Profiles.GetByName("Extended"))
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "FaxUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "CompleteNumber", party.GetContact().GetFax())));
                }

                if (party.GetContact().GetEMail() != null)
                {
                    contactElement.Add(new XElement(RamNamespace.Namespace + "EmailURIUniversalCommunication",
                        new XElement(RamNamespace.Namespace + "URIID", party.GetContact().GetEMail())));
                }

                elements.Add(contactElement);
            }

            XElement postalAddress = new XElement(RamNamespace.Namespace + "PostalTradeAddress",
                new XElement(RamNamespace.Namespace + "PostcodeCode", party.GetZip()),
                new XElement(RamNamespace.Namespace + "LineOne", party.GetStreet())
            );

            if (party.GetAdditionalAddress() != null)
            {
                postalAddress.Add(new XElement(RamNamespace.Namespace + "LineTwo", party.GetAdditionalAddress()));
            }

            postalAddress.Add(
                new XElement(RamNamespace.Namespace + "CityName", party.GetLocation()),
                new XElement(RamNamespace.Namespace + "CountryID", party.GetCountry())
            );

            elements.Add(postalAddress);

            if (party.GetVatid() != null && !isShipToTradeParty)
            {
                elements.Add(new XElement(RamNamespace.Namespace + "SpecifiedTaxRegistration",
                    new XElement(RamNamespace.Namespace + "ID", new XAttribute("schemeID", "VA"), party.GetVatid())));
            }

            if (party.GetTaxId() != null && !isShipToTradeParty)
            {
                elements.Add(new XElement(RamNamespace.Namespace + "SpecifiedTaxRegistration",
                    new XElement(RamNamespace.Namespace + "ID", new XAttribute("schemeID", "FC"), party.GetTaxId())));
            }

            return elements;
        }
    }
}
