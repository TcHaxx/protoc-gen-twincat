using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TcHaxx.ProtocGenTc;

public static class PlcObjectSerializer
{
    public static string Serialize<T>(T plcObject)
    {
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            OmitXmlDeclaration = false
        };
        using var memoryStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(memoryStream, settings);
        serializer.Serialize(xmlWriter, plcObject);
        var utf8String = Encoding.UTF8.GetString(memoryStream.ToArray());
        return utf8String;
    }
}
