namespace Ghostscript.NET.FacturX.ZUGFeRD;

public class LineCalculator
{
    private decimal _allowance = decimal.Zero;
    private decimal _charge = decimal.Zero;
    private readonly decimal _itemTotalNetAmount;
    private readonly decimal _itemTotalVatAmount;
    private readonly decimal _price;
    private readonly decimal _priceGross;

    //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
    //ORIGINAL LINE: public LineCalculator(ZUGFeRDExportableItem currentItem)
    public LineCalculator(IZUGFeRDExportableItem currentItem)
    {
        /*
                if (currentItem.getItemAllowances() != null && currentItem.getItemAllowances().length > 0)
                {
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: foreach(IZUGFeRDAllowanceCharge allowance @in currentItem.getItemAllowances())

                    {
                        addAllowance(allowance.getTotalAmount(currentItem));
                    }
                }
                if (currentItem.getItemCharges() != null && currentItem.getItemCharges().length > 0)
                {
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: foreach(IZUGFeRDAllowanceCharge charge @in currentItem.getItemCharges())

                    {
                        addCharge(charge.getTotalAmount(currentItem));
                    }
                }
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @decimal multiplicator = currentItem.getProduct().getVATPercent().divide(100);
        */
        decimal multiplicator = currentItem.GetProduct().GetVatPercent() / 100;
        _priceGross = currentItem.GetPrice(); // see https://github.com/ZUGFeRD/mustangproject/issues/159
        _price = _priceGross - _allowance + _charge;
        _itemTotalNetAmount = Math.Round(currentItem.GetQuantity() * _price / currentItem.GetBasisQuantity(), 2, MidpointRounding.AwayFromZero);
        _itemTotalVatAmount = _itemTotalNetAmount * multiplicator;
    }


    public virtual decimal GetPrice()
    {
        return _price;
    }

    public virtual decimal GetItemTotalNetAmount()
    {
        return _itemTotalNetAmount;
    }

    public virtual decimal GetItemTotalVatAmount()
    {
        return _itemTotalVatAmount;
    }

    public virtual decimal GetItemTotalGrossAmount()
    {
        return _itemTotalNetAmount;
    }

    public virtual decimal GetPriceGross()
    {
        return _priceGross;
    }

    public virtual void AddAllowance(decimal b)
    {
        _allowance += b;
    }

    public virtual void AddCharge(decimal b)
    {
        _charge += b;
    }
}