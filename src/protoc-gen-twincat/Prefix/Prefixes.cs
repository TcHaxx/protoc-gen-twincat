
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Prefix;

internal record Prefixes(
    GlobalPrefixes Global,
    Dictionary<DescriptorProto, MessagePrefixes> Message,
    Dictionary<EnumDescriptorProto, EnumPrefixes> Enum);
