using System;

namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	public interface IZUGFeRDExportableItem
	{
        /// <summary>
        /// Gets the <see cref="IZUGFeRDExportableProduct"/>
        /// </summary>
        /// <returns></returns>
        IZUGFeRDExportableProduct GetProduct();

        /// <summary>
        /// item level discounts </summary>
        /// <returns> array of the discounts on a single item
        /// 
        /// IZUGFeRDAllowanceCharge[] getItemAllowances() {
        /// return null;
        /// }
        /// 
        /// **
        /// item level price additions </returns>
        /// <returns> array of the additional charges on the item
        /// 
        /// IZUGFeRDAllowanceCharge[] getItemCharges() {
        /// return null;
        /// }
        ///  </returns>
        /// <summary>
        ///*
        /// BT 132 (issue https://github.com/ZUGFeRD/mustangproject/issues/247)
        /// @return
        /// </summary>
        string GetBuyerOrderReferencedDocumentLineId();

		/// <summary>
		/// The price of one item excl. taxes
		/// </summary>
		/// <returns> The price of one item excl. taxes </returns>
		decimal GetPrice();

		/// <summary>
		/// how many get billed
		/// </summary>
		/// <returns> the quantity of the item </returns>
		decimal GetQuantity();

        /// <summary>
        /// how many items units per price
        /// </summary>
        /// <returns> item units per price </returns>
        decimal GetBasisQuantity();

        /// <summary>
        ///*
        /// the ID of an additionally referenced document for this item </summary>
        /// <returns> the id as string </returns>
        string GetAdditionalReferencedDocumentId();


		/// <summary>
		///*
		/// allows to specify multiple(!) referenced documents along with e.g. their typecodes
		/// @return
		/// </summary>
		/*IReferencedDocument[] ReferencedDocuments
		{
			get
			{
				return null;
			}
		}*/
        /// <summary>
        ///*
        /// descriptive texts </summary>
        /// <returns> an array of strings of item specific "includedNotes", text values </returns>
        string[] GetNotes();


        /// <summary>
        ///*
        /// specifies the item level delivery period (there is also one on document level),
        /// this will be included in a BillingSpecifiedPeriod element </summary>
        /// <returns> the beginning of the delivery period </returns>
        DateTime? GetDetailedDeliveryPeriodFrom();

        /// <summary>
        ///*
        /// specifies the item level delivery period (there is also one on document level),
        /// this will be included in a BillingSpecifiedPeriod element </summary>
        /// <returns> the end of the delivery period </returns>
        DateTime? GetDetailedDeliveryPeriodTo();
    }
}