using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects;

#if DEBUG
await Console.Error.WriteLineAsync("Waiting for debugger to attach...");
while (!System.Diagnostics.Debugger.IsAttached)
{
    await Task.Delay(500);
}
#endif

var extensionRegistry = ExtensionRegistryBuilder.Build();

var response = new CodeGeneratorResponse
{
    SupportedFeatures = (ulong)CodeGeneratorResponse.Types.Feature.Proto3Optional
};

var request = CodeGeneratorRequest.Parser
    .WithExtensionRegistry(extensionRegistry)
    .ParseFrom(Console.OpenStandardInput());

var filteredFiles = request.ProtoFile.Where(item =>
     !ExcludedProtos.EXCLUDED_PROTO_FILES.Any(exclude => item.Name.Contains(exclude, StringComparison.OrdinalIgnoreCase)))
    .ToList();

foreach (var file in filteredFiles)
{
    response.File.AddRange(await GenerateResponseFilesAsync(file));
}

response.WriteTo(Console.OpenStandardOutput());


async Task<IEnumerable<CodeGeneratorResponse.Types.File>> GenerateResponseFilesAsync(FileDescriptorProto file)
{
    var result = new List<CodeGeneratorResponse.Types.File>();

    foreach (var enumType in file.EnumType)
    {
        var responseFile = await GenerateResponseFileFromEnumAsync(file, enumType);
        result.Add(responseFile);
    }

    foreach (var message in file.MessageType)
    {
        var responseFile = await GenerateResponseFileFromMessageAsync(file, message);
        result.Add(responseFile);
    }

    return result;
}

async Task<CodeGeneratorResponse.Types.File> GenerateResponseFileFromMessageAsync(FileDescriptorProto file, DescriptorProto message)
{
    var msgComment = CommentsProvider.GetComments(file, message);
    var tcDUT = TcDutFactory.CreateStruct(message, msgComment, file.GetPrefixes());
    var processedFields = new StringBuilder();

    foreach (var field in message.Field)
    {
        await Console.Error.WriteLineAsync($"Field: {field.Name} (Type: {field.Type})");
        var comments = CommentsProvider.GetComments(file, message, field);
        var processFieldValue = ProcessFieldValue(field, comments);
        processedFields.Append(processFieldValue);
    }

    tcDUT.WriteStructDeclaration(processedFields);

    return new CodeGeneratorResponse.Types.File
    {
        Name = message.Name + ".TcDUT",
        Content = SerializePlcObject(tcDUT),
    };
}

async Task<CodeGeneratorResponse.Types.File> GenerateResponseFileFromEnumAsync(FileDescriptorProto file, EnumDescriptorProto enumDescriptor)
{
    var enumComment = CommentsProvider.GetComments(file, enumDescriptor);
    var tcDUT = TcDutFactory.CreateEnum(enumDescriptor, enumComment, file.GetPrefixes());
    var processedFields = new StringBuilder();

    for (var i = 0; i < enumDescriptor.Value.Count; i++)
    {
        var enumValue = enumDescriptor.Value[i];
        await Console.Error.WriteLineAsync($"Name: {enumValue.Name} (Number: {enumValue.Number})");
        var comments = CommentsProvider.GetComments(file, enumDescriptor, enumValue);
        var isLast = i == enumDescriptor.Value.Count - 1;
        var processEnumValue = ProcessEnumValue(enumValue, comments, isLast);
        processedFields.Append(processEnumValue);
    }

    var options = enumDescriptor.Options;
    if (options.TryGetExtension(TchaxxExtensionsExtensions.EnumIntegerType, out var extensionValue))
    {
        ProtoMapper.TryGetTwinCatDataTypeFromEnumInterTypes(extensionValue, out var dataType);
        tcDUT.WriteEnumDeclaration(processedFields, dataType);
    }
    else
    {
        tcDUT.WriteEnumDeclaration(processedFields);
    }

    return new CodeGeneratorResponse.Types.File
    {
        Name = enumDescriptor.Name + ".TcDUT",
        Content = SerializePlcObject(tcDUT),
    };
}

static string ProcessFieldValue(FieldDescriptorProto field, Comments comments)
{
#pragma warning disable IDE0072 // Add missing cases
    var processFieldValue = field.Type switch
    {
        FieldDescriptorProto.Types.Type.Bool => BooleanFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Bytes => BytesFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Double => DoubleFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Enum => GenericFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Fixed32 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Fixed64 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Float => FloatFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Group => ProcessUnknownField(field),
        FieldDescriptorProto.Types.Type.Int32 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Int64 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Message => GenericFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Sfixed32 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Sfixed64 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Sint32 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Sint64 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.String => StringFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Uint32 => IntegerFieldProvider.ProcessField(field, comments),
        FieldDescriptorProto.Types.Type.Uint64 => IntegerFieldProvider.ProcessField(field, comments),
        _ => ProcessUnknownField(field),
    };
#pragma warning restore IDE0072 // Add missing cases
    return processFieldValue;
}

static string ProcessEnumValue(EnumValueDescriptorProto enumValue, Comments comments, bool isLastValue)
{
    var builder = new StringBuilder();
    if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
    {
        builder.AppendLine(comments.NormalizedComments(CommentType.Leading));
    }

    builder.Append($"{enumValue.Name} := {enumValue.Number}{(isLastValue ? string.Empty : ",")}");
    if (!string.IsNullOrWhiteSpace(comments.TrailingComments))
    {
        builder.Append($" {comments.NormalizedComments(CommentType.Trailing)}");
    }

    builder.AppendLine();
    return builder.ToString();
}

static string ProcessUnknownField(FieldDescriptorProto field)
{
    var error = $"Unhandled field type: {field.Name} : {field.Type}";
    Console.Error.WriteLine(error);
    return $"// {error}\r\n";
}

static string SerializePlcObject<T>(T plcObject)
{
    var serializer = new XmlSerializer(typeof(T));
    var settings = new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        Indent = true,
        OmitXmlDeclaration = false
    };
    using var memoryStream = new MemoryStream();
    using var xmlWriter = XmlWriter.Create(memoryStream, settings);
    serializer.Serialize(xmlWriter, plcObject);
    var utf8String = Encoding.UTF8.GetString(memoryStream.ToArray());
    return utf8String;
}

