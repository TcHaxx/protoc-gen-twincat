using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Fields;

public static class FieldExtensions
{
    extension(FieldDescriptorProto field)
    {
        internal uint GetFieldTagValue()
        {
            return ((uint)field.Number << 3) | (uint)field.GetWireType();
        }

        internal uint GetPackedRepetatedFieldTagValue()
        {
            return ((uint)field.Number << 3) | (uint)WireFormat.WireType.LengthDelimited;
        }

        internal WireFormat.WireType GetWireType()
        {
            return field.Type switch
            {
                FieldDescriptorProto.Types.Type.Double => WireFormat.WireType.Fixed64,
                FieldDescriptorProto.Types.Type.Float => WireFormat.WireType.Fixed32,
                FieldDescriptorProto.Types.Type.Int64 => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Uint64 => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Int32 => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Fixed64 => WireFormat.WireType.Fixed64,
                FieldDescriptorProto.Types.Type.Fixed32 => WireFormat.WireType.Fixed32,
                FieldDescriptorProto.Types.Type.Bool => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.String => WireFormat.WireType.LengthDelimited,
                FieldDescriptorProto.Types.Type.Group => WireFormat.WireType.StartGroup,
                FieldDescriptorProto.Types.Type.Message => WireFormat.WireType.LengthDelimited,
                FieldDescriptorProto.Types.Type.Bytes => WireFormat.WireType.LengthDelimited,
                FieldDescriptorProto.Types.Type.Uint32 => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Enum => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Sfixed32 => WireFormat.WireType.Fixed32,
                FieldDescriptorProto.Types.Type.Sfixed64 => WireFormat.WireType.Fixed64,
                FieldDescriptorProto.Types.Type.Sint32 => WireFormat.WireType.Varint,
                FieldDescriptorProto.Types.Type.Sint64 => WireFormat.WireType.Varint,
                _ => throw new NotSupportedException($"Unsupported field type: {field.Type}"),
            };
        }

        internal (string assignVar, string op) GetFieldAssignVarString(string context)
        {
            return (field.Type, context) switch
            {
                (FieldDescriptorProto.Types.Type.Bool, _) => ("bValue", "=>"),
                (FieldDescriptorProto.Types.Type.Group, _) => throw new NotImplementedException(),
                (FieldDescriptorProto.Types.Type.Enum, _) => ("nValue", "=>"),
                (FieldDescriptorProto.Types.Type.Message, _) => ("iMessage", ":="),
                (FieldDescriptorProto.Types.Type.String, "merge") => ("anyStringOut", ":="),
                (FieldDescriptorProto.Types.Type.String, "write") => ("sStringToWrite", ":="),
                (FieldDescriptorProto.Types.Type.String, _) => ("anyString", ":="),
                (FieldDescriptorProto.Types.Type.Bytes, _) => ("aBytes", ":="),
                (FieldDescriptorProto.Types.Type.Int32, _) => ("anyDint", ":="),
                (FieldDescriptorProto.Types.Type.Int64, _) => ("anyLint", ":="),
                (FieldDescriptorProto.Types.Type.Uint32, _) => ("anyUdint", ":="),
                (FieldDescriptorProto.Types.Type.Uint64, _) => ("anyUlint", ":="),
                (FieldDescriptorProto.Types.Type.Sint32, _) => ("anyDint", ":="),
                (FieldDescriptorProto.Types.Type.Sint64, _) => ("anyLint", ":="),
                (FieldDescriptorProto.Types.Type.Fixed32, _) => ("nValue", "=>"),
                (FieldDescriptorProto.Types.Type.Fixed64, _) => ("nValue", "=>"),
                (FieldDescriptorProto.Types.Type.Sfixed32, _) => ("nValue", "=>"),
                (FieldDescriptorProto.Types.Type.Sfixed64, _) => ("nValue", "=>"),
                (FieldDescriptorProto.Types.Type.Float, _) => ("fValue", "=>"),
                (FieldDescriptorProto.Types.Type.Double, _) => ("fValue", "=>"),
                _ => throw new NotImplementedException(),
            };
        }

        internal int GetFieldTagLength()
        {
            var tagValue = field.GetFieldTagValue();
            return ComputeRawVarint32Size(tagValue);
        }

        internal string Dump()
        {
            var label = field.HasLabel && field.Label != FieldDescriptorProto.Types.Label.Optional ? $"{field.Label.ToString().ToLower()} " : string.Empty;
            var type = field.HasTypeName ? field.TypeName.Split('.').Last() : field.Type.ToString().ToLower();
            return $"{label}{type} {field.Name} = {field.Number};";
        }
    }

    private static int ComputeRawVarint32Size(uint value)
    {
        if ((value & (0xffffffff << 7)) == 0)
        {
            return 1;
        }

        if ((value & (0xffffffff << 14)) == 0)
        {
            return 2;
        }

        if ((value & (0xffffffff << 21)) == 0)
        {
            return 3;
        }

        if ((value & (0xffffffff << 28)) == 0)
        {
            return 4;
        }

        return 5;
    }
}
