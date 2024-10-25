using System.Xml.Linq;

namespace Ghostscript.NET.FacturX.ZUGFeRD;

public interface IZUGFeRDTradeSettlement
{
    /// <summary>
    ///*
    /// </summary>
    /// <returns> zf2 xml for applicableHeaderTradeSettlement </returns>
    XElement GetSettlementXml();


    /// <summary>
    ///*
    /// </summary>
    /// <returns> zf2 xml for applicableHeaderTradePayment </returns>
    XElement GetPaymentXml();
}