namespace Ghostscript.NET.FacturX.ZUGFeRD;

public class Profile
{
    protected internal string Name, Id;

    /// <summary>
    ///*
    /// Contruct </summary>
    /// <param name="name"> human readable name of the profile, also used as basis to detemine the XMP Name </param>
    /// <param name="id"> XML Guideline ID </param>
    public Profile(string name, string id)
    {
        Name = name;
        Id = id;
    }

    /// <summary>
    ///*
    /// gets the name </summary>
    /// <returns> the name of the profile </returns>
    public virtual string GetName()
    {
        return Name;
    }

    /// <summary>
    ///*
    /// get guideline id </summary>
    /// <returns> the XML Guideline ID of the profile </returns>
    public virtual string GetId()
    {
        return Id;
    }

    /// <summary>
    ///*
    /// if the profile is embedded in PDF we need RDF metadata </summary>
    /// <returns> the XMP name string of the profile </returns>
    public virtual string GetXmpName()
    {
        if (Name.Equals("BASICWL"))
            return "BASIC WL";
        if (Name.Equals("EN16931"))
            return "EN 16931";
        return Name;
    }
}