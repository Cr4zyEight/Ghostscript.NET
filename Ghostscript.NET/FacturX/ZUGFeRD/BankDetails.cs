using System.Xml.Linq;

namespace Ghostscript.NET.FacturX.ZUGFeRD;

/// <summary>
/// provides e.g. the IBAN to transfer money to :-)
/// </summary>
public class BankDetails : ZugFeRdXmlWriter, IZUGFeRDTradeSettlementPayment
{
    protected internal string Iban, Bic, AccountName;

    public BankDetails(string iban, string bic)
    {
        Iban = iban;
        Bic = bic;
    }

    public string GetOwnPaymentInfoText()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///*
    ///  getOwn... methods will be removed in the future in favor of Tradeparty (e.g. Sender) class
    /// 
    /// </summary>
    [Obsolete]
    public string GetOwnBic()
    {
        return GetBic();
    }

    [Obsolete]
    public string GetOwnIban()
    {
        return GetIban();
    }

    public string GetAccountName()
    {
        return AccountName;
    }

    public XElement GetSettlementXml()
    {
        XElement settlementPaymentMeans = new XElement($"{RamNamespace.Prefix}:SpecifiedTradeSettlementPaymentMeans",
            new XElement($"{RamNamespace.Prefix}:TypeCode", "58"),
            new XElement($"{RamNamespace.Prefix}:Information", "SEPA credit transfer"),
            new XElement($"{RamNamespace.Prefix}:PayeePartyCreditorFinancialAccount",
                new XElement($"{RamNamespace.Prefix}:IBANID", XmlTools.EncodeXml(GetOwnIban()))
            )
        );

        // Conditionally add AccountName if it exists
        if (GetAccountName() != null)
        {
            settlementPaymentMeans.Element($"{RamNamespace.Prefix}:PayeePartyCreditorFinancialAccount")?
                .Add(new XElement($"{RamNamespace.Prefix}:AccountName", XmlTools.EncodeXml(GetAccountName())));
        }

        // Add BICID element for financial institution details
        settlementPaymentMeans.Add(new XElement($"{RamNamespace.Prefix}:PayeeSpecifiedCreditorFinancialInstitution",
            new XElement($"{RamNamespace.Prefix}:BICID", XmlTools.EncodeXml(GetOwnBic()))
        ));

        return settlementPaymentMeans;
    }


    public XElement GetPaymentXml()
    {
        return null;
    }

    public virtual string GetIban()
    {
        return Iban;
    }

    /// <summary>
    /// Sets the IBAN "ID", which means that it only needs to be a way to uniquely
    /// identify the IBAN. Of course you will specify your own IBAN in full length but
    /// if you deduct from a customer's account you may e.g. leave out the first or last
    /// digits so that nobody spying on the invoice gets to know the complete number </summary>
    /// <param name="iban"> the "IBAN ID", i.e. the IBAN or parts of it </param>
    /// <returns> fluent setter </returns>
    public virtual BankDetails SetIban(string iban)
    {
        Iban = iban;
        return this;
    }

    public virtual string GetBic()
    {
        return Bic;
    }

    /// <summary>
    ///*
    /// The bank identifier. Bank name is no longer neccessary in SEPA. </summary>
    /// <param name="bic"> the bic code </param>
    /// <returns> fluent setter </returns>
    public virtual BankDetails SetBic(string bic)
    {
        Bic = bic;
        return this;
    }


    /// <summary>
    /// set Holder </summary>
    /// <param name="name"> account name (usually account holder if != sender) </param>
    /// <returns> fluent setter </returns>
    public virtual BankDetails SetAccountName(string name)
    {
        AccountName = name;
        return this;
    }
}