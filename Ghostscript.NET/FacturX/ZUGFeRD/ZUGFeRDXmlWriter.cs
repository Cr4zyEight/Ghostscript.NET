using System.Xml.Linq;

namespace Ghostscript.NET.FacturX.ZUGFeRD;

public abstract class ZugFeRdXmlWriter
{
    protected ZUGFeRDXmlNamespace XsiNamespace = new ("xsi", "http://www.w3.org/2001/XMLSchema-instance");
    protected ZUGFeRDXmlNamespace RsmNamespace = new ("rsm", "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100");
    protected ZUGFeRDXmlNamespace RamNamespace = new ("ram", "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100");
    protected ZUGFeRDXmlNamespace UdtNamespace = new ("udt", "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100");
}