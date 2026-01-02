using System.Xml;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Methods;

public interface IMethodProcessor
{
    string Name { get; }
    XmlCDataSection Declaration { get; }
    Implementation Implementation { get; }
}
