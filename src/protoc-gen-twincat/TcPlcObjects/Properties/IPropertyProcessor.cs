using System.Xml;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Properties;

public interface IPropertyProcessor
{
    string Name { get; }
    XmlCDataSection Declaration { get; }
    Get? Getter { get; }
    Set? Setter { get; }
}
