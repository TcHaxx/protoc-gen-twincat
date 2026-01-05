using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Prefix;

internal static class PrefixesExtensions
{
    extension(Prefixes prefixes)
    {
        internal string GetFbNameWithTypePrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForFb(message, out var prefixedName)
                ? $"{prefixedName.Type}{message.Name}"
                : message.Name;
        }

        internal string GetFbNameWithTypePrefix(FieldDescriptorProto fieldMessage)
        {
            var strippedName = StripNestedTypeName(fieldMessage.TypeName);
            return prefixes.TryGetPrefixForFbByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Type}{strippedName}"
                : strippedName;
        }

        internal string GetFbNameWithInstancePrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForFb(message, out var prefixedName)
                ? $"{prefixedName.Instance}{message.Name}"
                : message.Name;
        }

        internal string GetFbNameWithInstancePrefix(FieldDescriptorProto fieldMessage)
        {
            var strippedName = StripNestedTypeName(fieldMessage.TypeName);
            return prefixes.TryGetPrefixForFbByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Instance}{strippedName}"
                : strippedName;
        }

        internal bool TryGetPrefixForFb(DescriptorProto message, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalFbPrefix;
            if (prefixes.Message.TryGetValue(message, out var value))
            {
                prefix = value.MessageFbPrefix ?? prefix;
            }

            return prefix is not null;
        }

        internal string GetStNameWithTypePrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForSt(message, out var prefixedName)
                ? $"{prefixedName.Type}{message.Name}"
                : message.Name;
        }

        internal string GetStNameWithTypePrefix(FieldDescriptorProto fieldMessage)
        {
            var strippedName = StripNestedTypeName(fieldMessage.TypeName);
            return prefixes.TryGetPrefixForStByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Type}{strippedName}"
                : strippedName;
        }

        internal string GetStNameWithInstancePrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForSt(message, out var prefixedName)
                ? $"{prefixedName.Instance}{message.Name}"
                : message.Name;
        }

        internal string GetStNameWithPropertyPrefix(DescriptorProto message)
        {
            return prefixes.TryGetPrefixForSt(message, out var prefixedName)
                ? $"{prefixedName.Property}{message.Name}"
                : message.Name;
        }

        internal string GetStNameWithPropertyPrefix(FieldDescriptorProto fieldMessage)
        {
            var strippedName = StripNestedTypeName(fieldMessage.TypeName);
            return prefixes.TryGetPrefixForStByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Property}{strippedName}"
                : strippedName;
        }

        internal string GetStNameWithInstancePrefix(FieldDescriptorProto fieldMessage)
        {
            var strippedName = StripNestedTypeName(fieldMessage.TypeName);
            return prefixes.TryGetPrefixForStByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Instance}{strippedName}"
                : strippedName;
        }
        internal bool TryGetPrefixForSt(DescriptorProto message, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalStPrefix;
            if (prefixes.Message.TryGetValue(message, out var value))
            {
                prefix = value.MessageStPrefix ?? prefix;
            }

            return prefix is not null;
        }

        internal string GetEnumNameWithTypePrefix(EnumDescriptorProto @enum)
        {
            return prefixes.TryGetEnumPrefix(@enum, out var prefixedName) ? $"{prefixedName.Type}{@enum.Name}" : @enum.Name;
        }

        internal string GetEnumNameWithTypePrefix(FieldDescriptorProto fieldEnum)
        {
            var strippedName = StripNestedTypeName(fieldEnum.TypeName);
            return prefixes.TryGetPrefixForEnumByName(strippedName, out var prefixedName)
                ? $"{prefixedName.Type}{strippedName}"
                : strippedName;
        }

        internal string GetEnumNameWithInstancePrefix(EnumDescriptorProto @enum)
        {
            return prefixes.TryGetEnumPrefix(@enum, out var prefixedName)
                ? $"{prefixedName.Instance}{@enum.Name}"
                : @enum.Name;
        }

        internal bool TryGetEnumPrefix(EnumDescriptorProto @enum, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalEnumPrefix;
            if (prefixes.Enum.TryGetValue(@enum, out var value))
            {
                prefix = value.EnumPrefix ?? prefix;
            }

            return prefix is not null;
        }

        private bool TryGetPrefixForFbByName(string name, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalFbPrefix;
            var msgPrefix = prefixes.Message.Where(x => x.Key.Name == name).Select(x => x.Value.MessageFbPrefix)
                .FirstOrDefault();
            prefix = msgPrefix ?? prefix;
            return prefix is not null;
        }

        private bool TryGetPrefixForStByName(string name, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalStPrefix;
            var msgPrefix = prefixes.Message.Where(x => x.Key.Name == name).Select(x => x.Value.MessageStPrefix)
                .FirstOrDefault();
            prefix = msgPrefix ?? prefix;
            return prefix is not null;
        }

        private bool TryGetPrefixForEnumByName(string name, [NotNullWhen(true)] out Extensions.v1.Prefix? prefix)
        {
            prefix = prefixes.Global.GlobalEnumPrefix;
            var enumPrefix = prefixes.Enum.Where(x => x.Key.Name == name).Select(x => x.Value.EnumPrefix)
                .FirstOrDefault();
            prefix = enumPrefix ?? prefix;
            return prefix is not null;
        }
    }

    private static string StripNestedTypeName(string nestedTypeName)
    {
        return nestedTypeName.Split('.').Last();
    }
}
