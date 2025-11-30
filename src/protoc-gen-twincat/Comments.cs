using System.Text;

namespace TcHaxx.ProtocGenTc;

internal record Comments(string? LeadingComments, string? TrailingComments)
{
    public string NormalizedComments(CommentType commentType, string indentation = "")
    {
        return TransformComment(commentType == CommentType.Leading ? LeadingComments : TrailingComments, indentation);
    }

    private static string TransformComment(string? comment, string indentation = "")
    {
        if (string.IsNullOrEmpty(comment))
        {
            return string.Empty;
        }

        var splitted = comment.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder(splitted.Length * 80);
        foreach (var line in splitted)
        {
            sb.Append($"{indentation}//{line}\n");
        }

        return sb.ToString().TrimEnd();
    }
}
