using System;
using System.Collections.Generic;
namespace Ghostscript.NET.FacturX.ZUGFeRD
{

	public class Profiles
	{
		private static readonly Dictionary<string, Profile> Zf2Map = new Dictionary<string, Profile>
	{
		{"MINIMUM", new Profile("MINIMUM", "urn:factur-x.eu:1p0:minimum")},
		 {"BASICWL", new Profile("BASICWL", "urn:factur-x.eu:1p0:basicwl")},
		 {"BASIC", new Profile("BASIC", "urn:cen.eu:en16931:2017#compliant#urn:factur-x.eu:1p0:basic")},
		 {"EN16931", new Profile("EN16931", "urn:cen.eu:en16931:2017")},
		 {"EXTENDED", new Profile("EXTENDED", "urn:cen.eu:en16931:2017#conformant#urn:factur-x.eu:1p0:extended")},
		 {"XRECHNUNG", new Profile("XRECHNUNG", "urn:cen.eu:en16931:2017#compliant#urn:xoev-de:kosit:standard:xrechnung_2.1")}
	};

		private static readonly Dictionary<string, Profile> Zf1Map = new Dictionary<string, Profile>
	{

		{"BASIC", new Profile("BASIC", "urn:ferd:CrossIndustryDocument:invoice:1p0:basic")},
		{"COMFORT", new Profile("COMFORT", "urn:ferd:CrossIndustryDocument:invoice:1p0:comfort")},
		{"EXTENDED", new Profile("EXTENDED", "urn:ferd:CrossIndustryDocument:invoice:1p0:extended")}
	};


		public static Profile GetByName(string name, int version)
		{
			Profile result = null;
			if (version == 1)
			{
				result = Zf1Map[name.ToUpper()];
			}
			else
			{
				result = Zf2Map[name.ToUpper()];
			}
			if (result == null)
			{
				throw new Exception("Profile not found");
			}
			return result;
		}
		public static Profile GetByName(string name)
		{
			return GetByName(name, 2);
		}

	}
}