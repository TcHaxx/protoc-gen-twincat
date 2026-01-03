using System.Text;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class FloatFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        sb.AppendLineIfNotNullOrEmpty(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF REAL;");
        }
        else
        {
            sb.Append($"\t{field.Name} : REAL;");
        }

        sb.AppendLine(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        return sb.ToString();
    }
}
