using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Methods;

internal class CalculateSize : IMethodProcessor
{
    private CalculateSize(XmlCDataSection decl, TcPlcObjectPOUMethodImplementation impl)
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
    public TcPlcObjectPOUMethodImplementation Implementation { get; }

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

    private static TcPlcObjectPOUMethodImplementation BuildImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        foreach (var field in message.Field)
        {
            sb.AppendLine($"// {field.Type} {field.Name} = {field.Number};");
            if (field.Type == FieldDescriptorProto.Types.Type.Message)
            {
                sb.AppendLine(
                    $"nSize := nSize + F_ComputeMessageSize(iMessage:= {prefixes.GetFbNameWithInstancePrefix(field)});");
            }
            else if (field.Label == FieldDescriptorProto.Types.Label.Repeated)
            {
                var instanceName = $"fbRepeated{field.Name}";
                sb.AppendLine($$"""
                                {{instanceName}}.CalculatePackedDataSize(nPackedDataSize => nRepeatedFieldSize);
                                nSize := nSize + {{field.GetFieldTagLength()}} + nRepeatedFieldSize;")
                                """);
            }
            else
            {
                var instanceName = prefixes.GetStNameWithInstancePrefix(message);
                var computeSizeType = GetComputeSizeTypeName(field);
                var parameterName = field.GetFieldAssignVarString(string.Empty);
                sb.AppendLine(
                    $"nSize := nSize + {field.GetFieldTagLength()} + F_Compute{computeSizeType}Size({parameterName}:= {instanceName}.{field.Name});");
            }
        }

        sb.AppendLine("CalculateSize := nSize;");
        return new() { ST = CData.From(sb.ToString()) };
    }

    private static string GetComputeSizeTypeName(FieldDescriptorProto field)
    {
        return field.Type switch
        {
            FieldDescriptorProto.Types.Type.Bool => "Bool",
            FieldDescriptorProto.Types.Type.Group => throw new NotImplementedException(),
            FieldDescriptorProto.Types.Type.Enum => "Enum",
            FieldDescriptorProto.Types.Type.Message => throw new NotImplementedException(),
            FieldDescriptorProto.Types.Type.String => "String",
            FieldDescriptorProto.Types.Type.Bytes => "Bytes",
            FieldDescriptorProto.Types.Type.Int32 => "Int32",
            FieldDescriptorProto.Types.Type.Int64 => "Int64",
            FieldDescriptorProto.Types.Type.Uint32 => "UInt32",
            FieldDescriptorProto.Types.Type.Uint64 => "UInt64",
            FieldDescriptorProto.Types.Type.Sint32 => "SInt32",
            FieldDescriptorProto.Types.Type.Sint64 => "SInt64",
            FieldDescriptorProto.Types.Type.Fixed32 => "Fixed32",
            FieldDescriptorProto.Types.Type.Fixed64 => "Fixed64",
            FieldDescriptorProto.Types.Type.Sfixed32 => "SFixed32",
            FieldDescriptorProto.Types.Type.Sfixed64 => "SFixed64",
            FieldDescriptorProto.Types.Type.Float => "Float",
            FieldDescriptorProto.Types.Type.Double => "Double",
            _ => throw new NotImplementedException(),
        };
    }
}
