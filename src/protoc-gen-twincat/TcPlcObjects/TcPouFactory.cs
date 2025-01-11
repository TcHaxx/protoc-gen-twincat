namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcPouFactory
{
    public static TcPOU Create(string filename)
    {
        var pou = new TcPOU
        {
            Version = Constants.TcPlcObjectVersion,
            POU = new TcPlcObjectPOU()
            {
                Name = filename,
                Id = Guid.NewGuid().ToString(),
            }
        };
        return pou;
    }
}
