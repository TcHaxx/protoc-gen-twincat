using System.Reflection;
using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Message;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcPouFactory
{
    public static TcPOU Create(DescriptorProto message, Comments comments, Prefixes prefixes)
    {
        var name = prefixes.GetFbNameWithTypePrefix(message);
        var pou = new TcPOU
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            POU = new TcPlcObjectPOU()
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
        subMessages.ToList().ForEach(x => sb.AppendLine($"    {prefixes.GetFbNameWithInstancePrefix(x)} : {prefixes.GetFbNameWithTypePrefix(x)};"));
        sb.AppendLine("    END_VAR");
        return sb.ToString();
    }


    public static void WriteMethod(this TcPOU tcPOU, StringBuilder declaration)
    {

    }

    private static void WriteHeader(this TcPOU pou)
    {
        var header = Helper.GetApplicationHeader(Assembly.GetExecutingAssembly());
        pou.POU.Declaration ??= new XmlDocument().CreateCDataSection(header);
    }
}
