using Google.Protobuf.Reflection;

namespace TcHaxx.ProtocGenTc.Message;

public static class MessageExtensions
{
    extension(DescriptorProto message)
    {
        internal IEnumerable<FieldDescriptorProto> GetSubMessages()
        {
            return message.Field.Where(field => field.Type == FieldDescriptorProto.Types.Type.Message);
        }
    }
}
