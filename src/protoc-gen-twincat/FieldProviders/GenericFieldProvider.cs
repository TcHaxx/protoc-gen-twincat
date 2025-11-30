using System.Text;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.FieldProviders;

internal static class GenericFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        // Remove leading '.Example.' from type name, e.g. .Example.ST_SubDataType -> ST_SubDataType
        var stripped = field.TypeName.StartsWith('.') ? field.TypeName.Split('.')[^1] : field.TypeName;
        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {stripped};");
        }
        else
        {
            sb.Append($"\t{field.Name} : {stripped};");
        }

        sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        sb.AppendLine();
        return sb.ToString();
    }
}
