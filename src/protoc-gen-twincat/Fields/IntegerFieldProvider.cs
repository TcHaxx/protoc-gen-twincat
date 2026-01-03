using System.Text;
using Google.Protobuf.Reflection;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTc.Fields;


internal static class IntegerFieldProvider
{

    internal static string ProcessField(FieldDescriptorProto field, Comments comments)
    {
        var sb = new StringBuilder();

        sb.AppendLineIfNotNullOrEmpty(CommentProvider.TransformComment(comments.LeadingComments, "\t"));

        var options = field.Options;
        if (ExtensionsHelper.TryGetAttributeDisplayMode(options, out var displaymodeAttribute))
        {
            sb.AppendLine($"\t{displaymodeAttribute}");
        }

        var dataType = string.Empty;
        if (options is not null && options.HasExtension(IntegerType))
        {
            if (!ProtoMapper.TryGetTwinCatDataTypeFromEnumInterTypes(options.GetExtension(IntegerType), out dataType))
            {
                var error = $"Unhandled integer type: {field.Name} : {field.Type}";
                Console.Error.WriteLine(error);
                sb.AppendLine($"\t// {error}");
                return sb.ToString();
            }
        }
        else
        {
            dataType = ProtoMapper.MapProtoIntegerToTwinCAT(field);
            if (string.IsNullOrEmpty(dataType))
            {
                var error = $"Unhandled integer type: {field.Name} : {field.Type}";
                Console.Error.WriteLine(error);
                sb.AppendLine($"\t// {error}");
                return sb.ToString();
            }
        }

        if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
        {
            sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {dataType};");
        }
        else
        {
            sb.Append($"\t{field.Name} : {dataType};");
        }

        sb.AppendLine(CommentProvider.TransformComment(comments.TrailingComments, "\t"));

        return sb.ToString();
    }

}
