using Google.Protobuf;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTc;
internal static class ExtensionRegistryBuilder
{
    public static ExtensionRegistry Build()
    {
        var registry = new ExtensionRegistry
        {
            MaxStringLen,
            AttributePackMode,
            AttributeDisplayMode,
            AttributeTcrpcenable,
        };
        return registry;
    }
}
