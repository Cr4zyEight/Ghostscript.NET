using System.Xml;

namespace Ghostscript.NET.FacturX.ZUGFeRD;

/// <summary>
///*
/// a named contact person in an organisation
/// for the organisation/company itsel please </summary>
/// <seealso cref="TradeParty"/>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonIgnoreProperties(ignoreUnknown = true) public class Contact implements org.mustangproject.ZUGFeRD.IZUGFeRDExportableContact
public class Contact : IZUGFeRDExportableContact
{
    protected internal string Fax;

    protected internal string Name, Phone, Email, Zip, Street, Location, Country;

    /// <summary>
    ///*
    /// default constructor.
    /// Name, phone and email of sender contact person are e.g. required by XRechnung </summary>
    /// <param name="name"> full name of the contact </param>
    /// <param name="phone"> full phone number </param>
    /// <param name="email"> email address of the contact </param>
    public Contact(string name, string phone, string email)
    {
        Name = name;
        Phone = phone;
        Email = email;
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
        Name = name;
        Phone = phone;
        Email = email;
        Street = street;
        Zip = zip;
        Location = location;
        Country = country;
    }

    /// <summary>
    ///*
    /// XML parsing constructor </summary>
    /// <param name="nodes"> the nodelist returned e.g. from xpath </param>
    public Contact(XmlNodeList nodes)
    {
        if (nodes.Count > 0)
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
                    if (currentItemNode.LocalName.Equals("PersonName")) Name = currentItemNode.FirstChild.Value;
                    if (currentItemNode.LocalName.Equals("TelephoneUniversalCommunication"))
                    {
                        XmlNodeList telNodeChildren = currentItemNode.ChildNodes;
                        for (int telChildIndex = 0; telChildIndex < telNodeChildren.Count; telChildIndex++)
                            if (!string.IsNullOrEmpty(telNodeChildren[telChildIndex].LocalName) &&
                                telNodeChildren[telChildIndex].LocalName.Equals("CompleteNumber"))
                                Phone = telNodeChildren[telChildIndex].InnerText;
                    }

                    if (currentItemNode.LocalName.Equals("EmailURIUniversalCommunication"))
                    {
                        XmlNodeList emailNodeChildren = currentItemNode.ChildNodes;
                        for (int emailChildIndex = 0; emailChildIndex < emailNodeChildren.Count; emailChildIndex++)
                            if (!string.IsNullOrEmpty(emailNodeChildren[emailChildIndex].LocalName) &&
                                emailNodeChildren[emailChildIndex].LocalName.Equals("URIID"))
                                Email = emailNodeChildren[emailChildIndex].InnerText;
                    }
                }
            }
    }


    public string GetId()
    {
        return null;
    }

    public string GetName()
    {
        return Name;
    }

    public string GetPhone()
    {
        return Phone;
    }

    public string GetFax()
    {
        return Fax;
    }

    public virtual string GetEMail()
    {
        return Email;
    }

    public virtual string GetZip()
    {
        return Zip;
    }

    public string GetVatid()
    {
        return null;
    }

    public string GetStreet()
    {
        return Street;
    }

    public string GetAdditionalAddress()
    {
        return null;
    }

    public string GetLocation()
    {
        return Location;
    }

    public string GetCountry()
    {
        return Country;
    }

    /// <summary>
    /// the first and last name of the contact
    /// </summary>
    /// <param name="name"> first and last name </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    ///*
    /// complete phone number of the contact </summary>
    /// <param name="phone"> the complete phone number </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetPhone(string phone)
    {
        Phone = phone;
        return this;
    }

    /// <summary>
    ///*
    /// (optional) complete fax number </summary>
    /// <param name="fax"> complete fax number of the contact </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetFax(string fax)
    {
        Fax = fax;
        return this;
    }

    /// <summary>
    ///*
    /// personal email address of the contact person </summary>
    /// <param name="email"> the email address of the contact </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetEMail(string email)
    {
        Email = email;
        return this;
    }

    /// <summary>
    ///*
    /// the postcode, if the address is different to the organisation </summary>
    /// <param name="zip"> the postcode of the contact </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetZip(string zip)
    {
        Zip = zip;
        return this;
    }

    /// <summary>
    /// street and number, if the address is different to the organisation
    /// </summary>
    /// <param name="street"> street and number of the contact </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetStreet(string street)
    {
        Street = street;
        return this;
    }

    /// <summary>
    ///*
    /// city of the contact person, if different from organisation </summary>
    /// <param name="location"> city </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetLocation(string location)
    {
        Location = location;
        return this;
    }

    /// <summary>
    ///*
    /// two-letter ISO country code of the contact, if different from organisation </summary>
    /// <param name="country"> two-letter iso code </param>
    /// <returns> fluent setter </returns>
    public virtual Contact SetCountry(string country)
    {
        Country = country;
        return this;
    }
}