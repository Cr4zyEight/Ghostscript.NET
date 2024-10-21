using System;
using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;
using System.Xml;
using Microsoft.Extensions.Logging;
namespace Ghostscript.NET.FacturX.ZUGFeRD
{

    public class ZugFeRdExporter
    {
        protected string GsDll = null;
        protected string SourcePdf = null;
        protected bool NoSourceCopy = false;
        protected string Profile = "EN16931";
        protected int Version = 2;
        protected IExportableTransaction? Trans = null;

        public ZugFeRdExporter(string gsDll)
        {
            this.GsDll = gsDll;
        }
        public ZugFeRdExporter Load(string pdFfilename)
        {
            string basename=Path.GetFileName(pdFfilename);
            this.SourcePdf = Path.GetTempPath() + basename;
            string d1=Path.GetDirectoryName(pdFfilename)+Path.DirectorySeparatorChar;
            string d2=Path.GetTempPath();
            if (d1.Equals(Path.GetTempPath())) {
                NoSourceCopy=true;
            } else {
                File.Copy(pdFfilename, SourcePdf);
            }
            return this;
        }
        public ZugFeRdExporter SetTransaction(IExportableTransaction trans)
        {
            this.Trans = trans;
            return this;
        }

        public ZugFeRdExporter SetZugFeRdVersion(int version)
        {
            this.Version = version;
            return this;
        }

        public ZugFeRdExporter SetProfile(string profile)
        {
            this.Profile = profile;
            return this;
        }


        public void Export(string targetFilename)
        {

            PdfConverter pc = new PdfConverter(SourcePdf, targetFilename);

            ZugFeRd2PullProvider zf2P = new ZugFeRd2PullProvider();
            zf2P.SetProfile(Profiles.GetByName(Profile));
            zf2P.GenerateXml(Trans);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            string tempfilename = Path.GetTempPath() + "\\factur-x.xml";
            File.WriteAllBytes(tempfilename, zf2P.GetXml());

            pc.EmbedXmlForZf(tempfilename, Convert.ToString(Version));
            pc.ConvertToPdfa3(GsDll);
            File.Delete(Path.GetTempPath() + "\\factur-x.xml");
            if (!NoSourceCopy) {
                File.Delete(SourcePdf);
            }

        }
    }
}
