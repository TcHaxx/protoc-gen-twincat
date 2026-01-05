using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using TcHaxx.Extensions.v1;
using static TcHaxx.Extensions.v1.TchaxxExtensionsExtensions;

namespace TcHaxx.ProtocGenTc;

/// <summary>
/// Provides helper methods for working with custom options/extensions.
/// </summary>
internal static class ExtensionsHelper
{

    /// <summary>
    /// Tries to get the value of a specified extension from an extendable message.
    /// </summary>
    /// <typeparam name="T">The type of the extendable message.</typeparam>
    /// <typeparam name="V">The type of the extension value.</typeparam>
    /// <param name="option">The extendable message instance.</param>
    /// <param name="extension">The extension to retrieve the value for.</param>
    /// <param name="value">When this method returns, contains the value of the extension if it exists; otherwise, the default value for the type of the extension.</param>
    /// <returns><c>true</c> if the extension exists and its value was retrieved; otherwise, <c>false</c>.</returns>
    public static bool TryGetExtension<T, V>(this IExtendableMessage<T>? option, Extension<T, V> extension, [NotNullWhen(true)] out V? value)
        where T : IExtendableMessage<T>
    {
        if (option is null || !option.HasExtension(extension))
        {
            value = default;
            return false;
        }

        value = option.GetExtension(extension)!;
        return true;
    }

    public static bool HasRepeatedLabel(this FieldDescriptorProto field)
    {
        return field.Label == FieldDescriptorProto.Types.Label.Repeated;
    }

    public static bool GetArrayLengthWhenRepeatedLabelOrFail(this FieldDescriptorProto field, out uint length)
    {
        length = 0;
        if (!field.HasRepeatedLabel())
        {
            return false;
        }

        if (!field.Options.TryGetExtension(ArrayLen, out var len))
        {
            throw new InvalidOperationException($"Field {field.Name} has label \"repeated\" but no TcHaxx.Extensions.v1.{nameof(ArrayLen)} extension");
        }

        length = len > 0 ? len - 1 : 0;
        return true;
    }

    public static bool GetArrayLengthWhenBytesFieldOrFail(this FieldDescriptorProto field, out uint length)
    {
        length = 0;
        if (!field.Options.TryGetExtension(ArrayLen, out var len))
        {
            throw new InvalidOperationException($"Field {field.Name} (bytes) required TcHaxx.Extensions.v1.{nameof(ArrayLen)} extension missing");
        }

        length = len > 0 ? len - 1 : 0;
        return true;
    }

    public static bool TryGetAttributeDisplayMode(FieldOptions? options, [NotNullWhen(true)] out string value)
    {
        value = string.Empty;
        if (options.TryGetExtension(AttributeDisplayMode, out var displayMode))
        {
            return false;
        }

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
}
