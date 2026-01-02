using System.Reflection;
using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Message;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects.Methods;
using TcHaxx.ProtocGenTc.TcPlcObjects.Properties;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcPouFactory
{
    public static TcPlcObject Create(DescriptorProto message, Comments comments, Prefixes prefixes)
    {
        var name = prefixes.GetFbNameWithTypePrefix(message);
        var pou = new TcPlcObject()
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            POU = new TcPOU()
            {
                Name = name,
                Id = Guid.NewGuid().ToString(),
                SpecialFunc = "None"
            }
        };
        pou.WriteHeader();
        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            pou.POU.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }

        var subMessages = message.GetSubMessages();

        pou.POU.Declaration.Data += EmitPouDeclaration(message, subMessages, prefixes);
        pou.POU.Implementation = Implementation.Empty;
        pou.POU.Method =
        [
            GenerateMethod(CalculateSize.From(message, prefixes)),
            GenerateMethod(MergeFrom.From(message, prefixes)),
            GenerateMethod(WriteTo.From(message, prefixes))
        ];
        pou.POU.Property =
        [
            GenerateProperty(DataType.From(message, prefixes)),
            GenerateProperty(Parser.From(message, prefixes)),
            GenerateProperty(Writer.From(message, prefixes)),
            GenerateProperty(MessageValue.From(message, prefixes))
        ];
        return pou;
    }

    private static string EmitPouDeclaration(DescriptorProto message, IEnumerable<FieldDescriptorProto> subMessages, Prefixes prefixes)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                        {attribute 'no_explicit_call' := 'do not call this POU directly'} 
                        FUNCTION_BLOCK INTERNAL FINAL {{prefixes.GetFbNameWithTypePrefix(message)}} IMPLEMENTS I_Message
                        VAR
                            _fbMessageParser : FB_MessageParser(ipMessage:= THIS^);
                            _fbMessageWriter : FB_MessageWriter(ipMessage:= THIS^);
                            {{prefixes.GetStNameWithInstancePrefix(message)}} : {{prefixes.GetStNameWithTypePrefix(message)}};
                        """);
        foreach (var repeatedField in message.Field.Where(f => f.Label == FieldDescriptorProto.Types.Label.Repeated))
        {
            var msgName = prefixes.GetStNameWithInstancePrefix(message);
            if (repeatedField.Type == FieldDescriptorProto.Types.Type.Message)
            {
                var varName = prefixes.GetStNameWithInstancePrefix(repeatedField);
                var fbName = prefixes.GetFbNameWithInstancePrefix(repeatedField);
                sb.AppendLine($$"""
                                    _fbRepeated{{repeatedField.Name}}Codec : FB_FieldCodecMessage(nTag:= 16#{{repeatedField.GetFieldTagValue().ToString("X2")}}, ipMessage:= {{fbName}});
                                    _fbRepeated{{repeatedField.Name}} : FB_RepeatedField(anyArray:= F_ToAnyType({{msgName}}.{{varName}}), anyFirstElem:= F_ToAnyType({{msgName}}.{{varName}}[0]));
                                """);
            }
            else
            {
                sb.AppendLine($$"""
                                    _fbRepeated{{repeatedField.Name}} : FB_RepeatedField(anyArray:= F_ToAnyType({{msgName}}.{{repeatedField.Name}}), anyFirstElem:= F_ToAnyType({{msgName}}.{{repeatedField.Name}}[0]));
                                """);
            }
        }
        subMessages.ToList().ForEach(x => sb.AppendLine($"    {prefixes.GetFbNameWithInstancePrefix(x)} : {prefixes.GetFbNameWithTypePrefix(x)};"));
        sb.AppendLine("    END_VAR");
        return sb.ToString();
    }


    public static void WriteMethod(this TcPOU tcPOU, StringBuilder declaration)
    {

    }

    private static void WriteHeader(this TcPlcObject pou)
    {
        var header = Helper.GetApplicationHeader(Assembly.GetExecutingAssembly());
        pou.POU.Declaration ??= new XmlDocument().CreateCDataSection(header);
    }

    private static Method GenerateMethod(IMethodProcessor methodProcessor)
    {
        return new Method()
        {
            Name = methodProcessor.Name,
            Id = Guid.NewGuid().ToString(),
            Declaration = methodProcessor.Declaration,
            Implementation = methodProcessor.Implementation
        };
    }

    private static Property GenerateProperty(IPropertyProcessor propertyProcessor)
    {
        return new Property()
        {
            Name = propertyProcessor.Name,
            Id = Guid.NewGuid().ToString(),
            Declaration = propertyProcessor.Declaration,
            Get = propertyProcessor.Getter,
            Set = propertyProcessor.Setter
        };
    }
}
