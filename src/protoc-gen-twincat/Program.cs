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
        sb.AppendLine(displaymodeAttribute);
    }
    if (options is not null && options.HasExtension(IntegerType))
    {
#pragma warning disable S3458 // Empty "case" clauses that fall through to the "default" should be omitted
        switch (options.GetExtension(IntegerType))
        {
            case EnumIntegerTypes.EitByte:
                sb.Append($"{field.Name} : BYTE;");
                break;
            case EnumIntegerTypes.EitWord:
                sb.Append($"{field.Name} : WORD;");
                break;
            case EnumIntegerTypes.EitDword:
                sb.Append($"{field.Name} : DWORD;");
                break;
            case EnumIntegerTypes.EitLword:
                sb.Append($"{field.Name} : LWORD;");
                break;
            case EnumIntegerTypes.EitSint:
                sb.Append($"{field.Name} : SINT;");
                break;
            case EnumIntegerTypes.EitUsint:
                sb.Append($"{field.Name} : USINT;");
                break;
            case EnumIntegerTypes.EitInt:
                sb.Append($"{field.Name} : INT;");
                break;
            case EnumIntegerTypes.EitUint:
                sb.Append($"{field.Name} : UINT;");
                break;
            case EnumIntegerTypes.EitDint:
                sb.Append($"{field.Name} : DINT;");
                break;
            case EnumIntegerTypes.EitUdint:
                sb.Append($"{field.Name} : UDINT;");
                break;
            case EnumIntegerTypes.EitLint:
                sb.Append($"{field.Name} : LINT;");
                break;
            case EnumIntegerTypes.EitUlint:
                sb.Append($"{field.Name} : ULINT;");
                break;
            case EnumIntegerTypes.EitDefault:
            default:
                var error = $"Unhandled integer type: {field.Name} : {field.Type}";
                Console.Error.WriteLine(error);
                sb.Append($"// {error}\r\n");
                break;
        }
#pragma warning restore S3458 // Empty "case" clauses that fall through to the "default" should be omitted
    }
    else
    {
#pragma warning disable IDE0010 // Add missing cases
        switch (field.Type)
        {
            case FieldDescriptorProto.Types.Type.Uint32:
            case FieldDescriptorProto.Types.Type.Fixed32:
                sb.Append($"{field.Name} : UDINT;");
                break;
            case FieldDescriptorProto.Types.Type.Uint64:
            case FieldDescriptorProto.Types.Type.Fixed64:
                sb.Append($"{field.Name} : ULINT;");
                break;
            case FieldDescriptorProto.Types.Type.Int32:
            case FieldDescriptorProto.Types.Type.Sint32:
            case FieldDescriptorProto.Types.Type.Sfixed32:
                sb.Append($"{field.Name} : DINT;");
                break;
            case FieldDescriptorProto.Types.Type.Int64:
            case FieldDescriptorProto.Types.Type.Sint64:
            case FieldDescriptorProto.Types.Type.Sfixed64:
                sb.Append($"{field.Name} : LINT;");
                break;
            default:
                var error = $"Unhandled integer type: {field.Name} : {field.Type}";
                Console.Error.WriteLine(error);
                sb.Append($"// {error}\r\n");
                break;
        }
#pragma warning restore IDE0010 // Add missing cases
    }
    sb.AppendLine();
    return sb.ToString();
}


static string ProcessGenericField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    // Remove leading '.Example.' from type name, e.g. .Example.ST_SubDataType -> ST_SubDataType
    var stripped = field.TypeName.StartsWith('.') ? field.TypeName.Split('.')[^1] : field.TypeName;
    sb.Append($"{field.Name} : {stripped};");
    sb.AppendLine();
    return sb.ToString();
}

static string ProcessStringField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();

    var options = field.Options;
    if (options is null)
    {
        sb.Append($"{field.Name} : STRING;");
    }
    else
    {
        Console.Error.WriteLine($"Field: {field.Name} has options: {options}");
        if (options.HasExtension(MaxStringLen))
        {
            var extensionValue = options.GetExtension(MaxStringLen);
            sb.Append($"{field.Name} : STRING({extensionValue});");
        }
        else
        {
            Console.Error.WriteLine($"Field: {field.Name} has options but no MaxStringLen extension");
            sb.Append($"{field.Name} : STRING;");
        }
    }

    sb.AppendLine();
    return sb.ToString();
}

static string ProcessDoubleField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    sb.Append($"{field.Name} : LREAL;");
    sb.AppendLine();
    return sb.ToString();
}

static string ProcessFloatField(FieldDescriptorProto field)
{
    var sb = new StringBuilder();
    sb.Append($"{field.Name} : REAL;");
    sb.AppendLine();
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
        sbDutAttributes.AppendLine(packmodeAttribute);
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
        case EnumPackMode.EpmMeightBytesAligned:
            value = "{attribute 'pack_mode' := '8'}";
            return true;
        default:
            Console.Error.WriteLine($"Unhandled pack mode: {packMode}");
            return false;
    }
}
