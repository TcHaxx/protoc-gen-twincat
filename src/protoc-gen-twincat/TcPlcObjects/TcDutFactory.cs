using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Xml;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcDutFactory
{
    public static TcDUT CreateEnum(EnumDescriptorProto enumDescriptor, Comments comments)
    {
        var dut = new TcDUT
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            DUT = new TcPlcObjectDUT()
            {
                Name = enumDescriptor.Name,
                Id = Guid.NewGuid().ToString(),
            }
        };
        dut.WriteHeader();
        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            dut.DUT.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }

        dut.DUT.Declaration.Data += GetDutEnumAttributes(enumDescriptor.Options);
        return dut;
    }

    public static TcDUT CreateStruct(DescriptorProto message, Comments comments)
    {
        var dut = new TcDUT
        {
            Version = Constants.TC_PLC_OBJECT_VERSION,
            DUT = new TcPlcObjectDUT()
            {
                Name = message.Name,
                Id = Guid.NewGuid().ToString(),
            }
        };
        dut.WriteHeader();
        if (!string.IsNullOrWhiteSpace(comments.LeadingComments))
        {
            dut.DUT.Declaration.Data += comments.NormalizedComments(CommentType.Leading) + Environment.NewLine;
        }

        dut.DUT.Declaration.Data += GetMessageAttributes(message.Options);

        return dut;
    }

    public static void WriteStructDeclaration(this TcDUT dut, StringBuilder declaration)
    {
        var sb = new StringBuilder();
        sb.Append($"""
            TYPE {dut.DUT.Name} :
            STRUCT
            
            """);
        sb.Append(declaration);
        sb.Append($"""
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
            value = $@"{{attribute 'pack_mode' := '{packMode}'}}";
            return true;
        }

        return false;
    }

    private static void WriteHeader(this TcDUT dut)
    {
        var header = GetApplicationHeader(Assembly.GetExecutingAssembly());
        dut.DUT.Declaration ??= new XmlDocument().CreateCDataSection(header);
    }

    /// <summary>
    /// Prints the application header based on <see cref="Assembly"/>-attributes as <see cref="string"/>.
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    private static string GetApplicationHeader(Assembly assembly)
    {
        var sb = new StringBuilder(1000);

        var version = assembly.GetName().Version;
        var repoUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                              .FirstOrDefault(x => x.Key.Equals("RepositoryUrl"))?.Value ?? string.Empty;

        var delimiter = new string(ProtocGenTc.Constants.CLI_DELIMITER, ProtocGenTc.Constants.WIDTH - 1);
        sb.AppendLine($"({delimiter}");
        sb.AppendLine($"{assembly.GetName().Name} v{version}".Center());
        if (!string.IsNullOrWhiteSpace(repoUrl))
        {
            sb.AppendLine($"{repoUrl.Center()}");
        }

        sb.AppendLine($"{delimiter})");

        return sb.ToString();
    }

    /// <summary>
    /// Centers a <see cref="string"/>.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="width"></param>
    /// <returns>centered string</returns>
    private static string Center(this string s, int width = ProtocGenTc.Constants.WIDTH)
    {
        return string.IsNullOrEmpty(s) ?
            string.Empty :
            s.PadLeft(((width - s.Length) / 2) + s.Length).PadRight(width);
    }
}
