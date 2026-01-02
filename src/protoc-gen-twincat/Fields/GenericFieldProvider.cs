using System.Text;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class GenericFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments, Prefixes prefixes)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        var nameType = prefixes.GetStNameWithTypePrefix(field);
        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {nameType};");
        }
        else
        {
            sb.Append($"\t{field.Name} : {nameType};");
        }

        sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        sb.AppendLine();
        return sb.ToString();
    }
}
