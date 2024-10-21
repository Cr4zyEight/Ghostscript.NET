using System;


namespace Ghostscript.NET.FacturX.ZUGFeRD
{

	public class VatAmount
	{

		public VatAmount(decimal basis, decimal calculated, string categoryCode) : base()
		{
			this.Basis = basis;
			this.Calculated = calculated;
			this.CategoryCode = categoryCode;
		}

		internal decimal Basis, Calculated, ApplicablePercent;

		internal string CategoryCode;

		public virtual decimal GetApplicablePercent()
		{

			return ApplicablePercent;
		}
		public virtual VatAmount SetApplicablePercent(decimal value)
		{
			this.ApplicablePercent = value;
			return this;
		}


		public virtual decimal GetBasis()
		{

			return Basis;
		}
		public virtual VatAmount SetBasis(decimal value)
		{
			this.Basis = Math.Round(value, 2, MidpointRounding.AwayFromZero);
			return this;

		}





		public virtual decimal GetCalculated() {
			return Calculated;
		}


		public virtual VatAmount SetCalculated(decimal value) {

			this.Calculated = value;
			return this;
		}


		/// 
		/// @deprecated Use <seealso cref="GetCategoryCode"/> 
		/// <returns> string with category code </returns>
		[Obsolete("Use <seealso cref=\"getCategoryCode() instead\"/>")]
		public virtual string GetDocumentCode()
		{

			return CategoryCode;
		}
		public virtual VatAmount SetDocumentCode(string value)

		{
			this.CategoryCode = value;
			return this;
		}



		public virtual string GetCategoryCode()
		{

			return CategoryCode;
		}
		public virtual VatAmount SetCategoryCode(string value)
		{
			this.CategoryCode = value;
			return this;
		}



		public virtual VatAmount Add(VatAmount v)
		{
			return new VatAmount(Basis + v.GetBasis(), Calculated + v.GetCalculated(), this.CategoryCode);
		}

		public virtual VatAmount Subtract(VatAmount v)
		{
			return new VatAmount(Basis - v.GetBasis(), Calculated - v.GetCalculated(), this.CategoryCode);
		}

	}
}