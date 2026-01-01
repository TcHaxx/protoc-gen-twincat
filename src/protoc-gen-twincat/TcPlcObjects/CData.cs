using System.Xml;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

public static class CData
{
    public static XmlCDataSection EmptyCData => new XmlDocument().CreateCDataSection(string.Empty);

    public static XmlCDataSection From(string data)
    {
        return new XmlDocument().CreateCDataSection(data);
    }
}
