using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Properties;

internal class MessageValue : IPropertyProcessor
{
    private MessageValue(XmlCDataSection decl, Get? getter)
    {
        Declaration = decl;
        Getter = getter;
    }

    internal static MessageValue From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var getter = BuildGetter(message, prefixes);
        return new MessageValue(decl, getter);
    }

    public string Name => Constants.PROPERTY_NAME_MESSAGE_VALUE;
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
                           PROPERTY PUBLIC {Constants.PROPERTY_NAME_MESSAGE_VALUE} : T_Any
                           """);
    }

    private static XmlCDataSection BuildGetterDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        return CData.EmptyCData;
    }

    private static Implementation BuildGetterImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var nameInst = prefixes.GetStNameWithInstancePrefix(message);
        return new Implementation()
        {
            ST = CData.From($"{Constants.PROPERTY_NAME_MESSAGE_VALUE} := F_ToAnyType(anyArg:= {nameInst});")
        };
    }
}
