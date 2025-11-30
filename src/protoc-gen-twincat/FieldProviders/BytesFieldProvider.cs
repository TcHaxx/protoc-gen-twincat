using System.Text;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.FieldProviders;

internal static class BytesFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        if (field.GetArrayLengthWhenBytesFieldOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF BYTE;");
        }

        sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        sb.AppendLine();
        return sb.ToString();
    }
}
