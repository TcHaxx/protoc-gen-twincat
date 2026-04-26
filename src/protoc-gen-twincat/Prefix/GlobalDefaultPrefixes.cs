namespace TcHaxx.ProtocGenTc.Prefix;

internal static class GlobalDefaultPrefixes
{
    public const string FB_INSTANCE_PREFIX = "_fb";
    public const string ST_INSTANCE_PREFIX = "_st";
    public static readonly Extensions.v1.Prefix FB = new() { Type = "FB_" };
    public static readonly Extensions.v1.Prefix ST = new() { Type = "ST_" };
    public static readonly Extensions.v1.Prefix Enum = new() { Type = "E_" };
}
