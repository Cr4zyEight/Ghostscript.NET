using System.Text;

namespace Ghostscript.NET.FacturX.ZUGFeRD;

public class ZUGFeRDExporter
{
    protected string GsDll;
    protected bool NoSourceCopy;
    protected string Profile = "EN16931";
    protected string SourcePdf;
    protected IExportableTransaction? Trans;
    protected int Version = 2;

    public ZUGFeRDExporter(string gsDll)
    {
        GsDll = gsDll;
    }

    public ZUGFeRDExporter Load(string pdFfilename)
    {
        string basename = Path.GetFileName(pdFfilename);
        SourcePdf = Path.GetTempPath() + basename;
        string d1 = Path.GetDirectoryName(pdFfilename) + Path.DirectorySeparatorChar;
        string d2 = Path.GetTempPath();
        if (d1.Equals(Path.GetTempPath()))
            NoSourceCopy = true;
        else
            File.Copy(pdFfilename, SourcePdf);
        return this;
    }

    public ZUGFeRDExporter SetTransaction(IExportableTransaction trans)
    {
        Trans = trans;
        return this;
    }

    public ZUGFeRDExporter SetZUGFeRDVersion(int version)
    {
        Version = version;
        return this;
    }

    public ZUGFeRDExporter SetProfile(string profile)
    {
        Profile = profile;
        return this;
    }

    public void Export(string targetFilename)
    {
        using PdfConverter pc = new(SourcePdf, targetFilename);

        ZUGFeRD2PullProvider zf2P = new();
        zf2P.SetProfile(Profiles.GetByName(Profile));
        zf2P.GenerateXml(Trans);
        UTF8Encoding encoding = new();

        string tempfilename = Path.GetTempPath() + "\\factur-x.xml";
        File.WriteAllBytes(tempfilename, zf2P.GetXml());

        pc.EmbedXmlForZf(tempfilename, Convert.ToString(Version));
        pc.ConvertToPdfa3(GsDll);
        File.Delete(Path.GetTempPath() + "\\factur-x.xml");
        if (!NoSourceCopy) File.Delete(SourcePdf);
    }
}