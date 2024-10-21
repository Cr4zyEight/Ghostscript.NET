using System.Xml;
using iText.StyledXmlParser.Jsoup.Nodes;

namespace Ghostscript.NET.FacturX.ZUGFeRD
{
	/// <summary>
	///*
	/// a named contact person in an organisation
	/// for the organisation/company itsel please </summary>
	/// <seealso cref="TradeParty"/>
	//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
	//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class Contact implements org.mustangproject.ZUGFeRD.IZUGFeRDExportableContact
	public class Contact : IZugFeRdExportableContact
	{

		protected internal string Name, Phone, Email, Zip, Street, Location, Country;
		protected internal string Fax = null;

		/// <summary>
		///*
		/// default constructor.
		/// Name, phone and email of sender contact person are e.g. required by XRechnung </summary>
		/// <param name="name"> full name of the contact </param>
		/// <param name="phone"> full phone number </param>
		/// <param name="email"> email address of the contact </param>
		public Contact(string name, string phone, string email)
		{
			this.Name = name;
			this.Phone = phone;
			this.Email = email;
		}

		/// <summary>
		///*
		/// empty constructor.
		/// as always, not recommended, for jackson...
		/// </summary>
		public Contact()
		{
		}

		/// <summary>
		///*
		/// complete specification of a named contact with a different address </summary>
		/// <param name="name"> full name </param>
		/// <param name="phone"> full phone number </param>
		/// <param name="email"> full email </param>
		/// <param name="street"> street+number </param>
		/// <param name="zip"> postcode </param>
		/// <param name="location"> city </param>
		/// <param name="country"> two-letter iso code </param>
		public Contact(string name, string phone, string email, string street, string zip, string location, string country)
		{
			this.Name = name;
			this.Phone = phone;
			this.Email = email;
			this.Street = street;
			this.Zip = zip;
			this.Location = location;
			this.Country = country;

		}

		/// <summary>
		///*
		/// XML parsing constructor </summary>
		/// <param name="nodes"> the nodelist returned e.g. from xpath </param>
		public Contact(XmlNodeList nodes)
		{
			if (nodes.Count > 0)
			{
                /*
			   will parse sth like
				<ram:DefinedTradeContact>
					<ram:PersonName>Name</ram:PersonName>
					<ram:TelephoneUniversalCommunication>
						<ram:CompleteNumber>069 100-0</ram:CompleteNumber>
					</ram:TelephoneUniversalCommunication>
					<ram:EmailURIUniversalCommunication>
						<ram:URIID>test@example.com</ram:URIID>
					</ram:EmailURIUniversalCommunication>
				</ram:DefinedTradeContact>
				 */
                for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
                {
                    XmlNode currentItemNode = nodes[nodeIndex];
                    if (!string.IsNullOrEmpty(currentItemNode.LocalName))
                    {
                        if (currentItemNode.LocalName.Equals("PersonName"))
                        {
                            Name = currentItemNode.FirstChild.Value;
                        }
                        if (currentItemNode.LocalName.Equals("TelephoneUniversalCommunication"))
                        {
                            XmlNodeList telNodeChildren = currentItemNode.ChildNodes;
                            for (int telChildIndex = 0; telChildIndex < telNodeChildren.Count; telChildIndex++)
                            {
                                if (!string.IsNullOrEmpty(telNodeChildren[telChildIndex].LocalName) &&
                                    telNodeChildren[telChildIndex].LocalName.Equals("CompleteNumber"))
                                {
                                    Phone = telNodeChildren[telChildIndex].InnerText;
                                }
                            }
                        }
                        if (currentItemNode.LocalName.Equals("EmailURIUniversalCommunication"))
                        {
                            XmlNodeList emailNodeChildren = currentItemNode.ChildNodes;
                            for (int emailChildIndex = 0; emailChildIndex < emailNodeChildren.Count; emailChildIndex++)
                            {
                                if (!string.IsNullOrEmpty(emailNodeChildren[emailChildIndex].LocalName) &&
                                    emailNodeChildren[emailChildIndex].LocalName.Equals("URIID"))
                                {
                                    Email = emailNodeChildren[emailChildIndex].InnerText;
                                }
                            }
                        }
                    }
                }
            }
        }


        public string GetName()
		{
			return Name;
		}

		/// <summary>
		/// the first and last name of the contact
		/// </summary>
		/// <param name="name"> first and last name </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetName(string name)
		{
			this.Name = name;
			return this;
		}

		public string GetPhone()
		{
			return Phone;
		}

		/// <summary>
		///*
		/// complete phone number of the contact </summary>
		/// <param name="phone"> the complete phone number </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetPhone(string phone)
		{
			this.Phone = phone;
			return this;
		}

		public string GetFax()
		{
			return Fax;
		}

		/// <summary>
		///*
		/// (optional) complete fax number </summary>
		/// <param name="fax"> complete fax number of the contact </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetFax(string fax)
		{
			this.Fax = fax;
			return this;
		}

		public virtual string GetEMail()
		{
			return Email;
		}

		/// <summary>
		///*
		/// personal email address of the contact person </summary>
		/// <param name="email"> the email address of the contact </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetEMail(string email)
		{
			this.Email = email;
			return this;
		}

		public virtual string GetZip()
		{
			return Zip;
		}

		/// <summary>
		///*
		/// the postcode, if the address is different to the organisation </summary>
		/// <param name="zip"> the postcode of the contact </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetZip(string zip)
		{
			this.Zip = zip;
			return this;
		}

		public string GetStreet()
		{
			return Street;
		}

		/// <summary>
		/// street and number, if the address is different to the organisation
		/// </summary>
		/// <param name="street"> street and number of the contact </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetStreet(string street)
		{
			this.Street = street;
			return this;
		}
		public string GetLocation()
		{
			return Location;
		}

		/// <summary>
		///*
		/// city of the contact person, if different from organisation </summary>
		/// <param name="location"> city </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetLocation(string location)
		{
			this.Location = location;
			return this;
		}

		public string GetCountry()
		{
			return Country;
		}

		/// <summary>
		///*
		/// two-letter ISO country code of the contact, if different from organisation </summary>
		/// <param name="country"> two-letter iso code </param>
		/// <returns> fluent setter </returns>
		public virtual Contact SetCountry(string country)
		{
			this.Country = country;
			return this;
		}

	}
}