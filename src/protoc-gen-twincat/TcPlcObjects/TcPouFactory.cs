using System.Reflection;
using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcPouFactory
{
    public static TcPOU Create(DescriptorProto message, Comments comments)
    {
        var pou = new TcPOU
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            POU = new TcPlcObjectPOU()
            {
                Name = message.Name,
                Id = Guid.NewGuid().ToString(),
                SpecialFunc = "None"
            }
        };
        pou.WriteHeader();
        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            pou.POU.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }
        return pou;
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
