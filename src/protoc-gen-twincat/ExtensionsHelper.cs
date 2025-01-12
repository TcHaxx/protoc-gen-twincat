using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;

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
}
