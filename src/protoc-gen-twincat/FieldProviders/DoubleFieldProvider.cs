using System.Text;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.FieldProviders;

internal static class DoubleFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF LREAL;");
        }
        else
        {
            sb.Append($"\t{field.Name} : LREAL;");
        }

        sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        sb.AppendLine();
        return sb.ToString();
    }
}
