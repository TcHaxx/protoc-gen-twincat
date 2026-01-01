namespace TcHaxx.ProtocGenTc.TcPlcObjects;

public static class TcPouImplementationExtensions
{
    extension(TcPlcObjectPOUImplementation)
    {
        public static TcPlcObjectPOUImplementation Empty => new() { ST = CData.EmptyCData };
    }
}
