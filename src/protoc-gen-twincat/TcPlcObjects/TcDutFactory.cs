using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using TcHaxx.ProtocGenTc.Fields;
using TcHaxx.ProtocGenTc.Prefix;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcDutFactory
{
    public static async Task<TcDUT> CreateEnum(FileDescriptorProto file, EnumDescriptorProto enumDescriptor, Prefixes prefixes)
    {
        var comments = CommentsProvider.GetComments(file, enumDescriptor);
        var name = prefixes.GetEnumNameWithTypePrefix(enumDescriptor);
        var dut = new TcDUT
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            DUT = new TcPlcObjectDUT() { Name = name, Id = Guid.NewGuid().ToString(), }
        };
        dut.WriteHeader();
        dut.DUT.Declaration.Data += GetDutEnumAttributes(enumDescriptor.Options);

        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            dut.DUT.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }

        var processedFields = new StringBuilder();
        await Console.Error.WriteLineAsync($"enum {enumDescriptor.Name} {{");
        for (var i = 0; i < enumDescriptor.Value.Count; i++)
        {
            var enumValue = enumDescriptor.Value[i];
            await Console.Error.WriteLineAsync($"  {enumValue.Name} = {enumValue.Number};");
            var commentsValue = CommentsProvider.GetComments(file, enumDescriptor, enumValue);
            var isLast = i == enumDescriptor.Value.Count - 1;
            var processEnumValue = ProcessEnumValue(enumValue, commentsValue, isLast);
            processedFields.Append(processEnumValue);
        }

        await Console.Error.WriteLineAsync($"}}");

        var options = enumDescriptor.Options;
        if (options.TryGetExtension(TchaxxExtensionsExtensions.EnumIntegerType, out var extensionValue))
        {
            ProtoMapper.TryGetTwinCatDataTypeFromEnumInterTypes(extensionValue, out var dataType);
            dut.WriteEnumDeclaration(processedFields, dataType);
        }
        else
        {
            dut.WriteEnumDeclaration(processedFields);
        }
        return dut;
    }

    public static async Task<TcDUT> CreateStruct(FileDescriptorProto file, DescriptorProto message, Prefixes prefixes)
    {
        var comments = CommentsProvider.GetComments(file, message);
        var name = prefixes.GetStNameWithTypePrefix(message);
        var dut = new TcDUT
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            DUT = new TcPlcObjectDUT() { Name = name, Id = Guid.NewGuid().ToString(), }
        };
        dut.WriteHeader();
        dut.DUT.Declaration.Data += GetMessageAttributes(message.Options);
        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            dut.DUT.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }

        var processedFields = new StringBuilder();
        await Console.Error.WriteLineAsync($"message {message.Name} {{");
        foreach (var field in message.Field)
        {
            await Console.Error.WriteLineAsync($"  {field.Dump()}");
            var commentsField = CommentsProvider.GetComments(file, message, field);
            var processFieldValue = ProcessFieldValue(field, commentsField, prefixes);
            processedFields.Append(processFieldValue);
            if (field.Label == FieldDescriptorProto.Types.Label.Repeated)
            {
                processedFields.AppendLine(RepeatedFieldHelper.WriteCountField(field, prefixes));
            }
        }

        await Console.Error.WriteLineAsync($"}}");
        dut.WriteStructDeclaration(processedFields);

        return dut;
    }

    public static void WriteStructDeclaration(this TcDUT dut, StringBuilder declaration)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                        TYPE {{dut.DUT.Name}} :
                        STRUCT
                        """);
        sb.Append(declaration);
        sb.Append("""
                  END_STRUCT
                  END_TYPE
                  """);

        dut.DUT.Declaration ??= new XmlDocument().CreateCDataSection(string.Empty);
        dut.DUT.Declaration.AppendData(sb.ToString());
    }

    public static void WriteEnumDeclaration(this TcDUT dut, StringBuilder declaration, string? enumIntegerType = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"""
                       TYPE {dut.DUT.Name} :
                       (
                       """);
        sb.Append(declaration);
        sb.Append($"""
                   ) {(string.IsNullOrWhiteSpace(enumIntegerType) ? string.Empty : enumIntegerType)};
                   END_TYPE
                   """);

        dut.DUT.Declaration ??= new XmlDocument().CreateCDataSection(string.Empty);
        dut.DUT.Declaration.AppendData(sb.ToString());
    }

    private static string GetMessageAttributes(MessageOptions? options)
    {
        var sbDutAttributes = new StringBuilder();
        sbDutAttributes.AppendLine("""
                                   {attribute 'no-analysis'}
                                   """);

        if (options is null)
        {
            return sbDutAttributes.ToString();
        }

        if (TryGetAttributePackMode(options, out var packmodeAttribute))
        {
            sbDutAttributes.AppendLine($"{packmodeAttribute}");
        }

        return sbDutAttributes.ToString();
    }

    private static string GetDutEnumAttributes(EnumOptions? options)
    {
        var sbDutAttributes = new StringBuilder();
        sbDutAttributes.AppendLine("""
                                   {attribute 'qualified_only'}
                                   {attribute 'strict'}
                                   {attribute 'to_string'}
                                   """);

        if (options is null)
        {
            return sbDutAttributes.ToString();
        }

        // TODO: Add more enum-specific attributes here if required in the future
        return sbDutAttributes.ToString();
    }

    /// <summary>
    /// Attempts to retrieve the attribute pack mode from the specified message options.
    /// </summary>
    /// <param name="options">The message options from which to extract the attribute pack mode. Can be null.</param>
    /// <param name="value">When this method returns <see langword="true"/>, contains the formatted attribute pack mode string; otherwise,
    /// an empty string.</param>
    /// <returns><see langword="true"/> if the attribute pack mode was found and extracted; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetAttributePackMode(MessageOptions? options, [NotNullWhen(true)] out string value)
    {
        value = string.Empty;
        if (options.TryGetExtension(TchaxxExtensionsExtensions.AttributePackMode, out var packMode))
        {
            value = $"{{attribute 'pack_mode' := '{(int)packMode}'}}";
            return true;
        }

        return false;
    }

    private static void WriteHeader(this TcDUT dut)
    {
        var header = Helper.GetApplicationHeader(Assembly.GetExecutingAssembly());
        dut.DUT.Declaration ??= new XmlDocument().CreateCDataSection(header);
    }

    private static string ProcessFieldValue(FieldDescriptorProto field, Comments comments, Prefixes prefixes)
    {
#pragma warning disable IDE0072 // Add missing cases
        var processFieldValue = field.Type switch
        {
            FieldDescriptorProto.Types.Type.Bool => BooleanFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Bytes => BytesFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Double => DoubleFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Enum => GenericFieldProvider.ProcessField(field, comments, prefixes),
            FieldDescriptorProto.Types.Type.Fixed32 => IntegerFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Fixed64 => IntegerFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Float => FloatFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Group => ProcessUnknownField(field),
            FieldDescriptorProto.Types.Type.Int32 => IntegerFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Int64 => IntegerFieldProvider.ProcessField(field, comments),
            FieldDescriptorProto.Types.Type.Message => GenericFieldProvider.ProcessField(field, comments, prefixes),
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

    private static string ProcessUnknownField(FieldDescriptorProto field)
    {
        var error = $"Unhandled field type: {field.Name} : {field.Type}";
        Console.Error.WriteLine(error);
        return $"// {error}\r\n";
    }

    private static string ProcessEnumValue(EnumValueDescriptorProto enumValue, Comments comments, bool isLastValue)
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
}
