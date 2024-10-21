
namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	public interface IExportableTransaction
    {
        /**
             * appears in /rsm:CrossIndustryDocument/rsm:HeaderExchangedDocument/ram:Name
             *
             * @return Name of document
             */
        string GetDocumentName();


        /**
         *
         *
         * @return Code number of Document type, e.g. "380" for invoiceF
         *	string getDocumentCode() {
            return DocumentCodeTypeConstants.INVOICE;
        }*/


        /**
         * Number, typically invoice number of the invoice
         *
         * @return invoice number
         */
        string GetNumber();


        /**
         * the date when the invoice was created
         *
         * @return when the invoice was created
         */
        DateTime? GetIssueDate();


        /**
         * when the invoice is to be paid
         *
         * @return when the invoice is to be paid
         */
        DateTime? GetDueDate();

        string GetContractReferencedDocument();


        /**
         * the sender of the invoice
         *
         * @return the contact person at the supplier side
         */
        IZugFeRdExportableTradeParty GetSender();


        /**
         * subject of the document e.g. invoice and order
         * number as human readable text
         *
         * @return string with document subject
         */
        string GetSubjectNote();

		/*
			IZUGFeRDAllowanceCharge[] getZFAllowances() {
				return null;
			}


			IZUGFeRDAllowanceCharge[] getZFCharges() {
				return null;
			}


			IZUGFeRDAllowanceCharge[] getZFLogisticsServiceCharges() {
				return null;
			}
		*/

		IZugFeRdExportableItem[] GetZfItems();


		/**
		 * the recipient
		 *
		 * @return the recipient of the invoice
		 */
		IZugFeRdExportableTradeParty GetRecipient();


        /**
         * the creditors payment informations
         * @deprecated use getTradeSettlement
         * @return an array of IZUGFeRDTradeSettlementPayment
         */
        IZugFeRdTradeSettlementPayment[]? GetTradeSettlementPayment();

        /**
         * the payment information for any payment means
         *
         * @return an array of IZUGFeRDTradeSettlement
         */
        IZugFeRdTradeSettlement[]? GetTradeSettlement();


        /**
         * Tax ID (not VAT ID) of the sender
         *
         * @return Tax ID (not VAT ID) of the sender
         */
        string GetOwnTaxId();


        /**
         * VAT ID (Umsatzsteueridentifikationsnummer) of the sender
         *
         * @return VAT ID (Umsatzsteueridentifikationsnummer) of the sender
         */
        string GetOwnVatid();

        /**
         * supplier identification assigned by the costumer
         *
         * @return the sender's identification
         */
        string GetOwnForeignOrganisationId();

		/**
		 * get delivery date
		 *
		 * @return the day the goods have been delivered
		 */
		DateTime? GetDeliveryDate();


        /**
         * get main invoice currency used on the invoice
         *
         * @return three character currency of this invoice
         */
        string GetCurrency();


        /**
         * get payment term descriptional text e.g. Bis zum 22.10.2015 ohne Abzug
         *
         * @return get payment terms
         */
        string GetPaymentTermDescription();

        /**
         * get payment terms. if set, getPaymentTermDescription() and getDueDate() are
         * ignored
         *
         * @return the IZUGFeRDPaymentTerms of the invoice
         **/
        IZugFeRdPaymentTerms GetPaymentTerms();

        /**
         * returns if a rebate agreements exists
         *
         * @return true if a agreement exists
         */
        bool RebateAgreementExists();

        /**
         * get reference document number typically used for Invoice Corrections Will be added as IncludedNote in comfort profile
         *
         * @return the ID of the document this document refers to
         */
        string GetReferenceNumber();


        /**
         * consignee identification (identification of the organisation the goods are shipped to [assigned by the costumer])
         *
         * @return the sender's identification
         */
        string GetShipToOrganisationId();


        /**
         * consignee name (name of the organisation the goods are shipped to)
         *
         * @return the consignee's organisation name
         */
        string GetShipToOrganisationName();


        /**
         * consignee street address (street of the organisation the goods are shipped to)
         *
         * @return consignee street address
         */
        string GetShipToStreet();


        /**
         * consignee street postal code (postal code of the organisation the goods are shipped to)
         *
         * @return consignee postal code
         */
        string GetShipToZip();


        /**
         * consignee city (city of the organisation the goods are shipped to)
         *
         * @return the consignee's city
         */
        string GetShipToLocation();


        /**
         * consignee two digit country code (country code of the organisation the goods are shipped to)
         *
         * @return the consignee's two character country iso code
         */
        string GetShipToCountry();


        /**
         * get the ID of the SellerOrderReferencedDocument, which sits in the ApplicableSupplyChainTradeAgreement/ApplicableHeaderTradeAgreement
         *
         * @return the ID of the document
         */
        string GetSellerOrderReferencedDocumentId();

        /**
         * get the ID of the BuyerOrderReferencedDocument, which sits in the ApplicableSupplyChainTradeAgreement
         *
         * @return the ID of the document
         */
        string GetBuyerOrderReferencedDocumentId();

        /**
         * get the ID of the preceding invoice, which is e.g. to be corrected if this is a correction
         *
         * @return the ID of the document
         */
        string GetInvoiceReferencedDocumentId();

        DateTime? GetInvoiceReferencedIssueDate();

        /**
         * get the issue timestamp of the BuyerOrderReferencedDocument, which sits in the ApplicableSupplyChainTradeAgreement
         *
         * @return the IssueDateTime in format CCYY-MM-DDTHH:MM:SS
         */
        string GetBuyerOrderReferencedDocumentIssueDateTime();


        /**
         * get the TotalPrepaidAmount located in SpecifiedTradeSettlementMonetarySummation (v1) or SpecifiedTradeSettlementHeaderMonetarySummation (v2)
         *
         * @return the total sum (incl. VAT) of prepayments, i.e. the difference between GrandTotalAmount and DuePayableAmount
         */
        decimal GetTotalPrepaidAmount();


		/***
		 * delivery address, i.e. ram:ShipToTradeParty (only supported for zf2)
		 * @return the IZUGFeRDExportableTradeParty delivery address
		 */

        IZugFeRdExportableTradeParty GetDeliveryAddress();


		/***
		 * specifies the document level delivery period, will be included in a BillingSpecifiedPeriod element
		 * @return the beginning of the delivery period
		 */
        DateTime? GetDetailedDeliveryPeriodFrom();

		/***
		 * specifies the document level delivery period, will be included in a BillingSpecifiedPeriod element
		 * @return the end of the delivery period
		 */
        DateTime? GetDetailedDeliveryPeriodTo();

        /**
         * get additional referenced documents acccording to BG-24 XRechnung (Rechnungsbegruendende Unterlagen),
         * i.e. ram:ApplicableHeaderTradeAgreement/ram:AdditionalReferencedDocument
         *
         * @return a array of objects from class FileAttachment

        FileAttachment[] getAdditionalReferencedDocuments() {
            return null;
        }
*/

        /***
         * additional text description
         * @return an array of strings of document wide "includedNotes" (descriptive text values)
         */
        string[] GetNotes();

        string GetSpecifiedProcuringProjectName();

        string GetSpecifiedProcuringProjectId();
    }
}