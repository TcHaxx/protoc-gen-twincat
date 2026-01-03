using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects.Methods;

internal class MergeFrom : IMethodProcessor
{
    private MergeFrom(XmlCDataSection decl, Implementation impl)
    {
        Declaration = decl;
        Implementation = impl;
    }

    internal static MergeFrom From(DescriptorProto message, Prefixes prefixes)
    {
        var decl = BuildDeclaration(message, prefixes);
        var impl = BuildImplementation(message, prefixes);
        return new MergeFrom(decl, impl);
    }

    public string Name => Constants.METHOD_NAME_MERGE_FROM;
    public XmlCDataSection Declaration { get; }
    public Implementation Implementation { get; }

    private static XmlCDataSection BuildDeclaration(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                        (* Merges the data from the specified coded input stream with the current message.*)
                        METHOD {{Constants.METHOD_NAME_MERGE_FROM}} : HRESULT
                        VAR_IN_OUT
                            fbParseCtx : FB_ParseContext;
                        END_VAR
                        VAR
                            nTag : UDINT;
                        END_VAR
                        """);
        return CData.From(sb.ToString());
    }
    private static Implementation BuildImplementation(DescriptorProto message, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("""
                      REPEAT
                      MergeFrom := fbParseCtx.ReadTag(nTag=> nTag);
                      IF SUCCEEDED(MergeFrom) AND nTag = 0 THEN
                          EXIT;
                      END_IF;
                      
                      IF FAILED(MergeFrom) THEN
                          RETURN;
                      END_IF
                      
                      CASE nTag OF
                      """);
        foreach (var field in message.Field)
        {
            sb.AppendLine($"    16#{field.GetFieldTagValue().ToString("X2")}:    // {field.Dump()}");

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
                sb.AppendLine(ProcessdField(message, field, prefixes));
            }
        }

        sb.AppendLine("""
                      ELSE
                          MergeFrom := fbParseCtx.SkipLastField();
                      END_CASE
                      
                      UNTIL (FAILED(MergeFrom) OR nTag = 0)
                      END_REPEAT
                      """);
        return new() { ST = CData.From(sb.ToString()) };
    }

    private static string ProcessMessage(DescriptorProto message, FieldDescriptorProto subMessage, Prefixes prefixes)
    {
        var subMsgFbName = prefixes.GetFbNameWithInstancePrefix(subMessage);
        var subMsgPropertyName = prefixes.GetStNameWithPropertyPrefix(subMessage);
        var msgStName = prefixes.GetStNameWithInstancePrefix(message);
        return $"""
                        MergeFrom := fbParseCtx.ReadMessage(ipMessage:= {subMsgFbName});
                        THIS^.{msgStName}.{subMessage.Name} := {subMsgFbName}.{subMsgPropertyName};
                """;
    }

    private static string ProcessRepeatedField(DescriptorProto message, FieldDescriptorProto repeatedField, Prefixes prefixes)
    {
        var suffix = $"Id{repeatedField.Number}";
        return $"""
                        MergeFrom := _fbRepeated{suffix}.AddEntriesFrom(fbParseCtx:= fbParseCtx, ipFieldCodec:= _fbFieldCodec{suffix});
                """;
    }

    private static string ProcessdField(DescriptorProto message, FieldDescriptorProto field, Prefixes prefixes)
    {
        var msgStName = prefixes.GetStNameWithInstancePrefix(message);
        return $"""
                        MergeFrom := fbParseCtx.Read{field.Type}({field.GetFieldAssignVarString("merge")}=> {msgStName}.{field.Name}); 
                """;
    }
}
