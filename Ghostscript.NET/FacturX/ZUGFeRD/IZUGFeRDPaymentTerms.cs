namespace Ghostscript.NET.FacturX.ZUGFeRD;

public interface IZUGFeRDPaymentTerms
{
    string GetDescription();

    DateTime GetDueDate();

    //		IZUGFeRDPaymentDiscountTerms getDiscountTerms();
}