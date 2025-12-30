using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTc.Prefix;

internal static class PrefixFactory
{
    internal static Prefixes GetPrefixes(this FileDescriptorProto fileDescriptorProto)
    {
        var globalPrefixes = GetGlobalPrefixes(fileDescriptorProto);
        var messagePrefixes = GetAllMessagePrefixes(fileDescriptorProto.MessageType);
        var enumPrefixes = GetAllEnumPrefixes(fileDescriptorProto.EnumType);
        return new Prefixes(globalPrefixes, messagePrefixes, enumPrefixes);
    }

    private static GlobalPrefixes GetGlobalPrefixes(FileDescriptorProto fileDescriptorProto)
    {
        fileDescriptorProto.Options.TryGetExtension(GlobalFbPrefix, out var fbPrefix);
        fileDescriptorProto.Options.TryGetExtension(GlobalStPrefix, out var stPrefix);
        fileDescriptorProto.Options.TryGetExtension(GlobalEnumPrefix, out var enumPrefix);

        return new GlobalPrefixes(fbPrefix, stPrefix, enumPrefix);
    }

    private static Dictionary<DescriptorProto, MessagePrefixes> GetAllMessagePrefixes(
        RepeatedField<DescriptorProto> messages)
    {
        return messages.ToDictionary(k => k, GetMessagePrefixes);
    }

    private static MessagePrefixes GetMessagePrefixes(DescriptorProto message)
    {
        message.Options.TryGetExtension(FbPrefix, out var fbPrefix);
        message.Options.TryGetExtension(StPrefix, out var stPrefix);

        return new MessagePrefixes(fbPrefix, stPrefix);
    }

    private static Dictionary<EnumDescriptorProto, EnumPrefixes> GetAllEnumPrefixes(
        RepeatedField<EnumDescriptorProto> enums)
    {
        return enums.ToDictionary(k => k, GetEnumPrefixes);
    }

    private static EnumPrefixes GetEnumPrefixes(EnumDescriptorProto @enum)
    {
        @enum.Options.TryGetExtension(EnumPrefix, out var prefix);

        return new EnumPrefixes(prefix);
    }
}
