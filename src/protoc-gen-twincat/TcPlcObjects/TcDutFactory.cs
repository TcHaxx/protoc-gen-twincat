using System.Reflection;
using System.Text;
using System.Xml;

namespace TcHaxx.ProtocGenTc.TcPlcObjects;

internal static class TcDutFactory
{
    public static TcDUT CreateTcDUT(string typeName, string? attributes)
    {
        var dut = new TcDUT
        {
            Version = Constants.TcPlcObjectVersion,
            DUT = new TcPlcObjectDUT()
            {
                Name = typeName,
                Id = Guid.NewGuid().ToString(),
            }
        };
        dut.WriteHeader();
        dut.DUT.Declaration.Data += attributes;
        return dut;
    }

    public static void WriteDeclaration(this TcDUT dut, StringBuilder declaration)
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

    private static void WriteHeader(this TcDUT dut)
    {
        var header = Helper.GetApplicationHeader(Assembly.GetExecutingAssembly());
        dut.DUT.Declaration ??= new XmlDocument().CreateCDataSection(header);
    }
}
