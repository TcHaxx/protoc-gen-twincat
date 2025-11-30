using System.Text;

namespace TcHaxx.ProtocGenTc.FieldProviders;

internal static class CommentProvider
{
    public static string GetLeadingComments(Comments comments)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(comments.LeadingComments))
        {
            sb.AppendLine(TransformComment(comments.LeadingComments, "\t"));
        }
        return sb.ToString();
    }

    public static string GetTrailingComments(Comments comments)
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
}
