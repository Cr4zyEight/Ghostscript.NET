namespace Ghostscript.NET.FacturX.ZUGFeRD;

public class TransactionCalculator
{
    protected internal IExportableTransaction Trans;

    /// <summary>
    ///*
    /// </summary>
    /// <param name="trans"> the invoice (or IExportableTransaction) to be calculated </param>
    public TransactionCalculator(IExportableTransaction trans)
    {
        Trans = trans;
    }

    /// <summary>
    ///*
    /// if something had already been paid in advance, this will get it from the transaction </summary>
    /// <returns> prepaid amount </returns>
    protected internal virtual decimal GetTotalPrepaid()
    {
        if (Trans.GetTotalPrepaidAmount() == null)
            return decimal.Zero;
        return Math.Round(Trans.GetTotalPrepaidAmount(), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    ///*
    /// the invoice total with VAT, corrected by prepaid amount, allowances and charges </summary>
    /// <returns> the invoice total including taxes </returns>
    public virtual decimal GetGrandTotal()
    {
        //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
        decimal res = GetTaxBasis();
        Dictionary<decimal, VatAmount> vatPercentAmountMap = GetVatPercentAmountMap();
        foreach (decimal currentTaxPercent in vatPercentAmountMap.Keys)
        {
            VatAmount amount = vatPercentAmountMap[currentTaxPercent];
            res += amount.GetCalculated();
        }

        return Math.Round(res, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    ///*
    /// returns total of charges for this tax rate </summary>
    /// <param name="percent"> a specific rate, or null for any rate </param>
    /// <returns> the total amount </returns>

    /*
    protected internal virtual decimal getChargesForPercent(decimal percent)
    {
        IZUGFeRDAllowanceCharge[] charges = trans.getZFCharges();
        return sumAllowanceCharge(percent, charges);
    }

    private decimal sumAllowanceCharge(decimal percent, IZUGFeRDAllowanceCharge[] charges)
    {
        decimal res = decimal.Zero;
        if ((charges != null) && (charges.Length > 0))
        {
            foreach (IZUGFeRDAllowanceCharge currentCharge in charges)
            {
                if ((percent == null) || (currentCharge.getTaxPercent().compareTo(percent) == 0))
                {
                    res = res + currentCharge.getTotalAmount(this);
                }
            }
        }
        return res;
    }

    /// <summary>
    ///*
    /// returns a (potentially concatenated) string of charge reasons, or "Charges" if none are defined </summary>
    /// <param name="percent"> a specific rate, or null for any rate </param>
    /// <returns> the space separated string </returns>
    protected internal virtual string getChargeReasonForPercent(decimal percent)
    {
        IZUGFeRDAllowanceCharge[] charges = trans.getZFCharges();
        string res = getAllowanceChargeReasonForPercent(percent, charges);
        if ("".Equals(res))
        {
            res = "Charges";
        }
        return res;
    }

    private string getAllowanceChargeReasonForPercent(decimal percent, IZUGFeRDAllowanceCharge[] charges)
    {
        string res = " ";
        if ((charges != null) && (charges.Length > 0))
        {
            foreach (IZUGFeRDAllowanceCharge currentCharge in charges)
            {
                if ((percent == null) || (currentCharge.getTaxPercent().compareTo(percent) == 0) && currentCharge.getReason() != null)
                {
                    res += currentCharge.getReason() + " ";
                }
            }
        }
        res = res.Substring(0, res.Length - 1);
        return res;
    }

    /// <summary>
    ///*
    /// returns a (potentially concatenated) string of allowance reasons, or "Allowances", if none are defined </summary>
    /// <param name="percent"> a specific rate, or null for any rate </param>
    /// <returns> the space separated string </returns>
    protected internal virtual string getAllowanceReasonForPercent(decimal percent)
    {
        IZUGFeRDAllowanceCharge[] allowances = trans.getZFAllowances();
        string res = getAllowanceChargeReasonForPercent(percent, allowances);
        if ("".Equals(res))
        {
            res = "Allowances";
        }
        return res;
    }


    /// <summary>
    ///*
    /// returns total of allowances for this tax rate </summary>
    /// <param name="percent"> a specific rate, or null for any rate </param>
    /// <returns> the total amount </returns>
    protected internal virtual decimal getAllowancesForPercent(decimal percent)
    {
        IZUGFeRDAllowanceCharge[] allowances = trans.getZFAllowances();
        return sumAllowanceCharge(percent, allowances);
    }
    */

    /// <summary>
    ///*
    /// returns the total net value of all items, without document level charges/allowances </summary>
    /// <returns> item sum </returns>
    protected internal virtual decimal GetTotal()
    {
        //JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:

        decimal res = decimal.Zero;
        foreach (IZUGFeRDExportableItem currentItem in Trans.GetZfItems())
        {
            LineCalculator lc = new(currentItem);
            res += lc.GetItemTotalGrossAmount();
        }

        return res;
    }

    /// <summary>
    ///*
    /// returns the total net value of the invoice, including charges/allowances on document
    /// level </summary>
    /// <returns> item sum +- charges/allowances </returns>
    protected internal virtual decimal GetTaxBasis()
    {
        return GetTotal() /*+ (getChargesForPercent(null).setScale(2, RoundingMode.HALF_UP)).subtract(getAllowancesForPercent(null).setScale(2, RoundingMode.HALF_UP)).setScale(2, RoundingMode.HALF_UP)*/;
    }

    /// <summary>
    /// which taxes have been used with which amounts in this transaction, empty for
    /// no taxes, or e.g. 19:190 and 7:14 if 1000 Eur were applicable to 19% VAT
    /// (=190 EUR VAT) and 200 EUR were applicable to 7% (=14 EUR VAT) 190 Eur
    /// </summary>
    /// <returns> which taxes have been used with which amounts in this invoice </returns>
    protected internal virtual Dictionary<decimal, VatAmount> GetVatPercentAmountMap()
    {
        Dictionary<decimal, VatAmount> hm = new();

        foreach (IZUGFeRDExportableItem currentItem in Trans.GetZfItems())
        {
            decimal percent = currentItem.GetProduct().GetVatPercent();

            LineCalculator lc = new(currentItem);
            VatAmount itemVatAmount = new(lc.GetItemTotalNetAmount(), lc.GetItemTotalVatAmount(), currentItem.GetProduct().GetTaxCategoryCode());

            decimal test1 = 1.45m;
            decimal test2 = 1.45000000m;


            if (!hm.ContainsKey(percent))
            {
                hm[percent] = itemVatAmount;
            }
            else
            {
                VatAmount current = hm[percent];
                hm[percent] = current.Add(itemVatAmount);
            }
        }

        /*
                IZUGFeRDAllowanceCharge[] charges = trans.getZFCharges();
                if ((charges != null) && (charges.Length > 0))
                {
                    foreach (IZUGFeRDAllowanceCharge currentCharge in charges)
                    {
                        VATAmount theAmount = hm[currentCharge.getTaxPercent().stripTrailingZeros()];
                        if (theAmount == null)
                        {
                            theAmount = new VATAmount(decimal.Zero, decimal.Zero, currentCharge.getCategoryCode() != null ? currentCharge.getCategoryCode() : "S");
                        }
                        theAmount.setBasis(theAmount.getBasis().add(currentCharge.getTotalAmount(this)));
                        decimal factor = currentCharge.getTaxPercent().divide(new decimal(100));
                        theAmount.setCalculated(theAmount.getBasis().multiply(factor));
                        hm[currentCharge.getTaxPercent().stripTrailingZeros()] = theAmount;
                    }
                }
                IZUGFeRDAllowanceCharge[] allowances = trans.getZFAllowances();
                if ((allowances != null) && (allowances.Length > 0))
                {
                    foreach (IZUGFeRDAllowanceCharge currentAllowance in allowances)
                    {
                        VATAmount theAmount = hm[currentAllowance.getTaxPercent().stripTrailingZeros()];
                        if (theAmount == null)
                        {
                            theAmount = new VATAmount(decimal.Zero, decimal.Zero, currentAllowance.getCategoryCode() != null ? currentAllowance.getCategoryCode() : "S");
                        }
                        theAmount.setBasis(theAmount.getBasis().subtract(currentAllowance.getTotalAmount(this)));
                        decimal factor = currentAllowance.getTaxPercent().divide(new decimal(100));
                        theAmount.setCalculated(theAmount.getBasis().multiply(factor));

                        hm[currentAllowance.getTaxPercent().stripTrailingZeros()] = theAmount;
                    }
                }*/

        return hm;
    }


    public decimal GetValue()
    {
        return GetTotal();
    }
}