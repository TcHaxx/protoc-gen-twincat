using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Google.Protobuf;
using Google.Protobuf.Collections;
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

var response = new CodeGeneratorResponse
{
    SupportedFeatures = (ulong)CodeGeneratorResponse.Types.Feature.Proto3Optional
};

var request = CodeGeneratorRequest.Parser
    .WithExtensionRegistry(extensionRegistry)
    .ParseFrom(Console.OpenStandardInput());

foreach (var file in request.ProtoFile)
{
    if (file.Name.Equals("google/protobuf/descriptor.proto"))
    {
        continue;
    }


    var sourceCodeInfoLocation = file.SourceCodeInfo.Location;

    foreach (var message in file.MessageType)
    {
        var fileName = message.Name + ".TcDUT";
        var msgComment = GetComments(sourceCodeInfoLocation, GetCurrentPath(file, message));
        var tcDUT = TcDutFactory.CreateTcDUT(message.Name, GetDutAttributes(message.Options), msgComment);
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

            var comments = GetComments(sourceCodeInfoLocation, GetCurrentPath(file, message, field));

#pragma warning disable IDE0072 // Add missing cases
            var processFieldValue = field.Type switch
            {
                FieldDescriptorProto.Types.Type.String => ProcessStringField(field, comments),
                FieldDescriptorProto.Types.Type.Double => ProcessDoubleField(field, comments),
                FieldDescriptorProto.Types.Type.Float => ProcessFloatField(field, comments),
                FieldDescriptorProto.Types.Type.Int64 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Uint64 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Int32 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Fixed64 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Fixed32 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Bool => throw new NotImplementedException(),
                FieldDescriptorProto.Types.Type.Group => throw new NotImplementedException(),
                FieldDescriptorProto.Types.Type.Message => ProcessGenericField(field, comments),
                FieldDescriptorProto.Types.Type.Bytes => ProcessBytesField(field, comments),
                FieldDescriptorProto.Types.Type.Uint32 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Enum => ProcessGenericField(field, comments),
                FieldDescriptorProto.Types.Type.Sfixed32 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Sfixed64 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Sint32 => ProcessIntegerField(field, comments),
                FieldDescriptorProto.Types.Type.Sint64 => ProcessIntegerField(field, comments),
                _ => ProcessUnknownField(field),
            };

            processedFields.Append(processFieldValue);


#pragma warning restore IDE0072 // Add missing cases
        }
        tcDUT.WriteDeclaration(processedFields);
        responseFile.Content += SerializePlcObject(tcDUT);
    }

    response.WriteTo(Console.OpenStandardOutput());
}

static string ProcessBytesField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();
    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }

    if (field.GetArrayLengthWhenBytesFieldOrFail(out var length))
    {
        sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF BYTE;");
    }

    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }

    sb.AppendLine();
    return sb.ToString();
}

static string ProcessIntegerField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();
    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }

    var options = field.Options;
    if (ExtensionsHelper.TryGetAttributeDisplayMode(options, out var displaymodeAttribute))
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
    if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
    {
        sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {dataType};");
    }
    else
    {
        sb.Append($"\t{field.Name} : {dataType};");
    }

    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }

    sb.AppendLine();
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

static string ProcessGenericField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();
    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }

    // Remove leading '.Example.' from type name, e.g. .Example.ST_SubDataType -> ST_SubDataType
    var stripped = field.TypeName.StartsWith('.') ? field.TypeName.Split('.')[^1] : field.TypeName;
    if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
    {
        sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF {stripped};");
    }
    else
    {
        sb.Append($"\t{field.Name} : {stripped};");
    }

    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }
    sb.AppendLine();
    return sb.ToString();
}

static string ProcessStringField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();

    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }
    var options = field.Options;
    if (options is null)
    {
        sb.Append($"\t{field.Name} : STRING;");
    }
    else
    {
        Console.Error.WriteLine($"Field: {field.Name} has options: {options}");
        if (options.TryGetExtension(MaxStringLen, out var extensionValue))
        {
            sb.Append($"\t{field.Name} : STRING({extensionValue});");
        }
        else
        {
            Console.Error.WriteLine($"Field: {field.Name} has options but no MaxStringLen extension");
            sb.Append($"\t{field.Name} : STRING;");
        }
    }
    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }
    sb.AppendLine();
    return sb.ToString();
}

static string ProcessDoubleField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();
    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }

    if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
    {
        sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF LREAL;");
    }
    else
    {
        sb.Append($"\t{field.Name} : LREAL;");
    }

    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }
    sb.AppendLine();
    return sb.ToString();
}

static string ProcessFloatField(FieldDescriptorProto field, Comments comments)
{
    var sb = new StringBuilder();
    if (!string.IsNullOrEmpty(comments.LeadingComments))
    {
        sb.AppendLine(Helper.TransformComment(comments.LeadingComments, "\t"));
    }

    if (field.GetArrayLengthWhenRepeatedLabelOrFail(out var length))
    {
        sb.Append($"\t{field.Name} : ARRAY[0..{length}] OF REAL;");
    }
    else
    {
        sb.Append($"\t{field.Name} : REAL;");
    }

    if (!string.IsNullOrEmpty(comments.TrailingComments))
    {
        sb.Append(Helper.TransformComment(comments.TrailingComments, "\t"));
    }
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

static string GetDutAttributes(MessageOptions? options)
{
    if (options is null)
    {
        return string.Empty;
    }
    var sbDutAttributes = new StringBuilder();
    if (ExtensionsHelper.TryGetAttributePackMode(options, out var packmodeAttribute))
    {
        sbDutAttributes.AppendLine($"{packmodeAttribute}");
    }
    return sbDutAttributes.ToString();
}

static RepeatedField<int> GetCurrentPath(FileDescriptorProto file, DescriptorProto message, FieldDescriptorProto? field = null)
{
    var path = new RepeatedField<int>
            {
                4,
                file.MessageType.IndexOf(message),
            };
    if (field is not null)
    {
        path.Add(2);
        path.Add(message.Field.IndexOf(field));
    }
    return path;
}

static Comments GetComments(RepeatedField<SourceCodeInfo.Types.Location> locations, RepeatedField<int> path)
{
    var leadingComments = locations
            .Where(x => x.Path.SequenceEqual(path) && x.HasLeadingComments)
            .Select(x => x.LeadingComments).FirstOrDefault();
    var trailingComments = locations
            .Where(x => x.Path.SequenceEqual(path) && x.HasTrailingComments)
            .Select(x => x.TrailingComments).FirstOrDefault();
    return new Comments(leadingComments, trailingComments);
}
