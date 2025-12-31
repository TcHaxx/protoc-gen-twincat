using System.Text;
using Google.Protobuf.Reflection;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTc.Fields;

internal static class StringFieldProvider
{
    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        sb.AppendLine(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        var options = field.Options;
        if (options is null)
        {
            sb.Append($"\t{field.Name} : STRING;");
        }
        else
        {
            Console.Error.WriteLine($"Field: {field.Name} has options: {options}");
            if (options.TryGetExtension(MaxStringLen, out var extensionValue))
            {
                sb.Append($"\t{field.Name} : STRING({extensionValue});");
            }
            else
            {
                Console.Error.WriteLine($"Field: {field.Name} has options but no MaxStringLen extension");
                sb.Append($"\t{field.Name} : STRING;");
            }
        }

        sb.Append(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        sb.AppendLine();
        return sb.ToString();
    }
}
