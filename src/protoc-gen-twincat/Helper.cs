using System.Reflection;
using System.Text;

namespace TcHaxx.ProtocGenTc;

/// <summary>
/// Some helper methods/functions and extension methods.
/// </summary>
internal static class Helper
{
    private const int WIDTH = 80;

    /// <summary>
    /// Centers a <see cref="string"/>.
    /// </summary>
    /// <param name="this"></param>
    /// <returns>centered string</returns>
    internal static string Center(this string @this, int width = WIDTH)
    {

        if (string.IsNullOrEmpty(@this))
        {
            return string.Empty;
        }
        return @this.PadLeft(((width - @this.Length) / 2)
                              + @this.Length)
                              .PadRight(width);
    }

    /// <summary>
    /// Prints the application header based on <see cref="Assembly"/>-attributes as <see cref="string"/>.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    internal static string GetApplicationHeader(Assembly assembly)
    {
        var sb = new StringBuilder(1000);

        var version = assembly.GetName().Version;
        var repoUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                              .FirstOrDefault(x => x.Key.Equals("RepositoryUrl"))?.Value ?? string.Empty;

        var delimiter = new string(Constants.CLI_DELIMITER, WIDTH - 1);
        _ = sb.AppendLine($"({delimiter}");
        _ = sb.AppendLine($"{assembly.GetName().Name} V{version}".Center());
        if (!string.IsNullOrWhiteSpace(repoUrl))
        {
            _ = sb.AppendLine($"{repoUrl.Center()}");
        }

        _ = sb.AppendLine($"{delimiter})");

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
