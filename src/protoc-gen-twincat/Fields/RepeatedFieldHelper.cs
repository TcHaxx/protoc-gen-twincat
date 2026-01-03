using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.Fields;

internal class RepeatedFieldHelper
{
    internal static string GetCountFieldName(FieldDescriptorProto field)
    {
        return $"{field.Name}_count";
    }

    internal static string WriteCountField(FieldDescriptorProto field, Prefixes prefixes)
    {
        var instanceName = GetCountFieldName(field);
        return $"""
                {instanceName} : UDINT; // Implict generated field for actual number of items in array {field.Name}
            """;
    }
}
