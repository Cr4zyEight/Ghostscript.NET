using System;

namespace Ghostscript.NET.FacturX.ZUGFeRD
{


    public interface IZugFeRdPaymentTerms
    {

        string GetDescription();

        DateTime GetDueDate();

        //		IZUGFeRDPaymentDiscountTerms getDiscountTerms();
    }
}