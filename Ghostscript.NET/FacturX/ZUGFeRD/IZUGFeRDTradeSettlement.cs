namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	public interface IZUGFeRDTradeSettlement
	{

		/// <summary>
		///*
		/// </summary>
		/// <returns> zf2 xml for applicableHeaderTradeSettlement </returns>
		string GetSettlementXml();


        /// <summary>
        ///*
        /// </summary>
        /// <returns> zf2 xml for applicableHeaderTradePayment </returns>
        string GetPaymentXml();
    }
}
