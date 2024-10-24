namespace Ghostscript.NET.FacturX.ZUGFeRD;

/// <summary>
///*
/// A organisation, i.e. usually a company
/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class TradeParty implements IZUGFeRDExportableTradeParty
public class TradeParty : IZUGFeRDExportableTradeParty
{
    protected internal string AdditionalAddress;

    protected internal IList<BankDetails> BankDetails = new List<BankDetails>();

    //protected internal IList<IZUGFeRDTradeSettlementDebit> debitDetails = new List<IZUGFeRDTradeSettlementDebit>();
    protected internal Contact Contact;
    protected internal string Id;

    protected internal string Name, Zip, Street, Location, Country;

    protected internal string TaxId, VatId;
    //	protected internal LegalOrganisation legalOrg = null;

    /// <summary>
    /// Default constructor.
    /// Probably a bad idea but might be needed by jackson or similar
    /// </summary>
    public TradeParty()
    {
    }


    /// <summary>
    /// </summary>
    /// <param name="name"> of the company </param>
    /// <param name="street"> street and number (use setAdditionalAddress for more parts) </param>
    /// <param name="zip"> postcode of the company </param>
    /// <param name="location"> city of the company </param>
    /// <param name="country"> two letter ISO code </param>
    public TradeParty(string name, string street, string zip, string location, string country)
    {
        Name = name;
        Street = street;
        Zip = zip;
        Location = location;
        Country = country;
    }

    /// <summary>
    /// XML parsing constructor </summary>
    /// <param name="nodes"> the nodelist returned e.g. from xpath </param>
    //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
    //ORIGINAL LINE: public TradeParty(NodeList nodes)
    public string GetId()
    {
        return Id;
    }

    public string GetGlobalId()
    {
        return null;
    }

    public string GetGlobalIdScheme()
    {
        return null;
    }

    public string GetVatid()
    {
        return VatId;
    }

    public string GetTaxId()
    {
        return TaxId;
    }

    public virtual string GetName()
    {
        return Name;
    }


    public virtual string GetZip()
    {
        return Zip;
    }

    public string GetStreet()
    {
        return Street;
    }

    public string GetLocation()
    {
        return Location;
    }

    public string GetCountry()
    {
        return Country;
    }

    public IZUGFeRDExportableContact GetContact()
    {
        return Contact;
    }


    public string GetAdditionalAddress()
    {
        return AdditionalAddress;
    }

    /// <summary>
    /// if it's a customer, this can e.g. be the customer ID
    /// </summary>
    /// <param name="id"> customer/seller number </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetId(string id)
    {
        Id = id;
        return this;
    }

    /// <summary>
    ///*
    /// (optional) a named contact person </summary>
    /// <seealso cref="ZUGFeRD.Contact"/>
    /// <param name="c"> the named contact person </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetContact(Contact c)
    {
        Contact = c;
        return this;
    }

    /// <summary>
    ///*
    /// required (for senders, if payment is not debit): the BIC and IBAN </summary>
    /// <param name="s"> bank credentials </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty AddBankDetails(BankDetails s)
    {
        BankDetails.Add(s);
        return this;
    }

    /// <summary>
    /// (optional)
    /// </summary>
    /// <param name="debitDetail"> </param>
    /// <returns> fluent setter </returns>
    /*public virtual TradeParty addDebitDetails(IZUGFeRDTradeSettlementDebit debitDetail)
    {
        debitDetails.add(debitDetail);
        return this;
    }*/
    /*
        public override IZUGFeRDLegalOrganisation getLegalOrganisation()
        {
            return legalOrg;
        }

        public virtual TradeParty setLegalOrganisation(LegalOrganisation legalOrganisation)
        {
            legalOrg = legalOrganisation;
            return this;
        }
    */
    public virtual IList<BankDetails> GetBankDetails()
    {
        return BankDetails;
    }

    /// <summary>
    ///*
    /// a general tax ID </summary>
    /// <param name="taxId"> tax number of the organisation </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty AddTaxId(string taxId)
    {
        TaxId = taxId;
        return this;
    }

    /// <summary>
    ///*
    /// the USt-ID </summary>
    /// <param name="vatId"> Ust-ID </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty AddVatId(string vatId)
    {
        VatId = vatId;
        return this;
    }

    public virtual TradeParty SetVatid(string vaTid)
    {
        VatId = vaTid;
        return this;
    }

    public virtual TradeParty SetTaxId(string tax)
    {
        TaxId = tax;
        return this;
    }

    /// <summary>
    ///*
    /// required, usually done in the constructor: the complete name of the organisation </summary>
    /// <param name="name"> complete legal name </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    ///*
    /// usually set in the constructor, required for recipients in german invoices: postcode </summary>
    /// <param name="zip"> postcode </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetZip(string zip)
    {
        Zip = zip;
        return this;
    }

    /// <summary>
    ///*
    /// usually set in constructor, required in germany, street and house number </summary>
    /// <param name="street"> street name and number </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetStreet(string street)
    {
        Street = street;
        return this;
    }

    /// <summary>
    ///*
    /// usually set in constructor, usually required in germany, the city of the organisation </summary>
    /// <param name="location"> city </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetLocation(string location)
    {
        Location = location;
        return this;
    }

    /// <summary>
    ///*
    /// two-letter ISO code of the country </summary>
    /// <param name="country"> two-letter-code </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetCountry(string country)
    {
        Country = country;
        return this;
    }


    public virtual string GetVatId()
    {
        return VatId;
    }

    public virtual IZUGFeRDTradeSettlement[]? GetAsTradeSettlement()
    {
        if (BankDetails.Count == 0) return null;
        return BankDetails.ToArray();
    }


    /// <summary>
    ///*
    /// additional parts of the address, e.g. which floor.
    /// Street address will become "lineOne", this will become "lineTwo" </summary>
    /// <param name="additionalAddress"> additional address description </param>
    /// <returns> fluent setter </returns>
    public virtual TradeParty SetAdditionalAddress(string additionalAddress)
    {
        AdditionalAddress = additionalAddress;
        return this;
    }
}