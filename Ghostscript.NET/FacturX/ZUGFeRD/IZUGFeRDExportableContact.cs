namespace Ghostscript.NET.FacturX.ZUGFeRD;

public interface IZUGFeRDExportableContact
{
    /// <summary>
    /// customer identification assigned by the seller
    /// </summary>
    /// <returns> customer identification </returns>
    string GetId();


    /// <summary>
    /// First and last name of the recipient
    /// </summary>
    /// <returns> First and last name of the recipient </returns>
    string GetName();

    string GetPhone();

    string GetEMail();

    string GetFax();


    /// <summary>
    /// Postal code of the recipient
    /// </summary>
    /// <returns> Postal code of the recipient </returns>
    string GetZip();


    /// <summary>
    /// VAT ID (Umsatzsteueridentifikationsnummer) of the contact
    /// </summary>
    /// <returns> VAT ID (Umsatzsteueridentifikationsnummer) of the contact </returns>
    string GetVatid();


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
    string GetAdditionalAddress();
}