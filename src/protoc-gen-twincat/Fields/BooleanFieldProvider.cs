using System.Text;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class BooleanFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));
        }

        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF BOOL;");
        }
        else
        {
            sb.Append($"\t{field.Name} : BOOL;");
        }

        if (!string.IsNullOrWhiteSpace(comments.TrailingComments))
        {
            sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));
        }

        sb.AppendLine();
        return sb.ToString();
    }
}
