using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Methods;

internal class WriteTo : IMethodProcessor
{
    private const string RETURN_WHEN_FAILED =
        """
        IF FAILED(WriteTo) THEN
            RETURN;
        END_IF
        """;

    private WriteTo(XmlCDataSection decl, Implementation impl)
    {
        Declaration = decl;
        Implementation = impl;
    }

    internal static WriteTo From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var impl = BuildImplementation(message, prefixes);
        return new WriteTo(decl, impl);
    }

    public string Name => Constants.METHOD_NAME_WRITE_TO;
    public XmlCDataSection Declaration { get; }
    public Implementation Implementation { get; }

    private static XmlCDataSection BuildDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                        (* Writes the data to the given coded output stream.*)
                        METHOD {{Constants.METHOD_NAME_WRITE_TO}} : HRESULT
                        VAR_IN_OUT
                        	(* Coded output stream to write the data to. *)
                        	fbWriteCtx	: FB_WriteContext;
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

            if (field.Label == FieldDescriptorProto.Types.Label.Repeated)
            {
                sb.AppendLine(ProcessRepeatedField(message, field, prefixes));
            }
            else if (field.Type == FieldDescriptorProto.Types.Type.Message)
            {
                sb.AppendLine(ProcessMessage(message, field, prefixes));
            }
            else
            {
                sb.AppendLine(ProcessScalarField(message, field, prefixes));
            }
        }
        return new() { ST = CData.From(sb.ToString()) };
    }

    private static string ProcessMessage(DescriptorProto message, FieldDescriptorProto subMessage, Prefixes prefixes)
    {
        var subMsgFbName = prefixes.GetFbNameWithInstancePrefix(subMessage);
        var subMsgPropertyName = prefixes.GetStNameWithPropertyPrefix(subMessage);
        var msgStName = prefixes.GetStNameWithInstancePrefix(message);
        return $"""
                WriteTo := fbWriteCtx.WriteTag({subMessage.Number}, {Constants.ENUM_WIRE_TYPE}.LengthDelimited);
                {RETURN_WHEN_FAILED}
                {subMsgFbName}.{subMsgPropertyName} := THIS^.{msgStName}.{subMessage.Name};
                WriteTo := fbWriteCtx.WriteMessage(ipMessage:= THIS^.{subMsgFbName});
                {RETURN_WHEN_FAILED}
                """;
    }

    private static string ProcessRepeatedField(DescriptorProto message, FieldDescriptorProto repeatedField, Prefixes prefixes)
    {
        var suffix = $"Id{repeatedField.Number}";
        return $"""
                WriteTo := _fbRepeated{suffix}.WriteTo(fbWriteCtx:= fbWriteCtx, ipFieldCodec:= _fbFieldCodec{suffix});
                {RETURN_WHEN_FAILED}
                """;
    }

    private static string ProcessScalarField(DescriptorProto message, FieldDescriptorProto field, Prefixes prefixes)
    {
        var msgStName = prefixes.GetStNameWithInstancePrefix(message);
        return $"""
                {WriteScalarTag(field)}
                WriteTo := fbWriteCtx.Write{field.Type}({field.GetFieldAssignVarString("write")}:= {msgStName}.{field.Name});
                {RETURN_WHEN_FAILED}
                """;
    }

    private static string WriteScalarTag(FieldDescriptorProto field)
    {
        return $"""
                WriteTo := fbWriteCtx.WriteTag({field.Number}, {Constants.ENUM_WIRE_TYPE}.{field.GetWireType()});
                {RETURN_WHEN_FAILED}
                """;
    }

}
