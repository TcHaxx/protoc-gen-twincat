using System.Text;

namespace TcHaxx.ProtocGenTc;

public static class StringBuilderExtensions
{
    extension(StringBuilder sb)
    {
        public StringBuilder AppendLineIfNotNullOrEmpty(string? value)
        {
            return string.IsNullOrEmpty(value) ? sb : sb.AppendLine(value);
        }
    }
}
