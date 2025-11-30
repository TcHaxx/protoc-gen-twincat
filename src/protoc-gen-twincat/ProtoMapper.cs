using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;

namespace TcHaxx.ProtocGenTc;

internal static class ProtoMapper
{
    public static string MapProtoIntegerToTwinCAT(FieldDescriptorProto field)
    {
#pragma warning disable IDE0072 // Add missing cases
        return field.Type switch
        {
            FieldDescriptorProto.Types.Type.Uint32 or
            FieldDescriptorProto.Types.Type.Fixed32 => "UDINT",
            FieldDescriptorProto.Types.Type.Uint64 or
            FieldDescriptorProto.Types.Type.Fixed64 => "ULINT",
            FieldDescriptorProto.Types.Type.Int32 or
            FieldDescriptorProto.Types.Type.Sint32 or
            FieldDescriptorProto.Types.Type.Sfixed32 => "DINT",
            FieldDescriptorProto.Types.Type.Int64 or
            FieldDescriptorProto.Types.Type.Sint64 or
            FieldDescriptorProto.Types.Type.Sfixed64 => "LINT",
            _ => string.Empty,
        };
#pragma warning restore IDE0072 // Add missing cases
    }

    public static bool TryGetTwinCatDataTypeFromEnumInterTypes(EnumIntegerTypes enumIntegerTypes, out string dataType)
    {
        switch (enumIntegerTypes)
        {
            case EnumIntegerTypes.EitByte:
                dataType = "BYTE";
                return true;
            case EnumIntegerTypes.EitWord:
                dataType = "WORD";
                return true;
            case EnumIntegerTypes.EitDword:
                dataType = "DWORD";
                return true;
            case EnumIntegerTypes.EitLword:
                dataType = "LWORD";
                return true;
            case EnumIntegerTypes.EitSint:
                dataType = "SINT";
                return true;
            case EnumIntegerTypes.EitUsint:
                dataType = "USINT";
                return true;
            case EnumIntegerTypes.EitInt:
                dataType = "INT";
                return true;
            case EnumIntegerTypes.EitUint:
                dataType = "UINT";
                return true;
            case EnumIntegerTypes.EitDint:
                dataType = "DINT";
                return true;
            case EnumIntegerTypes.EitUdint:
                dataType = "UDINT";
                return true;
            case EnumIntegerTypes.EitLint:
                dataType = "LINT";
                return true;
            case EnumIntegerTypes.EitUlint:
                dataType = "ULINT";
                return true;
            case EnumIntegerTypes.EitDefault:
                dataType = string.Empty;
                return false;
            default:
                dataType = string.Empty;
                return false;
        }
    }
}
