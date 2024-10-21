using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;


using System.Reflection;
namespace Ghostscript.NET.FacturX.ZUGFeRD
{

    public class ZUGFeRD2PullProvider
    {

        //// MAIN CLASS
        protected internal string ZugferdDateFormatting = "yyyyMMdd";
        protected internal byte[] ZugferdData;
        protected internal IExportableTransaction Trans;
        protected internal TransactionCalculator Calc;
        private string _paymentTermsDescription;
        protected internal Profile Profile = Profiles.GetByName("EN16931");


        /// <summary>
        /// enables the flag to indicate a test invoice in the XML structure
        /// </summary>
        public void SetTest()
        {
        }

        private string VatFormat(decimal value)
        {
            return XmlTools.ScaleDecimal(value, 2);
        }

        private string CurrencyFormat(decimal value)
        {
            return XmlTools.ScaleDecimal(value, 2);
        }

        private string PriceFormat(decimal value)
        {
            return XmlTools.ScaleDecimal(value, 4);
        }

        private string QuantityFormat(decimal value)
        {
            return XmlTools.ScaleDecimal(value, 4);
        }

        public byte[] GetXml()
        {
            return ZugferdData;
        }


        public Profile GetProfile()
        {
            return Profile;
        }

        // @todo check if the two boolean args can be refactored

        /// <summary>
        ///*
        /// returns the UN/CEFACT CII XML for companies(tradeparties), which is actually
        /// the same for ZF1 (v 2013b) and ZF2 (v 2016b) </summary>
        /// <param name="party"> </param>
        /// <param name="isSender"> some attributes are allowed only for senders in certain profiles </param>
        /// <param name="isShipToTradeParty"> some attributes are allowed only for senders or recipients
        /// @return </param>
        protected internal virtual string GetTradePartyAsXml(IZUGFeRDExportableTradeParty party, bool isSender, bool isShipToTradeParty)
        {
            string xml = "";
            // According EN16931 either GlobalID or seller assigned ID might be present for BuyerTradeParty
            // and ShipToTradeParty, but not both. Prefer seller assigned ID for now.
            if (party.GetId() != null)
            {
                xml += "	<ram:ID>" + XmlTools.EncodeXml(party.GetId()) + "</ram:ID>\n";
            }
            else if ((party.GetGlobalIdScheme() != null) && (party.GetGlobalId() != null))
            {
                xml = xml + "           <ram:GlobalID schemeID=\"" + XmlTools.EncodeXml(party.GetGlobalIdScheme()) + "\">" + XmlTools.EncodeXml(party.GetGlobalId()) + "</ram:GlobalID>\n";
            }
            xml += "	<ram:Name>" + XmlTools.EncodeXml(party.GetName()) + "</ram:Name>\n"; //$NON-NLS-2$

            if ((party.GetContact() != null) && (isSender || Profile == Profiles.GetByName("Extended")))
            {
                xml = xml + "<ram:DefinedTradeContact>\n" + "     <ram:PersonName>" + XmlTools.EncodeXml(party.GetContact().GetName()) + "</ram:PersonName>\n";
                if (party.GetContact().GetPhone() != null)
                {

                    xml = xml + "     <ram:TelephoneUniversalCommunication>\n" + "        <ram:CompleteNumber>" + XmlTools.EncodeXml(party.GetContact().GetPhone()) + "</ram:CompleteNumber>\n" + "     </ram:TelephoneUniversalCommunication>\n";
                }

                if ((party.GetContact().GetFax() != null) && (Profile == Profiles.GetByName("Extended")))
                {
                    xml = xml + "     <ram:FaxUniversalCommunication>\n" + "        <ram:CompleteNumber>" + XmlTools.EncodeXml(party.GetContact().GetFax()) + "</ram:CompleteNumber>\n" + "     </ram:FaxUniversalCommunication>\n";
                }
                if (party.GetContact().GetEMail() != null)
                {

                    xml = xml + "     <ram:EmailURIUniversalCommunication>\n" + "        <ram:URIID>" + XmlTools.EncodeXml(party.GetContact().GetEMail()) + "</ram:URIID>\n" + "     </ram:EmailURIUniversalCommunication>\n";
                }

                xml = xml + "  </ram:DefinedTradeContact>";

            }
            xml += "				<ram:PostalTradeAddress>\n" + "					<ram:PostcodeCode>" + XmlTools.EncodeXml(party.GetZip()) + "</ram:PostcodeCode>\n" + "					<ram:LineOne>" + XmlTools.EncodeXml(party.GetStreet()) + "</ram:LineOne>\n";
            if (party.GetAdditionalAddress() != null)
            {
                xml += "				<ram:LineTwo>" + XmlTools.EncodeXml(party.GetAdditionalAddress()) + "</ram:LineTwo>\n";
            }
            xml += "					<ram:CityName>" + XmlTools.EncodeXml(party.GetLocation()) + "</ram:CityName>\n" + "					<ram:CountryID>" + XmlTools.EncodeXml(party.GetCountry()) + "</ram:CountryID>\n" + "				</ram:PostalTradeAddress>\n";
            if ((party.GetVatid() != null) && (!isShipToTradeParty))
            {
                xml += "				<ram:SpecifiedTaxRegistration>\n" + "					<ram:ID schemeID=\"VA\">" + XmlTools.EncodeXml(party.GetVatid()) + "</ram:ID>\n" + "				</ram:SpecifiedTaxRegistration>\n";
            }
            if ((party.GetTaxId() != null) && (!isShipToTradeParty))
            {
                xml += "				<ram:SpecifiedTaxRegistration>\n" + "					<ram:ID schemeID=\"FC\">" + XmlTools.EncodeXml(party.GetTaxId()) + "</ram:ID>\n" + "				</ram:SpecifiedTaxRegistration>\n";

            }
            return xml;

        }


        /// <summary>
        ///*
        /// returns the XML for a charge or allowance on item level </summary>
        /// <param name="allowance"> </param>
        /// <param name="item">
        /// @return </param>
        /*	protected internal virtual string getAllowanceChargeStr(IZUGFeRDAllowanceCharge allowance, IAbsoluteValueProvider item)
            {
                string percentage = "";
                string chargeIndicator = "false";
                if ((allowance.getPercent() != null) && (profile == Profiles.getByName("Extended")))
                {
                    percentage = "<ram:CalculationPercent>" + vatFormat(allowance.getPercent()) + "</ram:CalculationPercent>";
                    percentage += "<ram:BasisAmount>" + item.getValue() + "</ram:BasisAmount>";
                }
                if (allowance.isCharge())
                {
                    chargeIndicator = "true";
                }

                string reason = "";
                if ((allowance.getReason() != null) && (profile == Profiles.getByName("Extended")))
                {
                    // only in extended profile
                    reason = "<ram:Reason>" + XMLTools.encodeXML(allowance.getReason()) + "</ram:Reason>";
                }
                string allowanceChargeStr = "<ram:AppliedTradeAllowanceCharge><ram:ChargeIndicator><udt:Indicator>" + chargeIndicator + "</udt:Indicator></ram:ChargeIndicator>" + percentage + "<ram:ActualAmount>" + priceFormat(allowance.getTotalAmount(item)) + "</ram:ActualAmount>" + reason + "</ram:AppliedTradeAllowanceCharge>";
                return allowanceChargeStr;
            }*/

        public void GenerateXml(IExportableTransaction trans)
        {
            this.Trans = trans;
            this.Calc = new TransactionCalculator(trans);

            bool hasDueDate = false;
            string germanDateFormatting = "dd.MM.yyyy";

            string exemptionReason = "";

            if (trans.GetPaymentTermDescription() != null)
            {
                _paymentTermsDescription = trans.GetPaymentTermDescription();
            }

            if ((string.ReferenceEquals(_paymentTermsDescription, null)) /*&& (trans.getDocumentCode() != org.mustangproject.ZUGFeRD.model.DocumentCodeTypeConstants.CORRECTEDINVOICE)*/)
            {
                _paymentTermsDescription = "Zahlbar ohne Abzug bis " + ((DateTime)trans.GetDueDate()).ToString(germanDateFormatting);

            }

            string senderReg = "";
            /*		if (trans.getOwnOrganisationFullPlaintextInfo() != null)
                    {
                        senderReg = "" + "<ram:IncludedNote>\n" + "		<ram:Content>\n" + XMLTools.encodeXML(trans.getOwnOrganisationFullPlaintextInfo()) + "		</ram:Content>\n" + "<ram:SubjectCode>REG</ram:SubjectCode>\n" + "</ram:IncludedNote>\n";

                    }
            */
            string rebateAgreement = "";
            if (trans.RebateAgreementExists())
            {
                rebateAgreement = "<ram:IncludedNote>\n" + "		<ram:Content>" + "Es bestehen Rabatt- und Bonusvereinbarungen.</ram:Content>\n" + "<ram:SubjectCode>AAK</ram:SubjectCode>\n" + "</ram:IncludedNote>\n";
            }

            string subjectNote = "";
            if (trans.GetSubjectNote() != null)
            {
                subjectNote = "<ram:IncludedNote>\n" + "		<ram:Content>" + XmlTools.EncodeXml(trans.GetSubjectNote()) + "</ram:Content>\n" + "</ram:IncludedNote>\n";
            }

            string typecode = "380";
            /*		if (trans.getDocumentCode() != null)
                    {
                        typecode = trans.getDocumentCode();
                    }*/
            string notes = "";
            if (trans.GetNotes() != null)
            {
                foreach (string currentNote in trans.GetNotes())
                {
                    notes = notes + "<ram:IncludedNote><ram:Content>" + XmlTools.EncodeXml(currentNote) + "</ram:Content></ram:IncludedNote>";

                }
            }
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + "<rsm:CrossIndustryInvoice xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:rsm=\"urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100\"" + " xmlns:ram=\"urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100\"" + " xmlns:udt=\"urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100\">\n" + "	<rsm:ExchangedDocumentContext>\n" + "		<ram:GuidelineSpecifiedDocumentContextParameter>\n" + "			<ram:ID>" + GetProfile().GetId() + "</ram:ID>\n" + "		</ram:GuidelineSpecifiedDocumentContextParameter>\n" + "	</rsm:ExchangedDocumentContext>\n" + "	<rsm:ExchangedDocument>\n" + "		<ram:ID>" + XmlTools.EncodeXml(trans.GetNumber()) + "</ram:ID>\n" + "		<ram:TypeCode>" + typecode + "</ram:TypeCode>\n" + "		<ram:IssueDateTime><udt:DateTimeString format=\"102\">" + ((DateTime)trans.GetIssueDate()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:IssueDateTime>\n" + notes + subjectNote + rebateAgreement + senderReg + "	</rsm:ExchangedDocument>\n" + "	<rsm:SupplyChainTradeTransaction>\n";
            int lineId = 0;
            foreach (IZUGFeRDExportableItem currentItem in trans.GetZfItems())
            {
                lineId++;
                if (currentItem.GetProduct().GetTaxExemptionReason() != null)
                {
                    exemptionReason = "<ram:ExemptionReason>" + XmlTools.EncodeXml(currentItem.GetProduct().GetTaxExemptionReason()) + "</ram:ExemptionReason>";
                }
                notes = "";
                if (currentItem.GetNotes() != null)
                {
                    foreach (string currentNote in currentItem.GetNotes())
                    {
                        notes = notes + "<ram:IncludedNote><ram:Content>" + XmlTools.EncodeXml(currentNote) + "</ram:Content></ram:IncludedNote>";

                    }
                }
                LineCalculator lc = new LineCalculator(currentItem);
                xml = xml + "		<ram:IncludedSupplyChainTradeLineItem>\n" + "			<ram:AssociatedDocumentLineDocument>\n" + "				<ram:LineID>" + lineId + "</ram:LineID>\n" + notes + "			</ram:AssociatedDocumentLineDocument>\n" + "			<ram:SpecifiedTradeProduct>\n";
                // + " <GlobalID schemeID=\"0160\">4012345001235</GlobalID>\n"
                if (currentItem.GetProduct().GetSellerAssignedId() != null)
                {
                    xml = xml + "				<ram:SellerAssignedID>" + XmlTools.EncodeXml(currentItem.GetProduct().GetSellerAssignedId()) + "</ram:SellerAssignedID>\n";
                }
                if (currentItem.GetProduct().GetBuyerAssignedId() != null)
                {
                    xml = xml + "				<ram:BuyerAssignedID>" + XmlTools.EncodeXml(currentItem.GetProduct().GetBuyerAssignedId()) + "</ram:BuyerAssignedID>\n";
                }
                string allowanceChargeStr = "";
                /*			if (currentItem.getItemAllowances() != null && currentItem.getItemAllowances().length > 0)
                            {
                                foreach (IZUGFeRDAllowanceCharge allowance in currentItem.getItemAllowances())
                                {
                                    allowanceChargeStr += getAllowanceChargeStr(allowance, currentItem);
                                }
                            }
                            if (currentItem.getItemCharges() != null && currentItem.getItemCharges().length > 0)
                            {
                                foreach (IZUGFeRDAllowanceCharge charge in currentItem.getItemCharges())
                                {
                                    allowanceChargeStr += getAllowanceChargeStr(charge, currentItem);

                                }
                            }
                */

                xml = xml + "					<ram:Name>" + XmlTools.EncodeXml(currentItem.GetProduct().GetName()) + "</ram:Name>\n" + "				<ram:Description>" + XmlTools.EncodeXml(currentItem.GetProduct().GetDescription()) + "</ram:Description>\n" + "			</ram:SpecifiedTradeProduct>\n" + "			<ram:SpecifiedLineTradeAgreement>\n" + "				<ram:GrossPriceProductTradePrice>\n" + "					<ram:ChargeAmount>" + PriceFormat(lc.GetPriceGross()) + "</ram:ChargeAmount>\n" + "<ram:BasisQuantity unitCode=\"" + XmlTools.EncodeXml(currentItem.GetProduct().GetUnit()) + "\">" + QuantityFormat(currentItem.GetBasisQuantity()) + "</ram:BasisQuantity>\n" + allowanceChargeStr + "				</ram:GrossPriceProductTradePrice>\n" + "				<ram:NetPriceProductTradePrice>\n" + "					<ram:ChargeAmount>" + PriceFormat(lc.GetPrice()) + "</ram:ChargeAmount>\n" + "					<ram:BasisQuantity unitCode=\"" + XmlTools.EncodeXml(currentItem.GetProduct().GetUnit()) + "\">" + QuantityFormat(currentItem.GetBasisQuantity()) + "</ram:BasisQuantity>\n" + "				</ram:NetPriceProductTradePrice>\n" + "			</ram:SpecifiedLineTradeAgreement>\n" + "			<ram:SpecifiedLineTradeDelivery>\n" + "				<ram:BilledQuantity unitCode=\"" + XmlTools.EncodeXml(currentItem.GetProduct().GetUnit()) + "\">" + QuantityFormat(currentItem.GetQuantity()) + "</ram:BilledQuantity>\n" + "			</ram:SpecifiedLineTradeDelivery>\n" + "			<ram:SpecifiedLineTradeSettlement>\n" + "				<ram:ApplicableTradeTax>\n" + "					<ram:TypeCode>VAT</ram:TypeCode>\n" + exemptionReason + "					<ram:CategoryCode>" + currentItem.GetProduct().GetTaxCategoryCode() + "</ram:CategoryCode>\n" + "					<ram:RateApplicablePercent>" + VatFormat(currentItem.GetProduct().GetVatPercent()) + "</ram:RateApplicablePercent>\n" + "				</ram:ApplicableTradeTax>\n";
                if ((currentItem.GetDetailedDeliveryPeriodFrom() != null) || (currentItem.GetDetailedDeliveryPeriodTo() != null))
                {
                    xml = xml + "<ram:BillingSpecifiedPeriod>";
                    if (currentItem.GetDetailedDeliveryPeriodFrom() != null)
                    {
                        xml = xml + "<ram:StartDateTime><udt:DateTimeString format='102'>" + ((DateTime)currentItem.GetDetailedDeliveryPeriodFrom()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:StartDateTime>";
                    }
                    if (currentItem.GetDetailedDeliveryPeriodTo() != null)
                    {
                        xml = xml + "<ram:EndDateTime><udt:DateTimeString format='102'>" + ((DateTime)currentItem.GetDetailedDeliveryPeriodTo()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:EndDateTime>";
                    }
                    xml = xml + "</ram:BillingSpecifiedPeriod>";

                }

                xml = xml + "				<ram:SpecifiedTradeSettlementLineMonetarySummation>\n" + "					<ram:LineTotalAmount>" + CurrencyFormat(lc.GetItemTotalNetAmount()) + "</ram:LineTotalAmount>\n" + "				</ram:SpecifiedTradeSettlementLineMonetarySummation>\n";
                if (currentItem.GetAdditionalReferencedDocumentId() != null)
                {
                    xml = xml + "			<ram:AdditionalReferencedDocument><ram:IssuerAssignedID>" + currentItem.GetAdditionalReferencedDocumentId() + "</ram:IssuerAssignedID><ram:TypeCode>130</ram:TypeCode></ram:AdditionalReferencedDocument>\n";

                }
                xml = xml + "			</ram:SpecifiedLineTradeSettlement>\n" + "		</ram:IncludedSupplyChainTradeLineItem>\n";

            }

            xml = xml + "		<ram:ApplicableHeaderTradeAgreement>\n";
            if (trans.GetReferenceNumber() != null)
            {
                xml = xml + "			<ram:BuyerReference>" + XmlTools.EncodeXml(trans.GetReferenceNumber()) + "</ram:BuyerReference>\n";

            }
            xml = xml + "			<ram:SellerTradeParty>\n" + GetTradePartyAsXml(trans.GetSender(), true, false) + "			</ram:SellerTradeParty>\n" + "			<ram:BuyerTradeParty>\n";
            // + " <ID>GE2020211</ID>\n"
            // + " <GlobalID schemeID=\"0088\">4000001987658</GlobalID>\n"

            xml += GetTradePartyAsXml(trans.GetRecipient(), false, false);
            xml += "			</ram:BuyerTradeParty>\n";

            if (trans.GetBuyerOrderReferencedDocumentId() != null)
            {
                xml = xml + "   <ram:BuyerOrderReferencedDocument>\n" + "       <ram:IssuerAssignedID>" + XmlTools.EncodeXml(trans.GetBuyerOrderReferencedDocumentId()) + "</ram:IssuerAssignedID>\n" + "   </ram:BuyerOrderReferencedDocument>\n";
            }
            if (trans.GetContractReferencedDocument() != null)
            {
                xml = xml + "   <ram:ContractReferencedDocument>\n" + "       <ram:IssuerAssignedID>" + XmlTools.EncodeXml(trans.GetContractReferencedDocument()) + "</ram:IssuerAssignedID>\n" + "    </ram:ContractReferencedDocument>\n";
            }

            // Additional Documents of XRechnung (Rechnungsbegruendende Unterlagen - BG-24 XRechnung)
            /*		if (trans.getAdditionalReferencedDocuments() != null)
                    {
                        foreach (FileAttachment f in trans.getAdditionalReferencedDocuments())
                        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final string documentContent = new string(Base64.getEncoder().encodeToString(f.getData()));
                            string documentContent = new string(Convert.ToBase64String(f.getData()));
                            xml = xml + "  <ram:AdditionalReferencedDocument>\n" + "    <ram:IssuerAssignedID>" + f.getFilename() + "</ram:IssuerAssignedID>\n" + "    <ram:TypeCode>916</ram:TypeCode>\n" + "    <ram:Name>" + f.getDescription() + "</ram:Name>\n" + "    <ram:AttachmentBinaryObject mimeCode=\"" + f.getMimetype() + "\"\n" + "      filename=\"" + f.getFilename() + "\">" + documentContent + "</ram:AttachmentBinaryObject>\n" + "  </ram:AdditionalReferencedDocument>\n";
                        }
                    }
            */
            xml = xml + "		</ram:ApplicableHeaderTradeAgreement>\n" + "		<ram:ApplicableHeaderTradeDelivery>\n";
            if (this.Trans.GetDeliveryAddress() != null)
            {
                xml += "<ram:ShipToTradeParty>" + GetTradePartyAsXml(this.Trans.GetDeliveryAddress(), false, true) + "</ram:ShipToTradeParty>";
            }

            xml += "			<ram:ActualDeliverySupplyChainEvent>\n" + "				<ram:OccurrenceDateTime>";

            if (trans.GetDeliveryDate() != null)
            {
                xml += "<udt:DateTimeString format=\"102\">" + ((DateTime)trans.GetDeliveryDate()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString>";
            }
            else
            {
                throw new System.InvalidOperationException("No delivery date provided");
            }
            xml += "</ram:OccurrenceDateTime>\n";
            xml += "			</ram:ActualDeliverySupplyChainEvent>\n" + "		</ram:ApplicableHeaderTradeDelivery>\n" + "		<ram:ApplicableHeaderTradeSettlement>\n" + "			<ram:PaymentReference>" + XmlTools.EncodeXml(trans.GetNumber()) + "</ram:PaymentReference>\n" + "			<ram:InvoiceCurrencyCode>" + trans.GetCurrency() + "</ram:InvoiceCurrencyCode>\n";

            if (trans.GetTradeSettlementPayment() != null)
            {
                foreach (IZUGFeRDTradeSettlementPayment payment in trans.GetTradeSettlementPayment())
                {
                    if (payment != null)
                    {
                        hasDueDate = true;
                        xml += payment.GetSettlementXml();
                    }
                }
            }
            if (trans.GetTradeSettlement() != null)
            {
                foreach (IZUGFeRDTradeSettlement payment in trans.GetTradeSettlement())
                {
                    if (payment != null)
                    {
                        if (payment is IZUGFeRDTradeSettlementPayment)
                        {
                            hasDueDate = true;
                        }
                        xml += payment.GetSettlementXml();
                    }
                }
            }
            /*		if (trans.getDocumentCode() == DocumentCodeTypeConstants.CORRECTEDINVOICE)
                    {
                        hasDueDate = false;
                    }
            */
            Dictionary<decimal, VatAmount> vatPercentAmountMap = Calc.GetVatPercentAmountMap();
            foreach (decimal currentTaxPercent in vatPercentAmountMap.Keys)
            {
                VatAmount amount = vatPercentAmountMap[currentTaxPercent];
                if (amount != null)
                {
                    xml += "			<ram:ApplicableTradeTax>\n" + "				<ram:CalculatedAmount>" + CurrencyFormat(amount.GetCalculated()) + "</ram:CalculatedAmount>\n" + "				<ram:TypeCode>VAT</ram:TypeCode>\n" + exemptionReason + "				<ram:BasisAmount>" + CurrencyFormat(amount.GetBasis()) + "</ram:BasisAmount>\n" + "				<ram:CategoryCode>" + amount.GetCategoryCode() + "</ram:CategoryCode>\n" + "				<ram:RateApplicablePercent>" + VatFormat(currentTaxPercent) + "</ram:RateApplicablePercent>\n" + "			</ram:ApplicableTradeTax>\n"; //$NON-NLS-2$

                }
            }
            if ((trans.GetDetailedDeliveryPeriodFrom() != null) || (trans.GetDetailedDeliveryPeriodTo() != null))
            {
                xml = xml + "<ram:BillingSpecifiedPeriod>";
                if (trans.GetDetailedDeliveryPeriodFrom() != null)
                {
                    xml = xml + "<ram:StartDateTime><udt:DateTimeString format='102'>" + ((DateTime)trans.GetDetailedDeliveryPeriodFrom()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:StartDateTime>";
                }
                if (trans.GetDetailedDeliveryPeriodTo() != null)
                {
                    xml = xml + "<ram:EndDateTime><udt:DateTimeString format='102'>" + ((DateTime)trans.GetDetailedDeliveryPeriodTo()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:EndDateTime>";
                }
                xml = xml + "</ram:BillingSpecifiedPeriod>";


            }
            /*
                    if ((trans.getZFCharges() != null) && (trans.getZFCharges().length > 0))
                    {

                        foreach (decimal currentTaxPercent in VATPercentAmountMap.Keys)
                        {
                            if (calc.getChargesForPercent(currentTaxPercent).compareTo(decimal.Zero) != 0)
                            {


                                xml = xml + "	 <ram:SpecifiedTradeAllowanceCharge>\n" + "        <ram:ChargeIndicator>\n" + "          <udt:Indicator>true</udt:Indicator>\n" + "        </ram:ChargeIndicator>\n" + "        <ram:ActualAmount>" + currencyFormat(calc.getChargesForPercent(currentTaxPercent)) + "</ram:ActualAmount>\n" + "        <ram:Reason>" + XMLTools.encodeXML(calc.getChargeReasonForPercent(currentTaxPercent)) + "</ram:Reason>\n" + "        <ram:CategoryTradeTax>\n" + "          <ram:TypeCode>VAT</ram:TypeCode>\n" + "          <ram:CategoryCode>" + VATPercentAmountMap[currentTaxPercent].getCategoryCode() + "</ram:CategoryCode>\n" + "          <ram:RateApplicablePercent>" + vatFormat(currentTaxPercent) + "</ram:RateApplicablePercent>\n" + "        </ram:CategoryTradeTax>\n" + "      </ram:SpecifiedTradeAllowanceCharge>	\n";

                            }
                        }

                    }

                    if ((trans.getZFAllowances() != null) && (trans.getZFAllowances().length > 0))
                    {
                        foreach (decimal currentTaxPercent in VATPercentAmountMap.Keys)
                        {
                            if (calc.getAllowancesForPercent(currentTaxPercent).compareTo(decimal.Zero) != 0)
                            {
                                xml = xml + "	 <ram:SpecifiedTradeAllowanceCharge>\n" + "        <ram:ChargeIndicator>\n" + "          <udt:Indicator>false</udt:Indicator>\n" + "        </ram:ChargeIndicator>\n" + "        <ram:ActualAmount>" + currencyFormat(calc.getAllowancesForPercent(currentTaxPercent)) + "</ram:ActualAmount>\n" + "        <ram:Reason>" + XMLTools.encodeXML(calc.getAllowanceReasonForPercent(currentTaxPercent)) + "</ram:Reason>\n" + "        <ram:CategoryTradeTax>\n" + "          <ram:TypeCode>VAT</ram:TypeCode>\n" + "          <ram:CategoryCode>" + VATPercentAmountMap[currentTaxPercent].getCategoryCode() + "</ram:CategoryCode>\n" + "          <ram:RateApplicablePercent>" + vatFormat(currentTaxPercent) + "</ram:RateApplicablePercent>\n" + "        </ram:CategoryTradeTax>\n" + "      </ram:SpecifiedTradeAllowanceCharge>	\n";
                            }
                        }
                    }
            */

            if (trans.GetPaymentTerms() == null)
            {
                xml = xml + "			<ram:SpecifiedTradePaymentTerms>\n" + "				<ram:Description>" + _paymentTermsDescription + "</ram:Description>\n";

                /*			if (trans.getTradeSettlement() != null)
                            {
                                foreach (IZUGFeRDTradeSettlement payment in trans.getTradeSettlement())
                                {
                                    if ((payment != null) && (payment is IZUGFeRDTradeSettlementDebit))
                                    {
                                        xml += payment.getPaymentXML();
                                    }
                                }
                            }
                */
                if (hasDueDate && (trans.GetDueDate() != null))
                {
                    xml = xml + "				<ram:DueDateDateTime><udt:DateTimeString format=\"102\">" + ((DateTime)trans.GetDueDate()).ToString(ZugferdDateFormatting) + "</udt:DateTimeString></ram:DueDateDateTime>\n"; // 20130704

                }
                xml = xml + "			</ram:SpecifiedTradePaymentTerms>\n";
            }
            else
            {
                xml = xml + BuildPaymentTermsXml();
            }


            //string allowanceTotalLine = "<ram:AllowanceTotalAmount>" + currencyFormat(calc.getAllowancesForPercent(null)) + "</ram:AllowanceTotalAmount>";

            //string chargesTotalLine = "<ram:ChargeTotalAmount>" + currencyFormat(calc.getChargesForPercent(null)) + "</ram:ChargeTotalAmount>";
            string chargesTotalLine = "";
            string allowanceTotalLine = "";
            xml = xml + "<ram:SpecifiedTradeSettlementHeaderMonetarySummation>\n" + "<ram:LineTotalAmount>" + CurrencyFormat(Calc.GetTotal()) + "</ram:LineTotalAmount>\n" + chargesTotalLine + allowanceTotalLine + "<ram:TaxBasisTotalAmount>" + CurrencyFormat(Calc.GetTaxBasis()) + "</ram:TaxBasisTotalAmount>\n" + "				<ram:TaxTotalAmount currencyID=\"" + trans.GetCurrency() + "\">" + CurrencyFormat(Calc.GetGrandTotal() - Calc.GetTaxBasis()) + "</ram:TaxTotalAmount>\n" + "				<ram:GrandTotalAmount>" + CurrencyFormat(Calc.GetGrandTotal()) + "</ram:GrandTotalAmount>\n" + "             <ram:TotalPrepaidAmount>" + CurrencyFormat(Calc.GetTotalPrepaid()) + "</ram:TotalPrepaidAmount>\n" + "				<ram:DuePayableAmount>" + CurrencyFormat(Calc.GetGrandTotal() - Calc.GetTotalPrepaid()) + "</ram:DuePayableAmount>\n" + "			</ram:SpecifiedTradeSettlementHeaderMonetarySummation>\n" + "		</ram:ApplicableHeaderTradeSettlement>\n";
            // + " <IncludedSupplyChainTradeLineItem>\n"
            // + " <AssociatedDocumentLineDocument>\n"
            // + " <IncludedNote>\n"
            // + " <Content>Wir erlauben uns Ihnen folgende Positionen aus der Lieferung Nr.
            // 2013-51112 in Rechnung zu stellen:</Content>\n"
            // + " </IncludedNote>\n"
            // + " </AssociatedDocumentLineDocument>\n"
            // + " </IncludedSupplyChainTradeLineItem>\n";

            xml = xml + "	</rsm:SupplyChainTradeTransaction>\n" + "</rsm:CrossIndustryInvoice>";

            System.Text.UTF8Encoding encoding = new
                     System.Text.UTF8Encoding();

            byte[] zugferdRaw;
            zugferdRaw = encoding.GetBytes(xml); ;

            ZugferdData = XmlTools.RemoveBom(zugferdRaw);
        }

        public void SetProfile(Profile p)
        {
            Profile = p;
        }
        private string BuildPaymentTermsXml()
        {
            string paymentTermsXml = "<ram:SpecifiedTradePaymentTerms>";

            IZUGFeRDPaymentTerms paymentTerms = Trans.GetPaymentTerms();
            //IZUGFeRDPaymentDiscountTerms discountTerms = paymentTerms.getDiscountTerms();
            DateTime dueDate = paymentTerms.GetDueDate();
            if (dueDate != null /*&& discountTerms != null && discountTerms.getBaseDate() != null*/)
            {
                throw new System.InvalidOperationException("if paymentTerms.dueDate is specified, paymentTerms.discountTerms.baseDate has not to be specified");
            }
            paymentTermsXml += "<ram:Description>" + paymentTerms.GetDescription() + "</ram:Description>";
            if (dueDate != null)
            {
                paymentTermsXml += "<ram:DueDateDateTime>";
                paymentTermsXml += "<udt:DateTimeString format=\"102\">" + dueDate.ToString(ZugferdDateFormatting) + "</udt:DateTimeString>";
                paymentTermsXml += "</ram:DueDateDateTime>";
            }
            /*
                    if (discountTerms != null)
                    {
                        paymentTermsXml += "<ram:ApplicableTradePaymentDiscountTerms>";
                        string currency = trans.getCurrency();
                        string basisAmount = currencyFormat(calc.getGrandTotal());
                        paymentTermsXml += "<ram:BasisAmount currencyID=\"" + currency + "\">" + basisAmount + "</ram:BasisAmount>";
                        paymentTermsXml += "<ram:CalculationPercent>" + discountTerms.getCalculationPercentage().ToString() + "</ram:CalculationPercent>";

                        if (discountTerms.getBaseDate() != null)
                        {
                            DateTime baseDate = discountTerms.getBaseDate();
                            paymentTermsXml += "<ram:BasisDateTime>";
                            paymentTermsXml += "<udt:DateTimeString format=\"102\">" + zugferdDateFormat.format(baseDate) + "</udt:DateTimeString>";
                            paymentTermsXml += "</ram:BasisDateTime>";

                            paymentTermsXml += "<ram:BasisPeriodMeasure unitCode=\"" + discountTerms.getBasePeriodUnitCode() + "\">" + discountTerms.getBasePeriodMeasure() + "</ram:BasisPeriodMeasure>";
                        }

                        paymentTermsXml += "</ram:ApplicableTradePaymentDiscountTerms>";
                    }
            */
            paymentTermsXml += "</ram:SpecifiedTradePaymentTerms>";
            return paymentTermsXml;
        }


    }
}