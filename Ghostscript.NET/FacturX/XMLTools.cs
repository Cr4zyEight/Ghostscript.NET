using System.Security;

namespace Ghostscript.NET.FacturX;

public class XmlTools
{
    public virtual string EscapeAttributeEntities(string s)
    {
//			return base.escapeAttributeEntities(s);
        return s;
    }

    public virtual string EscapeElementEntities(string s)
    {
//			return base.escapeElementEntities(s);
        return s;
    }

    /// <summary>
    /// Rounds the <paramref name="value"/> first then turn to string with fixed floating points defined by <paramref name="scale"/>
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="scale">The scale.</param>
    /// <returns></returns>
    public static string ScaleDecimal(decimal value, int scale)
    {
        return Math.Round(value, scale, MidpointRounding.AwayFromZero).ToString($"F{scale}");
    }


    public static string EncodeXml(string s)
    {
        return SecurityElement.Escape(s);
    }


    /// <summary>
    /// Returns the Byte Order Mark size and thus allows to skips over a BOM
    /// at the beginning of the given ByteArrayInputStream, if one exists.
    /// </summary>
    /// <param name="is"> the ByteArrayInputStream used </param>
    /// <exception cref="IOException"> if can not be read from is </exception>
    /// <seealso cref="<a href="https://www.w3.org/TR/xml/.sec-guessing">Autodetection of Character Encodings</a>"
    /// 
    /// public static int guessBOMSize(ByteArrayInputStream is) throws IOException {
    /// byte[] pad = new byte[4];
    /// is.read(pad);
    /// is.reset();
    /// int test2 = ((pad[0] & 0xFF) << 8) | (pad[1] & 0xFF);
    /// int test3 = ((test2 & 0xFFFF) << 8) | (pad[2] & 0xFF);
    /// int test4 = ((test3 & 0xFFFFFF) << 8) | (pad[3] & 0xFF);
    /// //
    /// if (test4 == 0x0000FEFF || test4 == 0xFFFE0000 || test4 == 0x0000FFFE || test4 == 0xFEFF0000) {
    ///		// UCS-4: BOM takes 4 bytes
    ///		return 4;
    /// } else if (test3 == 0xEFBBFF) {
    ///		// UTF-8: BOM takes 3 bytes
    ///		return 3;
    /// } else if (test2 == 0xFEFF || test2 == 0xFFFE) {
    ///		// UTF-16: BOM takes 2 bytes
    ///		return 2;
    /// }
    /// return 0;
    /// }/>
    /// <summary>
    ///*
    /// removes utf8 byte order marks from byte arrays, in case one is there </summary>
    /// <param name="zugferdRaw"> the CII XML </param>
    /// <returns> the byte array without bom </returns>
    public static byte[] RemoveBom(byte[] zugferdRaw)
    {
        byte[] zugferdData;
        if (zugferdRaw[0] == 0xEF && zugferdRaw[1] == 0xBB && zugferdRaw[2] == 0xBF)
        {
            // I don't like BOMs, lets remove it
            zugferdData = new byte[zugferdRaw.Length - 3];
            Array.Copy(zugferdRaw, 3, zugferdData, 0, zugferdRaw.Length - 3);
        }
        else
        {
            zugferdData = zugferdRaw;
        }

        return zugferdData;
    }
}