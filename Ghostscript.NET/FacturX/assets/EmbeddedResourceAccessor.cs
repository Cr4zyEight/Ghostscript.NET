using System.Reflection;

namespace Ghostscript.NET.FacturX.assets;

public class EmbeddedResourceAccessor
{
    /// <summary>
    /// Accesses the embedded resource files in Ghostscript.NET.FacturX.assets by resource names.<br/>
    /// Writes the file to the temp path and returns the file path
    /// </summary>
    /// <param name="embeddedResourceName">Name of the embedded resource.</param>
    /// <exception cref="System.IO.FileNotFoundException">Embedded resource not found: AdobeRGB1998.icc</exception>
    public static string WriteResourceAndGetPath(string embeddedResourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream($"Ghostscript.NET.FacturX.assets.{embeddedResourceName}");
        if (stream != null)
        {
            byte[] data = new BinaryReader(stream).ReadBytes((int)stream.Length);
            string tempFileName = Path.Combine(Path.GetTempPath(), embeddedResourceName);
            File.WriteAllBytes(tempFileName, data);
            return tempFileName;
        }
        else
        {
            throw new FileNotFoundException($"Embedded resource not found: {embeddedResourceName}");
        }
    }
}