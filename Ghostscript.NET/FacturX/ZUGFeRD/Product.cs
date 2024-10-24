namespace Ghostscript.NET.FacturX.ZUGFeRD;

/// <summary>
///*
/// describes a product, good or service used in an invoice item line
/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class Product implements org.mustangproject.ZUGFeRD.IZUGFeRDExportableProduct
public class Product : IZUGFeRDExportableProduct
{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
    protected internal bool IsIntraCommunitySupplyConflict;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods of the current type:
    protected internal bool IsReverseChargeConflict;
    protected internal string Unit, Name, Description, SellerAssignedId, BuyerAssignedId;
    protected internal decimal VatPercent;

    /// <summary>
    ///*
    /// default constructor </summary>
    /// <param name="name"> product short name </param>
    /// <param name="description"> product long name </param>
    /// <param name="unit"> a two/three letter UN/ECE rec 20 unit code, e.g. "C62" for piece </param>
    /// <param name="vatPercent"> product vat rate </param>
    public Product(string name, string description, string unit, decimal vatPercent)
    {
        Unit = unit;
        Name = name;
        Description = description;
        VatPercent = vatPercent;
    }


    /// <summary>
    ///*
    /// empty constructor
    /// just for jackson etc
    /// </summary>
    public Product()
    {
    }


    public virtual string GetSellerAssignedId()
    {
        return SellerAssignedId;
    }

    public virtual string GetBuyerAssignedId()
    {
        return BuyerAssignedId;
    }

    public string GetUnit()
    {
        return Unit;
    }

    public string GetName()
    {
        return Name;
    }

    public string GetDescription()
    {
        return Description;
    }

    public decimal GetVatPercent()
    {
        return VatPercent;
    }

    public bool GetIntraCommunitySupply()
    {
        return false;
    }

    public bool GetReverseCharge()
    {
        return false;
    }

    public string GetTaxCategoryCode()
    {
        if (IsIntraCommunitySupply())
            return "K"; // "K"; // within europe
        if (IsReverseCharge())
            return "AE"; // "AE"; // to out of europe...
        if (GetVatPercent().Equals(decimal.Zero))
            return "Z"; // "Z"; // zero rated goods
        // "S" - one of the "standard" rates (not
        // neccessarily a rate, even a deducted VAT
        // is standard calculation)
        return "S";
    }

    public string GetTaxExemptionReason()
    {
        if (IsIntraCommunitySupply())
            return "Intra-community supply";
        if (IsReverseCharge()) return "Reverse Charge";
        return null;
    }

    /// <summary>
    ///*
    /// how the seller identifies this type of product </summary>
    /// <param name="sellerAssignedId"> a unique string </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetSellerAssignedId(string sellerAssignedId)
    {
        SellerAssignedId = sellerAssignedId;
        return this;
    }

    /// <summary>
    ///*
    /// if the buyer provided an ID how he refers to this product </summary>
    /// <param name="buyerAssignedId"> a string the buyer provided </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetBuyerAssignedId(string buyerAssignedId)
    {
        BuyerAssignedId = buyerAssignedId;
        return this;
    }

    public bool IsReverseCharge()
    {
        return IsReverseChargeConflict;
    }

    public bool IsIntraCommunitySupply()
    {
        return IsIntraCommunitySupplyConflict;
    }

    /// <summary>
    ///*
    /// sets reverse charge(=delivery to outside EU) </summary>
    /// <returns> fluent setter </returns>
    public virtual Product SetReverseCharge()
    {
        IsReverseChargeConflict = true;
        SetVatPercent(decimal.Zero);
        return this;
    }


    /// <summary>
    ///*
    /// sets intra community supply(=delivery outside the country inside the EU) </summary>
    /// <returns> fluent setter </returns>
    public virtual Product SetIntraCommunitySupply()
    {
        IsIntraCommunitySupplyConflict = true;
        SetVatPercent(decimal.Zero);
        return this;
    }

    /// <summary>
    ///*
    /// sets a UN/ECE rec 20 or 21 code which unit the product ships in, e.g. C62=piece </summary>
    /// <param name="unit"> 2-3 letter UN/ECE rec 20 or 21 </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetUnit(string unit)
    {
        Unit = unit;
        return this;
    }

    /// <summary>
    /// name of the product </summary>
    /// <param name="name"> short name </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// description of the product (required) </summary>
    /// <param name="description"> long name </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    ///**
    /// VAT rate of the product </summary>
    /// <param name="vatPercent"> vat rate of the product </param>
    /// <returns> fluent setter </returns>
    public virtual Product SetVatPercent(decimal vatPercent)
    {
        VatPercent = vatPercent;
        return this;
    }
}