using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Fields;

public static class FieldExtensions
{
    extension(FieldDescriptorProto field)
    {
        internal int GetFieldTagValue()
        {
            return (field.Number << 3) | (int)field.GetWireType();
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
    }
}
