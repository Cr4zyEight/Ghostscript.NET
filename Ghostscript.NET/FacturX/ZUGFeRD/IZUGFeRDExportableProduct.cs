
namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	public interface IZugFeRdExportableProduct
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
		string GetSellerAssignedId()
		{
			return null;
		}

		/// <summary>
		/// Get the ID that had been assigned by the buyer to
		/// identify the product
		/// </summary>
		/// <returns> buyer assigned product ID </returns>
		string GetBuyerAssignedId()
		{
			return null;
		}

		/// <summary>
		/// VAT percent of the product (e.g. 19, or 5.1 if you like)
		/// </summary>
		/// <returns> VAT percent of the product </returns>
		decimal GetVatPercent();

		bool GetIntraCommunitySupply()
		{
			return false;
		}

		bool GetReverseCharge()
		{
			return false;
		}

		string GetTaxCategoryCode()
		{
			/*	if (isIntraCommunitySupply())
				{
					return "K"; // "K"; // within europe
				}
				else if (isReverseCharge())
				{
					return "AE"; // "AE"; // to out of europe...
				}
				else if (getVATPercent().compareTo(decimal.Zero) == 0)
				{
					return "Z"; // "Z"; // zero rated goods
				}
				else
				{*/
			return "S"; // "S"; // one of the "standard" rates (not
						// neccessarily a rate, even a deducted VAT
						// is standard calculation)
						//}
		}

		string GetTaxExemptionReason()
		{
			/*	if (isIntraCommunitySupply())
				{
					return "Intra-community supply";
				}
				else if (isReverseCharge())
				{
					return "Reverse Charge";
				}*/
			return null;
		}
	}
}