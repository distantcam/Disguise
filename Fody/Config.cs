using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

public class Config
{
    private readonly XElement xml;

    public Config()
    {
        xml = new XElement("Disguise");
    }

    public Config(XElement config)
    {
        xml = config != null ? config : new XElement("Disguise");
    }

    public bool SupressILdasm
    {
        get { return GetBool("SupressILdasm"); }
        set { SetBool("SupressILdasm", value); }
    }

    public XElement ToXml { get { return xml; } }

    private bool GetBool(XName name, bool dfault = true)
    {
        var attribute = xml.Attribute(name);

        if (attribute == null)
            return dfault;

        return XmlConvert.ToBoolean(attribute.Value);
    }

    private void SetBool(XName name, bool value, bool dfault = true)
    {
        var attribute = xml.Attribute(name);

        if (attribute == null)
        {
            if (value != dfault)
                xml.Add(new XAttribute(name, value));
        }
        else
        {
            if (value == dfault)
                attribute.Remove();
            else
                attribute.SetValue(value);
        }
    }
}