using System.Text;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class EnumFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments, Prefixes prefixes)
    {
        var sb = new StringBuilder();

        sb.AppendLineIfNotNullOrEmpty(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        var nameType = prefixes.GetEnumNameWithTypePrefix(field);
        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {nameType};");
        }
        else
        {
            sb.Append($"\t{field.Name} : {nameType};");
        }

        sb.AppendLine(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        return sb.ToString();
    }
}
