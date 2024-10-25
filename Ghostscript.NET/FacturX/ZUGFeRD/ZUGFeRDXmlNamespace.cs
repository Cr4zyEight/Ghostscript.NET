namespace Ghostscript.NET.FacturX.ZUGFeRD;

public class ZUGFeRDXmlNamespace(string prefix, string ns)
{
    protected internal string Prefix { get; set; } = prefix;
    protected internal string Namespace { get; set; } = ns;
}