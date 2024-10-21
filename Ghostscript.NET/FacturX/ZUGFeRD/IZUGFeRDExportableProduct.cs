
namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	public interface IZUGFeRDExportableProduct
	{

		/// 
		/// <summary>
		/// HUR	hour
		/// KGM	kilogram
		/// KTM	kilometre
		/// KWH	kilowatt hour
		/// LS	lump sum
		/// LTR	litre
		/// MIN	minute
		/// MMK	square millimetre
		/// MMT	millimetre
		/// MTK	square metre
		/// MTQ	cubic metre
		/// MTR	metre
		/// NAR	number of articles
		/// NPR	number of pairs
		/// P1	percent
		/// SET	set
		/// TNE	tonne (metric ton)
		/// WEE	week
		/// </summary>
		/// <returns> a UN/ECE rec 20 unit code see https://www.unece.org/fileadmin/DAM/cefact/recommendations/rec20/rec20_rev3_Annex2e.pdf </returns>
		string GetUnit();

		/// <summary>
		/// Short name of the product
		/// </summary>
		/// <returns> Short name of the product </returns>
		string GetName();

		/// <summary>
		/// long description of the product
		/// </summary>
		/// <returns> long description of the product </returns>
		string GetDescription();

		/// <summary>
		/// Get the ID that had been assigned by the seller to
		/// identify the product
		/// </summary>
		/// <returns> seller assigned product ID </returns>
		string GetSellerAssignedId();

		/// <summary>
		/// Get the ID that had been assigned by the buyer to
		/// identify the product
		/// </summary>
		/// <returns> buyer assigned product ID </returns>
		string GetBuyerAssignedId();

		/// <summary>
		/// VAT percent of the product (e.g. 19, or 5.1 if you like)
		/// </summary>
		/// <returns> VAT percent of the product </returns>
		decimal GetVatPercent();

		bool GetIntraCommunitySupply();

		bool GetReverseCharge();

        string GetTaxCategoryCode();

        string GetTaxExemptionReason();
    }
}