using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.Message;
using TcHaxx.ProtocGenTc.Prefix;
using TcHaxx.ProtocGenTc.TcPlcObjects;

// #if DEBUG
// await Console.Error.WriteLineAsync("Waiting for debugger to attach...");
// while (!System.Diagnostics.Debugger.IsAttached)
// {
//     await Task.Delay(500);
// }
// #endif

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

    foreach (var enumType in file.MessageType.GetAllNestedEnums())
    {
        var responseFile = await GenerateResponseFileFromEnumAsync(file, enumType);
        result.Add(responseFile);
    }

    foreach (var message in file.MessageType.GetAllMessages())
    {
        var responseFiles = await GenerateResponseFileFromMessageAsync(file, message);
        result.AddRange(responseFiles);
    }

    return result;
}

async Task<List<CodeGeneratorResponse.Types.File>> GenerateResponseFileFromMessageAsync(FileDescriptorProto file, DescriptorProto message)
{
    var prefixes = file.GetPrefixes();
    var tcDUT = await TcDutFactory.CreateStruct(file, message, prefixes);
    var tcPOU = TcPouFactory.Create(file, message, prefixes);

    List<CodeGeneratorResponse.Types.File> responses =
    [
        new()
        {
            Name = tcPOU.POU.Name + ".TcPOU", Content = PlcObjectSerializer.Serialize(tcPOU),
        },
        new()
        {
            Name = tcDUT.DUT.Name + ".TcDUT", Content = PlcObjectSerializer.Serialize(tcDUT),
        }
    ];

    return responses;
}

async Task<CodeGeneratorResponse.Types.File> GenerateResponseFileFromEnumAsync(FileDescriptorProto file, EnumDescriptorProto enumDescriptor)
{
    var tcDUT = await TcDutFactory.CreateEnum(file, enumDescriptor, file.GetPrefixes());

    return new CodeGeneratorResponse.Types.File
    {
        Name = tcDUT.DUT.Name + ".TcDUT",
        Content = PlcObjectSerializer.Serialize(tcDUT),
    };
}
