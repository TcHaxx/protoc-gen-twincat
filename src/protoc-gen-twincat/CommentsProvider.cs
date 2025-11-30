using System.Text;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc;

internal static class CommentsProvider
{
    public static Comments GetComments(FileDescriptorProto file, DescriptorProto message, FieldDescriptorProto? field = null)
    {
        var path = GetCurrentPath(file.MessageType, message, field);
        return MatchComments(file, path);
    }

    public static Comments GetComments(FileDescriptorProto file, EnumDescriptorProto enumDescriptor, EnumValueDescriptorProto? value = null)
    {
        var path = GetCurrentPath(file.EnumType, enumDescriptor, value);
        return MatchComments(file, path);
    }

    public static string GetLeadingComments(this Comments comments)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(comments.LeadingComments))
        {
            sb.AppendLine(TransformComment(comments.LeadingComments, "\t"));
        }
        return sb.ToString();
    }

    public static string GetTrailingComments(this Comments comments)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(comments.TrailingComments))
        {
            sb.AppendLine(TransformComment(comments.TrailingComments, "\t"));
        }
        return sb.ToString();
    }

    internal static string TransformComment(string? comment, string indentation = "")
    {
        if (string.IsNullOrEmpty(comment))
        {
            return string.Empty;
        }

        var splitted = comment.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder(splitted.Length * 80);
        foreach (var line in splitted)
        {
            _ = sb.AppendLine($"{indentation}//{line}");
        }
        return sb.ToString().TrimEnd();
    }

    private static Comments MatchComments(FileDescriptorProto file, RepeatedField<int> path)
    {
        var locations = file.SourceCodeInfo.Location;
        var leadingComments = locations
             .Where(x => x.Path.SequenceEqual(path) && x.HasLeadingComments)
             .Select(x => x.LeadingComments).FirstOrDefault();
        var trailingComments = locations
            .Where(x => x.Path.SequenceEqual(path) && x.HasTrailingComments)
            .Select(x => x.TrailingComments).FirstOrDefault();
        return new Comments(leadingComments, trailingComments);
    }

    private static RepeatedField<int> GetCurrentPath(RepeatedField<DescriptorProto> messageType, DescriptorProto message, FieldDescriptorProto? field = null)
    {
        var path = new RepeatedField<int> { 4, messageType.IndexOf(message) };
        if (field is not null)
        {
            path.Add(2);
            path.Add(message.Field.IndexOf(field));
        }

        return path;
    }

    private static RepeatedField<int> GetCurrentPath(RepeatedField<EnumDescriptorProto> enumType, EnumDescriptorProto enumDescriptor, EnumValueDescriptorProto? value = null)
    {
        var path = new RepeatedField<int> { 5, enumType.IndexOf(enumDescriptor) };
        if (value is not null)
        {
            path.Add(2);
            path.Add(enumDescriptor.Value.IndexOf(value));
        }

        return path;
    }
}
