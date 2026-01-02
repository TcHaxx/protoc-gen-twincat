using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Methods;

internal class CalculateSize : IMethodProcessor
{
    private CalculateSize(XmlCDataSection decl, Implementation impl)
    {
        Declaration = decl;
        Implementation = impl;
    }

    internal static CalculateSize From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var impl = BuildImplementation(message, prefixes);
        return new CalculateSize(decl, impl);
    }

    public string Name => Constants.METHOD_NAME_CALCULATE_SIZE;

    public XmlCDataSection Declaration { get; }
    public Implementation Implementation { get; }

    private static XmlCDataSection BuildDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                      (* Calculates the size of this message in Protocol Buffer wire format, in bytes. *)
                      METHOD {{Constants.METHOD_NAME_CALCULATE_SIZE}} : UDINT
                      VAR
                          nSize : UDINT;
                          nRepeatedFieldSize : UDINT;
                      END_VAR
                      """);
        return CData.From(sb.ToString());
    }

    private static Implementation BuildImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        foreach (var field in message.Field)
        {
            sb.AppendLine($"// {field.Dump()}");
            if (field.Type == FieldDescriptorProto.Types.Type.Message)
            {
                sb.AppendLine(
                    $"nSize := nSize + F_ComputeMessageSize(iMessage:= {prefixes.GetFbNameWithInstancePrefix(field)});");
            }
            else if (field.Label == FieldDescriptorProto.Types.Label.Repeated)
            {
                var suffix = $"Id{field.Number}";
                var instanceName = $"_fbRepeated{suffix}";
                sb.AppendLine($$"""
                                {{instanceName}}.CalculatePackedDataSize(nPackedDataSize => nRepeatedFieldSize);
                                nSize := nSize + {{field.GetFieldTagLength()}} + nRepeatedFieldSize;
                                """);
            }
            else
            {
                var instanceName = prefixes.GetStNameWithInstancePrefix(message);
                var computeSizeType = field.MapFieldTypeToTcTypeName();
                var parameterName = field.GetFieldAssignVarString(string.Empty);
                sb.AppendLine(
                    $"nSize := nSize + {field.GetFieldTagLength()} + F_Compute{computeSizeType}Size({parameterName}:= {instanceName}.{field.Name});");
            }
        }

        sb.AppendLine("CalculateSize := nSize;");
        return new() { ST = CData.From(sb.ToString()) };
    }
}
