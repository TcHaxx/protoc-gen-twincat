namespace TcHaxx.ProtocGenTc.TcPlcObjects;

public static class ImplementationExtensions
{
    extension(Implementation)
    {
        public static Implementation Empty => new() { ST = CData.EmptyCData };
    }
}
