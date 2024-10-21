	
namespace Ghostscript.NET.FacturX.ZUGFeRD
{

	public interface IZugFeRdExportableTradeParty
	{

		/// <summary>
		/// customer identification assigned by the seller
		/// </summary>
		/// <returns> customer identification </returns>
		string GetId()
		{
			return null;
		}

		/// <summary>
		/// customer global identification assigned by the seller
		/// </summary>
		/// <returns> customer identification </returns>
		string GetGlobalId()
		{
			return null;
		}

		/// <summary>
		///*
		/// gets the official representation </summary>
		/// <returns> the interface with the attributes of the legal organisation </returns>
		/*	IZUGFeRDLegalOrganisation getLegalOrganisation()
			{
				return null;
			}
			*/
		/// <summary>
		/// customer global identification scheme
		/// </summary>
		/// <returns> customer identification </returns>
		string GetGlobalIdScheme()
		{
			return null;
		}


		IZugFeRdExportableContact GetContact()
		{
			return null;
		}

		/// <summary>
		/// First and last name of the recipient
		/// </summary>
		/// <returns> First and last name of the recipient </returns>
		string GetName();
		/// <summary>
		/// Postal code of the recipient
		/// </summary>
		/// <returns> Postal code of the recipient </returns>
		string GetZip();
		/// <summary>
		/// VAT ID (Umsatzsteueridentifikationsnummer) of the contact
		/// </summary>
		/// <returns> VAT ID (Umsatzsteueridentifikationsnummer) of the contact </returns>
		string GetVatid()
		{
			return null;
		}
		/// <summary>
		/// two-letter country code of the contact
		/// </summary>
		/// <returns> two-letter iso country code of the contact </returns>
		string GetCountry();
		/// <summary>
		/// Returns the city of the contact
		/// </summary>
		/// <returns> Returns the city of the recipient </returns>
		string GetLocation();
		/// <summary>
		/// Returns the street address (street+number) of the contact
		/// </summary>
		/// <returns> street address (street+number) of the contact </returns>
		string GetStreet();
		/// <summary>
		/// returns additional address information which is display in xml tag "LineTwo"
		/// </summary>
		/// <returns> additional address information </returns>
		string GetAdditionalAddress()
		{
			return null;
		}
		/// <summary>
		///*
		/// obligatory for sender but not for recipient </summary>
		/// <returns> the tax id as string </returns>
		string GetTaxId()
		{
			return null;
		}
	}
}