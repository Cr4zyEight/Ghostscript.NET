using System.Text;

namespace Ghostscript.NET.Helpers;

public class Utf8StringWriter(StringBuilder sb) : StringWriter(sb)
{
    public override Encoding Encoding => Encoding.UTF8;
}