using System;

namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	/// <summary>
	/// provides e.g. the IBAN to transfer money to :-)
	/// </summary>
	public class BankDetails : IZUGFeRDTradeSettlementPayment
	{
		protected internal string Iban, Bic, AccountName = null;

		public BankDetails(string iban, string bic)
		{
			this.Iban = iban;
			this.Bic = bic;
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
			this.Iban = iban;
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
			this.Bic = bic;
			return this;
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


		/// <summary>
		/// set Holder </summary>
		/// <param name="name"> account name (usually account holder if != sender) </param>
		/// <returns> fluent setter </returns>
		public virtual BankDetails SetAccountName(string name)
		{
			AccountName = name;
			return this;
		}

		public string GetAccountName()
		{
			return AccountName;
		}
		public string GetSettlementXml()
		{
			string accountNameStr = "";
			if (GetAccountName() != null)
			{
				accountNameStr = "<ram:AccountName>" + XmlTools.EncodeXml(GetAccountName()) + "</ram:AccountName>\n";

			}

			string xml = "			<ram:SpecifiedTradeSettlementPaymentMeans>\n" + "				<ram:TypeCode>58</ram:TypeCode>\n" + "				<ram:Information>SEPA credit transfer</ram:Information>\n" + "				<ram:PayeePartyCreditorFinancialAccount>\n" + "					<ram:IBANID>" + XmlTools.EncodeXml(GetOwnIban()) + "</ram:IBANID>\n";
			xml += accountNameStr;
			xml += "				</ram:PayeePartyCreditorFinancialAccount>\n" + "				<ram:PayeeSpecifiedCreditorFinancialInstitution>\n" + "					<ram:BICID>" + XmlTools.EncodeXml(GetOwnBic()) + "</ram:BICID>\n" + "				</ram:PayeeSpecifiedCreditorFinancialInstitution>\n" + "			</ram:SpecifiedTradeSettlementPaymentMeans>\n";
			return xml;
		}

        public string GetPaymentXml()
        {
            return null;
        }
    }
}