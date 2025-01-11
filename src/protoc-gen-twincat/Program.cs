using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Google.Protobuf;
using Google.Protobuf.Compiler;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using TcHaxx.ProtocGenTc;
using TcHaxx.ProtocGenTc.TcPlcObjects;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

#if DEBUG
await Console.Error.WriteLineAsync("Waiting for debugger to attach...");
while (!System.Diagnostics.Debugger.IsAttached)
{
    await Task.Delay(500);
}
#endif

var extensionRegistry = ExtensionRegistryBuilder.Build();

var response = new CodeGeneratorResponse();
var request = CodeGeneratorRequest.Parser
    .WithExtensionRegistry(extensionRegistry)
    .ParseFrom(Console.OpenStandardInput());

foreach (var file in request.ProtoFile)
{
    if (file.Name.Equals("google/protobuf/descriptor.proto"))
    {
        continue;
    }

    foreach (var message in file.MessageType)
    {
        var fileName = message.Name + ".TcDUT";

        var tcDUT = TcDutFactory.CreateTcDUT(message.Name, GetDutAttributes(message.Options));
        var responseFile = new CodeGeneratorResponse.Types.File
        {
            Name = fileName,
            Content = string.Empty,
        };
        response.File.Add(responseFile);
        var processedFields = new StringBuilder();

        foreach (var field in message.Field)
        {
            await Console.Error.WriteLineAsync($"Field: {field.Name} (Type: {field.Type})");

#pragma warning disable IDE0072 // Add missing cases
            processedFields.Append(field.Type switch
            {
                FieldDescriptorProto.Types.Type.String => ProcessStringField(field),
                FieldDescriptorProto.Types.Type.Double => ProcessDoubleField(field),
                FieldDescriptorProto.Types.Type.Float => ProcessFloatField(field),
                FieldDescriptorProto.Types.Type.Int64 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Uint64 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Int32 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Fixed64 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Fixed32 => ProcessIntegerField(field),
                // FieldDescriptorProto.Types.Type.Bool => throw new NotImplementedException(),
                // FieldDescriptorProto.Types.Type.Group => throw new NotImplementedException(),
                FieldDescriptorProto.Types.Type.Message => ProcessGenericField(field),
                // FieldDescriptorProto.Types.Type.Bytes => throw new NotImplementedException(),
                FieldDescriptorProto.Types.Type.Uint32 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Enum => ProcessGenericField(field),
                FieldDescriptorProto.Types.Type.Sfixed32 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Sfixed64 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Sint32 => ProcessIntegerField(field),
                FieldDescriptorProto.Types.Type.Sint64 => ProcessIntegerField(field),
                _ => ProcessUnknownField(field),
            });
#pragma warning restore IDE0072 // Add missing cases
        }
        tcDUT.WriteDeclaration(processedFields);
        responseFile.Content += SerializePlcObject(tcDUT);
    }

    response.WriteTo(Console.OpenStandardOutput());
}

static string ProcessIntegerField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();

    var options = field.Options;
    if (TryGetAttributeDisplayMode(options, out var displaymodeAttribute))
    {
        sb.AppendLine($"\t{displaymodeAttribute}");
    }
    var dataType = string.Empty;
    if (options is not null && options.HasExtension(IntegerType))
    {
        if (!TryGetTwinCatDataTypeFromEnumInterTypes(options.GetExtension(IntegerType), out dataType))
        {
            var error = $"Unhandled integer type: {field.Name} : {field.Type}";
            Console.Error.WriteLine(error);
            sb.AppendLine($"\t// {error}");
            return sb.ToString();
        }
    }
    else
    {
        dataType = MapProtoIntegerToTwinCat(field);
        if (string.IsNullOrEmpty(dataType))
        {
            var error = $"Unhandled integer type: {field.Name} : {field.Type}";
            Console.Error.WriteLine(error);
            sb.AppendLine($"\t// {error}");
            return sb.ToString();
        }
    }
    if (GetArrayLengthWhenRepeatedLabelOrFail(field, out var length))
    {
        sb.AppendLine($"\t{field.Name} : ARRAY[0..{length}] OF {dataType};");
    }
    else
    {
        sb.AppendLine($"\t{field.Name} : {dataType};");
    }
    return sb.ToString();
}

static bool TryGetTwinCatDataTypeFromEnumInterTypes(EnumIntegerTypes enumIntegerTypes, out string dataType)
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

static string MapProtoIntegerToTwinCat(FieldDescriptorProto field)
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

static string ProcessGenericField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    // Remove leading '.Example.' from type name, e.g. .Example.ST_SubDataType -> ST_SubDataType
    var stripped = field.TypeName.StartsWith('.') ? field.TypeName.Split('.')[^1] : field.TypeName;
    if (GetArrayLengthWhenRepeatedLabelOrFail(field, out var length))
    {
        sb.AppendLine($"\t{field.Name} : ARRAY[0..{length}] OF {stripped};");
    }
    else
    {
        sb.AppendLine($"\t{field.Name} : {stripped};");
    }
    return sb.ToString();
}

static string ProcessStringField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();

    var options = field.Options;
    if (options is null)
    {
        sb.AppendLine($"\t{field.Name} : STRING;");
    }
    else
    {
        Console.Error.WriteLine($"Field: {field.Name} has options: {options}");
        if (options.HasExtension(MaxStringLen))
        {
            var extensionValue = options.GetExtension(MaxStringLen);
            sb.AppendLine($"\t{field.Name} : STRING({extensionValue});");
        }
        else
        {
            Console.Error.WriteLine($"Field: {field.Name} has options but no MaxStringLen extension");
            sb.AppendLine($"\t{field.Name} : STRING;");
        }
    }

    return sb.ToString();
}

static string ProcessDoubleField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    if (GetArrayLengthWhenRepeatedLabelOrFail(field, out var length))
    {
        sb.AppendLine($"\t{field.Name} : ARRAY[0..{length}] OF LREAL;");
    }
    else
    {
        sb.AppendLine($"\t{field.Name} : LREAL;");
    }
    return sb.ToString();
}

static string ProcessFloatField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    if (GetArrayLengthWhenRepeatedLabelOrFail(field, out var length))
    {
        sb.AppendLine($"\t{field.Name} : ARRAY[0..{length}] OF REAL;");
    }
    else
    {
        sb.AppendLine($"\t{field.Name} : REAL;");
    }
    return sb.ToString();
}

static string ProcessUnknownField(FieldDescriptorProto field)
{
    var error = $"Unhandled field type: {field.Name} : {field.Type}";
    Console.Error.WriteLine(error);
    return $"// {error}\r\n";
}

static string SerializePlcObject(TcDUT tcDUT)
{
    var serializer = new XmlSerializer(typeof(TcDUT));
    var settings = new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        Indent = true,
        OmitXmlDeclaration = false
    };
    using var memoryStream = new MemoryStream();
    using var xmlWriter = XmlWriter.Create(memoryStream, settings);
    serializer.Serialize(xmlWriter, tcDUT);
    var utf8String = Encoding.UTF8.GetString(memoryStream.ToArray());
    return utf8String;
}

static bool TryGetAttributeDisplayMode(FieldOptions? options, [NotNullWhen(true)] out string value)
{
    value = string.Empty;
    if (options is null || !options.HasExtension(AttributeDisplayMode))
    {
        return false;
    }

    var displayMode = options.GetExtension(AttributeDisplayMode);
    switch (displayMode)
    {
        case EnumDisplayMode.EdmDefault:
            return false;
        case EnumDisplayMode.EdmDec:
            value = "{attribute 'displaymode':='dec'}";
            return true;
        case EnumDisplayMode.EdmHex:
            value = "{attribute 'displaymode':='hex'}";
            return true;
        case EnumDisplayMode.EdmBin:
            value = "{attribute 'displaymode':='bin'}";
            return true;
        default:
            Console.Error.WriteLine($"Unhandled display mode: {displayMode}");
            return false;
    }
}

static string GetDutAttributes(MessageOptions? options)
{
    if (options is null)
    {
        return string.Empty;
    }
    var sbDutAttributes = new StringBuilder();
    if (TryGetAttributePackMode(options, out var packmodeAttribute))
    {
        sbDutAttributes.AppendLine($"{packmodeAttribute}");
    }
    return sbDutAttributes.ToString();
}

static bool TryGetAttributePackMode(MessageOptions? options, [NotNullWhen(true)] out string value)
{
    value = string.Empty;
    if (options is null || !options.HasExtension(AttributePackMode))
    {
        return false;
    }

    var packMode = options.GetExtension(AttributePackMode);
    switch (packMode)
    {
        case EnumPackMode.EpmDefault:
        case EnumPackMode.EpmOneByteAligned:
            value = "{attribute 'pack_mode' := '1'}";
            return true;
        case EnumPackMode.EpmTwoBytesAligned:
            value = "{attribute 'pack_mode' := '2'}";
            return true;
        case EnumPackMode.EpmFourBytesAligned:
            value = "{attribute 'pack_mode' := '4'}";
            return true;
        case EnumPackMode.EpmEightBytesAligned:
            value = "{attribute 'pack_mode' := '8'}";
            return true;
        default:
            Console.Error.WriteLine($"Unhandled pack mode: {packMode}");
            return false;
    }
}

static bool HasRepeatedLabel(FieldDescriptorProto field)
{
    return field.Label == FieldDescriptorProto.Types.Label.Repeated;
}

static bool GetArrayLengthWhenRepeatedLabelOrFail(FieldDescriptorProto field, out uint length)
{
    length = 0;
    if (!HasRepeatedLabel(field))
    {
        return false;
    }

    if (field.Options is null || !field.Options.HasExtension(ArrayLength))
    {
        throw new InvalidOperationException($"Field {field.Name} has label \"repeated\" but no TcHaxx.Extensions.v1.{nameof(ArrayLength)} extension");
    }

    var len = field.Options.GetExtension(ArrayLength);
    length = len > 0 ? len - 1 : 0;
    return true;
}
