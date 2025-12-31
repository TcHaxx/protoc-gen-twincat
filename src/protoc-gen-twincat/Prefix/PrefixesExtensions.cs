using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Prefix;

internal static class PrefixesExtensions
{
    extension(Prefixes prefixes)
    {
        internal string GetFbNameWithPrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForFb(message, out var prefixedName) ? $"{prefixedName}{message.Name}" : message.Name;
        }

        internal bool TryGetPrefixForFb(DescriptorProto message, [NotNullWhen(true)] out string? prefix)
        {
            prefix = prefixes.Global.GlobalFbPrefix;
            if (prefixes.Message.TryGetValue(message, out var value))
            {
                prefix = value.MessageFbPrefix ?? prefix;
            }

            return !string.IsNullOrEmpty(prefix);
        }

        internal string GetStNameWithPrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForSt(message, out var prefixedName) ? $"{prefixedName}{message.Name}" : message.Name;
        }

        internal bool TryGetPrefixForSt(DescriptorProto message, [NotNullWhen(true)] out string? prefix)
        {
            prefix = prefixes.Global.GlobalStPrefix;
            if (prefixes.Message.TryGetValue(message, out var value))
            {
                prefix = value.MessageStPrefix ?? prefix;
            }

            return !string.IsNullOrEmpty(prefix);
        }

        internal string GetEnumNameWithPrefix(EnumDescriptorProto @enum)
        {
            return prefixes.TryGetEnumPrefix(@enum, out var prefixedName) ? $"{prefixedName}{@enum.Name}" : @enum.Name;
        }

        internal bool TryGetEnumPrefix(EnumDescriptorProto @enum, [NotNullWhen(true)] out string? prefix)
        {
            prefix = prefixes.Global.GlobalEnumPrefix;
            if (prefixes.Enum.TryGetValue(@enum, out var value))
            {
                prefix = value.EnumPrefix ?? prefix;
            }

            return !string.IsNullOrEmpty(prefix);
        }
    }
}
