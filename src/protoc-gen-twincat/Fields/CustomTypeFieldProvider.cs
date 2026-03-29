using System.Text;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class CustomTypeFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments, Prefixes _)
    {
        if (!field.Options.TryGetExtension(TchaxxExtensionsExtensions.CustomType, out var customPlcType))
        {
            var error = $"CustomType extension not found for field: {field.Dump()}";
            Console.Error.WriteLine(error);
            return $"// {error}\r\n";
        }
        var sb = new StringBuilder();

        sb.AppendLineIfNotNullOrEmpty(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        field.GetArrayLengthWhenRepeatedLabelOrFail(out var arrayLength);
        var arrayPrefix = arrayLength > 0 ? $"ARRAY[0..{arrayLength}] OF " : string.Empty;

        sb.AppendLine($"\t{field.Name} : {arrayPrefix}{customPlcType};");

        sb.AppendLineIfNotNullOrEmpty(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        return sb.ToString();
    }
}
