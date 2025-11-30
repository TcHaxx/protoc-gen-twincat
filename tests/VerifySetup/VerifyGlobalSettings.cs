namespace TcHaxx.ProtocGenTcTests.VerifySetup;

internal static class VerifyGlobalSettings
{
    private static VerifySettings? _settings;

    /// <summary>
    /// Returns or initializes global settings for Verify
    /// </summary>
    /// <returns></returns>
    public static VerifySettings GetGlobalSettings()
    {
        return _settings ?? Instantiate();
    }

    private static VerifySettings Instantiate()
    {
        _settings = new VerifySettings();
        _settings.UseDirectory(".verified");
        return _settings;
    }
}
