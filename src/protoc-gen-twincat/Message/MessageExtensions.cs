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
}
