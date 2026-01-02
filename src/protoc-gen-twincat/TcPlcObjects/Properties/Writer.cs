using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Properties;

internal class Writer : IPropertyProcessor
{
    private Writer(XmlCDataSection decl, Get? getter)
    {
        Declaration = decl;
        Getter = getter;
    }

    internal static Writer From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var getter = BuildGetter(message, prefixes);
        return new Writer(decl, getter);
    }

    public string Name => Constants.PROPERTY_WRITER;
    public XmlCDataSection Declaration { get; }
    public Get? Getter { get; }
    public Set? Setter => null;


    private static Get BuildGetter(DescriptorProto message, Prefixes prefixes)
    {
        return new Get()
        {
            Name = Constants.PROPERTY_GET,
            Declaration = BuildGetterDeclaration(message, prefixes),
            Implementation = BuildGetterImplementation(message, prefixes)
        };
    }

    private static XmlCDataSection BuildDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        return CData.From($"""
                           (* Writer for this message.*)
                           PROPERTY PUBLIC {Constants.PROPERTY_WRITER} : REFERENCE TO FB_MessageWriter
                           """);
    }

    private static XmlCDataSection BuildGetterDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        return CData.EmptyCData;
    }

    private static Implementation BuildGetterImplementation(DescriptorProto message, Prefixes prefixes)
    {
        return new Implementation()
        {
            ST = CData.From($"{Constants.PROPERTY_WRITER} REF= THIS^._fbMessageWriter;")
        };
    }
}
