using System;
using System.Collections.Generic;


namespace Ghostscript.NET.FacturX.ZUGFeRD
{


    /// <summary>
    ///*
    /// describes any invoice line
    /// </summary>

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class Item implements org.mustangproject.ZUGFeRD.IZUGFeRDExportableItem
    public class Item : IZugFeRdExportableItem
	{
		protected internal decimal Price, Quantity, Tax, GrossPrice, LineTotalAmount;
		protected internal DateTime? DetailedDeliveryPeriodFrom = null, DetailedDeliveryPeriodTo = null;
		protected internal string Id;
		protected internal string ReferencedLineId = null;
		protected internal Product Product;
		protected internal List<string> Notes = null;
		//protected internal List<ReferencedDocument> referencedDocuments = null;
//		protected internal List<IZUGFeRDAllowanceCharge> Allowances = new List<IZUGFeRDAllowanceCharge>(), Charges = new List<IZUGFeRDAllowanceCharge>();

		/// <summary>
		///*
		/// default constructor </summary>
		/// <param name="product"> contains the products name, tax rate, and unit </param>
		/// <param name="price"> the base price of one item the product </param>
		/// <param name="quantity"> the number, dimensions or the weight of the delivered product or good in this context </param>
		public Item(Product product, decimal price, decimal quantity)
		{
			this.Price = price;
			this.Quantity = quantity;
			this.Product = product;
		}


		/// <summary>
		///*
		/// empty constructor
		/// do not use, but might be used e.g. by jackson
		/// 
		/// </summary>
		public Item()
		{
		}

		public virtual Item AddReferencedLineId(string s)
		{
			ReferencedLineId = s;
			return this;
		}

		/// <summary>
		///*
		/// BT 132 (issue https://github.com/ZUGFeRD/mustangproject/issues/247)
		/// @return
		/// </summary>
		public string GetBuyerOrderReferencedDocumentLineId()
		{
			return ReferencedLineId;
		}

		public virtual decimal GetLineTotalAmount()
		{
			return LineTotalAmount;
		}

		/// <summary>
		/// should only be set by calculator classes or maybe when reading from XML </summary>
		/// <param name="lineTotalAmount"> price*quantity of this line </param>
		/// <returns> fluent setter </returns>
		public virtual Item SetLineTotalAmount(decimal lineTotalAmount)
		{
			this.LineTotalAmount = lineTotalAmount;
			return this;
		}

		public virtual decimal GetGrossPrice()
		{
			return GrossPrice;
		}


		/// <summary>
		///*
		/// the list price without VAT (sic!), refer to EN16931-1 for definition </summary>
		/// <param name="grossPrice"> the list price without VAT </param>
		/// <returns> fluent setter </returns>
		public virtual Item SetGrossPrice(decimal grossPrice)
		{
			this.GrossPrice = grossPrice;
			return this;
		}


		public virtual decimal GetTax()
		{
			return Tax;
		}

		public virtual Item SetTax(decimal tax)
		{
			this.Tax = tax;
			return this;
		}

		public virtual Item SetId(string id)
		{
			this.Id = id;
			return this;
		}

		public virtual string GetId()
		{
			return Id;
		}

		public decimal GetPrice()
		{
			return Price;
		}

		public virtual Item SetPrice(decimal price)
		{
			this.Price = price;
			return this;
		}



		public decimal GetQuantity()
		{
			return Quantity;
		}

        public decimal GetBasisQuantity()
        {
            const int scale = 4;
            return Math.Round(decimal.One, scale, MidpointRounding.AwayFromZero);
        }

        public string GetAdditionalReferencedDocumentId()
        {
            throw null;
        }

        public virtual Item SetQuantity(decimal quantity)
		{
			this.Quantity = quantity;
			return this;
		}

		public IZugFeRdExportableProduct GetProduct()
		{
			return Product;
		}

/*		public override IZUGFeRDAllowanceCharge[] getItemAllowances()
		{
			if (Allowances.Count == 0)
			{
				return null;
			}
			else
			{
				return Allowances.ToArray();
			}
		}

		public override IZUGFeRDAllowanceCharge[] getItemCharges()
		{
			if (Charges.Count == 0)
			{
				return null;
			}
			else
			{
				return Charges.ToArray();
			}
		}

*/

		public string[] GetNotes()
		{
			if (Notes == null)
			{
				return null;
			}
			return Notes.ToArray();
		}

		public virtual Item SetProduct(Product product)
		{
			this.Product = product;
			return this;
		}


		/// <summary>
		///*
		/// Adds a item level addition to the price (will be multiplied by quantity) </summary>
		/// <seealso cref="org.mustangproject.Charge"/>
		/// <param name="izac"> a relative or absolute charge </param>
		/// <returns> fluent setter </returns>
/*		public virtual Item addCharge(IZUGFeRDAllowanceCharge izac)
		{
			Charges.Add(izac);
			return this;
		}

		/// <summary>
		///*
		/// Adds a item level reduction the price (will be multiplied by quantity) </summary>
		/// <seealso cref="org.mustangproject.Allowance"/>
		/// <param name="izac"> a relative or absolute allowance </param>
		/// <returns> fluent setter </returns>
		public virtual Item addAllowance(IZUGFeRDAllowanceCharge izac)
		{
			Allowances.Add(izac);
			return this;
		}
*/
		/// <summary>
		///*
		/// adds item level freetext fields (includednote) </summary>
		/// <param name="text"> UTF8 plain text </param>
		/// <returns> fluent setter </returns>
		public virtual Item AddNote(string text)
		{
			if (Notes == null)
			{
				Notes = new List<string>();
			}
			Notes.Add(text);
			return this;
		}

		/// <summary>
		///*
		/// adds item level Referenced documents along with their typecodes and issuerassignedIDs </summary>
		/// <param name="doc"> the ReferencedDocument to add </param>
		/// <returns> fluent setter </returns>
        /*
		public virtual Item addReferencedDocument(ReferencedDocument doc)
		{
			if (referencedDocuments == null)
			{
				referencedDocuments = new List<ReferencedDocument>();
			}
			referencedDocuments.Add(doc);
			return this;
		}

		public override IReferencedDocument[] getReferencedDocuments()
		{
			if (referencedDocuments == null)
			{
				return null;
			}
			return referencedDocuments.ToArray();
		}*/
		/// <summary>
		///*
		/// specify a item level delivery period
		/// (apart from the document level delivery period, and the document level
		/// delivery day, which is probably anyway required)
		/// </summary>
		/// <param name="from"> start date </param>
		/// <param name="to"> end date </param>
		/// <returns> fluent setter </returns>
		public virtual Item SetDetailedDeliveryPeriod(DateTime from, DateTime to)
		{
			DetailedDeliveryPeriodFrom = from;
			DetailedDeliveryPeriodTo = to;
			return this;
		}

		/// <summary>
		///*
		/// specifies the item level delivery period (there is also one on document level),
		/// this will be included in a BillingSpecifiedPeriod element </summary>
		/// <returns> the beginning of the delivery period </returns>
		public virtual DateTime? GetDetailedDeliveryPeriodFrom()
		{
			return DetailedDeliveryPeriodFrom;
		}

		/// <summary>
		///*
		/// specifies the item level delivery period (there is also one on document level),
		/// this will be included in a BillingSpecifiedPeriod element </summary>
		/// <returns> the end of the delivery period </returns>
		public virtual DateTime? GetDetailedDeliveryPeriodTo()
		{
			return DetailedDeliveryPeriodTo;
		}

	}
}
