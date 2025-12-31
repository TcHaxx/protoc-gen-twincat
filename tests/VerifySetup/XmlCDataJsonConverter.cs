using System.Xml;
using Argon;

namespace TcHaxx.ProtocGenTcTests.VerifySetup;

public class XmlCDataJsonConverter : Argon.JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is XmlCDataSection cdata)
        {
            writer.WriteValue($"<![CDATA[{cdata.Data}]]>");
            return;
        }

        if (value is XmlNode node)
        {
            var doc = new XmlDocument();
            var imported = doc.ImportNode(node, true);
            doc.AppendChild(imported);
            writer.WriteRawValue(doc.OuterXml);
            return;
        }

        writer.WriteNull();
    }

    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override bool CanConvert(Type type)
    {
        return typeof(XmlCDataSection).IsAssignableFrom(type) ||
        typeof(XmlNode).IsAssignableFrom(type);
    }
}

