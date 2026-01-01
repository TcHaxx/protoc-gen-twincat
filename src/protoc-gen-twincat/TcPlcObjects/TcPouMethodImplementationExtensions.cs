namespace TcHaxx.ProtocGenTc.TcPlcObjects;

public static class TcPouMethodImplementationExtensions
{
    extension(TcPlcObjectPOUMethodImplementation)
    {
        public static TcPlcObjectPOUMethodImplementation Empty => new() { ST = CData.EmptyCData };
    }
}
