using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Properties;

internal class DataType : IPropertyProcessor
{
    private DataType(XmlCDataSection decl, Get? getter, Set? setter, string name)
    {
        Declaration = decl;
        Getter = getter;
        Setter = setter;
        Name = name;
    }

    internal static DataType From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var getter = BuildGetter(message, prefixes);
        var setter = BuildSetter(message, prefixes);
        var name = prefixes.GetStNameWithPropertyPrefix(message);
        return new DataType(decl, getter, setter, name);
    }

    public string Name { get; }
    public XmlCDataSection Declaration { get; }
    public Get? Getter { get; }
    public Set? Setter { get; }


    private static Get BuildGetter(DescriptorProto message, Prefixes prefixes)
    {
        return new Get()
        {
            Name = Constants.PROPERTY_GET,
            Declaration = BuildGetterDeclaration(message, prefixes),
            Implementation = BuildGetterImplementation(message, prefixes)
        };
    }

    private static Set BuildSetter(DescriptorProto message, Prefixes prefixes)
    {
        return new Set()
        {
            Name = Constants.PROPERTY_SET,
            Declaration = BuildSetterDeclaration(message, prefixes),
            Implementation = BuildSetterImplementation(message, prefixes)
        };
    }

    private static XmlCDataSection BuildDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        var nameProp = prefixes.GetStNameWithPropertyPrefix(message);
        var nameType = prefixes.GetStNameWithTypePrefix(message);
        return CData.From($"PROPERTY PUBLIC {nameProp} : {nameType}");
    }


    private static XmlCDataSection BuildGetterDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        return CData.From($"""
                           VAR
                           END_VAR
                           """);
    }

    private static XmlCDataSection BuildSetterDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        return CData.From($"""
                           VAR
                           END_VAR
                           """);
    }


    private static Implementation BuildGetterImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var nameInst = prefixes.GetStNameWithInstancePrefix(message);
        var nameProp = prefixes.GetStNameWithPropertyPrefix(message);
        return new Implementation()
        {
            ST = CData.From($"""
                             {nameProp} := {nameInst};
                             """)
        };
    }

    private static Implementation BuildSetterImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var nameInst = prefixes.GetStNameWithInstancePrefix(message);
        var nameProp = prefixes.GetStNameWithPropertyPrefix(message);
        return new Implementation()
        {
            ST = CData.From($"""
                             {nameInst} := {nameProp};
                             """)
        };
    }
}
