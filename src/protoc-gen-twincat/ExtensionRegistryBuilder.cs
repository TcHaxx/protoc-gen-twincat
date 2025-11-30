using System.Reflection;
using Google.Protobuf;
using TcHaxx.Extensions.v1;

namespace TcHaxx.ProtocGenTc;

internal static class ExtensionRegistryBuilder
{
    public static ExtensionRegistry Build()
    {
        var extensionFields = typeof(TchaxxExtensionsExtensions)
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(Extension<,>))
                    .Select(f => f.GetValue(null))
                    .ToArray();

        var extensionRegistry = new ExtensionRegistry();
        foreach (var ext in extensionFields)
        {
            extensionRegistry.Add(ext as Extension);
        }

        return extensionRegistry;
    }
}
