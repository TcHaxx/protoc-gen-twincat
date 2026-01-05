using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Message;

public static class MessageExtensions
{
    extension(DescriptorProto message)
    {
        internal IEnumerable<FieldDescriptorProto> GetMessageFields()
        {
            return message.Field.Where(field => field.Type == FieldDescriptorProto.Types.Type.Message);
        }
    }

    extension(RepeatedField<DescriptorProto> messageType)
    {
        internal IEnumerable<DescriptorProto> GetAllMessages()
        {
            return messageType.SelectMany(Enumerate);
        }

        internal IEnumerable<EnumDescriptorProto> GetAllNestedEnums()
        {
            return messageType.SelectMany(EnumerateEnumType);
        }
    }

    private static IEnumerable<DescriptorProto> Enumerate(DescriptorProto message)
    {
        yield return message;
        foreach (var nested in message.NestedType)
        {
            foreach (var n in Enumerate(nested))
            {
                yield return n;
            }
        }
    }

    private static IEnumerable<EnumDescriptorProto> EnumerateEnumType(DescriptorProto message)
    {

        foreach (var enumType in message.EnumType)
        {
            yield return enumType;
        }

        foreach (var nested in message.NestedType)
        {
            foreach (var n in EnumerateEnumType(nested))
            {
                yield return n;
            }
        }
    }
}
